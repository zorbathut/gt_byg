using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct IntVector2
{
    public int x;
    public int z;

    public IntVector2(int in_x, int in_z)
    {
        x = in_x;
        z = in_z;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, z);
    }

    public static IntVector2 operator+(IntVector2 lhs, IntVector2 rhs)
    {
        return new IntVector2(lhs.x + rhs.x, lhs.z + rhs.z);
    }

    public static IntVector2 operator-(IntVector2 lhs, IntVector2 rhs)
    {
        return new IntVector2(lhs.x - rhs.x, lhs.z - rhs.z);
    }

    static IntVector2[] s_ManhattanAdjacencies = new IntVector2[] {
        new IntVector2(-1, 0),
        new IntVector2(1, 0),
        new IntVector2(0, -1),
        new IntVector2(0, 1),
    };

    public static IEnumerable<IntVector2> GetManhattanAdjacencies()
    {
        return s_ManhattanAdjacencies;
    }
}
