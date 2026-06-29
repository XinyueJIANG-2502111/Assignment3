using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isVictory = false;

    // 在 Inspector 中拖入刚才制作的方块预制件
    // Drag and drop the block prefab into this slot via Inspector
    public GameObject blockPrefab;

    // 每隔多少秒生成一个方块
    // How many seconds between each spawn
    public float spawnInterval = 1.0f;

    // 屏幕左右生成的 X 轴范围（暂定一个固定值，后续可动态计算）
    // The X-axis range for spawning (temporary fixed value, can be dynamic later)
    public float spawnRangeX = 2.0f;

    // 生成的 Y 轴高度（确保在屏幕上方外）
    // The Y-axis height for spawning (above the visible screen)
    public float spawnHeightY = 6.0f;

    [Header("Other Settings")]
    // 游戏总时长（秒） / Total game duration (in seconds)
    public float gameDuration = 30f; 
    // 剩余时间计数器 / Remainder time counter
    private float timeRemaining;     
    // 游戏是否已经结束的标志 / Flag to check if game is already over
    private bool isGameOver = false; 

    void Start()
    {
        // 初始化自适应边界 / Initialize adaptive boundaries
        Camera mainCam = Camera.main;
        Vector3 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        spawnRangeX = topRight.x - 0.5f; 
        spawnHeightY = topRight.y + 1.0f; 

        // 初始化游戏状态 / Initialize game state
        timeRemaining = gameDuration;
        isGameOver = false;
        isVictory = false;
        // 开启循环生成协程
        // Start the loop spawning coroutine
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // 只要游戏没结束，就一直生成方块 / Keep spawning as long as game is not over
        while (!isGameOver)
        {
            float randomX = Random.Range(-spawnRangeX, spawnRangeX);
            Vector3 spawnPos = new Vector3(randomX, spawnHeightY, 0);
            //Instantiate(blockPrefab, spawnPos, Quaternion.identity);
            SpawnWord();
            
            // 可以根据需要调整下落频率 / Adjust spawn speed as needed
            yield return new WaitForSeconds(0.8f); 
        }
    }

    [Header("Input Optimization")]
    // 点击判定增加的额外半径（单位：米），数值越大越容易点中
    // Extra radius added to the click detection (in units). Higher = easier to click.
    public float clickBufferRadius = 0.3f; 

    void Update()
    {
        if (isGameOver) return;

        // 倒计时逻辑 / Countdown logic
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            
            // (可选) 你可以在这里把时间印在 UI 文本上，比如：
            // timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
        else
        {
            // 时间到了！玩家成功撑过了30秒，判定胜利！
            // Time's up! Player survived 30 seconds, Victory!
            TriggerGameOver(true);
        }

        // 检测鼠标左键点击，或者手机单指触摸
        // Detect mouse left click or mobile single touch
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
    }

    void HandleClick(Vector3 screenPosition)
    {
        // 将屏幕点击的像素坐标转换为 2D 世界坐标
        // Convert screen pixel position to 2D world position
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);

        // 使用重叠圆圈检测，不仅检测点本身，还包含我们设置的缓冲区半径
        // Use OverlapCircle to detect objects within a small radius of the touch point
        Collider2D hitCollider = Physics2D.OverlapCircle(touchPos, clickBufferRadius);

        if (hitCollider != null)
        {
            // 确保点中的是我们想要消除的方块
            // Ensure the hit object is our target block
            BaseBlock block = hitCollider.GetComponent<BaseBlock>();
            if (block != null)
            {
                // 触发消除逻辑
                // Trigger the destruction logic
                Destroy(block.gameObject);
                
                // 这里以后可以用来触发连击加分、播放音效等
                // This can be used for combo counter, SFX, etc. later
                //Debug.Log("精准消灭高速方块！");
            }
        }
    }

    // 在 Scene 视图中画出这个点击缓冲圈，方便你调试
    // Draw the click buffer in Scene view for debugging purposes
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
        // 安全锁，防止在一帧内同时触发胜利和失败（比如30秒到的瞬间刚好方块触顶）
        // Safety lock to prevent simultaneous win/loss triggers within a single frame
        if (isGameOver) return; 
        isGameOver = true;

        isVictory = won;
        //Debug.Log(won ? "【胜利】坚持了30秒！" : "【失败】方块触顶了！");

        // 跳转到结算场景 / Load Result Scene
        SceneManager.LoadScene("ResultScene");
    }

    // [Header("Universal Trash Pool")]
    // public List<string> everythingTrashPool = new List<string>()
    // {
    //     // "Fucking Assignment", "Fucking Rules", "Fucking Cubes", 
    //     // "Fucking Unreal", "Fucking Unity", "Fucking Godot", 
    //     // "Fucking C++", "Fucking Bugs", 
    //     // "Fucking Physics", "Fucking arXiv", "Fucking LaTeX", "Fucking Boss", "Fucking Papers",
    //     // "Fucking Vulkan", "Fucking DirectX", "Fucking OpenGL"
    // };

    [Header("Data Source")]
    // 拖入你刚刚创建的 WordPoolAsset 资产文件 / Drag your WordPoolAsset here
    public WordPoolAsset wordPoolSource;

    // 运行时的“抽卡包” / The runtime dynamic spawning bag
    private List<string> runtimeSpawnBag = new List<string>();

    string GetNextWord()
    {
        // 安全检查：如果忘记拖入资产，返回一个保底词
        // Safety check: if asset is missing, return a fallback word
        if (wordPoolSource == null || wordPoolSource.everythingTrashPool.Count == 0)
        {
            Debug.LogError("【错误】GameManager 身上没有挂载词库资产！/ WordPoolAsset is missing!");
            return "Missing Word Asset";
        }

        // 如果包空了，从资产文件中复制一份放进去，并重新洗牌
        // If the bag is empty, refill it from the asset pool and shuffle
        if (runtimeSpawnBag.Count == 0)
        {
            runtimeSpawnBag.AddRange(wordPoolSource.everythingTrashPool);
            ShuffleBag(runtimeSpawnBag);
            Debug.Log("<color=cyan>【系统】词库抽空，已重新从资产洗牌！</color>");
        }

        int lastIndex = runtimeSpawnBag.Count - 1;
        string chosenWord = runtimeSpawnBag[lastIndex];
        runtimeSpawnBag.RemoveAt(lastIndex);

        return chosenWord;
    }

    void SpawnWord()
    {
        if (blockPrefab == null || wordPoolSource.everythingTrashPool.Count == 0) return;

        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeightY, 0);

        // 实例化自适应方块 / Instantiate the adaptive block
        GameObject newBlock = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);

        // 随机抽一个精神垃圾 / Pick a random spiritual trash
        string randomTrash = GetNextWord();

        // 调用 Setup 函数，方块会自动根据字数长短变长或变短！
        // Call Setup, the block will automatically resize based on word length!
        DynamicWordBlock script = newBlock.GetComponent<DynamicWordBlock>();
        if (script != null)
        {
            script.Setup(randomTrash);
        }
    }

    // 经典费舍尔-耶茨洗牌算法 / Classic Fisher-Yates Shuffle Algorithm
    // 这是一个原汁原味的 C# 数组/列表打乱算法，常用于游戏开发中的卡组洗牌
    void ShuffleBag(List<string> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // 随机挑选一个索引 / Pick a random index
            int k = Random.Range(0, n + 1);
        
            // 交换两个元素的位置 / Swap the two elements
            string value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}