using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [SerializeField]
    List<Structure> m_Buildables;

    int m_CurrentBuildable = 0;
    Structure m_BuildCursor;

    protected virtual void Awake()
    {
        ResyncBuildCursor();
    }

    protected virtual void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            m_BuildCursor.transform.Rotate(Vector3.up, 90);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            m_BuildCursor.transform.Rotate(Vector3.up, -90);
        }

        // Move highlight box, figure out what the user's pointing at
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << Layers.BuildTarget))
        {
            Vector3 targetPosition = Manager.GridFromWorld(hit.point);
            m_BuildCursor.transform.position = targetPosition;
        }
    }

    void ResyncBuildCursor()
    {
        if (m_BuildCursor)
        {
            Destroy(m_BuildCursor.gameObject);
        }

        m_BuildCursor = Instantiate(m_Buildables[m_CurrentBuildable]);

        foreach (Collider collider in m_BuildCursor.GetComponentsInChildren<Collider>())
        {
            Destroy(collider);
        }
    }
}
