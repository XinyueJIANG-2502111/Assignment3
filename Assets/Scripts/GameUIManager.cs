using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("TextMeshPro UI References")]
    public TextMeshProUGUI timerText;       
    public TextMeshProUGUI scoreText;       
    public TextMeshProUGUI trashCountText;  


    [Header("Circular Timer UI")]      
    public Image timerCircleImage;


    [Header("Pause Menu Windows")]
    public GameObject pauseMenuPanel;

    // Cache total duration for percentage calculation
    private float totalGameDuration;


    // Initialize UI states
    public void InitUI(float totalTime, int maxTrash)
    {
        UpdateTimer(totalTime);
        UpdateScore(0);
        UpdateTrashCount(0, maxTrash);

        // Ensure pause menu is hidden and time is running normally at start
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        totalGameDuration = totalTime; // Record total time
        timerCircleImage.color = Color.green;
    }

    // タイマー表示を更新 / Update the timer display
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText == null) return;

        // CeilToInt makes the countdown smoother for players (e.g., 29.1s shows as 30s)
        int seconds = Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        timerText.text = $"{seconds}";

        // Update the circle fill amount
        if (timerCircleImage != null && totalGameDuration > 0)
        {
            // Calculate the remaining time ratio (clamped between 0.0 and 1.0)
            float fillRatio = timeRemaining / totalGameDuration;
            
            timerCircleImage.fillAmount = fillRatio;

            // Turn the circle red when time is running out (e.g., last 5s)
            if (timeRemaining <= 5f)
            {
                timerCircleImage.color = Color.red;  // Warning color
            }
            else
            {
                timerCircleImage.color = Color.green; // Normal color
            }
        }
    }

    // Update the score display
    public void UpdateScore(int currentScore)
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {currentScore}";
    }

    // Update current trash overload count
    public void UpdateTrashCount(int currentCount, int maxAllowed)
    {
        if (trashCountText == null) return;
        
        trashCountText.text = $"Blocks: {currentCount}/{maxAllowed}";

        // 限界に近づくとテキストの色が赤色に変える
        // Change text color to flashing red if close to bursting limit
        if (currentCount >= maxAllowed - 3)
        {
            trashCountText.color = Color.red;
        }
        else
        {
            trashCountText.color = Color.white;
        }
    }

    // 一時停止ボタンがクリックされたとき / Triggered when clicking the pause button
    public void TogglePause()
    {
        if (pauseMenuPanel == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Click"); 
        }

        // Show the pause menu panel
        pauseMenuPanel.SetActive(true);

        // Freeze the timeline! Rigidbody2D, Coroutines, and Time.deltaTime will stop
        Time.timeScale = 0f; 
    }

    // 選択肢１：ゲームに戻る
    // Option 1: Resume Game
    public void ResumeGame()
    {
        if (pauseMenuPanel == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Cancel"); 
        }

        // Hide the pause menu panel
        pauseMenuPanel.SetActive(false);

        // Restore time scale to resume gameplay
        Time.timeScale = 1f; 
    }

    // 選択肢２：最初からやり直す 
    // Option 2: Restart Game
    public void RestartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Click"); 
        }
        // 注意：必ずタイムスケールを１に戻す
        // CRITICAL: Must reset timeScale to 1 before reloading, or the new scene will remain frozen
        Time.timeScale = 1f; 

        // ゲームシーンをもう一度ロードする / Reload current active gameplay scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // 選択肢３：タイトル画面に戻る
    // Option 3: Return to Main Title
    public void GoToTitle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Click"); 
        }
        // タイムスケールを１に戻す / Restore time scale here as well
        Time.timeScale = 1f; 

        // タイトル画面をロードする / Load the main title scene
        SceneManager.LoadScene("TitleScene");
    }
}