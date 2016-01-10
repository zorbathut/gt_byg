using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] Transform m_Occupied;
    [SerializeField] Transform m_Doorway;

    protected virtual void Awake()
    {
        // Validate position on grid

        // Validate occupied/doorway
        
    }

    public bool HasOccupied(Vector3 position)
    {
        Assert.IsTrue(position == Manager.GridFromWorld(position));

        foreach (Transform occupiedSquare in m_Occupied)
        {
            if (occupiedSquare.position ==  position)
            {
                return true;
            }
        }

        return false;
    }

    #if UNITY_EDITOR
    public virtual void OnDrawGizmos()
    {
        foreach (Transform occupiedSquare in m_Occupied)
        {
            Vector3 occupiedCenter;
            if (ValidateOccupied(occupiedSquare.position, out occupiedCenter))
            {
                GizmoUtil.DrawSquareAround(occupiedCenter, Constants.GridSize, Color.white);
            }
            else
            {
                GizmoUtil.DrawSquareAround(occupiedSquare.position, 1f, Color.red);
            }
        }

        foreach (Transform doorwaySquare in m_Doorway)
        {
            Vector3 doorwayCenter;
            bool doorwayAlongZ;
            if (ValidateDoorway(doorwaySquare.position, out doorwayCenter, out doorwayAlongZ))
            {
                GizmoUtil.DrawSquareAround(doorwayCenter, 1f, Color.white);
            }
            else
            {
                GizmoUtil.DrawSquareAround(doorwaySquare.position, 1f, Color.red);
            }
        }
    }
    #endif

    bool ValidateOccupied(Vector3 position, out Vector3 occupiedCenter)
    {
        occupiedCenter = Manager.GridFromWorld(position);
        return (occupiedCenter - position).magnitude < 0.1f && Mathf.Abs(position.y) < 0.1f;
    }

    bool ValidateDoorway(Vector3 position, out Vector3 doorwayCenter, out bool alongZ)
    {
        // Round to half-grid position
        doorwayCenter = new Vector3(MathUtil.RoundTo(position.x, Constants.GridSize / 2), position.y, MathUtil.RoundTo(position.z, Constants.GridSize / 2));

        // Check to see if we're on exactly one half-grid
        bool xAligned = Mathf.Abs(doorwayCenter.x - MathUtil.RoundTo(doorwayCenter.x, Constants.GridSize)) < 0.1f;
        bool zAligned = Mathf.Abs(doorwayCenter.z - MathUtil.RoundTo(doorwayCenter.z, Constants.GridSize)) < 0.1f;

        // Must be aligned on exactly one axis
        if (xAligned == zAligned)
        {
            // doesn't really matter
            alongZ = false;
            return false;
        }

        if (xAligned)
        {
            // If it's aligned on the x axis, that means it's a z-facing door
            alongZ = true;

            // doors must be placed between an occupied square and a non-occupied square
            if (HasOccupied(doorwayCenter - Vector3.forward * Constants.GridSize / 2) == HasOccupied(doorwayCenter + Vector3.forward * Constants.GridSize / 2))
            {
                return false;
            }
        }
        else
        {
            // If it's aligned on the x axis, that means it's an x-facing door
            alongZ = true;

            // doors must be placed between an occupied square and a non-occupied square
            if (HasOccupied(doorwayCenter - Vector3.right * Constants.GridSize / 2) == HasOccupied(doorwayCenter + Vector3.right * Constants.GridSize / 2))
            {
                return false;
            }
        }

        return Mathf.Abs(position.y) < 0.1f;
    }
}
