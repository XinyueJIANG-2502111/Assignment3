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
    // 之前拖 WordBlockPrefab 的槽位，现在用来拖你的随机图标方块预制件
    // Slot for your icon block prefab
    public GameObject blockPrefab;
    public float spawnInterval = 1.0f;

    // 虚拟网格行列数，用于将图标完美错开，防止扎堆
    // Grid configuration for pseudo-random discrete layout
    private int gridRows = 4;
    private int gridCols = 3;
    private List<Vector2> availableGridPositions = new List<Vector2>();
    private int currentGridIndex = 0;

    // 动态计算出的绝对安全屏幕边界（世界坐标）
    // Calculated solid boundaries in world space
    private float minX, maxX, minY, maxY;

    [Header("Other Settings")]
    public float gameDuration = 30f; 
    private float timeRemaining;     
    private bool isGameOver = false; 

    [Header("Input Optimization")]
    public float clickBufferRadius = 0.3f; 

    [Header("Icon Pool Settings (New)")]
    // 【新增】取代原本的纯文本词库资产，在这里直接放入你准备好的尺寸一致的图标
    // [New] Drag and drop your square icon sprites here via the Inspector
    public Sprite[] iconPool;

    

    void Start()
    {
        // 1. 彻底解决出界：根据当前摄像机和视口动态计算绝对安全边界
        // Calculate secure boundaries using the current main camera
        CalculateSecureBoundaries();

        // 2. 彻底解决扎堆：在安全范围内划分虚拟网格，并完成初始洗牌
        // Partition virtual grid cells and shuffle them initially
        GenerateGridPositions();

        // 3. 初始化游戏状态 / Initialize game state
        timeRemaining = gameDuration;
        isGameOver = false;
        isVictory = false;
        score = 0;

        if (uiManager != null)
        {
            uiManager.InitUI(gameDuration, maxAllowedBlocks);
        }

        // 4. 开启开局倒计时，然后再生成图标
        // Start opening sequence before releasing icons
        StartCoroutine(OpeningSequenceRoutine());
    }

    // 计算安全的屏幕边界（已扣除图标预估大小的内边距）
    // Secure boundary calculation considering block width and height cushions
    void CalculateSecureBoundaries()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        // 因为图标是 1:1 的正方形，尺寸十分稳定，左右上下预留 1.2f 的内边距绝对不会出界
        // Consistent 1.2f padding keeps the square icons completely on-screen
        float paddingX = 1.2f; 
        float paddingY = 1.2f; 

        minX = bottomLeft.x + paddingX;
        maxX = topRight.x - paddingX;
        minY = bottomLeft.y + paddingY;
        maxY = topRight.y - paddingY;
    }

    // 在安全边界内划分虚拟网格，从根本上消灭堆叠卡死
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
        currentGridIndex = 0;
    }

    // 开局 3, 2, 1 倒计时协程，压住主游戏时钟
    // Opening countdown locks core timers until completion
    IEnumerator OpeningSequenceRoutine()
    {
        // 临时锁住 Update 里的倒计时和垃圾统计跑路
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

        // 倒计时结束，释放限制
        isGameOver = false; 

        // 正式开始生成随机图标
        StartCoroutine(SpawnRoutine());
    }

    // 升级后的网格抖动生成协程
    // Upgraded spawn routine featuring grid jittering and color randomizations
    IEnumerator SpawnRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 场上图标满了就挂起 / Pause if screen is full
            if (GetCurrentBlockCount() >= maxAllowedBlocks) continue;

            if (blockPrefab != null && iconPool != null && iconPool.Length > 0)
            {
                // 【真·纯随机】直接在动态计算的绝对安全边界（minX到maxX，minY到maxY）里狂野抽点！
                // [True Random] Wildly pitch coordinates directly within secure screen boundaries
                float pureRandomX = Random.Range(minX, maxX);
                float pureRandomY = Random.Range(minY, maxY);
                Vector3 finalSpawnPos = new Vector3(pureRandomX, pureRandomY, 0f);

                // 实例化图标 / Instantiate the icon
                GameObject newBlock = Instantiate(blockPrefab, finalSpawnPos, Quaternion.identity);
                newBlock.transform.SetParent(this.transform); 

                // 随机抽图标与色彩 / Randomize look and feel
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

        // 倒计时逻辑：调用全新的平滑环形进度条 UI
        // Timer logic: hooks to the newly implemented circular progress bar interface
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (uiManager != null) uiManager.UpdateTimer(timeRemaining);
        }
        else
        {
            TriggerGameOver(true);
        }

        // 检测点触
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }

        // 动态统计场上的图标总数，实时回传至垃圾计数器
        int liveBlocks = GetCurrentBlockCount();
        if (uiManager != null)
        {
            uiManager.UpdateTrashCount(liveBlocks, maxAllowedBlocks);
        }

        // 触发爆血管的失败条件：如果场上图标积压到15个以上，当场输掉
        // Game Over condition: if the garbage screen overflows past the limits
        if (liveBlocks >= maxAllowedBlocks)
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
            // 确保点中的是带有全新 DynamicIconBlock 脚本的图标
            // Verify that the raycast catches the rewritten icon component
            DynamicIconBlock block = hitCollider.GetComponent<DynamicIconBlock>();
            if (block != null)
            {
                score++;
                if (uiManager != null) uiManager.UpdateScore(score);

                // 物理销毁
                //Destroy(block.gameObject);
                block.TriggerClickBlast();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(mouseWorld.x, mouseWorld.y, 0), clickBufferRadius);
        }
    }

    public void TriggerGameOver(bool won)
    {
        if (isGameOver) return; 
        isGameOver = true;

        isVictory = won;
        SceneManager.LoadScene("ResultScene");
    }

    // 辅助工具：获取当前作为子物体存活在 GameManager 下的图标数
    // Helper to extract active block count directly via transform hierachy
    int GetCurrentBlockCount()
    {
        return transform.childCount;
    }

    // 经典费舍尔-耶茨二维向量洗牌算法 / Fisher-Yates Shuffle for Grid Positions
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
}



// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;
// using UnityEngine;

// public class GameManager : MonoBehaviour
// {
//     [Header("UI System")]
//     public GameUIManager uiManager;
//     private int score; 
//     // 【新增】为了方便 UI 读取，把最大允许方块数也移到 GameManager 里来统一管理
//     // [New] Manage max allowed blocks here for global reference
//     public int maxAllowedBlocks = 15;

//     public static bool isVictory = false;
//     float minHeightY;
//     float maxHeightY;

//     // 在 Inspector 中拖入刚才制作的方块预制件
//     // Drag and drop the block prefab into this slot via Inspector
//     public GameObject blockPrefab;

//     // 每隔多少秒生成一个方块
//     // How many seconds between each spawn
//     public float spawnInterval = 1.0f;

//     // 屏幕左右生成的 X 轴范围（暂定一个固定值，后续可动态计算）
//     // The X-axis range for spawning (temporary fixed value, can be dynamic later)
//     public float spawnRangeX = 2.0f;

//     // 生成的 Y 轴高度（确保在屏幕上方外）
//     // The Y-axis height for spawning (above the visible screen)
//     public float spawnHeightY = 6.0f;


//     [Header("Other Settings")]
//     // 游戏总时长（秒） / Total game duration (in seconds)
//     public float gameDuration = 30f; 
//     // 剩余时间计数器 / Remainder time counter
//     private float timeRemaining;     
//     // 游戏是否已经结束的标志 / Flag to check if game is already over
//     private bool isGameOver = false; 


//     [Header("Input Optimization")]
//     // 点击判定增加的额外半径（单位：米），数值越大越容易点中
//     // Extra radius added to the click detection (in units). Higher = easier to click.
//     public float clickBufferRadius = 0.3f; 


//     [Header("Data Source")]
//     // 拖入你刚刚创建的 WordPoolAsset 资产文件 / Drag your WordPoolAsset here
//     public WordPoolAsset wordPoolSource;

//     // 运行时的“抽卡包” / The runtime dynamic spawning bag
//     private List<string> runtimeSpawnBag = new List<string>();


//     void Start()
//     {
//         // 初始化自适应边界 / Initialize adaptive boundaries
//         Camera mainCam = Camera.main;
//         Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
//         Vector3 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0));

//         // 稍微往内缩一点，防止方块卡出屏幕外
//         spawnRangeX = topRight.x - 1.0f; 
//         // 新增一个变量或者直接在这里用：
//         // 允许生成的 Y 轴范围变成整个屏幕高度
//         minHeightY = bottomLeft.y + 1.0f;
//         maxHeightY = topRight.y - 1.0f; 

//         // 初始化游戏状态 / Initialize game state
//         timeRemaining = gameDuration;
//         isGameOver = false;
//         isVictory = false;
//         score = 0;

//         if (uiManager != null)
//         {
//             uiManager.InitUI(gameDuration, maxAllowedBlocks);
//         }

//         // 开启循环生成协程
//         // Start the loop spawning coroutine
//         StartCoroutine(SpawnRoutine());
//     }

//     IEnumerator SpawnRoutine()
//     {
//         // 只要游戏没结束，就一直生成方块 / Keep spawning as long as game is not over
//         while (!isGameOver)
//         {
//             float randomX = Random.Range(-spawnRangeX, spawnRangeX);
//             Vector3 spawnPos = new Vector3(randomX, spawnHeightY, 0);
//             //Instantiate(blockPrefab, spawnPos, Quaternion.identity);
//             SpawnWord();
            
//             // 可以根据需要调整下落频率 / Adjust spawn speed as needed
//             yield return new WaitForSeconds(0.8f); 
//         }
//     }

//     void Update()
//     {
//         if (isGameOver) return;
//         if (Time.timeScale == 0f) return;

//         // 倒计时逻辑 / Countdown logic
//         if (timeRemaining > 0)
//         {
//             timeRemaining -= Time.deltaTime;
//             if (uiManager != null) uiManager.UpdateTimer(timeRemaining);
//         }
//         else
//         {
//             // 时间到了！玩家成功撑过了30秒，判定胜利！
//             // Time's up! Player survived 30 seconds, Victory!
//             TriggerGameOver(true);
//         }

//         // 检测鼠标左键点击，或者手机单指触摸
//         // Detect mouse left click or mobile single touch
//         if (Input.GetMouseButtonDown(0))
//         {
//             HandleClick(Input.mousePosition);
//         }

//         // Constantly sync live block count to UI
//         int liveBlocks = FindObjectsOfType<DynamicWordBlock>().Length;
//         if (uiManager != null)
//         {
//             uiManager.UpdateTrashCount(liveBlocks, maxAllowedBlocks);
//         }

//         Debug.Log(score);
//     }

//     void HandleClick(Vector3 screenPosition)
//     {
//         // 将屏幕点击的像素坐标转换为 2D 世界坐标
//         // Convert screen pixel position to 2D world position
//         Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
//         Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);

//         // 使用重叠圆圈检测，不仅检测点本身，还包含我们设置的缓冲区半径
//         // Use OverlapCircle to detect objects within a small radius of the touch point
//         Collider2D hitCollider = Physics2D.OverlapCircle(touchPos, clickBufferRadius);

//         if (hitCollider != null)
//         {
//             // 确保点中的是我们想要消除的方块
//             // Ensure the hit object is our target block
//             DynamicWordBlock block = hitCollider.GetComponent<DynamicWordBlock>();
//             if (block != null)
//             {
//                 // 【新增】击碎方块，加分，并同步给 UI！
//                 score++;
//                 if (uiManager != null) uiManager.UpdateScore(score);

//                 // 触发消除逻辑
//                 // Trigger the destruction logic
//                 Destroy(block.gameObject);

//                 // 这里以后可以用来触发连击加分、播放音效等
//                 // This can be used for combo counter, SFX, etc. later
//                 //Debug.Log("精准消灭高速方块！");
//             }
//         }
//     }

//     // 在 Scene 视图中画出这个点击缓冲圈，方便你调试
//     // Draw the click buffer in Scene view for debugging purposes
//     private void OnDrawGizmos()
//     {
//         if (Camera.main != null)
//         {
//             Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(new Vector3(mouseWorld.x, mouseWorld.y, 0), clickBufferRadius);
//         }
//     }

//     public void TriggerGameOver(bool won)
//     {
//         // 安全锁，防止在一帧内同时触发胜利和失败（比如30秒到的瞬间刚好方块触顶）
//         // Safety lock to prevent simultaneous win/loss triggers within a single frame
//         if (isGameOver) return; 
//         isGameOver = true;

//         isVictory = won;
//         //Debug.Log(won ? "【胜利】坚持了30秒！" : "【失败】方块触顶了！");

//         // 跳转到结算场景 / Load Result Scene
//         SceneManager.LoadScene("ResultScene");
//     }

    
//     string GetNextWord()
//     {
//         // 安全检查：如果忘记拖入资产，返回一个保底词
//         // Safety check: if asset is missing, return a fallback word
//         if (wordPoolSource == null || wordPoolSource.everythingTrashPool.Count == 0)
//         {
//             Debug.LogError("【错误】GameManager 身上没有挂载词库资产！/ WordPoolAsset is missing!");
//             return "Missing Word Asset";
//         }

//         // 如果包空了，从资产文件中复制一份放进去，并重新洗牌
//         // If the bag is empty, refill it from the asset pool and shuffle
//         if (runtimeSpawnBag.Count == 0)
//         {
//             runtimeSpawnBag.AddRange(wordPoolSource.everythingTrashPool);
//             ShuffleBag(runtimeSpawnBag);
//             Debug.Log("<color=cyan>【系统】词库抽空，已重新从资产洗牌！</color>");
//         }

//         int lastIndex = runtimeSpawnBag.Count - 1;
//         string chosenWord = runtimeSpawnBag[lastIndex];
//         runtimeSpawnBag.RemoveAt(lastIndex);

//         return chosenWord;
//     }

//     void SpawnWord()
//     {
//         if (blockPrefab == null || wordPoolSource.everythingTrashPool.Count == 0) return;

//         float randomX = Random.Range(-spawnRangeX, spawnRangeX);
//         float randomY = Random.Range(minHeightY, maxHeightY); // 全屏幕随机 Y 轴
//         Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

//         //float randomX = Random.Range(-spawnRangeX, spawnRangeX);
//         //Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, 0);

//         // 实例化自适应方块 / Instantiate the adaptive block
//         GameObject newBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);

//         // 随机抽一个精神垃圾 / Pick a random spiritual trash
//         string randomTrash = GetNextWord();

//         // 调用 Setup 函数，方块会自动根据字数长短变长或变短！
//         // Call Setup, the block will automatically resize based on word length!
//         DynamicWordBlock script = newBlock.GetComponent<DynamicWordBlock>();
//         if (script != null)
//         {
//             script.Setup(randomTrash);
//         }
//     }

//     // 经典费舍尔-耶茨洗牌算法 / Classic Fisher-Yates Shuffle Algorithm
//     // 这是一个原汁原味的 C# 数组/列表打乱算法，常用于游戏开发中的卡组洗牌
//     void ShuffleBag(List<string> list)
//     {
//         int n = list.Count;
//         while (n > 1)
//         {
//             n--;
//             // 随机挑选一个索引 / Pick a random index
//             int k = Random.Range(0, n + 1);
        
//             // 交换两个元素的位置 / Swap the two elements
//             string value = list[k];
//             list[k] = list[n];
//             list[n] = value;
//         }
//     }
// }