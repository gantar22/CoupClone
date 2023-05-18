using System.Linq;
using Model.Actions;
using Model.Cards;

namespace Model.State.Results
{
    public class ActionChallenged : Result
    {
        public int challengingPlayer;
        public ActionData challengedAction;
        public CardId claimedCardId;
        public int? targetPlayer;

        public ActionChallenged(int challengingPlayer, ActionData challengedAction, CardId claimedCardId, int? targetPlayer)
        {
            this.challengingPlayer = challengingPlayer;
            this.challengedAction = challengedAction;
            this.claimedCardId = claimedCardId;
            this.targetPlayer = targetPlayer;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardDatabase = config.cardDatabase;
            var challengersState = gameState.playerStates[challengingPlayer];
            var claimedCard = cardDatabase.GetCard(claimedCardId);
                
            var nextPhase = new Phase
            (
                text: $"{challengersState.playerName} is challenging your [{challengedAction.name}] with {claimedCard.cardName}",
                choosingPlayer: gameState.currentPlayersTurn,
                choices: currentPlayer.cards.Select((_, i) => (card: _, index: i))
                    .Where(_ => _.card.isFaceDown)
                    .Select(_ =>
                    {
                        var card = cardDatabase.GetCard(_.card.id);
                        var cardIndex = _.index;

                        if (card.id != claimedCardId)
                        {
                            return new Choice
                            (
                                title: $"Reveal {card.cardName}",
                                description: $"Reveal {card.cardName}. It will be useless to you afterwards",
                                justification: Choice.Justification.Free,
                                onChosen: new ActionChallengeSucceeds
                                (
                                    claimedCard: claimedCardId,
                                    revealedCardIndex: cardIndex
                                )
                            );
                        }                                
                        return new Choice()
                        {

                            title = $"Reveal {card.cardName}",
                            description =
                                $"Reveal {card.cardName} to prove you have it. You'll get a random replacement afterwards.",
                            justification = Choice.Justification.Free,
                            onChosen = new ActionChallengeFails
                            (
                                playerThatChallenged :challengingPlayer,
                                proposedAction: challengedAction,
                                revealedCardIndex: cardIndex,
                                targetPlayer: targetPlayer
                            )
                        };
                    })
            );
            return new ResultOutcome()
            {
                resultText = $"{challengersState.playerName} is challenging {currentPlayer.playerName}'s [{challengedAction.name}] with {claimedCard.cardName}",
                edits = s_NoEdits,
                newPhase = nextPhase, 
            };
        }
    }
}