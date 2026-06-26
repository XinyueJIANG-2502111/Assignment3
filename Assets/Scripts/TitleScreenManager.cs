using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    // 注入我们的游戏核心管理器 / Reference to the GameManager
    public GameManager gameManager; 

    // 这个函数直接绑定到全屏 Button 的 OnClick() 事件中
    // This function is bound to the full-screen Button's OnClick() event
    public void StartGame()
    {
        // TODO: 播放点击音效 / Play a crisp click SFX here

        GoToScene("GameScene");
        
    }

    // 跳转到指定名称的场景 / Load scene by its name
    public void GoToScene(string sceneName)
    {
        //Debug.Log("<color=yellow>【UI点击成功】</color> 准备跳转到场景: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // 或者通过 Build Settings 里的索引号跳转（速度稍快一些）/ Load scene by build index
    public void GoToSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}