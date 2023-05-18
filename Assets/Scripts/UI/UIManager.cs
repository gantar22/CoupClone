using System;
using System.Linq;
using Model;
using Model.State;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PlayerCoinsUI m_PlayerCoinsUI = default;
        [SerializeField] private PlayerHandUI m_PlayerHandUI = default;
        [SerializeField] private BoardStatusUI m_BoardStatusUI = default;
        [SerializeField] private OpponentsUI m_OpponentsUI = default;
        [SerializeField] private PhaseUI m_PhaseUI = default;
        [SerializeField] private LogUI m_LogUI = default;
        [SerializeField] private NewGameUI m_NewGameUI = default;
        [SerializeField] private PlayerTurnUI m_PlayerTurnUI = default;

        public void Refresh()
        {
            m_LogUI.Refresh();
        }
        
        public void UpdateGameState(GameState gameState, GameConfig config, int userPlayer)
        {
            var playerState = gameState.playerStates[userPlayer]; 
            m_PlayerTurnUI.SetPlayersTurn(gameState.currentPlayersTurn == userPlayer);
            m_PlayerCoinsUI.SetCoinCount(playerState.coinCount);
            m_PlayerHandUI.SetCardState(
                playerState.cards.Where(_=>_.isFaceDown).Select(_=>config.cardDatabase.GetCard(_.id)), 
                playerState.cards.Where(_=>!_.isFaceDown).Select(_=>config.cardDatabase.GetCard(_.id)));
            m_BoardStatusUI.SetStatus(gameState.treasuryCoinCount,gameState.courtDeck.Count);
            var opponentStates = gameState.playerStates
                .Select((_, i) => (_, gameState.currentPlayersTurn == i))
                .Where((_, i) => i != userPlayer);
            m_OpponentsUI.SetOpponentState(opponentStates,config.cardDatabase);
        }
        
        public void SetPlayerPhase(Phase phase, Action<Choice> inOnSelect)
        {
            m_PhaseUI.SetPlayerPhase(phase, inOnSelect);
        }

        public void SetAIPhase(GameState gameState, Phase phase)
        {
            m_PhaseUI.SetAIPhase(gameState.playerStates[phase.choosingPlayer].playerName);
        }
        
        public void PushLog(string log)
        {
            m_LogUI.AppendText(log);
        }

        public void EndGame(GameState gameState, int winningPlayer, Action onReset)
        {
            m_NewGameUI.Setup($"{gameState.playerStates[winningPlayer].playerName} won!\nPlay again?", onReset);
        }

        public void Init(Action onStart)
        {
            m_NewGameUI.Setup("New Game",onStart);
        }

        public void SetResultText(string resultResultText)
        {
            m_PhaseUI.SetResultText(resultResultText);
        }

        public void ClearPhaseText()
        {
            m_PhaseUI.ClearPhaseText();
        }
    }
}
