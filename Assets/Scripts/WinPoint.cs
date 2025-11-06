using UnityEngine;

public class WinPoint : MonoBehaviour
{
    public float radius = 0.25f;
    public int pixelsPerUnit = 128;
    public Color color = new Color(1f, 0f, 0f, 0.6f);
    
    int randomSeed;
    SpriteRenderer sr;

    void Awake()
    {
        randomSeed = Random.Range(0, 1000000);
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
        
        for (int i = 0; i < cols.Length; i++)
        {
            float dx = (i % size) - cx;
            float dy = (i / size) - cy;
            float distFromCenter = Mathf.Sqrt(dx * dx + dy * dy);
            float radiusInPixels = (size / 2f) * 0.9f;
            
            if (distFromCenter <= radiusInPixels)
            {
                float alpha = Mathf.Lerp(80, 30, distFromCenter / radiusInPixels);
                cols[i] = new Color32(255, 50, 50, (byte)alpha);
            }
            else
            {
                cols[i] = new Color32(0, 0, 0, 0);
            }
        }

        Color32 crackColor = new Color32(40, 40, 40, 180);
        Color32 crackBright = new Color32(200, 200, 220, 120);
        
        Random.InitState(randomSeed);
        int numCracks = Random.Range(5, 9);
        for (int i = 0; i < numCracks; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float crackLength = size * Random.Range(0.35f, 0.5f);
            float wobbleFreq = Random.Range(8f, 15f);
            float wobbleAmp = Random.Range(1.5f, 3.5f);
            for (float t = 0; t < 1f; t += 0.015f)
            {
                float dist = crackLength * (t + Random.Range(-0.05f, 0.05f));
                float wobble = Mathf.Sin(t * wobbleFreq) * wobbleAmp * t;
                float randomOffset = Random.Range(-0.5f, 0.5f);
                int px = Mathf.RoundToInt(cx + Mathf.Cos(angle) * dist + Mathf.Sin(angle) * wobble + randomOffset);
                int py = Mathf.RoundToInt(cy + Mathf.Sin(angle) * dist - Mathf.Cos(angle) * wobble + randomOffset);
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    int idx = py * size + px;
                    cols[idx] = crackColor;
                    int thickness = Random.Range(1, 3);
                    for (int dx = -thickness; dx <= thickness; dx++)
                    {
                        for (int dy = -thickness; dy <= thickness; dy++)
                        {
                            int nx = px + dx;
                            int ny = py + dy;
                            if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                            {
                                int nidx = ny * size + nx;
                                if (cols[nidx].a < 100 && Random.value > 0.3f)
                                {
                                    cols[nidx] = crackBright;
                                }
                            }
                        }
                    }
                }
            }
            
            //some branching cracks
            if (Random.value > 0.5f)
            {
                float branchPoint = Random.Range(0.3f, 0.7f);
                float branchAngle = angle + Random.Range(-0.8f, 0.8f);
                float branchLength = crackLength * Random.Range(0.2f, 0.4f);
                
                for (float t = 0; t < 1f; t += 0.02f)
                {
                    float dist = branchLength * t;
                    float startDist = crackLength * branchPoint;
                    int px = Mathf.RoundToInt(cx + Mathf.Cos(angle) * startDist + Mathf.Cos(branchAngle) * dist);
                    int py = Mathf.RoundToInt(cy + Mathf.Sin(angle) * startDist + Mathf.Sin(branchAngle) * dist);
                    
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        cols[py * size + px] = crackColor;
                    }
                }
            }
        }
        
        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                if (x * x + y * y <= 9)
                {
                    int px = Mathf.RoundToInt(cx) + x;
                    int py = Mathf.RoundToInt(cy) + y;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        cols[py * size + px] = new Color32(20, 20, 20, 200);
                    }
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
