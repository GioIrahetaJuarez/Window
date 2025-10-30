using UnityEngine;

[ExecuteAlways]
public class WinPoint : MonoBehaviour
{
    public float radius = 0.25f;
    public int pixelsPerUnit = 128;
    public Color color = new Color(1f, 0f, 0f, 0.6f);

    SpriteRenderer sr;

    void Awake()
    {
        EnsureVisual();
    }

    void OnValidate()
    {
        EnsureVisual();
    }

    void EnsureVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        int size = Mathf.Max(8, Mathf.RoundToInt(radius * 2f * pixelsPerUnit));
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] cols = new Color32[size * size];
        float cx = (size - 1) / 2f;
        float cy = (size - 1) / 2f;
        float r2 = (size / 2f) * (size / 2f);
        Color32 col32 = color;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float d2 = dx * dx + dy * dy;
                int idx = y * size + x;
                if (d2 <= r2)
                {
                    cols[idx] = col32;
                }
                else
                {
                    cols[idx] = new Color32(0, 0, 0, 0);
                }
            }
        }

        tex.SetPixels32(cols);
        tex.Apply();

        Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sr.sprite = s;
        sr.sortingOrder = 1000;
        
        Shader shader = Shader.Find("UI/Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null) sr.material = new Material(shader);
        
        sr.transform.localScale = Vector3.one;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
