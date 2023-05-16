using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    
    [SerializeField] private TMPro.TMP_Text m_LogTextTemplate = default;
    
    [SerializeField] private Transform m_LogTextParent = default;
    [SerializeField] private ScrollRect m_ScrollRect = default;

    public void Refresh()
    {
        foreach(Transform child in m_LogTextParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AppendText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }
        var logText = Instantiate(m_LogTextTemplate, m_LogTextParent);
        logText.text = text;
        m_ScrollRect.verticalNormalizedPosition = -1;
    }
}
