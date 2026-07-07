using UnityEngine;
using TMPro; // 必须引入 TMP 命名空间 / Required for TextMeshProUGUI
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("TextMeshPro UI References")]
    // 拖入你场景中显示倒计时的文本 / Drag your countdown text here
    public TextMeshProUGUI timerText;       
    // 拖入你场景中显示得分的文本 / Drag your score text here
    public TextMeshProUGUI scoreText;       
    // 拖入你场景中显示当前方块堆积数量的文本 / Drag your block count text here
    public TextMeshProUGUI trashCountText;  

    [Header("Circular Timer UI")]      
    // 拖入你刚刚制作的圆环 Image 物体 / Drag your circular Image component here
    public Image timerCircleImage;

    [Header("Pause Menu Windows")]
    // 拖入你刚刚制作的整个 PauseMenuPanel 物体 / Drag the entire PauseMenuPanel here
    public GameObject pauseMenuPanel;

    // 缓存总游戏时间用于计算百分比 / Cache total duration for percentage calculation
    private float totalGameDuration;

    // 初始化 UI 状态
    // Initialize UI states
    public void InitUI(float totalTime, int maxTrash)
    {
        UpdateTimer(totalTime);
        UpdateScore(0);
        UpdateTrashCount(0, maxTrash);

        // 确保游戏开始时，暂停面板是隐藏的，且时间流速正常
        // Ensure pause menu is hidden and time is running normally at start
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        totalGameDuration = totalTime; // 记录总时间 / Record total time
        timerCircleImage.color = Color.green;
    }

    // 更新倒计时显示 / Update the timer display
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) return;

        // Mathf.CeilToInt 可以让时间向上取整（比如 29.1 秒显示为 30 秒，视觉更舒服）
        // CeilToInt makes the countdown smoother for players (e.g., 29.1s shows as 30s)
        int seconds = Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        timerText.text = $"{seconds}";

        // 2. 更新环形进度条的填充比例 / Update the circle fill amount
        if (timerCircleImage != null && totalGameDuration > 0)
        {
            // 计算剩余时间的百分比比例 (值在 0.0 到 1.0 之间)
            // Calculate the remaining time ratio (clamped between 0.0 and 1.0)
            float fillRatio = timeRemaining / totalGameDuration;
            
            // 赋值给 Image，物理平滑刷新
            timerCircleImage.fillAmount = fillRatio;

            // 【加码高血压特效】时间快到时（比如剩最后 5 秒），让圆环变成警告红！
            // [Juicy FX] Turn the circle red when time is running out (e.g., last 5s)
            if (timeRemaining <= 5f)
            {
                timerCircleImage.color = Color.red;
            }
            else
            {
                timerCircleImage.color = Color.green; // 正常时间颜色 / Normal color
            }
        }
    }

    // 更新得分显示 / Update the score display
    public void UpdateScore(int currentScore)
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {currentScore}";
    }

    // 更新场上垃圾数量显示 / Update current trash overload count
    public void UpdateTrashCount(int currentCount, int maxAllowed)
    {
        if (trashCountText == null) return;
        
        trashCountText.text = $"Blocks: {currentCount}/{maxAllowed}";

        // 【高血压视觉特效】如果场上垃圾快爆满了，让文字变成惊悚的红色！
        // [Visul Tint] Change text color to flashing red if close to bursting limit
        if (currentCount >= maxAllowed - 3)
        {
            trashCountText.color = Color.red;
        }
        else
        {
            trashCountText.color = Color.white;
        }
    }

    // 【1. 点击右上角暂停键时触发】 / Triggered when clicking the pause button
    public void TogglePause()
    {
        if (pauseMenuPanel == null) return;

        // 打开暂停面板 / Show the pause menu panel
        pauseMenuPanel.SetActive(true);

        // 斩断时间线！物理、生成协程、Update里的Time.deltaTime全部静止
        // Freeze the timeline! Rigidbody2D, Coroutines, and Time.deltaTime will stop
        Time.timeScale = 0f; 
    }

    // 【2. 选项一：继续游戏】 / Option 1: Resume Game
    public void ResumeGame()
    {
        if (pauseMenuPanel == null) return;

        // 隐藏暂停面板 / Hide the pause menu panel
        pauseMenuPanel.SetActive(false);

        // 恢复时间流速，游戏继续 / Restore time scale to resume gameplay
        Time.timeScale = 1f; 
    }

    // 【3. 选项二：重新开始】 / Option 2: Restart Game
    public void RestartGame()
    {
        // 极重要：重新加载场景前，必须手动把时间流速调回 1，否则新场景也会是静止的！
        // CRITICAL: Must reset timeScale to 1 before reloading, or the new scene will remain frozen
        Time.timeScale = 1f; 

        // 重新加载当前正在玩的场景 / Reload current active gameplay scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // 【4. 选项三：回到标题画面】 / Option 3: Return to Main Title
    public void GoToTitle()
    {
        // 同样需要恢复时间流速 / Restore time scale here as well
        Time.timeScale = 1f; 

        // 假设你的主界面场景名字叫 "TitleScene" (根据你实际的名字修改)
        // Load your main menu scene (change "TitleScene" to your actual scene name)
        SceneManager.LoadScene("TitleScene");
    }
}