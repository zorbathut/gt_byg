using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CursorLock : MonoBehaviour
{
    bool m_LockCursor = true;

    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_LockCursor = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_LockCursor = true;
        }

        if (m_LockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
