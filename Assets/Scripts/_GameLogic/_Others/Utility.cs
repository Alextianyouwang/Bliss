using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static bool IsVisibleFromCamera(Camera cam, GameObject target, bool hardTest)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (Plane p in planes)
        {
            if (p.GetDistanceToPoint(target.transform.position) < 0)
            {

                return false;
            }
        }
        if (hardTest)
        {
            if (target.GetComponent<Renderer>().isVisible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }

    }
    public static bool IsPositionInCamera(Camera cam, Vector3 pos)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (Plane p in planes)
        {
            if (p.GetDistanceToPoint(pos) < 0)
            {

                return false;
            }
        }
        return true;

    }

    public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
    {
        Vector3 c = current.eulerAngles;
        Vector3 t = target.eulerAngles;
        return Quaternion.Euler(
          Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime, maxSpeed, deltaTime),
          Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime, maxSpeed, deltaTime),
          Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime, maxSpeed, deltaTime)
        );
    }

    public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = new Vector3();
        p.x = uu * p0.x + 2 * u * t * p1.x + tt * p2.x;
        p.y = uu * p0.y + 2 * u * t * p1.y + tt * p2.y;
        p.z = uu * p0.z + 2 * u * t * p1.z + tt * p2.z;
        return p;
    }
    public static int GetFirstNullIndexInList<T>(T[] array)
    {
        foreach (T t in array)
        {
            if (t == null)
                return Array.IndexOf(array, t);
        }
        return array.Length;
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float LerpHelper(ref float defaultValue, float targetValue, float Multiplier)
    {
        bool isPositive = defaultValue - targetValue > 0 ? true : false;
        if (isPositive)
            defaultValue -= defaultValue > targetValue ? Time.deltaTime * Multiplier : 0;
        else
            defaultValue += defaultValue < targetValue ? Time.deltaTime * Multiplier : 0;

        return defaultValue;
    }
}
