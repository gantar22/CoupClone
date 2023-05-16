using System;
using Model.State;
using UnityEngine;

namespace UI
{
    public class PhaseUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text m_TitleText = default;
        [SerializeField] private Transform m_ChoiceContainer = default;
        [SerializeField] private ChoiceUI m_ChoiceTemplate = default;
        [SerializeField] private TMPro.TMP_Text m_ResultText = default;

        public void SetPlayerPhase(Phase phase, Action<Choice> inOnSelect)
        {
            m_TitleText.text = phase.text;
            foreach (Transform child in m_ChoiceContainer)
            {
                Destroy(child.gameObject);  // todo: object pooling
            }
            
            foreach (var choice in phase.choices)
            {
                var choiceUI = Instantiate(m_ChoiceTemplate, m_ChoiceContainer);
                choiceUI.SetChoice(choice, selected =>
                {
                    m_TitleText.text = "";
                    foreach (Transform child in m_ChoiceContainer)
                    {
                        Destroy(child.gameObject);  // todo: object pooling
                    }
                    inOnSelect(selected);
                });
            }
        }
        
        public void SetAIPhase(string currentAIName)
        {            
            m_TitleText.text = $"{currentAIName} is deciding...";
            foreach (Transform child in m_ChoiceContainer)
            {
                Destroy(child.gameObject);  // todo: object pooling
            }
        }
        
        public void SetResultText(string result)
        {
            m_ResultText.text = result;
        }

        public void ClearPhaseText()
        {
            m_TitleText.text = "";
        }
    }
}
