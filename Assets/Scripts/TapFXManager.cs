using UnityEngine;

public class GlobalTouchFXManager : MonoBehaviour
{
    public static GlobalTouchFXManager Instance { get; private set; }

    [Header("Juicy Tap FX Prefab (MUST BE UI IMAGE NOW)")]
    public GameObject tapEffectPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnTapEffect(Input.mousePosition);
        }
    }

    void SpawnTapEffect(Vector3 screenPosition)
    {
        if (tapEffectPrefab == null) return;

        // 【核心改变】动态寻找当前场景里最顶层的 Canvas 
        // [Core Change] Dyamically find the active Canvas in the current scene
        Canvas currentCanvas = FindFirstObjectByType<Canvas>();
        if (currentCanvas == null) return;

        // 直接在 Canvas 肚子里生成这个 UI 特效
        // Instantiate the UI effect as a child of the Canvas
        GameObject fxInstance = Instantiate(tapEffectPrefab, currentCanvas.transform);

        // 将鼠标的屏幕坐标，平移给 UI 元素的本地坐标
        // Assign screen position directly to the UI elements
        fxInstance.transform.position = screenPosition;

        // 【极重要】确保新生成的特效不会拦截它底下 UI 按钮的后续点击
        // [Critical] Make sure the ripple doesn't block underlying button clicks
        var img = fxInstance.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.raycastTarget = false; 
        }
    }
}