using System;
using Model.State;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChoiceUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text m_TitleText = default;
        [SerializeField] private TMPro.TMP_Text m_DescriptionText = default;
        [SerializeField] private Button m_SelectButton = default;
        [SerializeField] private TMPro.TMP_Text m_ButtonText = default;

        public void SetChoice(Choice inChoice, Action<Choice> inOnSelect)
        {
            m_TitleText.text = inChoice.title;
            m_DescriptionText.text = inChoice.description;
            m_SelectButton.onClick.RemoveAllListeners();
            m_SelectButton.onClick.AddListener(() => inOnSelect(inChoice));
        }
    }
}
