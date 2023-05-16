using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI
{
    public class NewGameUI : MonoBehaviour
    {
        [SerializeField] private GameObject m_ParentObject = default;
        [SerializeField] private TMPro.TMP_Text m_TitleText = default;
        [SerializeField] private Button m_GoButton = default;

        public void Setup(string title, Action onGo)
        {
            m_ParentObject.SetActive(true);
            m_TitleText.text = title;
            m_GoButton.onClick.RemoveAllListeners();
            m_GoButton.onClick.AddListener(() =>
            {
                m_ParentObject.SetActive(false);
                onGo();
            });
        }
    }
}
