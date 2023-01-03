using UnityEngine;

public class CubeZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 5;
    [SerializeField] private float minZoom   = 10;
    [SerializeField] private float maxZoom   = 50;

    private Camera sceneCamera;

    void Start()
    {
        sceneCamera = GameObject.FindObjectOfType<Camera>();
    }

    void Update()
    {
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            sceneCamera.fieldOfView -= Input.mouseScrollDelta.y * zoomSpeed;
            sceneCamera.fieldOfView  = Mathf.Clamp(sceneCamera.fieldOfView, minZoom, maxZoom);
        }
    }
}
