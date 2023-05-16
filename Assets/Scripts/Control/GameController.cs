using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Cards;
using Model.State;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using Util;

namespace Control
{
    public class GameController : MonoBehaviour
    {
        private const int playerId = 0;
        
        [SerializeField] private UIManager m_UIManager = default;
        [SerializeField] private GameConfig m_GameConfig = default;
        
        GameState m_CurrentGameState;

        private void Start()
        {
            m_UIManager.Init(NewGame);
        }

        void NewGame()
        {
            GameState gameState = new GameState()
            {
                courtDeck = m_GameConfig.cardDatabase.cardMap.SelectMany(_=>Enumerable.Repeat(_.Key,m_GameConfig.cardRepeatCount)).ToList(),
                currentPlayersTurn = 0,
                playerStates = new PlayerState[m_GameConfig.aiPlayerNames.Length + 1],
                treasuryCoinCount = 50,
            };
            for (int i = 0; i < m_GameConfig.aiPlayerNames.Length + 1; i++)
            {
                gameState.playerStates[i] = new PlayerState()
                {
                    playerName = i == playerId ? "Player" : m_GameConfig.aiPlayerNames[i - 1],
                    coinCount = 2,
                    cards = new List<(CardId id,bool isFaceDown)>(),
                };
                for (int j = 0; j < m_GameConfig.cardsPerPlayer; j++)
                {
                    var card = gameState.courtDeck.PopRandom();
                    gameState.playerStates[i].cards.Add((card, true));
                }
            }

            m_CurrentGameState = gameState;
            m_UIManager.Refresh();
            m_UIManager.UpdateGameState(m_CurrentGameState, m_GameConfig, 0);
            
            // First Turn
            var initialPhase = PhaseGeneration.GenerateInitialPhase(m_CurrentGameState, m_GameConfig);
            SetPhase(initialPhase);
        }

        void SetPhase(Phase phase)
        {
            m_UIManager.UpdateGameState(m_CurrentGameState, m_GameConfig, playerId);
            if (phase.choosingPlayer == playerId)
            {
                m_UIManager.SetPlayerPhase(phase, HandleChoice);
                return;
            }
            
            m_UIManager.SetAIPhase(m_CurrentGameState,phase);
            StartCoroutine(AIChoice(phase));
        }

        IEnumerator AIChoice(Phase phase)
        {
            yield return new WaitForSeconds(1);

            var bluffs = phase.choices.Where(_ => _.justification == Choice.Justification.Bluff).ToArray();
            var nonbluffs = phase.choices.Where(_ => _.justification != Choice.Justification.Bluff).ToArray();
            if (bluffs.Any() && UnityEngine.Random.value > m_GameConfig.aiBluffProbability)
            {// The AI will bluff
                var selection = UnityEngine.Random.Range(0, bluffs.Length);
                HandleChoice(bluffs[selection]);
            }
            else
            {// The AI will not bluff
                var selection = UnityEngine.Random.Range(0, nonbluffs.Length);
                HandleChoice(nonbluffs[selection]);
            }
        }


        static bool IsTheGameOver(GameState gameState, out int winningPlayer)
        {
            (PlayerState player, int playerIndex)[] alivePlayers = gameState.playerStates
                .Select(((player, i) => (player,i)))
                .Where(_ => _.player.cards.Any(_ => _.isFaceDown)).ToArray();
            if (alivePlayers.Length == 1)
            {
                winningPlayer = alivePlayers[0].playerIndex;
                return true;
            }

            winningPlayer = -1;
            return false;
        }

        void HandleChoice(Choice choice)
        {
            var result = choice.result();
            StartCoroutine(HandleResult(result));
        }
        IEnumerator HandleResult(Choice.Result result)
        {
            // perform edits and check for winner
            foreach (var edit in result.edits)
            {
                var log = LogGameStateEdit.GetLogFromEdit(m_CurrentGameState,m_GameConfig, edit);
                m_UIManager.PushLog(log);
                
                m_CurrentGameState = PerformGameStateEdit.PerformEdit(m_CurrentGameState, edit);
                
                // if we had animations from m_UIManager for each edit we could play them here, but we'd probably want run them concurrently and wait for them to have all finished
                
                if (IsTheGameOver(m_CurrentGameState, out var winningPlayer))
                {
                    m_UIManager.PushLog($"Player {winningPlayer} wins!");
                    m_UIManager.EndGame(m_CurrentGameState, winningPlayer, NewGame);
                    yield break;
                }
            }
            // display result text and wait
            m_UIManager.SetResultText(result.resultText);
            m_UIManager.ClearPhaseText();
            yield return new WaitForSeconds(m_GameConfig.turnWaitTime);

            Phase nextPhase;
            if (!result.newPhase.HasValue)
            {
                // move current player
                do
                {
                    m_CurrentGameState.currentPlayersTurn++;
                    m_CurrentGameState.currentPlayersTurn %= m_CurrentGameState.playerStates.Length;
                } while (!m_CurrentGameState.playerStates[m_CurrentGameState.currentPlayersTurn].cards.Any(_=>_.isFaceDown));
                
                nextPhase = PhaseGeneration.GenerateInitialPhase(m_CurrentGameState, m_GameConfig);
            }
            else
            {
                if (result.newPhase.Value.choosingPlayer == playerId) // Hide result text when the player is directly responding
                    m_UIManager.SetResultText("");
                nextPhase = result.newPhase.Value;
            }
            SetPhase(nextPhase);
        }
    }
}
