using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Actions;
using Model.Cards;
using Model.State;
using Util;

namespace Control
{
    public static class ResultResolution
    {
        public static List<GameStateEdit> s_NoEdits = new List<GameStateEdit>();
        public static Choice.Result GetResult(GameState gameState, CardDatabase cardDatabase, ResultArguments args)
        {
            var currentPlayer = gameState.playerStates[gameState.currentPlayersTurn];
            switch (args)
            {
                case ResultArguments.GenericResult genericResult:
                    return genericResult.result;
                case ResultArguments.ActionChallenged resultInfo:
                {
                    var challengersState = gameState.playerStates[resultInfo.challengingPlayer];
                    var claimedCard = cardDatabase.GetCard(resultInfo.claimedCard);
                    
                    var nextPhase = new Phase
                    (
                        text: $"{challengersState.playerName} is challenging your [{resultInfo.challengedAction.name}] with {claimedCard.cardName}",
                        choosingPlayer: gameState.currentPlayersTurn,
                        choices: currentPlayer.cards.Select((_, i) => (card: _, index: i))
                            .Where(_ => _.card.isFaceDown)
                            .Select(_ =>
                            {
                                var card = cardDatabase.GetCard(_.card.id);
                                var cardIndex = _.index;

                                if (card.id != resultInfo.claimedCard)
                                {
                                    return new Choice
                                    (
                                        title: $"Reveal {card.cardName}",
                                        description: $"Reveal {card.cardName}. It will be useless to you afterwards",
                                        justification: Choice.Justification.Free,
                                        onChosen: new ResultArguments.ActionChallengeSucceeds()
                                        {
                                            claimedCard = resultInfo.claimedCard,
                                            revealedCardIndex = cardIndex,
                                        }
                                    );
                                }                                
                                return new Choice()
                                {

                                    title = $"Reveal {card.cardName}",
                                    description =
                                        $"Reveal {card.cardName} to prove you have it. You'll get a random replacement afterwards.",
                                    justification = Choice.Justification.Free,
                                    onChosen = new ResultArguments.ActionChallengeFails()
                                    {
                                        playerThatChallenged = resultInfo.challengingPlayer,
                                        proposedAction = resultInfo.challengedAction,
                                        revealedCardIndex = cardIndex,
                                        targetPlayer = resultInfo.targetPlayer,
                                    }
                                };
                            })
                    );
                    return new Choice.Result()
                    {
                        resultText = $"{challengersState.playerName} is challenging {currentPlayer.playerName}'s [{resultInfo.challengedAction.name}] with {claimedCard.cardName}",
                        edits = s_NoEdits,
                        newPhase = nextPhase, 
                    };
                }
                case ResultArguments.ActionChallengeFails resultInfo:
                {
                    var newCourtDeck = gameState.courtDeck.ToList();
                    var newCard = newCourtDeck.PopRandom();
                    var claimedCardId = currentPlayer.cards[resultInfo.revealedCardIndex].id;
                    var claimedCard = cardDatabase.GetCard(claimedCardId);
                    var challenger = gameState.playerStates[resultInfo.playerThatChallenged];
                    
                    // Card gets revealed and swapped, then the challenger must reveal a card
                    return new Choice.Result()
                    {
                        resultText = $"{currentPlayer.playerName} revealed the claimed card: {claimedCard.cardName}.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, resultInfo.revealedCardIndex),
                            new GameStateEdit.PlayerStateEdit.RemoveCard(gameState.currentPlayersTurn, resultInfo.revealedCardIndex),
                            new GameStateEdit.CourtDeck(newCourtDeck),
                            new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, newCard),
                        },
                        newPhase = new Phase
                        (
                            choosingPlayer: resultInfo.playerThatChallenged,
                            text:
                                $"{currentPlayer.playerName} wasn't bluffing, they had {claimedCard.cardName}. Now you have to lose an influence.",
                            choices: challenger.cards
                                .Select((_, i) => (card: _, index: i))
                                .Where(_ => _.card.isFaceDown)
                                .Select(_ =>
                                {
                                    var revealedCard = cardDatabase.GetCard(_.card.id);

                                    ResultArguments nextResultArgs;
                                    if (resultInfo.targetPlayer.HasValue)
                                    {
                                        nextResultArgs = new ResultArguments.DecisionToBlock()
                                        {
                                            targetedPlayer = resultInfo.targetPlayer.Value,
                                            action = resultInfo.proposedAction,
                                        };
                                    }
                                    else
                                    {
                                        nextResultArgs = ActionOutCome(resultInfo.proposedAction,null);
                                    }
                                    
                                    var nextResult = GetResult(gameState,cardDatabase,nextResultArgs); // peek ahead by one
                                    nextResult.edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(resultInfo.playerThatChallenged, _.index));
                                    
                                    return new Choice
                                    (
                                        title: $"Reveal {revealedCard.cardName}",
                                        description: "It will be useless to you afterwards",
                                        justification: Choice.Justification.Free,
                                        onChosen: new ResultArguments.GenericResult() { result = nextResult }
                                    );
                                })
                        )
                    };
                }
                case ResultArguments.ActionChallengeSucceeds resultInfo:
                {
                    var revealedCard = cardDatabase.GetCard(currentPlayer.cards[resultInfo.revealedCardIndex].id);
                    var claimedCardName = cardDatabase.GetCard(resultInfo.claimedCard).cardName;
                    return new Choice.Result()
                    {
                        resultText = $"{currentPlayer.playerName} didn't reveal {claimedCardName}, they revealed {revealedCard.cardName} instead.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.PlayerStateEdit.RevealCard(gameState.currentPlayersTurn, resultInfo.revealedCardIndex),
                        },
                        newPhase = null, // new turn because the challenge succeeded
                    };
                }
                case ResultArguments.BlockAttempted resultInfo:
                {
                    var targetedPlayer = gameState.playerStates[resultInfo.targetedPlayer].playerName;
                    var cardUsedToBlock = cardDatabase.GetCard(resultInfo.cardClaimedToBlock).name;
                    return new Choice.Result()
                    {
                        resultText =
                            $"{targetedPlayer} claims to block {currentPlayer.playerName}'s [{resultInfo.actionToBlock.name}] block with {cardUsedToBlock}",
                        edits = s_NoEdits,
                        newPhase = new Phase
                        (
                            choosingPlayer: gameState.currentPlayersTurn,
                            text:
                                $"{targetedPlayer} is blocking [{resultInfo.actionToBlock.name}] as {cardUsedToBlock}.\nDo you challenge?",
                            choices: new[]
                            {
                                new Choice
                                (
                                    title: "Pass",
                                    description: "Allow the block and pass your turn",
                                    justification: Choice.Justification.Free,
                                    onChosen: new ResultArguments.BlockNotChallenged() { blockingPlayer = resultInfo.targetedPlayer }
                                ),
                                new Choice
                                (
                                    title: "Challenge",
                                    description: $"Challenge {targetedPlayer}'s claim to be blocking as {cardUsedToBlock}",
                                    justification: Choice.Justification.Free,
                                    onChosen: new ResultArguments.BlockChallenged()
                                    {
                                        actionBeingBlocked = resultInfo.actionToBlock,
                                        cardBeingChallenged = resultInfo.cardClaimedToBlock,
                                        challengedBlocker = resultInfo.targetedPlayer,
                                    }
                                ),
                            }
                        ),
                    };
                }
                case ResultArguments.BlockChallenged resultInfo:
                {
                    var cardUsedToBlock = cardDatabase.GetCard(resultInfo.cardBeingChallenged).name;
                    var blockingPlayer = gameState.playerStates[resultInfo.challengedBlocker];
                    return new Choice.Result()
                    {
                        resultText =
                            $"{currentPlayer.playerName} challenges {blockingPlayer.playerName}'s claim to block as {cardUsedToBlock}",
                        edits = new List<GameStateEdit>(),
                        newPhase = new Phase
                        (
                            text:
                                $"{gameState.playerStates[gameState.currentPlayersTurn].playerName} challenged your block.",
                            choosingPlayer: resultInfo.challengedBlocker,
                            // one choice for each card that could be revealed
                            // if it's the claimed blocker, then it gets recycled and the block is successful
                            // otherwise we just reveal the card and the action goes through
                            choices: blockingPlayer.cards
                                .Select((card, index) => (card, index))
                                .Where(_ => _.card.isFaceDown)
                                .Select(_ => (card: cardDatabase.GetCard(_.card.id), _.index))
                                .Select(cardData =>
                                {
                                    if (cardData.card.id == resultInfo.cardBeingChallenged)
                                    {
                                        return new Choice
                                        (
                                            title: $"Reveal {cardData.card.cardName}",
                                            description:
                                                $"Reveal {cardData.card.cardName} to prove your innocence. You'll get a random replacement in exchange.",
                                            justification: Choice.Justification.Free,
                                            onChosen: new ResultArguments.BlockChallengeFails()
                                            {
                                                actionBlocked = resultInfo.actionBeingBlocked,
                                                blockingPlayer = resultInfo.challengedBlocker,
                                                revealedCardIndex = cardData.index,
                                            }
                                        );
                                    }

                                    return new Choice
                                    (
                                        title: $"Reveal {cardData.card.cardName}",
                                        description: $"Reveal {cardData.card.cardName}. It will be useless to you afterwards.",
                                        justification: Choice.Justification.Free,
                                        onChosen: new ResultArguments.BlockChallengeSucceeds()
                                        {
                                            actionNotBlocked = resultInfo.actionBeingBlocked,
                                            cardClaimedToBlock = resultInfo.cardBeingChallenged,
                                            challengedBlocker = resultInfo.challengedBlocker,
                                            revealedCardIndex = cardData.index,
                                        }
                                    );
                                })
                        )
                    };
                }
                case ResultArguments.BlockChallengeFails resultInfo:
                {
                    var blockingPlayer = gameState.playerStates[resultInfo.blockingPlayer];
                    var revealedCard = cardDatabase.GetCard(gameState.playerStates[resultInfo.blockingPlayer].cards[resultInfo.revealedCardIndex].id);
                    var edits = new List<GameStateEdit>();
                    // lose the old card
                    edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(resultInfo.blockingPlayer, resultInfo.revealedCardIndex)); // to notify other players
                    edits.Add(new GameStateEdit.PlayerStateEdit.RemoveCard(resultInfo.blockingPlayer, resultInfo.revealedCardIndex));
                    
                    // get a new card
                    var newCourtDeck = gameState.courtDeck.ToList();
                    var newCard = newCourtDeck.PopRandom();
                    edits.Add(new GameStateEdit.CourtDeck(newCourtDeck));
                    edits.Add(new GameStateEdit.PlayerStateEdit.AddCard(resultInfo.blockingPlayer, newCard)); 
                    return new Choice.Result()
                    {
                        edits = edits,
                        resultText = $"{blockingPlayer.playerName} successfully blocked the {resultInfo.actionBlocked.name} and had to swap out their {revealedCard}!",
                        newPhase = null, // new turn because the block succeeded
                    };
                }
                case ResultArguments.BlockChallengeSucceeds resultInfo:
                {
                    var cardClaimedToBlock = cardDatabase.GetCard(resultInfo.cardClaimedToBlock).cardName;
                    var revealedCard = cardDatabase.GetCard(gameState.playerStates[resultInfo.challengedBlocker].cards[resultInfo.revealedCardIndex].id).cardName;
                    var targetedPlayer = gameState.playerStates[resultInfo.challengedBlocker];
                    
                    var actionResult = GetResult(gameState,cardDatabase,ActionOutCome(resultInfo.actionNotBlocked,resultInfo.challengedBlocker));
                    actionResult.edits.Add(new GameStateEdit.PlayerStateEdit.RevealCard(
                        resultInfo.challengedBlocker,
                        resultInfo.revealedCardIndex));
                    actionResult.resultText = 
                        $"{targetedPlayer.playerName} didn't reveal {cardClaimedToBlock}, but instead gave up {revealedCard}\n" 
                        + actionResult.resultText;
                    return actionResult;
                }
                case ResultArguments.BlockNotChallenged resultInfo:
                {
                    var blockingPlayer = gameState.playerStates[resultInfo.blockingPlayer];
                    return new Choice.Result()
                    {
                        resultText =
                            $"{currentPlayer.playerName} decided not to challenge {blockingPlayer.playerName}'s block.",
                        edits = s_NoEdits,
                        newPhase = null, // end turn
                    };
                }
                case ResultArguments.DecisionToBlock resultInfo:
                {
                    var targetPlayer = gameState.playerStates[resultInfo.targetedPlayer];
                    return new Choice.Result()
                    {
                        resultText = $"{gameState.playerStates[resultInfo.targetedPlayer].playerName} gets to decide to block.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,currentPlayer.coinCount - resultInfo.action.coinCost),
                            new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount + resultInfo.action.coinCost),
                        },
                        newPhase = new Phase
                        (
                            text: $"{currentPlayer.playerName} wants to use [{resultInfo.action.name}] on you. Do you want to block?",
                            choosingPlayer: resultInfo.targetedPlayer,
                            choices: resultInfo.action.cardsThatCanCounterThis
                                .Select(cardThatCouldBeUsedToBlock =>
                                {
                                    var blockingCard = cardDatabase.GetCard(cardThatCouldBeUsedToBlock).name;
                                    var playerHasBlocker =
                                        targetPlayer.cards.Any(_ => _.isFaceDown && _.id == cardThatCouldBeUsedToBlock); // nested loop
                                    return new Choice
                                    (
                                        title: $"Block as {blockingCard}",
                                        description:
                                            $"Block [{resultInfo.action.name}] as {blockingCard}",
                                        justification: playerHasBlocker
                                            ? Choice.Justification.UseCard
                                            : Choice.Justification.Bluff,
                                        onChosen: new ResultArguments.BlockAttempted()
                                        {
                                            actionToBlock = resultInfo.action,
                                            cardClaimedToBlock = cardThatCouldBeUsedToBlock,
                                            targetedPlayer = resultInfo.targetedPlayer,
                                        }
                                    );
                                }).Append(new Choice
                                (
                                    title: "Pass",
                                    description: $"Don't block the [{resultInfo.action.name}]",
                                    justification: Choice.Justification.Free,
                                    onChosen: ActionOutCome(resultInfo.action,resultInfo.targetedPlayer)
                                ))
                        ),
                    };
                }
                case ResultArguments.DecisionToChallengeAction resultInfo:
                {
                    var decidingPlayer = gameState.playerStates[resultInfo.decidingPlayer];
                    ResultArguments passResult;
                    {
                        var nextPlayer = (resultInfo.decidingPlayer + 1) % gameState.playerStates.Length;
                        if (nextPlayer ==
                            gameState.currentPlayersTurn) // end when the priority gets back to the acting player
                        {
                            passResult = ActionOutCome(resultInfo.action, resultInfo.targetPlayer);
                        }
                        else
                        {
                            passResult = new ResultArguments.DecisionToChallengeAction()
                            {
                                action = resultInfo.action,
                                decidingPlayer = nextPlayer,
                                targetPlayer = resultInfo.targetPlayer,
                                claimedCard = resultInfo.claimedCard,
                            };
                        }
                    }
                    return new Choice.Result()
                    {
                        resultText = $"{decidingPlayer.playerName} gets to decide to challenge.",
                        edits = s_NoEdits,
                        newPhase = new Phase
                        (
                            text:
                                $"{currentPlayer.playerName} wants to use [{resultInfo.action.name}]. Do you want to challenge?",
                            choosingPlayer: resultInfo.decidingPlayer,
                            choices: new[]
                            {
                                new Choice
                                (
                                    title: "Challenge",
                                    description:
                                        $"Accuse {currentPlayer.playerName} of not having {cardDatabase.GetCard(resultInfo.claimedCard).cardName}",
                                    justification: Choice.Justification.Free,
                                    onChosen: new ResultArguments.ActionChallenged()
                                    {
                                        challengedAction = resultInfo.action,
                                        challengingPlayer = resultInfo.decidingPlayer,
                                        targetPlayer = resultInfo.targetPlayer,
                                        claimedCard = resultInfo.claimedCard,
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
                case ResultArguments.DiscardedFromExchange resultInfo:
                {
                    var newCourtDeck = gameState.courtDeck.ToList();
                    newCourtDeck.Add(currentPlayer.cards[resultInfo.cardIndex].id);
                    return new Choice.Result()
                    {
                        resultText = $"{gameState.currentPlayersTurn} discarded a card",
                        edits = new List<GameStateEdit>
                        {
                            new GameStateEdit.PlayerStateEdit.RemoveCard(
                                gameState.currentPlayersTurn, resultInfo.cardIndex),
                            new GameStateEdit.CourtDeck(newCourtDeck),
                        },
                        newPhase = GetDiscardPhase(resultInfo.cardsLeftToDiscard, gameState, cardDatabase),
                    };
                }
                case ResultArguments.ExchangeCards resultInfo:
                {                    
                    var courtDeck = gameState.courtDeck.ToList();
                    var cardsToMove = new List<CardId>();
                    for(int i = 0; i < resultInfo.cardTotal; i++)
                    {
                        cardsToMove.Add(courtDeck.PopRandom());
                    }

                    return new Choice.Result()
                    {
                        resultText = $"{currentPlayer.playerName} gets to exchange {resultInfo.cardTotal} cards.",
                        edits = cardsToMove
                            .Select(_ =>
                                new GameStateEdit.PlayerStateEdit.AddCard(gameState.currentPlayersTurn, _) as
                                    GameStateEdit)
                            .Prepend(new GameStateEdit.CourtDeck(courtDeck))
                            .ToList(),
                        newPhase = GetDiscardPhase(resultInfo.cardTotal, gameState, cardDatabase),
                    };
                }
                case ResultArguments.PlayerLosesInfluenceDueToAction resultInfo:
                {
                    var targetPlayer = gameState.playerStates[resultInfo.targetPlayer];
                    var revealedCard = cardDatabase.GetCard(gameState.playerStates[resultInfo.targetPlayer].cards[resultInfo.cardIndex].id);
                    
                    return new Choice.Result()
                    {
                        resultText = $"{targetPlayer.playerName} revealed {revealedCard.cardName}.",
                        edits = new List<GameStateEdit>
                        {
                            new GameStateEdit.PlayerStateEdit.RevealCard(resultInfo.targetPlayer, resultInfo.cardIndex),
                        },
                        newPhase = null,
                    };
                }
                case ResultArguments.PlayerMustLoseInfluenceDueToAction resultInfo:
                {
                    return new Choice.Result()
                    {
                        resultText =
                            $"{gameState.playerStates[resultInfo.targetPlayer].playerName} must now lose influence.",
                        edits = s_NoEdits,
                        newPhase = new Phase
                        (
                            text: "Select a card to reveal",
                            choosingPlayer: resultInfo.targetPlayer,
                            choices: gameState.playerStates[resultInfo.targetPlayer]
                                .cards
                                .Select((card, index) => (card: card, index: index))
                                .Where(_ => _.card.isFaceDown)
                                .Select(_ =>
                                {
                                    var card = _.card;
                                    var cardIndex = _.index;
                                    return new Choice
                                    (
                                        title: cardDatabase.GetCard(card.id).cardName,
                                        description:
                                            $"Reveal and lose the power of {cardDatabase.GetCard(card.id).cardName}",
                                        justification: Choice.Justification.Free,
                                        onChosen: new ResultArguments.PlayerLosesInfluenceDueToAction()
                                        {
                                            cardIndex = _.index,
                                            targetPlayer = resultInfo.targetPlayer,
                                        }
                                    );
                                })
                        ),
                    };
                }
                case ResultArguments.StealCoins resultInfo:
                {
                    var coinsToTake = Math.Min(gameState.playerStates[resultInfo.targetPlayer].coinCount, resultInfo.amount);
                    return new Choice.Result
                    {
                        resultText = $"{currentPlayer.playerName} took {coinsToTake} coin(s) from {gameState.playerStates[resultInfo.targetPlayer].playerName}.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.PlayerStateEdit.CoinCount(resultInfo.targetPlayer,gameState.playerStates[resultInfo.targetPlayer].coinCount - coinsToTake),
                            new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                                gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                        },
                        newPhase = null, // end turn on completion
                    };
                }
                case ResultArguments.TakeCoins resultInfo:
                {
                    var coinsToTake = Math.Min(gameState.treasuryCoinCount, resultInfo.amount);
                    return new Choice.Result()
                    {
                        resultText = $"{currentPlayer.playerName} took {coinsToTake} coin(s) from the treasury.",
                        edits = new List<GameStateEdit>()
                        {
                            new GameStateEdit.TreasuryCoinCount(gameState.treasuryCoinCount - coinsToTake),
                            new GameStateEdit.PlayerStateEdit.CoinCount(gameState.currentPlayersTurn,
                                gameState.playerStates[gameState.currentPlayersTurn].coinCount + coinsToTake),
                        },
                        newPhase = null, // end turn on completion
                    };
                    
                }
                case ResultArguments.TargetPicking resultInfo:
                {
                    return new Choice.Result()
                    {
                        resultText = $"{currentPlayer.playerName} must pick a target for {resultInfo.action.name}",
                        edits = new List<GameStateEdit>(),
                        newPhase = new Phase
                        (
                            text: $"Select target player to [{resultInfo.action.name}]",
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
                                    ResultArguments onTarget;
                                    if (resultInfo.sourceCard.TryGetValue(out var sourceCard))
                                    {
                                        onTarget = new ResultArguments.DecisionToChallengeAction()
                                        {
                                            action = resultInfo.action,
                                            claimedCard = sourceCard,
                                            targetPlayer =
                                                targetPlayer, // by passing a non-null value we expect the challenge flow to handle blocking
                                            decidingPlayer = (gameState.currentPlayersTurn + 1) %
                                                             gameState.playerStates.Length,
                                        };
                                    }
                                    else
                                    {
                                        onTarget = new ResultArguments.DecisionToBlock()
                                        {
                                            action = resultInfo.action,
                                            targetedPlayer = targetPlayer,
                                        };
                                    }

                                    return new Choice
                                    (
                                        title: $"Target {playerState.playerName}",
                                        description: $"Target {playerState.playerName} with {resultInfo.action.name}",
                                        justification: Choice.Justification.Free,
                                        onChosen: onTarget
                                    );
                                })
                        ),
                    };
                }
                default:
                    throw new ArgumentOutOfRangeException($"Variant of {nameof(args)} not handled.");
            }
        }
        
        
        // Invariant: targetedPlayer != null iff action.actionType == TakeCoins or PlayerLosesInfluence
        public static ResultArguments ActionOutCome(ActionData action, int? targetedPlayer)
        {
            switch (action.actionType)
            {
                case ActionType.TakeCoins:
                    return new ResultArguments.TakeCoins() { amount = action.amount };
                case ActionType.StealCoins:
                    Debug.Assert(targetedPlayer != null, nameof(targetedPlayer) + " != null");
                    return new ResultArguments.StealCoins() { amount = action.amount, targetPlayer = targetedPlayer.Value};
                case ActionType.ExchangeCards:
                    return new ResultArguments.ExchangeCards() { cardTotal = action.amount };
                case ActionType.PlayerLosesInfluence:
                    Debug.Assert(targetedPlayer != null, nameof(targetedPlayer) + " != null");
                    return new ResultArguments.PlayerMustLoseInfluenceDueToAction() { targetPlayer = targetedPlayer.Value };// todo handle amount similar to how exchange does
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Phase? GetDiscardPhase(
            int cardsToDiscard,
            GameState gameState,
            CardDatabase cardDatabase)
        {
            if (cardsToDiscard == 0)
            {
                return null;
            }

            return new Phase
            (
                text:
                    $"Choose [{cardsToDiscard}] card{(cardsToDiscard > 1 ? "s" : "")} to put back in the court deck.",
                choosingPlayer: gameState.currentPlayersTurn,
                choices: gameState.playerStates[gameState.currentPlayersTurn].cards
                    .Select((card, index) => (card:card, index:index))
                    .Where(_ => _.card.isFaceDown)
                    .Select(_ =>
                    {
                        var card = _.card;
                        var cardIndex = _.index;
                        return new Choice
                        (
                            title: cardDatabase.GetCard(card.id).cardName,
                            description: $"Return {cardDatabase.GetCard(card.id).cardName} to the court deck",
                            justification: Choice.Justification.Free,
                            onChosen: new ResultArguments.DiscardedFromExchange()
                            {
                                cardIndex = cardIndex,
                                cardsLeftToDiscard = cardsToDiscard - 1,
                            }
                        );
                    })
            );
        }
    }
}