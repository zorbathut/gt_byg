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

    // li'l bit of abstraction here
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

        public void Clear(IntVector2 position, T structure)
        {
            if (Lookup(position))
            {
                Assert.IsTrue(Lookup(position) == structure);
                m_Data[position.x].Remove(position.z);
                if (m_Data[position.x].Count == 0)
                {
                    // guess we can clean this up
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

    public static Vector3 GridFromWorld(Vector3 input)
    {
        return new Vector3(MathUtil.RoundTo(input.x, Constants.GridSize), 0f, MathUtil.RoundTo(input.z, Constants.GridSize));
    }

    public static IntVector2 IndexFromGrid(Vector3 input)
    {
        Assert.IsTrue(input == GridFromWorld(input), string.Format("Tried to get index from {0} which is off-grid", input));
        return new IntVector2(Mathf.RoundToInt(input.x / Constants.GridSize), Mathf.RoundToInt(input.z / Constants.GridSize));
    }

    public static Vector3 GridFromIndex(IntVector2 input)
    {
        return new Vector3(input.x * Constants.GridSize, 0, input.z * Constants.GridSize);
    }

    /////////////////////////////////////////////
    // STRUCTURE PLACEMENT
    //

    public bool AttemptPlace(Structure structure, Transform transform, out string errorMessage)
    {
        errorMessage = null;

        Structure newStructure = Instantiate(structure);
        newStructure.transform.position = transform.position;
        newStructure.transform.rotation = transform.rotation;

        Vector3 playerTarget = GridFromWorld(GameObject.FindGameObjectWithTag(Tags.Player).transform.position);

        foreach (Vector3 position in newStructure.GetOccupied())
        {
            if (m_WorldLookup.Lookup(IndexFromGrid(position)))
            {
                errorMessage = "That building would overlap another building.";
                Destroy(newStructure.gameObject);
                return false;
            }

            if (playerTarget == position)
            {
                errorMessage = "Standing in a construction zone is dangerous.";
                Destroy(newStructure.gameObject);
                return false;
            }
        }

        // Can be built; go ahead and do it

        foreach (Vector3 position in newStructure.GetOccupied())
        {
            m_WorldLookup.Set(IndexFromGrid(position), newStructure);
        }

        ResyncDoorwaysAround(newStructure);

        return true;
    }

    public bool AttemptRemove(Vector3 position, out string errorMessage)
    {
        errorMessage = null;

        Structure targetStructure = m_WorldLookup.Lookup(IndexFromGrid(position));

        if (!targetStructure)
        {
            errorMessage = "There is nothing to remove.";
            return false;
        }

        foreach (Vector3 occupiedPosition in targetStructure.GetOccupied())
        {
            m_WorldLookup.Clear(IndexFromGrid(occupiedPosition), targetStructure);
        }

        ResyncDoorwaysAround(targetStructure);

        Destroy(targetStructure.gameObject);

        return true;
    }

    void ResyncDoorwaysAround(Structure structure)
    {
        // First, figure out which set of structures needs to be resynced
        HashSet<Structure> resync = new HashSet<Structure>();

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

            // we don't add the structure itself because that breaks if the structure is being removed; instead, we just add whatever's in the location where the structure "should" be
            Structure location = m_WorldLookup.Lookup(IndexFromGrid(occupiedPosition));
            if (location)
            {
                resync.Add(location);
            }
        }

        // Now that we have an appropriate set of structures, resynchronize linkages in all of them
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
