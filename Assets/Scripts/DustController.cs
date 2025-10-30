using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DustController : MonoBehaviour
{
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color32 dustColor = new Color32(180, 180, 180, 255);
    public int pixelsPerUnit = 64;

    Texture2D dustTexture;
    SpriteRenderer sr;
    Sprite dustSprite;
    int totalPixels;
    int clearedPixels;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("DustController: SpriteRenderer component not found!");
            return;
        }
        
        // Force completely unlit material - try multiple shader options
        Shader shader = Shader.Find("UI/Default"); // UI shader is always unlit
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            sr.material = mat;
            Debug.Log($"DustController: Changed material to {shader.name}");
        }
        else
        {
            Debug.LogWarning("DustController: Could not find suitable unlit shader!");
        }
        
        // Explicitly set render properties
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10; // Above background, below UI
        
        InitializeTexture();
    }

    void Start()
    {
        // Fallback: if sprite is still null after Awake, reinitialize
        if (sr != null && sr.sprite == null)
        {
            Debug.LogWarning("DustController: Sprite was null in Start, reinitializing...");
            InitializeTexture();
        }
    }

    void InitializeTexture()
    {
        dustTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        dustTexture.wrapMode = TextureWrapMode.Clamp;
        dustTexture.filterMode = FilterMode.Bilinear;

        Color32[] fill = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < fill.Length; i++) fill[i] = dustColor;
        dustTexture.SetPixels32(fill);
        dustTexture.Apply();

        totalPixels = textureWidth * textureHeight;
        clearedPixels = 0;

        dustSprite = Sprite.Create(dustTexture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sr.sprite = dustSprite;
    }
    public void EraseAt(Vector2 worldPos, float radiusWorldUnits)
    {
        if (dustTexture == null) return;

        // Convert world position to local space of the sprite
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector2 spriteSize = sr.sprite.bounds.size;
        Vector2 normalized = new Vector2(localPos.x / spriteSize.x + 0.5f, localPos.y / spriteSize.y + 0.5f);

        if (normalized.x < 0f || normalized.x > 1f || normalized.y < 0f || normalized.y > 1f) return;

        int centerX = Mathf.RoundToInt(normalized.x * (textureWidth - 1));
        int centerY = Mathf.RoundToInt(normalized.y * (textureHeight - 1));
        int radiusPx = Mathf.RoundToInt(radiusWorldUnits * pixelsPerUnit);
        if (radiusPx <= 0) radiusPx = 1;

        int x0 = Mathf.Clamp(centerX - radiusPx, 0, textureWidth - 1);
        int x1 = Mathf.Clamp(centerX + radiusPx, 0, textureWidth - 1);
        int y0 = Mathf.Clamp(centerY - radiusPx, 0, textureHeight - 1);
        int y1 = Mathf.Clamp(centerY + radiusPx, 0, textureHeight - 1);

        Color32[] tex = dustTexture.GetPixels32();
        int clearedThisCall = 0;

        for (int y = y0; y <= y1; y++)
        {
            int dy = y - centerY;
            int dy2 = dy * dy;
            for (int x = x0; x <= x1; x++)
            {
                int dx = x - centerX;
                if (dx * dx + dy2 <= radiusPx * radiusPx)
                {
                    int idx = y * textureWidth + x;
                    if (tex[idx].a != 0)
                    {
                        tex[idx].a = 0;
                        tex[idx].r = 0;
                        tex[idx].g = 0;
                        tex[idx].b = 0;
                        clearedThisCall++;
                    }
                }
            }
        }

        if (clearedThisCall > 0)
        {
            clearedPixels += clearedThisCall;
            dustTexture.SetPixels32(tex);
            dustTexture.Apply();
        }
    }

    public float GetClearedPercent()
    {
        if (totalPixels == 0) return 0f;
        return (float)clearedPixels / (float)totalPixels;
    }
}
