using UnityEngine;
using TMPro;

public class UIBreathEffect : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    [Header("Breath Settings")]
    [Tooltip("Breathing speed, higher means faster")]
    public float breathSpeed = 3.5f;

    [Tooltip("Minimum alpha value (0 to 1)")]
    public float minAlpha = 0.0f;

    [Tooltip("Maximum alpha value (0 to 1)")]
    public float maxAlpha = 1.0f;

    void Start()
    {
        // 获取当前物体上的 TextMeshPro 组件
        // Get the TextMeshPro component on this GameObject
        textComponent = GetComponent<TextMeshProUGUI>();
        
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI not found! Please ensure this script is attached to a text object.");
            enabled = false; // 如果没找到则禁用此脚本 / Disable script if component missing
        }
    }

    void Update()
    {
        // 使用数学公式 Mathf.Sin 产生一个在 -1 到 1 之间连续循环的波形
        // Use Mathf.Sin to generate a continuous wave oscillating between -1 and 1
        float sinValue = Mathf.Sin(Time.time * breathSpeed);

        // 将 -1 到 1 的波形映射到 0 到 1 之间
        // Remap the sin value from [-1, 1] to [0, 1]
        float normalizedValue = (sinValue + 1.0f) / 2.0f;

        // 根据你设置的最小和最大透明度进行线性插值
        // Linearly interpolate between minAlpha and maxAlpha
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, normalizedValue);

        // 获取文本当前的颜色，并修改它的 Alpha 通道
        // Get current text color and update its Alpha channel
        Color textColor = textComponent.color;
        textColor.a = currentAlpha;
        textComponent.color = textColor;
    }
}