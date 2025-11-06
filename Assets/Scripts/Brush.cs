using UnityEngine;

public class Brush : MonoBehaviour
{
    public DustController dustController;
    public float brushRadius = 0.25f;
    public float minBrushRadius = 0.1f;
    public float maxBrushRadius = 0.8f;
    public float scrollSensitivity = 0.1f;
    public float maxAllowedSpeed = 5f;
    public float speedSmoothing = 8f;
    public float speedForgivenessTime = 0.12f;
    public Color brushCursorColor = new Color(1f, 1f, 1f, 0.5f);
    public Color brushCursorOutlineColor = new Color(0f, 0f, 0f, 0.8f);
    public int brushCursorSegments = 32;

    Vector2 lastPos;
    Vector2 currentMouseWorldPos;
    bool dragging = false;
    float lastTime;
    float smoothSpeed = 0f;
    float aboveTime = 0f;

    GameManager gm;
    WinPoint[] failPoints;
    Camera mainCamera;
    ScrubSoundManager soundManager;

    void Start()
    {
        gm = GameManager.Instance;
        failPoints = Object.FindObjectsByType<WinPoint>(FindObjectsSortMode.None);
        mainCamera = Camera.main;
        soundManager = Object.FindFirstObjectByType<ScrubSoundManager>();
        Cursor.visible = true;
    }

    void Update()
    {
        // Update mouse position for cursor visualization
        UpdateMousePosition();
        
        // Handle brush size adjustment with scroll wheel
        HandleBrushSizeAdjustment();
        if (Input.GetMouseButtonDown(0))
        {
            if (mainCamera == null) return;
            
            var mp = Input.mousePosition;
            if (mainCamera.pixelRect.width == 0 || mainCamera.pixelRect.height == 0) return;
            if (!mainCamera.pixelRect.Contains(mp)) return;

            dragging = true;
            lastTime = Time.time;
            lastPos = currentMouseWorldPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (soundManager != null)
            {
                soundManager.StopScrubSound();
            }
        }

        if (dragging && !gm.IsGameOver)
        {
            if (mainCamera == null) return;
            var mp = Input.mousePosition;
            if (mainCamera.pixelRect.width == 0 || mainCamera.pixelRect.height == 0) return;
            if (!mainCamera.pixelRect.Contains(mp)) return;
            Vector2 curPos = currentMouseWorldPos;
            float curTime = Time.time;
            float dt = curTime - lastTime;
            if (dt <= 0f) dt = Time.deltaTime;
            float instSpeed = Vector2.Distance(curPos, lastPos) / Mathf.Max(0.0001f, dt);
            float t = 1f - Mathf.Exp(-speedSmoothing * dt);
            smoothSpeed = Mathf.Lerp(smoothSpeed, instSpeed, t);
            if (smoothSpeed > maxAllowedSpeed)
            {
                aboveTime += dt;
                if (aboveTime >= speedForgivenessTime)
                {
                    gm.Lose("TooFast, the glass broke! :(");
                    return;
                }
            }
            else
            {
                aboveTime = Mathf.Max(0f, aboveTime - dt);
            }
            float dist = Vector2.Distance(curPos, lastPos);
            int steps = Mathf.CeilToInt(dist / (brushRadius * 0.25f));
            steps = Mathf.Max(1, steps);
            if (soundManager != null && steps > 0)
            {
                soundManager.PlayScrubSound();
            }
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(lastPos, curPos, (float)i / (float)steps);
                dustController.EraseAt(p, brushRadius);
                foreach (var fp in failPoints)
                {
                    if (fp == null) continue;
                    float d = Vector2.Distance(p, fp.transform.position);
                    if (d <= fp.radius + brushRadius)
                    {
                        gm.Lose("try to avoid the cracked glass! :(");
                        return;
                    }
                }
            }

            lastPos = curPos;
            lastTime = curTime;
            float progress = dustController.GetClearedPercent();
            gm.UpdateProgress(progress);
            if (progress >= gm.winClearPercent)
            {
                gm.Win();
            }
        }
    }

    void UpdateMousePosition()
    {
        if (mainCamera == null) return;
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -mainCamera.transform.position.z));
        currentMouseWorldPos = new Vector2(worldPos.x, worldPos.y);
    }

    void HandleBrushSizeAdjustment()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            brushRadius += scrollDelta * scrollSensitivity;
            brushRadius = Mathf.Clamp(brushRadius, minBrushRadius, maxBrushRadius);
        }
    }

    void OnDrawGizmos()
    {
        // Draw brush cursor preview in scene view
        if (mainCamera != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(currentMouseWorldPos, brushRadius);
        }
    }

    void OnGUI()
    {
        if (mainCamera == null || gm == null || gm.IsGameOver) return;
        Vector3 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y; 
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        Vector3 worldEdge = worldCenter + new Vector3(brushRadius, 0, 0);
        Vector3 screenCenter = mainCamera.WorldToScreenPoint(worldCenter);
        Vector3 screenEdge = mainCamera.WorldToScreenPoint(worldEdge);
        float screenRadius = Vector3.Distance(screenCenter, screenEdge);
        DrawCircle(mousePos, screenRadius, brushCursorOutlineColor, 2f);
        DrawCircle(mousePos, screenRadius, brushCursorColor, 1f);
        GUI.color = Color.white;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
        string sizeText = $"Brush Size: {brushRadius:F2}\n(Scroll wheel to adjust)";
        GUI.Label(new Rect(10, 40, 300, 50), sizeText, style);
    }

    void DrawCircle(Vector2 center, float radius, Color color, float thickness)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        
        int segments = brushCursorSegments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2f;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2f;
            
            Vector2 p1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 p2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
            
            DrawLine(p1, p2, color, thickness);
        }
        Destroy(texture);
    }

    void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        Texture2D lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, color);
        lineTexture.Apply();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y, distance, thickness), lineTexture);
        GUI.matrix = matrix;
        Destroy(lineTexture);
    }
}
