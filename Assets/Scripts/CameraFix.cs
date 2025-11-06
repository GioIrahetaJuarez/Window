using UnityEngine;

public class CameraFix : MonoBehaviour
{
    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;
        cam.orthographic = true;
        cam.transparencySortMode = TransparencySortMode.Orthographic;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = -10f;
        cam.farClipPlane = 10f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.cullingMask = -1;
        transform.position = new Vector3(0, 0, -10);
        cam.enabled = false;// Force camera to render immediately
        cam.enabled = true;
    }
}
