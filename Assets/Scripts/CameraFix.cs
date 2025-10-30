using UnityEngine;

public class CameraFix : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        // Force proper 2D orthographic setup
        cam.orthographic = true;
        cam.transparencySortMode = TransparencySortMode.Orthographic;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = -10f; // CRITICAL: must include z=0 where sprites are
        cam.farClipPlane = 10f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.cullingMask = -1; // Everything
        
        // Force position
        transform.position = new Vector3(0, 0, -10);
        
        Debug.Log($"CameraFix: Forced camera setup - ortho={cam.orthographic}, size={cam.orthographicSize}, near={cam.nearClipPlane}, far={cam.farClipPlane}, pos={transform.position}");
        
        // Force camera to render immediately
        cam.enabled = false;
        cam.enabled = true;
    }
}
