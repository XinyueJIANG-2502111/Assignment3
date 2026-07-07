using UnityEngine;
using TMPro; // 引入 TMP 命名空间 / Required for modifying button text

public class ResultScreenManager : MonoBehaviour
{
    [Header("UI Text References")]
    public GameObject winTextObject;  
    public GameObject loseTextObject; 

    [Header("Button Text References")]
    // 拖入你左边“重来”按钮下的 Text 组建 / Text component of the restart button
    public TextMeshProUGUI restartButtonText; 
    // 拖入你右边“退出”按钮下的 Text 组件 / Text component of the quit button
    public TextMeshProUGUI quitButtonText;    

    void Start()
    {
        if (GameManager.isVictory)
        {
            // 1. 显示胜利文字 / Show Victory text
            if (winTextObject != null) winTextObject.SetActive(true);
            if (loseTextObject != null) loseTextObject.SetActive(false);

            // 2. 动态修改通关时的按钮文案（这里以方案 A 为例）
            // Dynamically change button texts for Victory state
            if (restartButtonText != null) restartButtonText.text = "Go to the next fucking challenge.";
            if (quitButtonText != null) quitButtonText.text = "Fuck you, I'm out.";
        }
        else
        {
            // 1. 显示失败文字 / Show Loss text
            if (winTextObject != null) winTextObject.SetActive(false);
            if (loseTextObject != null) loseTextObject.SetActive(true);

            // 2. 恢复失败时的经典文案
            // Restore classic button texts for Loss state
            if (restartButtonText != null) restartButtonText.text = "Suck it up and try again.";
            if (quitButtonText != null) quitButtonText.text = "Fuck you, I quit.";
        }
    }

    public void RestartGame() 
    { 
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        //ScreenTransitionManager.Instance.PlayDissolveTransition("GameScene"); 
    }
    public void QuitGame() { 
    #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
    #endif 
        Application.Quit(); 
    }
}