using UnityEngine;

public class BackgroundLayer : MonoBehaviour
{
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color color1 = new Color(0.3f, 0.6f, 0.9f); // light blue
    public Color color2 = new Color(0.2f, 0.8f, 0.3f); // light green
    public int checkerSize = 64;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Create a checkered pattern texture as background
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        Color32[] pixels = new Color32[textureWidth * textureHeight];
        
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                bool checker = ((x / checkerSize) + (y / checkerSize)) % 2 == 0;
                pixels[y * textureWidth + x] = checker ? (Color32)color1 : (Color32)color2;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        // Create sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), 64f);
        sr.sprite = sprite;
        sr.sortingOrder = -100; // Behind dust layer
        
        // Use unlit shader
        Shader shader = Shader.Find("UI/Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            sr.material = new Material(shader);
        }

        Debug.Log($"BackgroundLayer: Created {textureWidth}x{textureHeight} background at sortingOrder={sr.sortingOrder}");
    }
}
