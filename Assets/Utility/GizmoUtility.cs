using UnityEngine;
using System.Collections;

public static class GizmoUtility
{
    public static void DrawSquare(Vector3 min, Vector3 max)
    {
        Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z));
        Gizmos.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));
        Gizmos.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
        Gizmos.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z));
    }
}
