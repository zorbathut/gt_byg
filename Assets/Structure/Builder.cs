using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [SerializeField] List<Structure> m_Buildables;

    int m_CurrentBuildable = 1;
    Structure m_BuildCursor;

    protected virtual void Awake()
    {
        ResyncBuildCursor();
    }

    protected virtual void Update()
    {
        for (int i = 0; i < m_Buildables.Count; ++i)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                m_CurrentBuildable = i;
                ResyncBuildCursor();
            }
        }

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

        string errorMessage = null;

        if (Input.GetMouseButtonDown(0))
        {
            Manager.instance.Place(m_Buildables[m_CurrentBuildable], m_BuildCursor.transform, out errorMessage);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Manager.instance.Remove(m_BuildCursor.transform.position, out errorMessage);
        }

        if (errorMessage != null)
        {
            Debug.LogFormat("Error: {0}", errorMessage);
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
