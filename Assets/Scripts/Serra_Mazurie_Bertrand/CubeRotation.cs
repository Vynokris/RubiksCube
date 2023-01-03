using System;
using System.Collections;
using UnityEngine;

public class CubeRotation : MonoBehaviour
{
    public  Vector3 rotationAxis    { get; private set; } = Vector3.right;
    public  int     rotationSlice   { get; private set; } = 0;
    public  bool    rotationReverse { get; private set; } = false;
    public  bool    rotating        { get; private set; } = false;
    public  float   rotationSpeed                         = 200;

    private Camera               sceneCamera;
    private MouseSwipeController mouseSwipe;
    private CubeSlicer           cubeSlicer;
    private CubeShuffle          cubeShuffle;
    private CubeResolve          cubeResolve;

    void Awake()
    {
        sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mouseSwipe  = gameObject.GetComponent<MouseSwipeController>();
        cubeSlicer  = gameObject.GetComponent<CubeSlicer >();
        cubeShuffle = gameObject.GetComponent<CubeShuffle>();
        cubeResolve = gameObject.GetComponent<CubeResolve>();
    }

    private IEnumerator RotateSubCube(GameObject subCube, Vector3 axis, bool reverse, float speed)
    {
        Vector3    localAxis = Quaternion.Inverse(subCube.transform.localRotation) * axis;
        Vector3    startPos  = subCube.transform.localPosition;
        Quaternion targetRot = subCube.transform.localRotation * Extensions.QuaternionAxisAngle(localAxis, reverse ? -90 : 90);

        float timer = 0;
        while (subCube.transform.localRotation != targetRot && timer < 2f)
        {
            // Smoothly interpolate between the sub-cube's start rotation and its target rotation.
            float remainingAngle            = Extensions.QuaternionAngle(subCube.transform.localRotation, targetRot);
            float rotationAmount            = Mathf.Min(1f, (speed * Time.deltaTime) / remainingAngle);
            subCube.transform.localRotation = Extensions.QuaternionSlerp(subCube.transform.localRotation, targetRot, rotationAmount);

            // Smoothly rotate the sub-cube's position around the rotation normal.
            Quaternion gradualRot           = Extensions.QuaternionAxisAngle(axis, speed * Time.deltaTime * (reverse ? -1 : 1));
            subCube.transform.localPosition = gradualRot * subCube.transform.localPosition;

            timer += Time.deltaTime;
            rotating = true;
            yield return null;
        }

        // Round the rotation and position of the sub cube to erase the error margin of the lerped rotation.
        subCube.transform.localEulerAngles = new Vector3(Mathf.Round(subCube.transform.localEulerAngles.x), Mathf.Round(subCube.transform.localEulerAngles.y), Mathf.Round(subCube.transform.localEulerAngles.z));
        subCube.transform.localPosition    = Extensions.QuaternionAxisAngle(axis, reverse ? -90 : 90) * startPos;
        subCube.transform.localPosition    = new Vector3(MathF.Round(subCube.transform.localPosition.x, 1), MathF.Round(subCube.transform.localPosition.y, 1), MathF.Round(subCube.transform.localPosition.z, 1));
        rotating = false;
    }

    public void RotateSlice(Vector3 axis, int slice, bool reverse, bool addToHistory = true)
    {
        // Update the rotation values.
        rotationAxis    = axis;
        rotationSlice   = slice;
        rotationReverse = reverse;

        // Save this rotation so that the cube can be resolved later.
        if (addToHistory)
            cubeResolve.AddRotationEvent(rotationAxis, rotationSlice, rotationReverse);

        // Rotate each sub-cube from the selected slice.
        if (slice >= 0)
            foreach (GameObject subCube in cubeSlicer.GetSlice(axis, slice))
                StartCoroutine(RotateSubCube(subCube, axis, reverse, rotationSpeed));

        // Rotate every sub-cube of the rubik's cube.
        else if (slice == -1)
            foreach (Transform subCube in transform.GetChild(0))
                StartCoroutine(RotateSubCube(subCube.gameObject, axis, reverse, rotationSpeed));
    }

    public void StopRotation()
    {
        if (rotating) {
            StopAllCoroutines();
            rotating = false;
        }
    }

    void Update()
    {
        if (mouseSwipe.LeftSwipe().swipeEnded && !cubeShuffle.shuffling && !cubeResolve.resolving)
        {
            // Cast a ray from the camera at the point where the swipe started.
            SwipeData  leftSwipe = mouseSwipe.LeftSwipe();
            Ray        ray = sceneCamera.ScreenPointToRay(leftSwipe.fromPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Get hit gameobject and normal.
                GameObject hitCube   = hit.transform.parent.gameObject;
                Vector3    hitNormal = hit.normal;

                // Get the world vector of the mouse swipe.
                Vector3 swipeVec = (sceneCamera.ScreenToPlane(leftSwipe.toPos, hit.normal, hit.point) - hit.point).normalized;

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

                // Get the slice index.
                int slice = cubeSlicer.MaskSubCubePos(hitCube.transform.position, rotVec);

                // Get the rotation direction.
                bool reverse = Vector3.Dot(Vector3.Cross(axisAlignedSwipe, hit.normal), rotVec) > 0;

                // Let the user rotate multiple slices at once along the same normal.
                if (rotating && !(rotationAxis == rotVec && rotationSlice != slice))
                    return;

                // Rotate the slice.
                RotateSlice(rotVec, slice, reverse);
            }
        }
    }
}
