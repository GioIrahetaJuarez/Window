using UnityEngine;

public class Brush : MonoBehaviour
{
    public DustController dustController;
    public float brushRadius = 0.25f; 
    public float maxAllowedSpeed = 5f;
    public float speedSmoothing = 8f;
    public float speedForgivenessTime = 0.12f;

    Vector2 lastPos;
    bool dragging = false;
    float lastTime;
    float smoothSpeed = 0f;
    float aboveTime = 0f;

    GameManager gm;
    WinPoint[] failPoints;

    void Start()
    {
        gm = GameManager.Instance;
        failPoints = Object.FindObjectsByType<WinPoint>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            
            var mp = Input.mousePosition;
            if (cam.pixelRect.width == 0 || cam.pixelRect.height == 0) return;
            if (!cam.pixelRect.Contains(mp)) return;

            dragging = true;
            lastTime = Time.time;
            Vector3 sp = new Vector3(mp.x, mp.y, -cam.transform.position.z);
            lastPos = cam.ScreenToWorldPoint(sp);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging && !gm.IsGameOver)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            
            var mp = Input.mousePosition;
            if (cam.pixelRect.width == 0 || cam.pixelRect.height == 0) return;
            if (!cam.pixelRect.Contains(mp)) return;

            Vector3 sp = new Vector3(mp.x, mp.y, -cam.transform.position.z);
            Vector2 curPos = cam.ScreenToWorldPoint(sp);
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
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(lastPos, curPos, (float)i / (float)steps);
                dustController.EraseAt(p, brushRadius);
                foreach (var fp in failPoints)
                {
                    if (fp == null) continue;
                    float d = Vector2.Distance(p, fp.transform.position);
                    if (d <= fp.radius)
                    {
                        gm.Lose("try to avoid the red points! :(");
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
}
