using System.Collections.Generic;
using System.Linq;

namespace Model.State.Results
{
    public class DiscardedFromExchange : Result
    {
        public int cardIndex;
        public int cardsLeftToDiscard;

        public DiscardedFromExchange(int cardIndex, int cardsLeftToDiscard)
        {
            this.cardIndex = cardIndex;
            this.cardsLeftToDiscard = cardsLeftToDiscard;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var newCourtDeck = gameState.courtDeck.ToList();
            newCourtDeck.Add(currentPlayer.cards[cardIndex].id);
            return new ResultOutcome()
            {
                resultText = $"{gameState.currentPlayersTurn} discarded a card",
                edits = new List<GameStateEdit>
                {
                    new GameStateEdit.PlayerStateEdit.RemoveCard(
                        gameState.currentPlayersTurn, cardIndex),
                    new GameStateEdit.CourtDeck(newCourtDeck),
                },
                newPhase = Logic.GetDiscardPhase(cardsLeftToDiscard, gameState, config.cardDatabase),
            };
        }
    }
}