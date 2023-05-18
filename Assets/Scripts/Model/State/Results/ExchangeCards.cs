using System.Collections.Generic;
using System.Linq;
using Model.Cards;
using Util;

namespace Model.State.Results
{
    public class ExchangeCards : Result
    {
        public int cardTotal;

        public ExchangeCards(int cardTotal)
        {
            this.cardTotal = cardTotal;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var courtDeck = gameState.courtDeck.ToList();
            var cardsToMove = new List<CardId>();
            for(int i = 0; i < cardTotal; i++)
            {
                cardsToMove.Add(courtDeck.PopRandom());
            }

            return new ResultOutcome()
            {
                resultText = $"{currentPlayer.playerName} gets to exchange {cardTotal} cards.",
                edits = cardsToMove
                    .Select(_ =>
                        new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, _) as
                            GameStateEdit)
                    .Prepend(new GameStateEdit.CourtDeck(courtDeck))
                    .ToList(),
                newPhase = Logic.GetDiscardPhase(cardTotal, gameState, config.cardDatabase),
            };
        }
    }
}