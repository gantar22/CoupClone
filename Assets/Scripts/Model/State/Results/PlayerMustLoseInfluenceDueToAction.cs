using System.Linq;

namespace Model.State.Results
{
    public class PlayerMustLoseInfluenceDueToAction : Result
    {
        public int targetPlayer;

        public PlayerMustLoseInfluenceDueToAction(int targetPlayer)
        {
            this.targetPlayer = targetPlayer;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            return new ResultOutcome()
            {
                resultText =
                    $"{gameState.playerStates[targetPlayer].playerName} must now lose influence.",
                edits = s_NoEdits,
                newPhase = new Phase
                (
                    text: "Select a card to reveal",
                    choosingPlayer: targetPlayer,
                    choices: gameState.playerStates[targetPlayer]
                        .cards
                        .Select((card, index) => (card, index))
                        .Where(_ => _.card.isFaceDown)
                        .Select(_ =>
                        {
                            var card = _.card;
                            var cardIndex = _.index;
                            return new Choice
                            (
                                title: config.cardDatabase.GetCard(card.id).cardName,
                                description:
                                $"Reveal and lose the power of {config.cardDatabase.GetCard(card.id).cardName}",
                                justification: Choice.Justification.Free,
                                onChosen: new PlayerLosesInfluenceDueToAction
                                (
                                    cardIndex: cardIndex,
                                    targetPlayer: targetPlayer
                                )
                            );
                        })
                ),
            };
        }
    }
}