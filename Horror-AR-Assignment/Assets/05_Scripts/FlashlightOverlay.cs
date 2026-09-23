using UnityEngine;
using UnityEngine.UI;

public class FlashlightOverlay : MonoBehaviour
{
    [Header("Visual Settings")]
    [Range(0.02f, 0.6f)] public float BeamSize = 0.15f;
    [Range(0.8f, 3f)] public float Darkness = 0.95f;

    [Header("Animation")]
    public bool FlickerEnabled = true;
    [Range(0f, 0.2f)] public float FlickerAmount = 0.05f;

    [Header("Game Integration")]
    public bool ShrinkWithProgress = true;

    private RawImage overlayImage;
    private Texture2D cachedTexture;

    private void Start()
    {
        var uiBuilder = FindObjectOfType<UIBuilder>();
        Canvas uiCanvas = uiBuilder != null ? uiBuilder.UICanvas : null;

        if (uiCanvas == null)
        {
            Debug.LogError("FlashlightOverlay: UIBuilder or its canvas not found in scene!");
            return;
        }
        
        var imgGO = new GameObject("Vignette", typeof(RectTransform));
        imgGO.transform.SetParent(uiCanvas.transform, false);
        
        imgGO.transform.SetAsFirstSibling();

        var rect = (RectTransform)imgGO.transform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        float diagonal = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        rect.sizeDelta = new Vector2(diagonal, diagonal);

        overlayImage = imgGO.AddComponent<RawImage>();
        overlayImage.raycastTarget = false;
        overlayImage.color = Color.white;

        cachedTexture = GenerateRadialGradient(512);
        overlayImage.texture = cachedTexture;
    }

    private void Update()
    {
        if (overlayImage == null || cachedTexture == null) return;

        if (FlickerEnabled)
        {
            float noise = Mathf.PerlinNoise(Time.time * 4f, 0f);
            float flickerAlpha = Mathf.Lerp(Darkness, Darkness - FlickerAmount, noise);
            overlayImage.color = new Color(0f, 0f, 0f, flickerAlpha);
        }
        
        if (ShrinkWithProgress && GameManager.Instance != null)
        {
            int keys = GameManager.Instance.KeysCollected;
            float targetSize = Mathf.Max(0.15f, 0.35f - (keys * 0.10f));
            
            BeamSize = Mathf.Lerp(BeamSize, targetSize, Time.deltaTime * 2f);
            
            if (Mathf.Abs(BeamSize - targetSize) > 0.001f)
            {
                cachedTexture = GenerateRadialGradient(512);
                overlayImage.texture = cachedTexture;
            }
        }
    }

    private Texture2D GenerateRadialGradient(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        float center = size * 0.5f;
        float beamRadius = BeamSize * center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float t = Mathf.InverseLerp(beamRadius, beamRadius * 1.5f, dist);
                float alpha = Mathf.SmoothStep(0f, Darkness, t);

                pixels[y * size + x] = new Color(0f, 0f, 0f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void OnDestroy()
    {
        if (cachedTexture != null)
            Destroy(cachedTexture);
    }
}