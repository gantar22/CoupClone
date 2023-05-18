using System;
using System.Collections.Generic;

namespace Model.State.Results
{
    public class StealCoins : Result
    {
        public int targetPlayer;
        public int amount;

        public StealCoins(int targetPlayer, int amount)
        {
            this.targetPlayer = targetPlayer;
            this.amount = amount;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var coinsToTake = Math.Min(gameState.playerStates[targetPlayer].coinCount, amount);
            return new ResultOutcome
            {
                resultText = $"{currentPlayer.playerName} took {coinsToTake} coin(s) from {gameState.playerStates[targetPlayer].playerName}.",
                edits = new List<GameStateEdit>()
                {
                    new GameStateEdit.PlayerStateEdit.CoinCount(targetPlayer,gameState.playerStates[targetPlayer].coinCount - coinsToTake),
                    new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                        gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                },
                newPhase = null, // end turn on completion
            };
        }
    }
}