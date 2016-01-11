using UnityEngine;
using System.Collections.Generic;

public static class MathUtil
{
    public static float RoundTo(float value, float accuracy)
    {
        return Mathf.Round(value / accuracy) * accuracy;
    }

    public static Vector3 RoundTo(Vector3 value, float accuracy)
    {
        return new Vector3(RoundTo(value.x, accuracy), RoundTo(value.y, accuracy), RoundTo(value.z, accuracy));
    }

    static Vector3[] s_ManhattanAdjacencies = new Vector3[] {
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
    };

    public static IEnumerable<Vector3> GetManhattanAdjacencies()
    {
        return s_ManhattanAdjacencies;
    }
}
