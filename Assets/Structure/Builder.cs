using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [SerializeField] List<Structure> m_Buildables;

    [SerializeField] Material m_ConstructionMaterial;

    int m_CurrentBuildable = 1;
    Structure m_BuildCursor;

    protected virtual void Awake()
    {
        ResyncBuildCursor();
    }

    protected virtual void Update()
    {
        string popupMessage = null;

        for (int i = 0; i < m_Buildables.Count; ++i)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                m_CurrentBuildable = i;
                ResyncBuildCursor();

                if (m_Buildables[i])
                {
                    popupMessage = string.Format("Selected building {0}", m_Buildables[i]);
                }
                else
                {
                    popupMessage = "Selected removal tool";
                }
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

        if (Input.GetMouseButtonDown(0) && !m_BuildCursor.GetDestroyTool())
        {
            Manager.instance.AttemptPlace(m_Buildables[m_CurrentBuildable], m_BuildCursor.transform, out popupMessage);
        }
        else if (Input.GetMouseButtonDown(0) && m_BuildCursor.GetDestroyTool())
        {
            Manager.instance.AttemptRemove(m_BuildCursor.transform.position, out popupMessage);
        }

        if (popupMessage != null)
        {
            GameObject.FindGameObjectWithTag(Tags.UI).GetComponent<MainUI>().GetPopupText().DisplayText(popupMessage, Color.white);
            Debug.LogFormat("Error: {0}", popupMessage);
        }
    }

    void ResyncBuildCursor()
    {
        if (m_BuildCursor)
        {
            Destroy(m_BuildCursor.gameObject);
            m_BuildCursor = null;
        }

        m_BuildCursor = Instantiate(m_Buildables[m_CurrentBuildable]);

        if (!m_BuildCursor.GetDestroyTool())
        {
            // Set all materials
            foreach (MeshRenderer renderer in m_BuildCursor.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = m_ConstructionMaterial;
            }
        }

        foreach (Collider collider in m_BuildCursor.GetComponentsInChildren<Collider>())
        {
            Destroy(collider);
        }
    }
}
