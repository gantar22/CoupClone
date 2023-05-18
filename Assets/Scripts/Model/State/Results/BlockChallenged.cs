using System.Collections.Generic;
using System.Linq;
using Model.Actions;
using Model.Cards;

namespace Model.State.Results
{
    public class BlockChallenged : Result
    {
        public int challengedBlocker;
        public ActionData actionBeingBlocked;
        public CardId cardBeingChallenged;

        public BlockChallenged(int challengedBlocker, ActionData actionBeingBlocked, CardId cardBeingChallenged)
        {
            this.challengedBlocker = challengedBlocker;
            this.actionBeingBlocked = actionBeingBlocked;
            this.cardBeingChallenged = cardBeingChallenged;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardDatabase = config.cardDatabase;
            var cardUsedToBlock = cardDatabase.GetCard(cardBeingChallenged).name;
            var blockingPlayer = gameState.playerStates[challengedBlocker];
            return new ResultOutcome()
            {
                resultText =
                    $"{currentPlayer.playerName} challenges {blockingPlayer.playerName}'s claim to block as {cardUsedToBlock}",
                edits = new List<GameStateEdit>(),
                newPhase = new Phase
                (
                    text:
                    $"{gameState.playerStates[gameState.currentPlayersTurn].playerName} challenged your block.",
                    choosingPlayer: challengedBlocker,
                    // one choice for each card that could be revealed
                    // if it's the claimed blocker, then it gets recycled and the block is successful
                    // otherwise we just reveal the card and the action goes through
                    choices: blockingPlayer.cards
                        .Select((card, index) => (card, index))
                        .Where(_ => _.card.isFaceDown)
                        .Select(_ => (card: cardDatabase.GetCard(_.card.id), _.index))
                        .Select(cardData =>
                        {
                            if (cardData.card.id == cardBeingChallenged)
                            {
                                return new Choice
                                (
                                    title: $"Reveal {cardData.card.cardName}",
                                    description:
                                    $"Reveal {cardData.card.cardName} to prove your innocence. You'll get a random replacement in exchange.",
                                    justification: Choice.Justification.Free,
                                    onChosen: new BlockChallengeFails
                                    (
                                        actionBlocked: actionBeingBlocked,
                                        blockingPlayer: challengedBlocker,
                                        revealedCardIndex: cardData.index
                                    )
                                );
                            }

                            return new Choice
                            (
                                title: $"Reveal {cardData.card.cardName}",
                                description: $"Reveal {cardData.card.cardName}. It will be useless to you afterwards.",
                                justification: Choice.Justification.Free,
                                onChosen: new BlockChallengeSucceeds
                                (
                                    actionNotBlocked: actionBeingBlocked,
                                    cardClaimedToBlock: cardBeingChallenged,
                                    challengedBlocker: challengedBlocker,
                                    revealedCardIndex: cardData.index
                                )
                            );
                        })
                )
            };
        }
    }
}