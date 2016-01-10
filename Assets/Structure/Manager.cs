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

        public void Set(int x, int z, T structure)
        {
            Set(new IntVector2(x, z), structure);
        }

        public void Clear(IntVector2 position)
        {
            if (Lookup(position))
            {
                m_Data[position.x][position.z] = null;
            }
            else
            {
                Assert.IsNotNull(Lookup(position));
            }
        }

        public void Clear(int x, int z)
        {
            Clear(new IntVector2(x, z));
        }
    }
    GridLookup<Structure> m_WorldLookup = new GridLookup<Structure>();
    List<Structure> m_StructureList = new List<Structure>();

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
        return new Vector3(MathUtil.RoundTo(input.x, Constants.GridSize), input.y, MathUtil.RoundTo(input.z, Constants.GridSize));
    }

    public static IntVector2 IndexFromWorld(Vector3 input)
    {
        return new IntVector2(Mathf.RoundToInt(input.x / Constants.GridSize), Mathf.RoundToInt(input.z / Constants.GridSize));
    }

    public static Vector3 GridFromIndex(IntVector2 input)
    {
        return new Vector3(input.x * Constants.GridSize, 0, input.z * Constants.GridSize);
    }

    /////////////////////////////////////////////
    // STRUCTURE PLACEMENT
    //

    public bool PlaceAttempt(Structure structure, Vector3 position, out string errorMessage)
    {
        errorMessage = null;

        IntVector2 target = IndexFromWorld(position);
        IntVector2 playerTarget = IndexFromWorld(GameObject.FindGameObjectWithTag(Tags.Player).transform.position);



        return true;
    }

    public void Remove(Vector3 position, out string errorMessage)
    {
        errorMessage = null;


        //m_StructureList.Remove(removalTarget);
        //Destroy(removalTarget.gameObject);
    }
}
