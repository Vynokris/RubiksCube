using System.Collections;
using UnityEngine;
using TMPro;

public static class Extensions
{
    public static IEnumerator FadeText(TextMeshProUGUI text, float fadeDuration, bool fadeIn)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, (fadeIn ? timer / fadeDuration : 1 - timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        if (fadeIn) text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        else        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
    }

    public static Vector3 ScreenToPlane(this Camera camera, Vector2 screenPos, Vector3 planeNormal, Vector3 planeOffset)
    {
        Plane   plane = new Plane(planeNormal, planeOffset);
        Ray     ray   = camera.ScreenPointToRay(screenPos);
        float   dist  ; plane.Raycast(ray, out dist);
        return ray.origin + ray.direction * dist;
    }

    public static Quaternion QuaternionNegate(this Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    public static float QuaternionAngle(Quaternion start, Quaternion end)
    {
        return Mathf.Acos(Quaternion.Dot(start, end)) * 2f * Mathf.Rad2Deg;
    }

    public static Quaternion QuaternionAxisAngle(Vector3 axis, float angle)
    {
        angle *= Mathf.Deg2Rad;
        return new Quaternion(
            axis.x * Mathf.Sin(angle / 2),
            axis.y * Mathf.Sin(angle / 2),
            axis.z * Mathf.Sin(angle / 2),
            Mathf.Cos(angle / 2)
        );
    }

    public static Quaternion QuaternionLerp(Quaternion start, Quaternion end, float t)
    {
        // Negate the second quaterion if the dot product is negative.
        float dot = Quaternion.Dot(start, end);
        if(dot < 0.0f) 
            end = end.QuaternionNegate();

        // Lerp all values one by one.
        Quaternion result;
        result.x = start.x - t * (start.x - end.x);
        result.y = start.y - t * (start.y - end.y);
        result.z = start.z - t * (start.z - end.z);
        result.w = start.w - t * (start.w - end.w);
        return result.normalized;
    }

    public static Quaternion QuaternionSlerp(Quaternion start, Quaternion end, float t)
    {
        // See https://en.wikipedia.org/wiki/Slerp#Geometric_Slerp

        float omega       = Mathf.Acos(Quaternion.Dot(start, end)); // Angle in radians between start and end.
        float sinOmega    = Mathf.Sin(omega);
        float startFactor = Mathf.Sin((1 - t) * omega) / sinOmega;
        float endFactor   = Mathf.Sin(     t  * omega) / sinOmega;

        Quaternion result = new Quaternion(0, 0, 0, 0);
        result.x = startFactor * start.x + endFactor * end.x;
        result.y = startFactor * start.y + endFactor * end.y;
        result.z = startFactor * start.z + endFactor * end.z;
        result.w = startFactor * start.w + endFactor * end.w;
        return result.normalized;
    }
}