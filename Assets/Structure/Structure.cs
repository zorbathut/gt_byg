using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] Transform m_Occupied;
    [SerializeField] Transform m_Doorway;

    public void Initialize(Structure template, IntVector2 origin)
    {
        
    }

    public virtual void OnDrawGizmos()
    {
        //GizmoUtility.DrawSquare(new Vector3(transform.position.x - (Constants.GridSize / 2) * m_Width, 0f, transform.position.z - (Constants.GridSize / 2) * m_Length), new Vector3(transform.position.x + (Constants.GridSize / 2) * m_Width, 0f, transform.position.z + (Constants.GridSize / 2) * m_Length));
    }
}
