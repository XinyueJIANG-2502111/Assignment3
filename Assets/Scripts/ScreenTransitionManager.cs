using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenTransitionManager : MonoBehaviour
{
    // 经典的单例模式 / Static singleton instance
    public static ScreenTransitionManager Instance { get; private set; }

    [Header("Transition Settings")]
    [Tooltip("转场使用的消融材质球 / The M_UI_ScreenDissolve material")]
    public Material dissolveMaterial;
    
    [Tooltip("消融吞噬持续时间 / Duration of the dissolve effect")]
    public float transitionDuration = 0.5f;

    private CanvasGroup transitionCanvasGroup;
    private bool isTransitioning = false;

    void Awake()
    {
        // 确保全局只有一个转场管理器
        // Singleton enforcement pipeline
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 跨场景不毁灭 / Persistent across scenes

        // 运行时动态构建 UI 围墙，防止不同场景手动摆放导致的图层穿帮
        // Dynamically initialize the transition canvas layer
        InitTransitionUI();
    }

    // 动态生成绝对处于最顶层的 Canvas 遮罩
    // Setup a bulletproof topmost canvas at runtime
    private void InitTransitionUI()
    {
        // 1. 创建根 Canvas
        GameObject canvasObj = new GameObject("Transition_Canvas");
        canvasObj.transform.SetParent(this.transform);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 强行置顶，压制一切游戏业务 UI / Ultimate priority

        canvasObj.AddComponent<CanvasScaler>();
        transitionCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
        transitionCanvasGroup.blocksRaycasts = false; // 平时允许穿透点击

        // 2. 创建全屏消融 Image
        GameObject maskObj = new GameObject("DissolveMask_Image");
        maskObj.transform.SetParent(canvasObj.transform);
        
        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.black; // 纯黑底

        // 锚点强行拉满全屏
        RectTransform rect = maskImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        // 3. 挂载材质
        if (dissolveMaterial != null)
        {
            maskImage.material = dissolveMaterial;
            dissolveMaterial.SetFloat("_IntroProgress", 0f); // 初始全透明
        }
        else
        {
            Debug.LogError("<color=red>【TransitionManager】</color> 警告：未分配消融材质球！");
        }
    }

    /// <summary>
    /// 外部调用的核心核心公共接口 / The unified interface to trigger the switch
    /// </summary>
    /// <param name="targetSceneName">要跳转的目标场景名</param>
    public void PlayDissolveTransition(string targetSceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(targetSceneName));
    }

    IEnumerator TransitionRoutine(string targetSceneName)
    {
        isTransitioning = true;
        transitionCanvasGroup.blocksRaycasts = true; // 拦截全屏点击，防止玩家在转场时瞎点

        // 【阶段一：黑洞滋生，吞噬当前场景】
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDuration);
            if (dissolveMaterial != null) dissolveMaterial.SetFloat("_IntroProgress", progress);
            yield return null;
        }

        // 【阶段二：暗中切关】
        // 此时屏幕全黑，安全载入新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 【阶段三：在新场景中反向燃尽，显露战场】
        elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            // 反向插值：从 1 缩回 0
            float progress = Mathf.Clamp01(1.0f - (elapsed / transitionDuration));
            if (dissolveMaterial != null) dissolveMaterial.SetFloat("_IntroProgress", progress);
            yield return null;
        }

        // 重置状态
        if (dissolveMaterial != null) dissolveMaterial.SetFloat("_IntroProgress", 0f);
        transitionCanvasGroup.blocksRaycasts = false; // 释放点击
        isTransitioning = false;
    }
}