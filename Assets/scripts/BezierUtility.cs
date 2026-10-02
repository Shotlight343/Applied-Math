using UnityEngine;

public static class BezierUtility
{
    // Quadratic Bezier: 3 control points (p0 start, p1 control, p2 end)
    public static Vector3 QuadraticLerp(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    // Cubic Bezier: 4 control points (p0 start, p1, p2 controls, p3 end)
    public static Vector3 CubicLerp(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0
             + 3f * u * u * t * p1
             + 3f * u * t * t * p2
             + t * t * t * p3;
    }
}
