using System.Collections.Generic;
using System.Linq;
using Model.Cards;
using Model.State;
using UnityEngine;

namespace UI
{
    public class OpponentsUI : MonoBehaviour
    {
        [SerializeField] private OpponentInfoUI m_OpponentUITemplate = default;
        
        [SerializeField] Transform m_OpponentContainer = default;
        
        public void SetOpponentState(IEnumerable<(PlayerState player,bool isCurrentTurn)> opponentStates,CardDatabase cardDatabase)
        {
            foreach (Transform child in m_OpponentContainer)
            {
                Destroy(child.gameObject);  // todo: object pooling
            }

            foreach (var (opponentState,currentTurn) in opponentStates)
            {
                var opponentUI = Instantiate(m_OpponentUITemplate, m_OpponentContainer);
                opponentUI.SetOpponentInfo(
                    currentTurn,
                    opponentState.playerName,
                    opponentState.coinCount,
                    opponentState.cards.Select(_=>(cardDatabase.GetCard(_.id),_.isFaceDown)));
            }
        }
    }
}
