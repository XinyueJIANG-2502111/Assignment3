using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreenManager : MonoBehaviour
{
    [Header("UI Text References")]
    public GameObject winTextObject;  
    public GameObject loseTextObject; 

    void Start()
    {
        // 保持之前的胜负判定显示逻辑
        if (GameManager.isVictory)
        {
            if (winTextObject != null) winTextObject.SetActive(true);
            if (loseTextObject != null) loseTextObject.SetActive(false);
        }
        else
        {
            if (winTextObject != null) winTextObject.SetActive(false);
            if (loseTextObject != null) loseTextObject.SetActive(true);
        }
    }

    // 选项一：重来一局（绑定到重来按钮）
    // Option 1: Restart the game (Bound to Restart Button)
    public void RestartGame()
    {
        Debug.Log("重来一局，再次复仇！");
        SceneManager.LoadScene("GameplayScene");
    }

    // 选项二：掀桌退出（绑定到 Fuck you, I quit 按钮）
    // Option 2: Quit the game (Bound to Quit Button)
    public void QuitGame()
    {
        Debug.Log("Fuck you, I quit! 正在退出游戏...");

        // 1. 如果是在 Unity 编辑器里运行，点击时停止播放 / If running in Unity Editor, stop playing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // 2. 如果是在手机端或打包后的包体运行，直接关闭程序 / If running on mobile or build, close the app
        Application.Quit();
    }
}