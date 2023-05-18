using System.Collections.Generic;
using System.Linq;
using Model.Actions;
using Util;

namespace Model.State.Results
{
    public class ActionChallengeFails : Result
    {
        public int revealedCardIndex;
        public int playerThatChallenged;
        public ActionData proposedAction;
        public int? targetPlayer;

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardDatabase = config.cardDatabase;
            var newCourtDeck = gameState.courtDeck.ToList();
            var newCard = newCourtDeck.PopRandom();
            var claimedCardId = currentPlayer.cards[revealedCardIndex].id;
            var claimedCard = cardDatabase.GetCard(claimedCardId);
            var challenger = gameState.playerStates[playerThatChallenged];
            
            // Card gets revealed and swapped, then the challenger must reveal a card
            return new ResultOutcome()
            {
                resultText = $"{currentPlayer.playerName} revealed the claimed card: {claimedCard.cardName}.",
                edits = new List<GameStateEdit>()
                {
                    new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, revealedCardIndex),
                    new GameStateEdit.PlayerStateEdit.RemoveCard(gameState.currentPlayersTurn, revealedCardIndex),
                    new GameStateEdit.CourtDeck(newCourtDeck),
                    new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, newCard),
                },
                newPhase = new Phase
                (
                    choosingPlayer: playerThatChallenged,
                    text:
                    $"{currentPlayer.playerName} wasn't bluffing, they had {claimedCard.cardName}. Now you have to lose an influence.",
                    choices: challenger.cards
                        .Select((_, i) => (card: _, index: i))
                        .Where(_ => _.card.isFaceDown)
                        .Select(_ =>
                        {
                            var revealedCard = cardDatabase.GetCard(_.card.id);

                            Result nextResultArgs;
                            if (targetPlayer.HasValue)
                            {
                                nextResultArgs = new DecisionToBlock()
                                {
                                    targetedPlayer = targetPlayer.Value,
                                    action = proposedAction,
                                };
                            }
                            else
                            {
                                nextResultArgs = Logic.ActionOutCome(proposedAction,null);
                            }
                            
                            var nextResult = nextResultArgs.GetResult(gameState,config); // peek ahead by one
                            nextResult.edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(playerThatChallenged, _.index));
                            
                            return new Choice
                            (
                                title: $"Reveal {revealedCard.cardName}",
                                description: "It will be useless to you afterwards",
                                justification: Choice.Justification.Free,
                                onChosen: new GenericResult() { result = nextResult }
                            );
                        })
                )
            };
        }
    }
}