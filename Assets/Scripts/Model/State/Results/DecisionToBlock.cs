using System.Collections.Generic;
using System.Linq;
using Model.Actions;

namespace Model.State.Results
{
    public class DecisionToBlock : Result
    {
        public int targetedPlayer;
        public ActionData action;
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var targetPlayer = gameState.playerStates[targetedPlayer];
            return new ResultOutcome()
            {
                resultText = $"{gameState.playerStates[targetedPlayer].playerName} gets to decide to block.",
                edits = new List<GameStateEdit>()
                {
                    new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,currentPlayer.coinCount - action.coinCost),
                    new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount + action.coinCost),
                },
                newPhase = new Phase
                (
                    text: $"{currentPlayer.playerName} wants to use [{action.name}] on you. Do you want to block?",
                    choosingPlayer: targetedPlayer,
                    choices: action.cardsThatCanCounterThis
                        .Select(cardThatCouldBeUsedToBlock =>
                        {
                            var blockingCard = config.cardDatabase.GetCard(cardThatCouldBeUsedToBlock).name;
                            var playerHasBlocker =
                                targetPlayer.cards.Any(_ => _.isFaceDown && _.id == cardThatCouldBeUsedToBlock); // nested loop
                            return new Choice
                            (
                                title: $"Block as {blockingCard}",
                                description:
                                $"Block [{action.name}] as {blockingCard}",
                                justification: playerHasBlocker
                                    ? Choice.Justification.UseCard
                                    : Choice.Justification.Bluff,
                                onChosen: new BlockAttempted()
                                {
                                    actionToBlock = action,
                                    cardClaimedToBlock = cardThatCouldBeUsedToBlock,
                                    targetedPlayer = targetedPlayer,
                                }
                            );
                        }).Append(new Choice
                        (
                            title: "Pass",
                            description: $"Don't block the [{action.name}]",
                            justification: Choice.Justification.Free,
                            onChosen: Logic.ActionOutCome(action,targetedPlayer)
                        ))
                ),
            };
        }
    }
}