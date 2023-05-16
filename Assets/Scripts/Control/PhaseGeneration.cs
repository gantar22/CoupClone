using System;
using System.Collections.Generic;
using Model;
using Model.Actions;
using Model.State;
using System.Linq;
using Model.Cards;
using UnityEngine;
using Util;

namespace Control
{
    public static class PhaseGeneration
    {
        public static Phase GenerateInitialPhase(GameState gameState, GameConfig config)
        {
            var allActionsWithChoices = new List<(ActionData action, Choice choice)>();
            foreach (var action in config.actionDatabase.defaultActions)
            {
                var choice = GenerateChoiceFromAction(action, gameState, config.cardDatabase);
                if (choice.HasValue)
                {
                    allActionsWithChoices.Add((action, choice.Value));
                }
            }

            foreach (var cardId in config.cardDatabase.cardMap.Keys)
            {
                var card = config.cardDatabase.cardMap[cardId]; 
                if(!card.action.HasValue)
                    continue;
                var choice = GenerateChoiceFromAction(card.action.Value, gameState, config.cardDatabase, card.id);
                if (choice.HasValue)
                {
                    allActionsWithChoices.Add((card.action.Value, choice.Value));
                }
            }
            
            var currentPlayersCoinCount = gameState.playerStates[gameState.currentPlayersTurn].coinCount;
            var forcedActions = allActionsWithChoices // by having a choice we know that the action can actually be taken
                .Where(_ => _.action.obligatoryCoinCost.HasValue && _.action.obligatoryCoinCost.Value <= currentPlayersCoinCount).ToArray();
            
            if (forcedActions.Any())
            {
                var forcedAction = forcedActions.OrderBy(_=>_.action.obligatoryCoinCost.Value).First();
                return new Phase()
                {
                    text = $"You must perform {forcedAction.action.name}.",
                    choosingPlayer = gameState.currentPlayersTurn,
                    choices = new[] { forcedAction.choice },
                };
            } // else return all of the normal actions

            return new Phase()
            {
                text = "Choose an action for your turn",
                choosingPlayer = gameState.currentPlayersTurn,
                choices = allActionsWithChoices.Select(_ => _.choice),
            };
        }
        
        public static Choice.Justification GenerateChoiceJustification(ActionData action,PlayerState playerState, CardId sourceCardOption)
        {
            if (action.obligatoryCoinCost.HasValue && playerState.coinCount >= action.obligatoryCoinCost.Value)
            {
                return Choice.Justification.Forced;
            }

            if (sourceCardOption != null)
            {
                var playerHasCard = playerState.cards.Any(_ => _.id == sourceCardOption);
                return playerHasCard ? Choice.Justification.UseCard : Choice.Justification.Bluff;
            }

            return Choice.Justification.Free;
        }
        
        
        static Choice.Result ReactToActionBeingChallenged(
            GameState gameState,
            CardDatabase cardDatabase,
            int challenger,
            CardData card,
            int cardIndex,
            CardId sourceCard,
            Func<Choice.Result> failedChallengeContinuation)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var challengersState = gameState.playerStates[challenger];
            // Revealed Card is the claimed card
            if (card.id == sourceCard)
            {
                var newCourtDeck = gameState.courtDeck.ToList();
                var newCard = newCourtDeck.PopRandom();

                // Card gets revealed and swapped, then the challenger must reveal a card
                return new Choice.Result()
                {
                    resultText = $"{currentPlayer.playerName} revealed the claimed card: {card.cardName}.",
                    edits = new List<GameStateEdit>()
                    {
                        new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, cardIndex),
                        new GameStateEdit.PlayerStateEdit.RemoveCard(gameState.currentPlayersTurn, cardIndex),
                        new GameStateEdit.CourtDeck(newCourtDeck),
                        new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, newCard),
                    },
                    newPhase = new Phase()
                    {
                        choosingPlayer = challenger,
                        text = $"{currentPlayer.playerName} wasn't bluffing, they had {card.cardName}. Now you have to lose an influence.",
                        choices = challengersState.cards.Select((_, i) => (card: _, index: i))
                            .Where(_ => _.card.isFaceDown)
                            .Select(_ =>
                            {
                                var revealedCard = cardDatabase.GetCard(_.card.id);
                                var cardIndex = _.index;

                                var failedChallengeResult = failedChallengeContinuation();
                                failedChallengeResult.edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(challenger, cardIndex));

                                return new Choice()
                                {
                                    title = $"Reveal {revealedCard.cardName}",
                                    description = "It will be useless to you afterwards",
                                    justification = Choice.Justification.Free,
                                    result = () => failedChallengeResult, // skip to blocking phase
                                };
                            }),
                    }
                };
            }
            else
            { // card revealed wasn't the one that was claimed
                return new Choice.Result()
                {
                    resultText = $"{currentPlayer.playerName} didn't reveal {cardDatabase.GetCard(sourceCard).cardName}, they revealed {card.cardName} instead.",
                    edits = new List<GameStateEdit>()
                    {
                        new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, cardIndex),
                    },
                    newPhase = null, // a successful challenge ends the turn
                };
            }
        }

        static Choice.Result ChallengeAction(
            GameState gameState,
            CardDatabase cardDatabase,
            int challenger,
            ActionData proposedAction,
            CardId sourceCard,
            Func<Choice.Result> failedChallengeContinuation
                )
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var challengersState = gameState.playerStates[challenger];
            var cardName = cardDatabase.GetCard(sourceCard).cardName;
            return new Choice.Result()
            {
                resultText = $"{challengersState.playerName} is challenging {currentPlayer.playerName}'s [{proposedAction.name}] with {cardName}",
                edits = new List<GameStateEdit>(),
                // Now the acting player must reveal a card
                newPhase = new Phase()
                {
                    text =
                        $"{challengersState.playerName} is challenging your [{proposedAction.name}] with {cardName}",
                    choosingPlayer = gameState.currentPlayersTurn,
                    // There's one choice per card that's face down
                    choices = currentPlayer.cards.Select((_, i) => (card: _, index: i))
                        .Where(_ => _.card.isFaceDown)
                        .Select(_ =>
                        {
                            var card = cardDatabase.GetCard(_.card.id);
                            var cardIndex = _.index;


                            return new Choice()
                            {
                                title = $"Reveal {card.cardName}",
                                description = card.id == sourceCard
                                    ? $"Reveal {card.cardName} to prove you have it. You'll get a random replacement afterwards."
                                    : $"Reveal {card.cardName}. It will be useless to you afterwards",
                                justification = Choice.Justification.Free,
                                result = () => ReactToActionBeingChallenged(
                                    gameState,
                                    cardDatabase,
                                    challenger,
                                    card,
                                    cardIndex,
                                    sourceCard,
                                    failedChallengeContinuation)
                            };
                        })
                }
            };
        }

        static IEnumerable<Choice> GenerateChallengesForAction(
            GameState gameState,
            CardDatabase cardDatabase,
            int choosingPlayer,
            ActionData proposedAction,
            CardId sourceCard,
            Func<Choice.Result> passContinuation,
            Func<Choice.Result> failedChallengeContinuation)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            var cardName = cardDatabase.GetCard(sourceCard).cardName;

            return new List<Choice>()
            {
                // Challenge
                new Choice()
                {
                    title = "Challenge",
                    description = $"Accuse {currentPlayer.playerName} of not having {cardName}",
                    justification = Choice.Justification.Free,
                    result = () => ChallengeAction(
                        gameState,
                        cardDatabase,
                        choosingPlayer,
                        proposedAction,
                        sourceCard,
                        failedChallengeContinuation)
                },
                // Pass
                new Choice()
                {
                    title = "Pass",
                    description = "Don't challenge",
                    justification = Choice.Justification.Free,
                    result = passContinuation
                }
            };
        }

        static IEnumerable<Choice> GenerateBlockingChoices(
            GameState gameState,
            CardDatabase cardDatabase,
            int choosingPlayer,
            ActionData proposedAction,
            Func<Choice.Result> actionOutcome)
        {
            var choices = new List<Choice>();
            var choosingPlayerState = gameState.playerStates[choosingPlayer];
            var currentPlayerState = gameState.playerStates[gameState.currentPlayersTurn];

            // Add one option per card that can block
            foreach (var blocker in proposedAction.cardsThatCanCounterThis)
            {
                var playerHasBlocker =
                    choosingPlayerState.cards.Any(_ => _.isFaceDown && _.id == blocker); // note nested iteration
                // Claim card to block with
                choices.Add(new Choice()
                {
                    title = $"Block as {cardDatabase.GetCard(blocker).name}",
                    description = $"Block [{proposedAction.name}] as {cardDatabase.GetCard(blocker).name}",
                    justification = playerHasBlocker ? Choice.Justification.UseCard : Choice.Justification.Bluff,
                    result = () => new Choice.Result()
                    {
                        resultText = $"{choosingPlayerState.playerName} claims to block {currentPlayerState.playerName}'s [{proposedAction.name}] block with {cardDatabase.GetCard(blocker).name}",
                        edits = new List<GameStateEdit>(), // could manage allocation nicer
                        newPhase = new Phase()
                        {
                            choosingPlayer = gameState.currentPlayersTurn,
                            text =
                                $"{choosingPlayerState.playerName} is blocking [{proposedAction.name}] as {cardDatabase.GetCard(blocker).cardName}.\nDo you challenge?",
                            choices = new[]
                            {
                                // Challenge
                                new Choice()
                                {
                                    title = "Challenge",
                                    description =
                                        $"Challenge {choosingPlayerState.playerName}'s claim to be blocking as {cardDatabase.GetCard(blocker).name}",
                                    result = () => new Choice.Result()
                                    {
                                        resultText = $"{currentPlayerState.playerName} challenges {choosingPlayerState.playerName}'s claim to block as {cardDatabase.GetCard(blocker).name}",
                                        edits = new List<GameStateEdit>(),
                                        newPhase = new Phase()
                                        {
                                            text =
                                                $"{gameState.playerStates[gameState.currentPlayersTurn].playerName} challenged your block.",
                                            choosingPlayer = choosingPlayer,
                                            // one choice for each card that could be revealed
                                            // if it's the claimed blocker, then it gets recycled and the block is successful
                                            // otherwise we just reveal the card and the action goes through
                                            choices = choosingPlayerState.cards
                                                .Select((card, index) => (card, index))
                                                .Where(_ => _.card.isFaceDown)
                                                .Select(_ => (card: cardDatabase.GetCard(_.card.id),
                                                    index: _.index))
                                                .Select(cardData => new Choice()
                                                {
                                                    title = $"Reveal {cardData.card.cardName}",
                                                    description = cardData.card.id == blocker
                                                        ? $"Reveal {cardData.card.cardName} to prove your innocence. You'll get a random replacement in exchange."
                                                        : $"Reveal {cardData.card.cardName}. It will be useless to you afterwards.",
                                                    justification = Choice.Justification.Free,
                                                    result = () =>
                                                    {
                                                        // Reveal and exchange, then the block is successful
                                                        if (cardData.card.id == blocker)
                                                        {                                    
                                                            var edits = new List<GameStateEdit>();
                                                            edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(
                                                                choosingPlayer,
                                                                cardData.index)); // to notify other players
                                                            edits.Add(new GameStateEdit.PlayerStateEdit.RemoveCard(
                                                                choosingPlayer, cardData.index));
                                                            var newCourtDeck = gameState.courtDeck.ToList();
                                                            var newCard = newCourtDeck.PopRandom();
                                                            edits.Add(new GameStateEdit.CourtDeck(newCourtDeck));
                                                            edits.Add(new GameStateEdit.PlayerStateEdit.AddCard(
                                                                choosingPlayer, newCard));

                                                            return new Choice.Result()
                                                            {
                                                                resultText = $"{choosingPlayerState.playerName} successfully blocked the {proposedAction.name} and had to swap out their {cardData.card.cardName}!",
                                                                edits = edits,
                                                                newPhase = null, // because the block worked, we end the turn without using the continuation
                                                            };
                                                        } // Reveal, then the block is unsuccessful and we continue with the original action
                                                        else
                                                        {
                                                            var continuationResult = actionOutcome();
                                                            continuationResult.edits.Add(
                                                                new GameStateEdit.PlayerStateEdit.RevealCard(
                                                                    choosingPlayer,
                                                                    cardData.index));
                                                            continuationResult.resultText =
                                                                $"choosingPlayerState.playerName didn't reveal {cardDatabase.GetCard(blocker).cardName}, but instead gave up {cardData.card.cardName}\n" +
                                                                continuationResult.resultText;
                                                            return continuationResult; // modify the continuation to add the revealed card as punishment for bluffing
                                                        }
                                                    }
                                                }),
                                        },
                                    },
                                },
                                // Allow the block, pass turn
                                new Choice()
                                {
                                    title = "Pass",
                                    description = "Allow the block and pass your turn",
                                    result = () => new Choice.Result()
                                    {
                                        resultText = $"{currentPlayerState.playerName} decided not to challenge {choosingPlayerState.playerName}'s block.",
                                        edits = new List<GameStateEdit>(), // could manage allocation nicer
                                        newPhase = null, // end turn
                                    }
                                }
                            }
                        },
                    }
                });
            }

            // Pass
            choices.Add(new Choice()
            {
                title = "Pass",
                description = "",
                justification = Choice.Justification.Free,
                result = actionOutcome,
            });

            return choices;
        }

        static Phase? GetDiscardPhase(
            int cardsToDiscard,
            GameState gameState,
            CardDatabase cardDatabase)
        {
            if (cardsToDiscard == 0)
            {
                return null;
            }

            return new Phase()
            {
                text =
                    $"Choose [{cardsToDiscard}] card{(cardsToDiscard > 1 ? "s" : "")} to put back in the court deck.",
                choosingPlayer = gameState.currentPlayersTurn,
                choices = gameState.playerStates[gameState.currentPlayersTurn].cards
                    .Select((card, index) => (card:card, index:index))
                    .Where(_ => _.card.isFaceDown)
                    .Select(_ =>
                    {
                        var card = _.card;
                        var cardIndex = _.index;
                        return new Choice()
                        {
                            title = cardDatabase.GetCard(card.id).cardName,
                            description = $"Return {cardDatabase.GetCard(card.id).cardName} to the court deck",
                            justification = Choice.Justification.Free,
                            result = () =>
                            {
                                var newCourtDeck = gameState.courtDeck.ToList();
                                newCourtDeck.Add(card.id);
                                return new Choice.Result()
                                {
                                    resultText = $"{gameState.currentPlayersTurn} discarded a card",
                                    edits = new List<GameStateEdit>
                                    {
                                        new GameStateEdit.PlayerStateEdit.RemoveCard(
                                            gameState.currentPlayersTurn, cardIndex),
                                        new GameStateEdit.CourtDeck(newCourtDeck),
                                    },
                                    newPhase = GetDiscardPhase(cardsToDiscard - 1, gameState, cardDatabase),
                                };
                            }
                        };
                    }),
            };
        }
        
        static Func<Choice.Result> GetActionOutcome(ActionData action,
            GameState gameState,
            CardDatabase cardDatabase,
            int? targetPlayer = null) // target player is null iff action.actionType == take coins or exchange
        {
            var currentPlayerState = gameState.playerStates[gameState.currentPlayersTurn];
            switch (action.actionType)
            {
                case ActionType.TakeCoins:
                    var coinsToTake = Math.Min(gameState.treasuryCoinCount, action.amount);
                    return () => new Choice.Result()
                    {
                        resultText = $"{currentPlayerState.playerName} took {coinsToTake} coin(s) from the treasury.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount - coinsToTake),
                            new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                                gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                        },
                        newPhase = null, // end turn on completion
                    };
                case ActionType.StealCoins:   
                    if(targetPlayer == null)
                        throw new Exception("Target missing for targeted action");
                    coinsToTake = Math.Min(gameState.playerStates[targetPlayer.Value].coinCount, action.amount);
                    return () => new Choice.Result()
                    {
                        resultText = $"{currentPlayerState.playerName} took {coinsToTake} coin(s) from {gameState.playerStates[targetPlayer.Value].playerName}.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.PlayerStateEdit.CoinCount(targetPlayer.Value,gameState.playerStates[targetPlayer.Value].coinCount - coinsToTake),
                            new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                                gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                        },
                        newPhase = null, // end turn on completion
                    };
                case ActionType.ExchangeCards:
                    var courtDeck = gameState.courtDeck.ToList();
                    var cardsToMove = new List<CardId>();
                    for(int i = 0; i < action.amount; i++)
                    {
                        cardsToMove.Add(courtDeck.PopRandom());
                    }

                    return () => new Choice.Result()
                    {
                        resultText = $"{currentPlayerState.playerName} gets to exchange {action.amount} cards.",
                        edits = cardsToMove
                            .Select(_ =>
                                new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, _) as
                                    GameStateEdit)
                            .Prepend(new GameStateEdit.CourtDeck(courtDeck))
                            .ToList(),
                        newPhase = GetDiscardPhase(action.amount, gameState, cardDatabase),
                    };
                case ActionType.PlayerLosesInfluence:
                    if(targetPlayer == null)
                        throw new Exception("Target missing for targeted action");
                    return () => new Choice.Result()
                    {
                        resultText = $"{gameState.playerStates[targetPlayer.Value].playerName} must now lose influence.",
                        edits = new List<GameStateEdit>(),
                        newPhase = new Phase()
                        {
                            text = $"Select card to reveal",
                            choosingPlayer = targetPlayer.Value,
                            choices = gameState.playerStates[targetPlayer.Value].cards
                                .Select((card, index) => (card:card, index:index))
                                .Where(_ => _.card.isFaceDown)
                                .Select(_ =>
                                {
                                    var card = _.card;
                                    var cardIndex = _.index;
                                    return new Choice()
                                    {
                                        title = cardDatabase.GetCard(card.id).cardName,
                                        description = $"Reveal and lose the power of {cardDatabase.GetCard(card.id).cardName}",
                                        justification = Choice.Justification.Free,
                                        result = () =>
                                        {
                                            return new Choice.Result()
                                            {
                                                resultText = $"{gameState.playerStates[targetPlayer.Value].playerName} revealed {cardDatabase.GetCard(card.id).cardName}.",
                                                edits = new List<GameStateEdit>
                                                {
                                                    new GameStateEdit.PlayerStateEdit.RevealCard(
                                                        targetPlayer.Value, cardIndex)
                                                },
                                                newPhase = null,
                                            };
                                        }
                                    };
                                }),
                        },
                    };
                default:
                    Debug.LogError("Unhandled action type");
                    return () => default;
            }
        }
        
        static Choice.Result GetResultOfPickingTargetedAction(
            ActionData action,
            GameState gameState,
            CardDatabase cardDatabase,
            CardId sourceCardOption,
            List<GameStateEdit> costEdits)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            return new Choice.Result()
            {
                resultText = $"{currentPlayer.playerName} must pick a target for {action.name}",
                edits = new List<GameStateEdit>(),
                newPhase = new Phase()
                {
                    text = $"Select target player to [{action.name}]",
                    choices = gameState.playerStates
                        .Select((playerState, targetPlayer) => new Choice()
                        {
                            title = $"Target {playerState.playerName}",
                            description = $"Target {playerState.playerName} with {action.name}",
                            justification = Choice.Justification.Free,
                            result = () =>
                            {
                                Func<Choice.Result> actionOutcome = GetActionOutcome(action,gameState,cardDatabase,targetPlayer);
                                
                                Func<Choice.Result> blockingPhase = () => new Choice.Result()
                                {
                                    resultText = $"{gameState.playerStates[targetPlayer].playerName} gets to decide to block.",
                                    edits = costEdits,
                                    newPhase = new Phase()
                                    {
                                        text = $"{currentPlayer.playerName} wants to use [{action.name}] on you. Do you want to block?",
                                        choosingPlayer = targetPlayer,
                                        choices = GenerateBlockingChoices(gameState,cardDatabase,targetPlayer,action,actionOutcome),
                                    },
                                };
                                if (!sourceCardOption)
                                {
                                    return blockingPhase();
                                }

                                var accumulatedContinuation = blockingPhase;
                                for (int challengingPlayer = gameState.playerStates.Length - 1; challengingPlayer >= 0; challengingPlayer--)
                                {
                                    if (challengingPlayer == gameState.currentPlayersTurn || !gameState.playerStates[challengingPlayer].cards.Any(_ => _.isFaceDown))
                                        continue;

                                    var player = challengingPlayer; // avoid capture of locals
                                    var passContinuation = accumulatedContinuation;
                                    accumulatedContinuation = () => new Choice.Result()
                                    {
                                        resultText = $"{gameState.playerStates[player].playerName} gets to decide to challenge.",
                                        edits = new List<GameStateEdit>(),
                                        newPhase = new Phase()
                                        {
                                            text = $"{currentPlayer.playerName} wants to use [{action.name}]. Do you want to challenge?",
                                            choosingPlayer = player,
                                            choices = GenerateChallengesForAction(
                                                gameState,
                                                cardDatabase,
                                                player,
                                                action,
                                                sourceCardOption,
                                                passContinuation,
                                                blockingPhase),
                                        }
                                    };
                                }

                                return accumulatedContinuation();
                            },
                        })
                        .Where((_, i) => i != gameState.currentPlayersTurn && gameState.playerStates[i].cards.Any(_ => _.isFaceDown)),
                },
            };
        }

        static Choice.Result GetResultOfNonTargetedAction(
            ActionData action,
            GameState gameState,
            CardDatabase cardDatabase,
            CardId sourceCardOption,
            List<GameStateEdit> costEdits,
            Func<Choice.Result> actionOutcome)
        {
            if (!sourceCardOption)
            {
                // the action can't be counter acted, so we pay the cost and resolve the action
                var outcome = actionOutcome();
                costEdits.AddRange(outcome.edits);
                outcome.edits = costEdits;
                return outcome;
            }

            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
   
            var accumulatedContinuation = actionOutcome;
            for (int challengingPlayer = gameState.playerStates.Length - 1; challengingPlayer >= 0; challengingPlayer--)
            {
                if (challengingPlayer == gameState.currentPlayersTurn || !gameState.playerStates[challengingPlayer].cards.Any(_ => _.isFaceDown))
                    continue;

                var player = challengingPlayer;
                var passContinuation = accumulatedContinuation;
                accumulatedContinuation = () => new Choice.Result()
                {                                            
                    resultText = $"{gameState.playerStates[player].playerName} gets to decide to challenge.",
                    edits = new List<GameStateEdit>(),
                    newPhase = new Phase()
                    {
                        text = $"{currentPlayer.playerName} wants to use [{action.name}]. Do you want to challenge?",
                        choosingPlayer = player,
                        choices = GenerateChallengesForAction(
                            gameState,
                            cardDatabase,
                            player,
                            action,
                            sourceCardOption,
                            passContinuation,
                            actionOutcome),
                    }
                };
            }

            return accumulatedContinuation();
        }

        // returns null if the cost can't be paid
        // sourceCard is null when the action is a default action
        static Choice? GenerateChoiceFromAction(ActionData action, GameState gameState, CardDatabase cardDatabase, CardId sourceCardOption = null)
        {
            var playerState = gameState.playerStates[gameState.currentPlayersTurn];
            if (action.coinCost > playerState.coinCount)
                return null; // Choice can't be afforded
            
            return new Choice()
            {
                title = action.name,
                description = action.description,
                justification = GenerateChoiceJustification(action,playerState, sourceCardOption),
                result = () =>
                {               
                    var costEdits = new List<GameStateEdit>(); 
                    costEdits.Add(new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,playerState.coinCount - action.coinCost));
                    costEdits.Add(new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount + action.coinCost));


                    switch (action.actionType)
                    {
                        case ActionType.StealCoins:
                        case ActionType.PlayerLosesInfluence:
                            // pick target
                            return GetResultOfPickingTargetedAction(action,gameState,cardDatabase,sourceCardOption,costEdits);
                        case ActionType.TakeCoins:
                        case ActionType.ExchangeCards:
                        default:
                            return GetResultOfNonTargetedAction(action, gameState, cardDatabase, sourceCardOption,
                                costEdits,GetActionOutcome(action,gameState,cardDatabase));
                    }
                },
            };
        }
    }
}