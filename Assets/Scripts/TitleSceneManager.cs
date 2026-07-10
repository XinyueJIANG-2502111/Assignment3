using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsMenuPanel;
    public GameObject guidePanel;


    [Header("Scene Settings")]
    public string gameplaySceneName = "GameScene";


    [Header("Intro Dissolve Settings")]
    public Material introDissolveMaterial;
    public GameObject introMaskObject;
    public float fadeOutDuration = 0.5f;
    private bool isTransitioning = false;

    void Start()
    {
        // すべてのポップアップを閉じる
        // Ensure all popups are closed when the title scene starts
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (introMaskObject != null) introMaskObject.SetActive(false);

        // エフェクトを初期化
        // Initialize the dissolve material progress
        if (introDissolveMaterial != null)
        {
            introDissolveMaterial.SetFloat("_IntroProgress", 0f);
        }
    }

    // パネルをクリックしたときにゲームを開始する
    // Triggered when clicking the full-screen transparent panel
    public void StartGame()
    {
        // 任意のポップアップが開いている場合は、入力を無視する
        // Ignore input if any popup is currently open
        if (settingsMenuPanel.activeSelf || guidePanel.activeSelf) return;

        // 重複入力を防ぐために、すでにシーン遷移中の場合は入力を無視する
        // Prevents multi-click scene loading crashes
        if (isTransitioning) return;
        isTransitioning = true;

        // クリック音を再生する
        // play click sound effect
        AudioManager.Instance.PlaySFX("Click");

        // シーン遷移のエフェクトを再生してからゲームプレイシーンをロードする
        // Play the dissolve visual sequence before loading the scene
        StartCoroutine(BurnAndLoadSceneRoutine());
    }

    IEnumerator BurnAndLoadSceneRoutine()
    {
        if (introMaskObject != null)
        {
            introMaskObject.SetActive(true);
        }

        if (introDissolveMaterial != null)
        {
            float elapsed = 0f;

            // Drive the Shader's progress from 0 (Normal) to 1 (Fully Dissolved/Burned)
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeOutDuration);

                introDissolveMaterial.SetFloat("_IntroProgress", progress);
                yield return null;
            }
        }
        else
        {
            // マテリアルが見つからなかった場合でも一定期間待つ
            // Wait for the duration even if the material is missing, to keep timing consistent
            yield return new WaitForSeconds(fadeOutDuration);
        }

        // エフェクト再生完了、ゲームプレイシーンをロードする
        // Scene transition complete. Load the gameplay scene.
        SceneManager.LoadScene(gameplaySceneName);
    }


    /// <summary>
    /// Buttons : Open/Close Settings, Open/Close Guide, Quit Game
    /// Don't forget to assign these methods to the corresponding buttons in the Inspector!
    /// </summary>
    // Button: Open Settings Menu
    public void OpenSettings()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
        }
    }

    // Button: Close Settings Menu
    public void CloseSettings()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
    }

    // Button: Open How To Play Guide
    public void OpenGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
    }

    // Button: Close How To Play Guide
    public void CloseGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    // Button: Quit Game
    public void QuitGame()
    {
    #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
    #endif 
        Application.Quit();
    }
}