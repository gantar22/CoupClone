using System.Collections.Generic;
using Model.Cards;

namespace Model.State.Results
{
    public class ActionChallengeSucceeds : Result
    {
        public int revealedCardIndex; // revealed by the current player, not the challenging player
        public CardId claimedCard;
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardDatabase = config.cardDatabase;
            var revealedCard = cardDatabase.GetCard(currentPlayer.cards[revealedCardIndex].id);
            var claimedCardName = cardDatabase.GetCard(claimedCard).cardName;
            return new ResultOutcome()
            {
                resultText = $"{currentPlayer.playerName} didn't reveal {claimedCardName}, they revealed {revealedCard.cardName} instead.",
                edits = new List<GameStateEdit>()
                {
                    new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, revealedCardIndex),
                },
                newPhase = null, // new turn because the challenge succeeded
            };
        }
    }
}