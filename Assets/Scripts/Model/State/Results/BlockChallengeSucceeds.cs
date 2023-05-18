using Model.Actions;
using Model.Cards;

namespace Model.State.Results
{
    public class BlockChallengeSucceeds : Result
    {
        public int challengedBlocker;
        public ActionData actionNotBlocked;
        public CardId cardClaimedToBlock;
        public int revealedCardIndex;

        public BlockChallengeSucceeds(int challengedBlocker, ActionData actionNotBlocked, CardId cardClaimedToBlock, int revealedCardIndex)
        {
            this.challengedBlocker = challengedBlocker;
            this.actionNotBlocked = actionNotBlocked;
            this.cardClaimedToBlock = cardClaimedToBlock;
            this.revealedCardIndex = revealedCardIndex;
        }

        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var cardDatabase = config.cardDatabase;
            var cardClaimedToBlockName = cardDatabase.GetCard(cardClaimedToBlock).cardName;
            var revealedCard = cardDatabase.GetCard(gameState.playerStates[challengedBlocker].cards[revealedCardIndex].id).cardName;
            var targetedPlayer = gameState.playerStates[challengedBlocker];
                
            var actionResult = Logic.ActionOutCome(actionNotBlocked,challengedBlocker).GetResult(gameState,config);
            actionResult.edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(
                challengedBlocker,
                revealedCardIndex));
            actionResult.resultText = 
                $"{targetedPlayer.playerName} didn't reveal {cardClaimedToBlockName}, but instead gave up {revealedCard}\n" 
                + actionResult.resultText;
            return actionResult;
        }
    }
}