using UnityEngine;
using UnityEngine.SceneManagement; // 用于切换到正式游戏场景 / Required for scene switching

public class TitleMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    // 拖入你的设置面板 / Drag your SettingsMenuPanel here
    public GameObject settingsMenuPanel;
    // 拖入你的游戏说明面板 / Drag your GuidePanel here
    public GameObject guidePanel;

    [Header("Scene Settings")]
    // 正式游戏场景的名字 / The name of your gameplay scene
    public string gameplaySceneName = "GameplayScene";

    void Start()
    {
        // 游戏刚开始时，确保所有弹窗都是关闭的
        // Ensure all popups are closed when the title scene starts
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    // 【1. 点击全屏空白处时触发】 / Triggered when clicking the full-screen transparent panel
    public void StartGame()
    {
        // 安全锁：如果玩家正开着设置面板或说明面板，点击空白处不应该误切场景
        // Safety check: Do not start the game if any config panel is currently open
        if (settingsMenuPanel.activeSelf || guidePanel.activeSelf) return;

        Debug.Log("Fucking game started!");
        SceneManager.LoadScene(gameplaySceneName);
    }

    // 【2. 点击 Settings 按钮】 / Open Settings Menu
    public void OpenSettings()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
        }
    }

    // 【3. 点击关闭 Settings 按钮】 / Close Settings Menu
    public void CloseSettings()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
    }

    // 【4. 次级按钮：点击展示游戏说明】 / Open How To Play Guide
    public void OpenGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
    }

    // 【5. 点击关闭游戏说明】 / Close How To Play Guide
    public void CloseGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
    #endif 
        Application.Quit();
    }
}