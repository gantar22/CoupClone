using System.Collections.Generic;
using System.Linq;
using Model.Actions;
using Util;

namespace Model.State.Results
{
    public class BlockChallengeFails : Result
    {
        public int blockingPlayer;
        public ActionData actionBlocked;
        public int revealedCardIndex;
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var cardDatabase = config.cardDatabase;
            var blockingPlayerState = gameState.playerStates[blockingPlayer];
            var revealedCard = cardDatabase.GetCard(gameState.playerStates[blockingPlayer].cards[revealedCardIndex].id);
            var edits = new List<GameStateEdit>();
            // lose the old card
            edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(blockingPlayer, revealedCardIndex)); // to notify other players
            edits.Add(new GameStateEdit.PlayerStateEdit.RemoveCard(blockingPlayer, revealedCardIndex));
                
            // get a new card
            var newCourtDeck = gameState.courtDeck.ToList();
            var newCard = newCourtDeck.PopRandom();
            edits.Add(new GameStateEdit.CourtDeck(newCourtDeck));
            edits.Add(new GameStateEdit.PlayerStateEdit.AddCard(blockingPlayer, newCard)); 
            return new ResultOutcome()
            {
                edits = edits,
                resultText = $"{blockingPlayerState.playerName} successfully blocked the {actionBlocked.name} and had to swap out their {revealedCard}!",
                newPhase = null, // new turn because the block succeeded
            };
        }
    }
}