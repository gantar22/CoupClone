namespace Model.State.Results
{
    public class BlockNotChallenged : Result
    {
        public int blockingPlayer;
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            return new ResultOutcome()
            {
                resultText =
                    $"{currentPlayer.playerName} decided not to challenge {gameState.playerStates[blockingPlayer].playerName}'s block.",
                edits = s_NoEdits,
                newPhase = null, // end turn
            };
        }
    }
}