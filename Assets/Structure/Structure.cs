using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Structure : MonoBehaviour
{
    [SerializeField] Transform m_Occupied;
    [SerializeField] Transform m_Doorway;

    [SerializeField] bool m_DestroyTool = false;

    [SerializeField, HideInInspector] bool m_Standardized = false;
    
    protected virtual void Awake()
    {
        // Run an initialization pass to validate positions for development and generate intermediate info
        if (!m_Standardized && !m_DestroyTool)
        {
            // Validate that occupied flags are on grid
            foreach (Transform occupiedSquare in m_Occupied)
            {
                Assert.IsTrue(ValidateOccupied(occupiedSquare.transform.position));
            }

            // Validate that doorways are on grid; also, attach StructureDoorwayInfo so we don't have
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

    // true if this structure occupies a grid position; O(n) in number of Occupied's
    public bool HasOccupied(Vector3 position)
    {
        Assert.IsTrue(position == Manager.GridFromWorld(position));

        foreach (Vector3 occupied in GetOccupied())
        {
            if ((occupied - position).magnitude < Constants.GridSize / 100)  // we'll get tons of asserts long before this epsilon is too large
            {
                return true;
            }
        }

        return false;
    }

    // true if this structure has a doorway; O(n) in number of Doorway's
    public bool HasDoorway(Vector3 position)
    {
        foreach (Transform doorway in m_Doorway.transform)
        {
            if ((doorway.transform.position - position).magnitude < Constants.GridSize / 100)  // we'll get tons of asserts long before this epsilon is too large
            {
                return true;
            }
        }

        return false;
    }

    public bool GetDestroyTool()
    {
        return m_DestroyTool;
    }

    // This is kinda unnecessarily slow, but it's not called often enough to really matter, and frequently it's called when the object has, in fact, moved
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

            // Figure out which axis the doorway is facing along
            bool alongZ = info.m_OriginalAlongZ;

            // Deal with 90-degree rotations; we'll have a swapped direction for doors in that case
            {
                float angle;
                Vector3 axis;
                transform.rotation.ToAngleAxis(out angle, out axis);
                if (Mathf.Abs(angle - 90) < 1 || Mathf.Abs(angle - 270) < 1)
                {
                    alongZ = !alongZ;
                }
            }

            // Grab the two structures next to the doorway
            Structure lhs = Manager.instance.StructureFromGrid(doorway.transform.position - (alongZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
            Structure rhs = Manager.instance.StructureFromGrid(doorway.transform.position + (alongZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
            Assert.IsTrue(lhs == this || rhs == this);  // One structure must be us
            Assert.IsTrue(lhs != this || rhs != this);  // The other structure must be the other structure (or null)
            
            // Assume we're blocked
            bool doorwayBlocked = true;

            // For whichever structure isn't us, see if it has a doorway at the appropriate place
            // If it does, we remove our doorway
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
        // Draw red squares if things are misaligned, white if they're aligned properly
        public virtual void OnDrawGizmos()
        {
            if (m_DestroyTool)
            {
                return;
            }

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
        // Validating an occupied flag is easy; just make sure it's on the grid and on y=0
        // In theory we should also check if there are duplicates, but nobody's made that mistake yet. Add if it turns out someone does.
        Vector3 clampedCenter = Manager.GridFromWorld(position);
        return position == clampedCenter && position.y == 0;
    }

    bool ValidateDoorway(Vector3 position, out bool alongZ)
    {
        // Validating a doorway is kinda tricky

        // Round to half-grid position
        Vector3 clampedCenter = new Vector3(MathUtil.RoundTo(position.x, Constants.GridSize / 2), 0f, MathUtil.RoundTo(position.z, Constants.GridSize / 2));

        // Check to see if we're on exactly *one* half-grid - if so, we're in the middle of a wall. Two half-grids would be a corner, zero would be in the middle of a square.
        bool xAligned = clampedCenter.x == MathUtil.RoundTo(clampedCenter.x, Constants.GridSize);
        bool zAligned = clampedCenter.z == MathUtil.RoundTo(clampedCenter.z, Constants.GridSize);

        if (xAligned == zAligned)
        {
            // Result doesn't really matter; we're in an invalid state anyway
            alongZ = false;
            return false;
        }

        if (xAligned)
        {
            // If it's aligned on a wall along the x axis, that means it's a z-facing door
            alongZ = true;

            // Doors must be placed between an occupied square and a non-occupied square
            if (HasOccupied(clampedCenter - Vector3.forward * Constants.GridSize / 2) == HasOccupied(clampedCenter + Vector3.forward * Constants.GridSize / 2))
            {
                return false;
            }
        }
        else
        {
            // If it's aligned on a wall along the z axis, that means it's an x-facing door
            alongZ = false;

            // Doors must be placed between an occupied square and a non-occupied square
            if (HasOccupied(clampedCenter - Vector3.right * Constants.GridSize / 2) == HasOccupied(clampedCenter + Vector3.right * Constants.GridSize / 2))
            {
                return false;
            }
        }

        // Verify it's grid-aligned and on y=0
        return position == clampedCenter && position.y == 0f;
    }
}
