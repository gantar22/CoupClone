using System.Collections.Generic;
using System.Linq;
using Model.Cards;
using UnityEngine;

namespace UI
{
    public class OpponentInfoUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text m_OpponentNameText = default;
        [SerializeField] private GameObject m_TurnIndicator = default;

        public void SetOpponentInfo(
            bool isCurrentTurn,
            string opponentName,
            int coinCoint,
            IEnumerable<(CardData card, bool isFaceDown)> cards)
        {
            m_OpponentNameText.text =
                $"Name {opponentName}\nCards: {cards.Count()}\nCoins: {coinCoint}\nRevealedInfluences:\n{string.Join("\n", cards.Where(x => !x.isFaceDown).Select(x => x.card.cardName))}";
            m_TurnIndicator.SetActive(isCurrentTurn);
        }
    }
}
