using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Structure : MonoBehaviour
{
    [SerializeField] Transform m_Occupied;
    [SerializeField] Transform m_Doorway;

    [SerializeField, HideInInspector] bool m_Standardized = false;
    protected virtual void Awake()
    {
        // Run an initialization pass to generate intermediate info
        if (!m_Standardized)
        {
            // Clamp occupied/doorway to grid
            foreach (Transform occupiedSquare in m_Occupied)
            {
                Assert.IsTrue(ValidateOccupied(occupiedSquare.transform.position));
            }

            // Clamp doorways to doorway grid
            foreach (Transform doorwaySquare in m_Doorway)
            {
                bool alongZ;
                bool valid = ValidateDoorway(doorwaySquare.position, out alongZ);
                Assert.IsTrue(valid);

                StructureDoorwayInfo info = doorwaySquare.gameObject.AddComponent<StructureDoorwayInfo>();
                info.m_OriginalAlongZ = alongZ;
            }

            m_Standardized = true;
        }
    }

    public bool HasOccupied(Vector3 position)
    {
        Assert.IsTrue(position == Manager.GridFromWorld(position));

        foreach (Vector3 occupied in GetOccupied())
        {
            if (occupied ==  position)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasDoorway(Vector3 position)
    {
        foreach (Transform doorway in m_Doorway.transform)
        {
            if (doorway.transform.position == position)
            {
                return true;
            }
        }

        return false;
    }

    // This is potentially unnecessarily slow, but it's not called often enough to really matter, and frequently it's called when the object has, in fact, moved
    // If it turned out to be a speed issue one could easily cache the result using the position and rotation as keys to invalidate the cache
    public IEnumerable<Vector3> GetOccupied()
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (Transform occupiedSquare in m_Occupied)
        {
            Assert.IsTrue(occupiedSquare.position == Manager.GridFromWorld(occupiedSquare.position));
            positions.Add(occupiedSquare.position);
        }

        return positions;
    }

    public void ResyncDoorways()
    {
        foreach (Transform doorway in m_Doorway.transform)
        {
            StructureDoorwayInfo info = doorway.GetComponent<StructureDoorwayInfo>();

            Assert.IsTrue(info != null);
            if (!info)
            {
                continue;
            }

            Debug.LogFormat("Processing door {0} along {1}", doorway.transform.position, info.m_OriginalAlongZ);
            Structure lhs = Manager.instance.StructureFromGrid(doorway.transform.position - (info.m_OriginalAlongZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
            Structure rhs = Manager.instance.StructureFromGrid(doorway.transform.position + (info.m_OriginalAlongZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);

            bool doorwayBlocked = true;

            Assert.IsTrue(lhs != this || rhs != this);
            Assert.IsTrue(lhs == this || rhs == this);
            if (lhs != null && lhs != this)
            {
                doorwayBlocked &= !lhs.HasDoorway(doorway.transform.position);
            }
            if (rhs != null && rhs != this)
            {
                doorwayBlocked &= !rhs.HasDoorway(doorway.transform.position);
            }

            doorway.gameObject.SetActive(doorwayBlocked);
        }
    }

    #if UNITY_EDITOR
        public virtual void OnDrawGizmos()
        {
            foreach (Transform occupiedSquare in m_Occupied)
            {
                if (ValidateOccupied(occupiedSquare.position))
                {
                    GizmoUtil.DrawSquareAround(occupiedSquare.position, Constants.GridSize, Color.white);
                }
                else
                {
                    GizmoUtil.DrawSquareAround(occupiedSquare.position, 1f, Color.red);
                }
            }

            foreach (Transform doorwaySquare in m_Doorway)
            {
                bool doorwayAlongZ;
                if (ValidateDoorway(doorwaySquare.position, out doorwayAlongZ))
                {
                    GizmoUtil.DrawSquareAround(doorwaySquare.position, 1f, Color.white);
                }
                else
                {
                    GizmoUtil.DrawSquareAround(doorwaySquare.position, 1f, Color.red);
                }
            }
        }
    #endif

    bool ValidateOccupied(Vector3 position)
    {
        Vector3 clampedCenter = Manager.GridFromWorld(position);
        return position == clampedCenter && position.y == 0;
    }

    bool ValidateDoorway(Vector3 position, out bool alongZ)
    {
        // Round to half-grid position
        Vector3 clampedCenter = new Vector3(MathUtil.RoundTo(position.x, Constants.GridSize / 2), 0f, MathUtil.RoundTo(position.z, Constants.GridSize / 2));

        // Check to see if we're on exactly one half-grid
        bool xAligned = clampedCenter.x == MathUtil.RoundTo(clampedCenter.x, Constants.GridSize);
        bool zAligned = clampedCenter.z == MathUtil.RoundTo(clampedCenter.z, Constants.GridSize);

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
            if (HasOccupied(clampedCenter - Vector3.forward * Constants.GridSize / 2) == HasOccupied(clampedCenter + Vector3.forward * Constants.GridSize / 2))
            {
                return false;
            }
        }
        else
        {
            // If it's aligned on the z axis, that means it's an x-facing door
            alongZ = false;

            // doors must be placed between an occupied square and a non-occupied square
            if (HasOccupied(clampedCenter - Vector3.right * Constants.GridSize / 2) == HasOccupied(clampedCenter + Vector3.right * Constants.GridSize / 2))
            {
                return false;
            }
        }

        return position == clampedCenter && position.y == 0f;
    }
}
