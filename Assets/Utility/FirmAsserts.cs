using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirmAsserts : MonoBehaviour
{
    #if UNITY_EDITOR
    HashSet<string> m_Ignored = new HashSet<string>();

    bool m_WasPlaying;

    public void OnEnable()
    {
        Application.logMessageReceived += Logger;
        EditorApplication.playmodeStateChanged += StateChange;
    }

    void StateChange()
    {
        if (m_WasPlaying && !EditorApplication.isPlaying)
        {
            m_Ignored.Clear();
        }

        m_WasPlaying = EditorApplication.isPlaying;
    }

    void Logger(string text, string stackTrace, LogType type)
    {
        if (EditorApplication.isPlaying && type == LogType.Assert)
        {
            string signature = stackTrace;
            if (!m_Ignored.Contains(signature))
            {
                // No good way to reset these afterwards, so we just clobber the old values - this actually still breaks a bit, unfortunately
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                bool result = EditorUtility.DisplayDialog("Assert", text + "\n" + stackTrace, "Skip", "Skip Forever");
                if (result == false)
                {
                    m_Ignored.Add(signature);
                }
            }
        }
    }
    #endif
}
