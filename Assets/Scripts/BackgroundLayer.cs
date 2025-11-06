using UnityEngine;
using System.IO;

public class BackgroundLayer : MonoBehaviour
{
    public string imagePath = "background.jpg";

    void Start()
    {
        StartCoroutine(LoadBackground());
    }

    System.Collections.IEnumerator LoadBackground()
    {
        yield return null;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        string fullPath = Path.Combine(Application.dataPath, imagePath);
        
        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            
            if (tex.LoadImage(fileData))
            {
                Texture2D resizedTex = ResizeTexture(tex, 512, 512);
                Destroy(tex);
                
                resizedTex.wrapMode = TextureWrapMode.Clamp;
                resizedTex.filterMode = FilterMode.Bilinear;

                Sprite sprite = Sprite.Create(resizedTex, 
                    new Rect(0, 0, 512, 512), 
                    new Vector2(0.5f, 0.5f), 
                    64f);
                
                sr.sprite = sprite;
                sr.sortingOrder = -100;
                
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
        }
    }

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
}
