using System;
using System.Collections.Generic;

namespace Model.State.Results
{
    public class TakeCoins : Result
    {
        public int amount;

        public TakeCoins(int amount)
        {
            this.amount = amount;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var coinsToTake = Math.Min(gameState.treasuryCoinCount, amount);
            return new ResultOutcome()
            {
                resultText = $"{currentPlayer.playerName} took {coinsToTake} coin(s) from the treasury.",
                edits = new List<GameStateEdit>()
                {
                    new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount - coinsToTake),
                    new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                        gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                },
                newPhase = null, // end turn on completion
            };
        }
    }
}