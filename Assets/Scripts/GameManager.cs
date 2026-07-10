using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI System")]
    public GameUIManager uiManager;
    private int score; 
    public int maxAllowedBlocks = 15;

    public static bool isVictory = false;

    [Header("Spawning & Boundary Settings (Upgraded)")]
    public GameObject blockPrefab;
    public float spawnInterval = 1.0f;

    // ランダム離散レイアウトのグリッド構成
    // Grid configuration for pseudo-random discrete layout
    private int gridRows = 10;
    private int gridCols = 7;
    private List<Vector2> availableGridPositions = new List<Vector2>();
    //private int currentGridIndex = 0;

    // 画面の境界
    // Calculated solid boundaries in world space
    private float minX, maxX, minY, maxY;

    [Header("Other Settings")]
    public float gameDuration = 30f; 
    private float timeRemaining;     
    private bool isGameOver = false; 

    [Header("Input Optimization")]
    public float clickBufferRadius = 0.3f; 

    [Header("Icon Pool Settings")]
    // アイテムの画像を格納する容器
    // Container for all the icon sprites to be randomly selected
    public Sprite[] iconPool;

    [Header("Glitch Transition Settings")]
    public RectTransform glitchMaskRect;
    public float transitionDuration = 0.5f;

    void Start()
    {
        // 境界を計算する
        // Calculate secure boundaries using the current main camera
        CalculateSecureBoundaries();

        // 仮想グリッドセルを分割し、最初にシャッフルする
        // Partition virtual grid cells and shuffle them initially
        GenerateGridPositions();

        // ゲームシーンの初期化 / Initialize game state
        timeRemaining = gameDuration;
        isGameOver = false;
        isVictory = false;
        score = 0;

        if (uiManager != null)
        {
            uiManager.InitUI(gameDuration, maxAllowedBlocks);
        }

        // 開始前にカウントダウンをする
        // Start opening sequence before releasing icons
        StartCoroutine(OpeningSequenceRoutine());
    }

    // 画面境界を計算する
    // Secure boundary calculation considering block width and height cushions
    void CalculateSecureBoundaries()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        // padding to ensure blocks don't spawn partially off-screen
        float paddingX = 1.2f; 
        float paddingY = 1.2f; 

        minX = bottomLeft.x + paddingX;
        maxX = topRight.x - paddingX;
        minY = bottomLeft.y + paddingY;
        maxY = topRight.y - paddingY;
    }

    // グリッドセルを構築し、そのシーケンスをシャッフルする
    // Construct grid cells and shuffle their sequence
    void GenerateGridPositions()
    {
        availableGridPositions.Clear();

        float stepX = (maxX - minX) / gridCols;
        float stepY = (maxY - minY) / gridRows;

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                float centerX = minX + (c + 0.5f) * stepX;
                float centerY = minY + (r + 0.5f) * stepY;
                availableGridPositions.Add(new Vector2(centerX, centerY));
            }
        }

        ShuffleGrid(availableGridPositions);
        //currentGridIndex = 0;
    }

    // カウントダウン
    // Opening countdown locks core timers until completion
    IEnumerator OpeningSequenceRoutine()
    {
        isGameOver = true; 

        if (uiManager != null && uiManager.timerText != null)
        {
            uiManager.timerText.gameObject.SetActive(true);

            uiManager.timerText.text = "3";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "2";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "1";
            yield return new WaitForSeconds(1.0f);

            uiManager.timerText.text = "<color=red>GO!</color>"; 
            yield return new WaitForSeconds(0.8f);
        }

        isGameOver = false; 

        // ゲーム開始後、アイテムの生成を開始する
        // Start spawning icons after countdown
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Pause if screen is full
            if (GetCurrentBlockCount() > maxAllowedBlocks) continue;

            if (blockPrefab != null && iconPool != null && iconPool.Length > 0)
            {
                // 画面境界内でランダムな位置を生成する / Generate a random position within the secure boundaries
                float pureRandomX = Random.Range(minX, maxX);
                float pureRandomY = Random.Range(minY, maxY);
                Vector3 finalSpawnPos = new Vector3(pureRandomX, pureRandomY, 0f);

                // Instantiate the icon
                GameObject newBlock = Instantiate(blockPrefab, finalSpawnPos, Quaternion.identity);
                newBlock.transform.SetParent(this.transform); 

                // Randomize look and feel
                Sprite randomIcon = iconPool[Random.Range(0, iconPool.Length)];
                Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.8f, 1f);

                DynamicIconBlock iconScript = newBlock.GetComponent<DynamicIconBlock>();
                if (iconScript != null)
                {
                    iconScript.Setup(randomIcon, randomColor);
                }
            }
        }
    }

    void Update()
    {
        if (isGameOver) return;
        if (Time.timeScale == 0f) return;

        // カウントダウン：円環の進捗バーで残り時間を表示
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (uiManager != null) uiManager.UpdateTimer(timeRemaining);
        }
        else
        {
            TriggerGameOver(true);
        }

        // クリックを検出する
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }

        // 画面上のアイテム数をカウントする
        int liveBlocks = GetCurrentBlockCount();
        if (uiManager != null)
        {
            uiManager.UpdateTrashCount(liveBlocks, maxAllowedBlocks);
        }

        // ゲームオーバー条件：アイテムの数が設定した限界（15）を越えた場合
        // Game Over condition: if the garbage screen overflows past the limits
        if (liveBlocks > maxAllowedBlocks)
        {
            TriggerGameOver(false);
        }
    }

    void HandleClick(Vector3 screenPosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D hitCollider = Physics2D.OverlapCircle(touchPos, clickBufferRadius);

        if (hitCollider != null)
        {
            // レイキャストが書き換えられたアイコン コンポーネントをキャッチすることを確認する
            // Verify that the raycast catches the rewritten icon component
            DynamicIconBlock block = hitCollider.GetComponent<DynamicIconBlock>();
            if (block != null)
            {
                score++;
                if (uiManager != null) uiManager.UpdateScore(score);

                // エフェクトを再生し、アイテムを削除する
                // Trigger the click blast effect and destroy the icon
                block.TriggerClickBlast();
            }
        }
    }

    // 画面上にあるアイテムの数を取得する
    // Helper to extract active block count directly via transform hierachy
    int GetCurrentBlockCount()
    {
        return transform.childCount;
    }

    // Fisher-Yates Shuffle for Grid Positions
    void ShuffleGrid(List<Vector2> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Vector2 value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // ゲームオーバーのトリガー（エフェクトを再生してからシーン遷移）
    // Upgraded Game Over trigger that intercepts direct scene loading
    public void TriggerGameOver(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;
        isVictory = isWin;

        // プレイヤーからの入力をロックする
        // Lock out all player inputs instantly
        var allBlocks = FindObjectsOfType<DynamicIconBlock>();
        foreach (var block in allBlocks)
        {
            Collider2D col = block.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        // 画面オフの遷移シーケンスを起動します
        // Fire the screen-off transition sequence
        StartCoroutine(GlitchScreenOffRoutine(isWin));
    }

    IEnumerator GlitchScreenOffRoutine(bool isWin)
    {
        if (glitchMaskRect != null)
        {
            float elapsed = 0f;

            // Phase 1: Condense into a sharp horizontal pixel line
            while (elapsed < transitionDuration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.4f);
                
                glitchMaskRect.localScale = new Vector3(1f, Mathf.Lerp(0f, 0.02f, t), 1f);
                yield return null;
            }

            // Micro-freeze to emphasize system failure
            yield return new WaitForSeconds(0.03f);

            elapsed = 0f;
            // Phase 2: Snap expansion to swallow the entire screen in total darkness
            while (elapsed < transitionDuration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.6f);

                float curve = Mathf.Pow(t, 3);
                glitchMaskRect.localScale = new Vector3(1f, Mathf.Lerp(0.02f, 1f, curve), 1f);
                yield return null;
            }
        }
        else
        {
            // UIが見つからなかった場合、一定の時間後に直接シーン遷移する
            // Fallback: If glitch mask is missing, wait a moment and load scene directly
            yield return new WaitForSeconds(transitionDuration);
        }

        // エフェクト再生完了後、リザルトシーンに遷移する
        // Screen is now pitch black. Load the results safely.
        SceneManager.LoadScene("ResultScene");
    }


    // デバッグ用：カーソルの位置にクリック判定範囲を描画する
    // Debugging: Draw the click buffer radius around the mouse cursor
    private void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(mouseWorld.x, mouseWorld.y, 0), clickBufferRadius);
        }
    }
}