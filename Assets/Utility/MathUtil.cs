using UnityEngine;

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
}
