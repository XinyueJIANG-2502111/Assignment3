using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is attached to the UI effect prefab. It automatically expands and fades out the effect, then destroys itself to save memory.
/// </summary>
public class AutoDestroyEffect : MonoBehaviour
{
    private Image uiImage;
    
    public float expandSpeed = 5f;  // Expansion speed
    public float fadeSpeed = 4f;    // Fade speed
    private float alpha = 1f;

    void Awake()
    {
        // Get UI Image component
        uiImage = GetComponent<Image>();
        
        // Assign a random color
        Color randomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.9f, 1f);
        
        if (uiImage != null)
        {
            uiImage.color = randomColor;
        }
    }

    void Update()
    {
        // Smoothly scale up the effect
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        // Fade out the opacity
        alpha -= fadeSpeed * Time.deltaTime;
        
        // Fade out the UI Image color
        if (uiImage != null)
        {
            Color c = uiImage.color;
            uiImage.color = new Color(c.r, c.g, c.b, alpha);
        }

        // Destroy when invisible to save memory
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}