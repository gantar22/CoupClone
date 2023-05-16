using System.Collections.Generic;
using System.Linq;
using Model.Cards;
using UnityEngine;

namespace UI
{
    public class PlayerHandUI : MonoBehaviour
    {
        [SerializeField] private CardUI m_CardUITemplate = default;
        
        [SerializeField] Transform m_ActiveCardContainer = default;
        [SerializeField] Transform m_RevealedCardContainer = default;
        
        public void SetCardState(IEnumerable<CardData> activeCards, IEnumerable<CardData> revealedCards)
        {
            foreach (Transform child in m_ActiveCardContainer)
            {
                Destroy(child.gameObject);  // todo: object pooling
            }
            foreach (Transform child in m_RevealedCardContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var card in activeCards.Take(2)) // only show 2 cards max, todo: make better UI for this
            {
                var cardUI = Instantiate(m_CardUITemplate, m_ActiveCardContainer);
                cardUI.SetCard(card);
            }
            foreach (var card in revealedCards.Take(2))
            {
                var cardUI = Instantiate(m_CardUITemplate, m_RevealedCardContainer);
                cardUI.SetCard(card);
            }
        }
    }
}
