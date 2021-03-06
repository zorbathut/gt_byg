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

        // Check if the player chose a tool
        for (int i = 0; i < m_Buildables.Count; ++i)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                m_CurrentBuildable = i;
                ResyncBuildCursor();

                if (m_BuildCursor.IsDestroyTool())
                {
                    popupMessage = "Selected removal tool";
                }
                else
                {
                    popupMessage = string.Format("Selected building {0}", m_Buildables[i]);
                }
            }
        }

        // Check for rotations
        if (Input.mouseScrollDelta.y > 0)
        {
            m_BuildCursor.transform.Rotate(Vector3.up, 90);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            m_BuildCursor.transform.Rotate(Vector3.up, -90);
        }

        // Move highlight box, figure out what the user's pointing at
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(screenRay, out hit, Mathf.Infinity))
        {
            // Our target point slightly penetrates through; this is so aiming at a building actually targets a point inside the building, instead of the very edge of the building
            // Makes destroying buildings a whole lot easier
            // 0.01 meters is more than enough to avoid floating-point inaccuracy
            Vector3 targetPoint = hit.point + screenRay.direction * 0.01f;

            Vector3 targetPosition = Manager.GridFromWorld(targetPoint);
            m_BuildCursor.transform.position = targetPosition;

            m_BuildCursor.gameObject.SetActive(true);
        }
        else
        {
            m_BuildCursor.gameObject.SetActive(false);
        }

        // Actually do things
        if (Input.GetMouseButtonDown(0) && !m_BuildCursor.IsDestroyTool())
        {
            Manager.instance.AttemptPlace(m_Buildables[m_CurrentBuildable], m_BuildCursor.transform, out popupMessage);
        }
        else if (Input.GetMouseButtonDown(0) && m_BuildCursor.IsDestroyTool())
        {
            Manager.instance.AttemptRemove(m_BuildCursor.transform.position, out popupMessage);
        }

        // Show our popup message, if we have one
        if (popupMessage != null)
        {
            GameObject.FindGameObjectWithTag(Tags.UI).GetComponent<MainUI>().GetPopupText().DisplayText(popupMessage, Color.white);
            Debug.LogFormat("Message: {0}", popupMessage);
        }
    }

    void ResyncBuildCursor()
    {
        // Get rid of old cursor
        if (m_BuildCursor)
        {
            Destroy(m_BuildCursor.gameObject);
            m_BuildCursor = null;
        }

        // Create new cursor
        m_BuildCursor = Instantiate(m_Buildables[m_CurrentBuildable]);
        m_BuildCursor.gameObject.SetActive(false);  // Start disabled - on the off chance the player mashes two buttons in one frame, this will make the old one invisible

        if (!m_BuildCursor.IsDestroyTool())
        {
            // Override materials for the construction tool
            foreach (MeshRenderer renderer in m_BuildCursor.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = m_ConstructionMaterial;
            }
        }

        // Get rid of all collisions so we don't walk into our own tool
        foreach (Collider collider in m_BuildCursor.GetComponentsInChildren<Collider>())
        {
            Destroy(collider);
        }
    }
}
