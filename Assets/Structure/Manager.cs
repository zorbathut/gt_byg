using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    /////////////////////////////////////////////
    // SINGLETON
    //

    static Manager s_Manager = null;
    public static Manager instance
    {
        get
        {
            return s_Manager;
        }
    }

    /////////////////////////////////////////////
    // VARIABLES
    //

    // li'l bit of abstraction here; stores an arbitrary infinite 2d grid
    // In a previous incarnation, I had it used on several different types. In this incarnation, it's just one type.
    class GridLookup<T> where T : Object
    {
        Dictionary<int, Dictionary<int, T>> m_Data = new Dictionary<int, Dictionary<int, T>>();

        public T Lookup(int x, int z)
        {
            if (!m_Data.ContainsKey(x))
            {
                return null;
            }

            if (!m_Data[x].ContainsKey(z))
            {
                return null;
            }

            return m_Data[x][z];
        }

        public T Lookup(IntVector2 position)
        {
            return Lookup(position.x, position.z);
        }

        public void Set(IntVector2 position, T structure)
        {
            Assert.IsNull(Lookup(position));

            if (!m_Data.ContainsKey(position.x))
            {
                m_Data[position.x] = new Dictionary<int, T>();
            }

            m_Data[position.x][position.z] = structure;
        }

        // Validates that the cleared index contains the expected structure
        public void Clear(IntVector2 position, T structure)
        {
            if (Lookup(position))
            {
                Assert.IsTrue(Lookup(position) == structure);
                m_Data[position.x].Remove(position.z);
                if (m_Data[position.x].Count == 0)
                {
                    // Row empty, remove it entirely
                    m_Data.Remove(position.x);
                }
            }
            else
            {
                Assert.IsNotNull(Lookup(position));
            }
        }
    }
    GridLookup<Structure> m_WorldLookup = new GridLookup<Structure>();

    /////////////////////////////////////////////
    // INFRASTRUCTURE
    //

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    /////////////////////////////////////////////
    // COORDINATES
    //

    // Snaps to the nearest world "grid" values
    public static Vector3 GridFromWorld(Vector3 input)
    {
        return new Vector3(MathUtil.RoundTo(input.x, Constants.GridSize), 0f, MathUtil.RoundTo(input.z, Constants.GridSize));
    }

    // Gives integer lookup values from the grid values; must be a correct grid coordinate
    public static IntVector2 IndexFromGrid(Vector3 input)
    {
        Assert.IsTrue(input == GridFromWorld(input), string.Format("Tried to get index from {0} which is off-grid", input));
        return new IntVector2(Mathf.RoundToInt(input.x / Constants.GridSize), Mathf.RoundToInt(input.z / Constants.GridSize));
    }

    // Convert integer lookups back into grid values
    public static Vector3 GridFromIndex(IntVector2 input)
    {
        return new Vector3(input.x * Constants.GridSize, 0, input.z * Constants.GridSize);
    }

    /////////////////////////////////////////////
    // STRUCTURE PLACEMENT
    //

    // Try to place a structure at the given transform; returns false and sets errorMessage on failure
    public bool AttemptPlace(Structure structure, Transform transform, out string errorMessage)
    {
        errorMessage = null;

        // Create an instance so we can use the structure's built-in placement information; this is maybe a little slow, but given that it's in response to player input, nobody will notice
        Structure newStructure = Instantiate(structure);
        newStructure.transform.position = transform.position;
        newStructure.transform.rotation = transform.rotation;

        Vector3 playerGrid = GridFromWorld(GameObject.FindGameObjectWithTag(Tags.Player).transform.position);

        // Make sure each square that will be filled by a building isn't currently filled by either structure or player
        foreach (Vector3 position in newStructure.GetOccupied())
        {
            if (m_WorldLookup.Lookup(IndexFromGrid(position)))
            {
                errorMessage = "That building would overlap another building.";
                Destroy(newStructure.gameObject);
                return false;
            }

            if (playerGrid == position)
            {
                errorMessage = "Standing in a construction zone is dangerous.";
                Destroy(newStructure.gameObject);
                return false;
            }
        }

        // At this point we've verified that the structure can be built

        // Set world lookup indices
        foreach (Vector3 position in newStructure.GetOccupied())
        {
            m_WorldLookup.Set(IndexFromGrid(position), newStructure);
        }

        // At this point, at least it's plugged into the world properly, even if it's not necessarily fully set up
        // An exception after this comment will still result in a structure that's removable by the player

        ResyncDoorwaysAround(newStructure);

        return true;
    }

    public bool AttemptRemove(Vector3 position, out string errorMessage)
    {
        errorMessage = null;

        // Find structure
        Structure targetStructure = m_WorldLookup.Lookup(IndexFromGrid(position));
        if (!targetStructure)
        {
            errorMessage = "There is nothing to remove.";
            return false;
        }

        // Clear every related world lookup
        foreach (Vector3 occupiedPosition in targetStructure.GetOccupied())
        {
            m_WorldLookup.Clear(IndexFromGrid(occupiedPosition), targetStructure);
        }

        // Resync doorways of stuff that *was* adjacent
        // that info is still available in targetStructure, even though targetStructure itself is no longer in the world lookup db
        ResyncDoorwaysAround(targetStructure);

        // And we're done! Clean up the object
        Destroy(targetStructure.gameObject);

        return true;
    }

    void ResyncDoorwaysAround(Structure structure)
    {
        // First, figure out which set of structures needs to be resynced
        HashSet<Structure> resync = new HashSet<Structure>();

        // Iterate over every grid adjacent to every grid that this structure occupies; add those to the pending resync
        foreach (Vector3 occupiedPosition in structure.GetOccupied())
        {
            foreach (Vector3 delta in MathUtil.GetManhattanAdjacencies())
            {
                Structure adjacent = m_WorldLookup.Lookup(IndexFromGrid(occupiedPosition + delta * Constants.GridSize));
                if (adjacent)
                {
                    resync.Add(adjacent);
                }
            }

            // We don't add the passed-in structure itself because that breaks if the structure is mid-removal; instead, we just add whatever's in the location where the structure "should" be
            // This will be null on removal, but that's OK, we just won't add it
            Structure location = m_WorldLookup.Lookup(IndexFromGrid(occupiedPosition));
            if (location)
            {
                resync.Add(location);
            }
        }

        // Now that we have an appropriate set of structures, resynchronize linkages in all of them
        // In theory these could share information; in practice, we're looking at a 2x speedup there at best
        // And even with impractically large structures, the performance of recalculating doorways is negligable
        foreach (Structure target in resync)
        {
            target.ResyncDoorways();
        }
    }

    /////////////////////////////////////////////
    // STATE LOOKUP
    //

    public Structure StructureFromGrid(Vector3 grid)
    {
        return m_WorldLookup.Lookup(IndexFromGrid(grid));
    }
}
