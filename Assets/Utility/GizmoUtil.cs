#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public static class GizmoUtil
{
    public static void DrawSquare(Vector3 min, Vector3 max, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z));
        Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));
        Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
        Gizmos.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z));
    }

    public static void DrawSquareAround(Vector3 center, float diameter, Color color)
    {
        DrawSquare(center - new Vector3(diameter / 2, 0, diameter / 2), center + new Vector3(diameter / 2, 0, diameter / 2), color);
    }
}
#endif
