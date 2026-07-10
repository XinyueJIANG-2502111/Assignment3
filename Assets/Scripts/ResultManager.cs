using UnityEngine;
using TMPro;
public class ResultScreenManager : MonoBehaviour
{
    [Header("UI Text References")]
    public GameObject winTextObject;  
    public GameObject loseTextObject; 

    [Header("Button Text References")]
    // Text component of the restart button
    public TextMeshProUGUI restartButtonText; 
    // Text component of the quit button
    public TextMeshProUGUI quitButtonText;    

    void Start()
    {
        if (GameManager.isVictory)
        {
            // クリアの場合のテキストを表示 / Show Victory text
            if (winTextObject != null) winTextObject.SetActive(true);
            if (loseTextObject != null) loseTextObject.SetActive(false);

            // オプションボタンのテキストをクリア状況に応じて変更
            // Dynamically change button texts for Victory state
            if (restartButtonText != null) restartButtonText.text = "Go to the next fucking challenge.";
            if (quitButtonText != null) quitButtonText.text = "Fuck you, I'm out.";
        }
        else
        {
            // クリアできなかった場合の表示
            if (winTextObject != null) winTextObject.SetActive(false);
            if (loseTextObject != null) loseTextObject.SetActive(true);

            if (restartButtonText != null) restartButtonText.text = "Suck it up and try again.";
            if (quitButtonText != null) quitButtonText.text = "Fuck you, I quit.";
        }
    }

    public void RestartGame() 
    { 
        // クリック音を再生 / Play click sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Click"); 
        }

        // ゲームシーンにもどる / Load the gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    public void QuitGame() { 
    #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; 
    #endif 
        Application.Quit(); 
    }
}