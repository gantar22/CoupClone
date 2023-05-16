using Model.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private TMP_Text m_CardTitleText;
        [SerializeField] private TMP_Text m_ActionTitleText;
        [SerializeField] private TMP_Text m_ActionDescriptionText;
    
        public void SetCard(CardData card)
        {
            m_Image.color = card.baseColor;
            m_CardTitleText.text = card.cardName;
            if (card.action.HasValue)
            {
                m_ActionTitleText.text = card.action.Value.name;
                m_ActionDescriptionText.text = card.action.Value.description;
            }
            else
            {
                m_ActionTitleText.text = "";
                m_ActionDescriptionText.text = "";
            }
        }
    }
}
