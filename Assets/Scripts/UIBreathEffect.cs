using UnityEngine;
using TMPro;

/// <summary>
/// This script is attached to a TextMeshProUGUI component to create a breathing effect by smoothly changing the text's alpha transparency over time.
/// </summary>
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
        // Get the TextMeshPro component on this GameObject
        textComponent = GetComponent<TextMeshProUGUI>();
        
        // Check if the component is found
        if (textComponent == null)
        {
            Debug.LogError("TextMeshProUGUI not found! Please ensure this script is attached to a text object.");
            enabled = false; // Disable script if component missing
        }
    }

    void Update()
    {
        // Use Mathf.Sin to generate a continuous wave oscillating between -1 and 1
        float sinValue = Mathf.Sin(Time.time * breathSpeed);

        // Remap the sin value from [-1, 1] to [0, 1]
        float normalizedValue = (sinValue + 1.0f) / 2.0f;

        // 線形補間を行う
        // Linearly interpolate between minAlpha and maxAlpha
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, normalizedValue);

        // テキストの色を取得し、不透明度を更新する
        // Get current text color and update its Alpha channel
        Color textColor = textComponent.color;
        textColor.a = currentAlpha;
        textComponent.color = textColor;
    }
}