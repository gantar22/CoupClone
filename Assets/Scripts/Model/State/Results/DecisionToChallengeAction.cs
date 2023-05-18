using Model.Actions;
using Model.Cards;

namespace Model.State.Results
{
    public class DecisionToChallengeAction : Result
    { // handle the continuation when generating choices based off of challenging player index
        public int decidingPlayer;
        public CardId claimedCard;
        public ActionData action; // we need to store enough info here to be able to generate ResultArguments for the action to resolve.
        public int? targetPlayer; // we could just store the args here, but i'd like to have the raw info if we need to use it


        public override ResultOutcome GetResult(GameState gameState, GameConfig config)
        {                    
            var decidingPlayerState = gameState.playerStates[decidingPlayer];
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];

            Result passResult;
            {
                var nextPlayer = (decidingPlayer + 1) % gameState.playerStates.Length;
                if (nextPlayer ==
                    gameState.currentPlayersTurn) // end when the priority gets back to the acting player
                {
                    passResult = Model.Logic.ActionOutCome(action, targetPlayer);
                }
                else
                {
                    passResult = new DecisionToChallengeAction()
                    {
                        action = action,
                        decidingPlayer = nextPlayer,
                        targetPlayer = targetPlayer,
                        claimedCard = claimedCard,
                    };
                }
            }
            return new ResultOutcome()
            {
                resultText = $"{decidingPlayerState.playerName} gets to decide to challenge.",
                edits = s_NoEdits,
                newPhase = new Phase
                (
                    text:
                    $"{currentPlayer.playerName} wants to use [{action.name}]. Do you want to challenge?",
                    choosingPlayer: decidingPlayer,
                    choices: new[]
                    {
                        new Choice
                        (
                            title: "Challenge",
                            description:
                            $"Accuse {currentPlayer.playerName} of not having {config.cardDatabase.GetCard(claimedCard).cardName}",
                            justification: Choice.Justification.Free,
                            onChosen: new ActionChallenged()
                            {
                                challengedAction = action,
                                challengingPlayer = decidingPlayer,
                                targetPlayer = targetPlayer,
                                claimedCardId = claimedCard,
                            }
                        ),
                        new Choice
                        (
                            title: "Pass",
                            description: "Don't challenge",
                            justification: Choice.Justification.Free,
                            onChosen: passResult
                        )
                    }
                )
            };
        }
    }
}