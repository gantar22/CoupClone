using System.Collections.Generic;
using System.Linq;
using Model.Actions;
using Model.Cards;
using Util;

namespace Model.State.Results
{
    public class TargetPicking : Result
    {
        public ActionData action;
        public Optional<CardId> sourceCard;
        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            return new ResultOutcome()
            {
                resultText = $"{currentPlayer.playerName} must pick a target for {action.name}",
                edits = new List<GameStateEdit>(),
                newPhase = new Phase
                (
                    text: $"Select target player to [{action.name}]",
                    // I forgot this when I was using the object initializer, that's why it was worth going back and using a constructor
                    // that's a pretty nasty bug if player 0 gets to pick the target for every action
                    choosingPlayer: gameState.currentPlayersTurn, 
                    choices: gameState.playerStates
                        .Select((playerState, targetPlayer) => (playerState, targetPlayer))
                        .Where(_ => _.targetPlayer != gameState.currentPlayersTurn)
                        .Select(_ =>
                        {
                            var playerState = _.playerState;
                            var targetPlayer = _.targetPlayer;
                            Result onTarget;
                            if (sourceCard.TryGetValue(out var sourceCardId))
                            {
                                onTarget = new DecisionToChallengeAction()
                                {
                                    action = action,
                                    claimedCard = sourceCardId,
                                    targetPlayer =
                                        targetPlayer, // by passing a non-null value we expect the challenge flow to handle blocking
                                    decidingPlayer = (gameState.currentPlayersTurn + 1) %
                                                     gameState.playerStates.Length,
                                };
                            }
                            else
                            {
                                onTarget = new DecisionToBlock()
                                {
                                    action = action,
                                    targetedPlayer = targetPlayer,
                                };
                            }

                            return new Choice
                            (
                                title: $"Target {playerState.playerName}",
                                description: $"Target {playerState.playerName} with {action.name}",
                                justification: Choice.Justification.Free,
                                onChosen: onTarget
                            );
                        })
                ),
            };
        }
    }
}