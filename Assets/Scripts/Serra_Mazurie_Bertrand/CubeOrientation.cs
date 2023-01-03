using UnityEngine;

public class CubeOrientation : MonoBehaviour
{
    public  float                rotationSpeed = 200f;
    private Camera               sceneCamera;
    private MouseSwipeController mouseSwipe;
    private CubeRotation         cubeRotation;
    private CubeShuffle          cubeShuffle;

    void Awake()
    {
        sceneCamera  = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mouseSwipe   = gameObject.GetComponent<MouseSwipeController>();
        cubeRotation = gameObject.GetComponent<CubeRotation>();
        cubeShuffle  = gameObject.GetComponent<CubeShuffle>();
    }

    void Update()
    {
        if (mouseSwipe.RightSwipe().swipeEnded && !cubeRotation.rotating && !cubeShuffle.shuffling)
        {
            // Cast a ray from the camera at the point where the swipe started.
            SwipeData  rightSwipe = mouseSwipe.RightSwipe();
            Ray        ray = sceneCamera.ScreenPointToRay(rightSwipe.fromPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Get hit gameobject and normal.
                GameObject hitCube   = hit.transform.parent.gameObject;
                Vector3    hitNormal = hit.normal;

                // Get the world vector of the mouse swipe.
                Vector3 swipeVec = (sceneCamera.ScreenToPlane(rightSwipe.toPos, hit.normal, hit.point) - hit.point).normalized;

                // Get the closest rotation vector to the world swipe and the closest world axis to the world swipe.
                Vector3 axisAlignedSwipe = new Vector3(0, 0, 0);
                Vector3 rotVec           = new Vector3(0, 0, 0);
                {
                    float dotRht = Vector3.Dot(hitCube.transform.right,   swipeVec);
                    float dotUp  = Vector3.Dot(hitCube.transform.up,      swipeVec);
                    float dotFwd = Vector3.Dot(hitCube.transform.forward, swipeVec);
                    float dotMax = Mathf.Max(Mathf.Abs(dotRht), Mathf.Max(Mathf.Abs(dotUp), Mathf.Abs(dotFwd)));
                    if      (dotMax == Mathf.Abs(dotRht)) { rotVec = Vector3.Cross(hitCube.transform.right,   hit.normal); axisAlignedSwipe = Mathf.Sign(dotRht) * hitCube.transform.right;   }
                    else if (dotMax == Mathf.Abs(dotUp )) { rotVec = Vector3.Cross(hitCube.transform.up,      hit.normal); axisAlignedSwipe = Mathf.Sign(dotUp ) * hitCube.transform.up;      }
                    else if (dotMax == Mathf.Abs(dotFwd)) { rotVec = Vector3.Cross(hitCube.transform.forward, hit.normal); axisAlignedSwipe = Mathf.Sign(dotFwd) * hitCube.transform.forward; }
                    rotVec = new Vector3(Mathf.RoundToInt(Mathf.Abs(rotVec.x)), Mathf.RoundToInt(Mathf.Abs(rotVec.y)), Mathf.RoundToInt(Mathf.Abs(rotVec.z)));
                }

                // Get the rotation direction.
                bool reverse = Vector3.Dot(Vector3.Cross(axisAlignedSwipe, hit.normal), rotVec) > 0;

                // Rotate the whole cube.
                cubeRotation.RotateSlice(rotVec, -1, reverse);
            }
        }
    }
}
