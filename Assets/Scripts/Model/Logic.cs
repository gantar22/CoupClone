using System;
using System.Linq;
using Model.Actions;
using Model.Cards;
using Model.State;
using UnityEngine;

namespace Model
{
    public static class Logic
    {
             
        // Invariant: targetedPlayer != null iff action.actionType == TakeCoins or PlayerLosesInfluence
        public static State.Results.Result ActionOutCome(ActionData action, int? targetedPlayer)
        {
            switch (action.actionType)
            {
                case ActionType.TakeCoins:
                    return new State.Results.TakeCoins() { amount = action.amount };
                case ActionType.StealCoins:
                    Debug.Assert(targetedPlayer != null, nameof(targetedPlayer) + " != null");
                    return new State.Results.StealCoins() { amount = action.amount, targetPlayer = targetedPlayer.Value};
                case ActionType.ExchangeCards:
                    return new State.Results.ExchangeCards() { cardTotal = action.amount };
                case ActionType.PlayerLosesInfluence:
                    Debug.Assert(targetedPlayer != null, nameof(targetedPlayer) + " != null");
                    return new State.Results.PlayerMustLoseInfluenceDueToAction() { targetPlayer = targetedPlayer.Value };// todo handle amount similar to how exchange does
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Phase? GetDiscardPhase(
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
                            onChosen: new State.Results.DiscardedFromExchange()
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