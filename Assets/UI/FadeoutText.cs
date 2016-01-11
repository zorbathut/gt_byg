using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class FadeoutText : MonoBehaviour
{
    [SerializeField] AnimationCurve m_AlphaCurve;

    Color m_ColorBase;
    float m_Time;

    public void DisplayText(string text, Color color)
    {
        gameObject.SetActive(true);
        GetComponent<Text>().text = text;
        m_ColorBase = color;

        // causes us to hit 0 after the Update call; hacky, but works for now
        m_Time = -Time.deltaTime;

        Update();
    }

    public virtual void Update()
    {
        m_Time += Time.deltaTime;

        float alphaMultiple = m_AlphaCurve.Evaluate(m_Time);
        if (alphaMultiple <= 0f)
        {
            gameObject.SetActive(false);
        }

        GetComponent<Text>().color = new Color(m_ColorBase.r, m_ColorBase.g, m_ColorBase.b, m_ColorBase.a * alphaMultiple);
    }
}
