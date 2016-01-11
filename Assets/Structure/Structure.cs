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
                Assert.IsTrue(ValidateDoorway(doorwaySquare.position));
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
            // Grab the two structures next to the doorway
            Structure lhs = Manager.instance.StructureFromGrid(doorway.transform.position - (IsDoorwayFacingZ(doorway.transform.position) ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
            Structure rhs = Manager.instance.StructureFromGrid(doorway.transform.position + (IsDoorwayFacingZ(doorway.transform.position) ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
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

    static bool IsDoorwayFacingZ(Vector3 position)
    {
        // "more magic"
        return Mathf.Abs(position.x - MathUtil.RoundTo(position.x, Constants.GridSize)) < Constants.GridSize / 100;

        // Doorways are required to be grid-aligned on exactly one axis and half-grid-aligned on the other, so they are effectively placed at the midpoint of a wall
        // This is validated at runtime, so we simply assume it here
        
        // If a doorway is aligned to the x-axis, then that means it's inside a wall running along that x-axis
        // And if it's inside an x-parallel wall, then the doorway is actually facing the z axis

        // The only other part of that equation is a small epsilon just in case Unity doesn't do an integer-accurate rotation
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
                if (ValidateDoorway(doorwaySquare.position))
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
        return position == Manager.GridFromWorld(position) && position.y == 0;
    }

    bool ValidateDoorway(Vector3 position)
    {
        // Validating a doorway is kinda tricky

        // Doorways don't go on the same grid - they go between grid points, on half-grid vertices
        // But only *specific* half-grid vertices - ones directly between two normal grid vertices, not midway between a set of four
        // This makes it a little awkward to validate

        // First, validate that we're on exactly one half-grid position
        // Round to half-grid position
        Vector3 clampedCenter = new Vector3(MathUtil.RoundTo(position.x, Constants.GridSize / 2), 0f, MathUtil.RoundTo(position.z, Constants.GridSize / 2));

        {
            // Check to see if we're on exactly *one* half-grid - if so, we're in the middle of a wall. Two half-grids would be a corner, zero would be in the middle of a square.
            bool xAligned = clampedCenter.x == MathUtil.RoundTo(clampedCenter.x, Constants.GridSize);
            bool zAligned = clampedCenter.z == MathUtil.RoundTo(clampedCenter.z, Constants.GridSize);

            if (xAligned == zAligned)
            {
                // We're on either zero half-grids or two half-grids, both of which are wrong
                return false;
            }
        }

        // Second, make sure the doorway is between an occupied square and a non-occupied square
        // Structures can't have internal doors - they would always be open, which is silly
        // And they also can't have 100% external doors because then why are they part of this structure?
        bool facingZ = IsDoorwayFacingZ(position);

        bool lhsOccupied = HasOccupied(position - (facingZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);
        bool rhsOccupied = HasOccupied(position + (facingZ ? Vector3.forward : Vector3.right) * Constants.GridSize / 2);

        if (lhsOccupied == rhsOccupied)
        {
            // We have either zero occupied (external door) or two occupied (internal door), both of which are invalid
            return false;

        }

        // Finally, verify it's half-grid-aligned and on y=0
        return position == clampedCenter && position.y == 0f;
    }
}
