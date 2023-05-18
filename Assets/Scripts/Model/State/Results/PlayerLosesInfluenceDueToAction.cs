using System.Collections.Generic;

namespace Model.State.Results
{
    public class PlayerLosesInfluenceDueToAction : Result
    {
        public int targetPlayer;
        public int cardIndex;

        public PlayerLosesInfluenceDueToAction(int targetPlayer, int cardIndex)
        {
            this.targetPlayer = targetPlayer;
            this.cardIndex = cardIndex;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var targetPlayerState = gameState.playerStates[targetPlayer];
            var revealedCard = config.cardDatabase.GetCard(gameState.playerStates[targetPlayer].cards[cardIndex].id);
                
            return new ResultOutcome()
            {
                resultText = $"{targetPlayerState.playerName} revealed {revealedCard.cardName}.",
                edits = new List<GameStateEdit>
                {
                    new GameStateEdit.PlayerStateEdit.RevealCard(targetPlayer, cardIndex),
                },
                newPhase = null,
            };
        }
    }
}