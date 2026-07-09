using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    public string gameplaySceneName = "GameScene";

    [Header("Intro Dissolve Settings")]
    public Material introDissolveMaterial; // 拖入 M_UI_ScreenDissolve 材质球
    public GameObject introMaskObject;    // 拖入挂载了该材质的全屏 UI Image 物体 
    public float fadeOutDuration = 0.5f;  // 入场消融持续时间
    private bool isTransitioning = false;

    void Start()
    {
        // 游戏刚开始时，确保所有弹窗都是关闭的
        // Ensure all popups are closed when the title scene starts
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (introMaskObject != null) introMaskObject.SetActive(false); // 确保遮罩物体激活，准备燃尽

        // 【开局初始化】确保遮罩处于完全“未消融（全透明/不遮挡）”状态，或者直接把进度重置为 0
        // [Init] Reset the material progress so the screen is interactive at start
        if (introDissolveMaterial != null)
        {
            introDissolveMaterial.SetFloat("_IntroProgress", 0f);
        }
    }

    // 【1. 点击全屏空白处时触发】 / Triggered when clicking the full-screen transparent panel
    public void StartGame()
    {
        // 安全锁一：如果面板开着，不触发
        if (settingsMenuPanel.activeSelf || guidePanel.activeSelf) return;

        // 安全锁二：如果已经在转场中了，直接无视（防止疯狂连击导致多次 LoadScene 崩溃）
        // Prevents multi-click scene loading crashes
        if (isTransitioning) return;
        isTransitioning = true;

        // play click sound effect
        AudioManager.Instance.PlaySFX("Click");

        // 废弃原先的直接切场景：SceneManager.LoadScene(gameplaySceneName);
        // 替换为：启动协程，先放烟花，放完再切！
        // [New Lifecycle] Play the dissolve visual sequence before loading the scene
        StartCoroutine(BurnAndLoadSceneRoutine());
    }

    IEnumerator BurnAndLoadSceneRoutine()
    {
        if (introMaskObject != null)
        {
            introMaskObject.SetActive(true); // 激活它，准备随时燃尽
        }

        if (introDissolveMaterial != null)
        {
            float elapsed = 0f;

            // 让材质的 _IntroProgress 从 0 匀速狂飙到 1 
            // Drive the Shader's progress from 0 (Normal) to 1 (Fully Dissolved/Burned)
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeOutDuration);

                // 推进消融，黑色逐渐镂空吞噬整个主界面
                introDissolveMaterial.SetFloat("_IntroProgress", progress);
                yield return null;
            }
        }
        else
        {
            // 防御性保底：万一忘了挂材质，也至少等一两帧
            yield return new WaitForSeconds(fadeOutDuration);
        }

        // 此时画面已经全屏燃尽，变成了满屏高亮霓虹洞洞然后归于虚无，正式切关！
        // Visual sequence concluded. Fire scene loader now!
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