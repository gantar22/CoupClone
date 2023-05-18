using Model.Actions;
using Model.Cards;

namespace Model.State.Results
{
    public class BlockAttempted : Result
    {
        public int targetedPlayer;
        public ActionData actionToBlock;
        public CardId cardClaimedToBlock;

        public BlockAttempted(int targetedPlayer, ActionData actionToBlock, CardId cardClaimedToBlock)
        {
            this.targetedPlayer = targetedPlayer;
            this.actionToBlock = actionToBlock;
            this.cardClaimedToBlock = cardClaimedToBlock;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardDatabase = config.cardDatabase;
            var targetedPlayerName = gameState.playerStates[targetedPlayer].playerName;
            var cardUsedToBlock = cardDatabase.GetCard(cardClaimedToBlock).name;
            return new ResultOutcome()
            {
                resultText =
                    $"{targetedPlayerName} claims to block {currentPlayer.playerName}'s [{actionToBlock.name}] block with {cardUsedToBlock}",
                edits = s_NoEdits,
                newPhase = new Phase
                (
                    choosingPlayer: gameState.currentPlayersTurn,
                    text:
                    $"{targetedPlayerName} is blocking [{actionToBlock.name}] as {cardUsedToBlock}.\nDo you challenge?",
                    choices: new[]
                    {
                        new Choice
                        (
                            title: "Pass",
                            description: "Allow the block and pass your turn",
                            justification: Choice.Justification.Free,
                            onChosen: new BlockNotChallenged(blockingPlayer: targetedPlayer)
                        ),
                        new Choice
                        (
                            title: "Challenge",
                            description: $"Challenge {targetedPlayerName}'s claim to be blocking as {cardUsedToBlock}",
                            justification: Choice.Justification.Free,
                            onChosen: new BlockChallenged
                            (
                                actionBeingBlocked: actionToBlock,
                                cardBeingChallenged: cardClaimedToBlock,
                                challengedBlocker: targetedPlayer
                            )
                        ),
                    }
                ),
            };
        }
    }
}