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
                return new Phase
                (
                    text: $"You must perform {forcedAction.action.name}.",
                    choosingPlayer: gameState.currentPlayersTurn,
                    choices: new[] { forcedAction.choice }
                );
            } // else return all of the normal actions

            return new Phase
            (
                text: "Choose an action for your turn",
                choosingPlayer: gameState.currentPlayersTurn,
                choices: allActionsWithChoices.Select(_ => _.choice)
            );
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
        
        // returns null if the cost can't be paid
        // sourceCard is null when the action is a default action
        static Choice? GenerateChoiceFromAction(ActionData action, GameState gameState, CardDatabase cardDatabase, CardId sourceCardOption = null)
        {
            var playerState = gameState.playerStates[gameState.currentPlayersTurn];
            if (action.coinCost > playerState.coinCount)
                return null; // Choice can't be afforded

            Model.State.Results.Result onSelect;
            switch (action.actionType)
            {
                case ActionType.StealCoins:
                case ActionType.PlayerLosesInfluence:
                    // pick target
                    onSelect = new Model.State.Results.TargetPicking
                    (
                        action: action,
                        sourceCard: new Optional<CardId>(sourceCardOption)
                    );
                    break;
                case ActionType.TakeCoins:
                case ActionType.ExchangeCards:
                    if (sourceCardOption)
                    {
                        onSelect = new Model.State.Results.DecisionToChallengeAction
                        (
                            action: action,
                            claimedCard: sourceCardOption,
                            decidingPlayer: (gameState.currentPlayersTurn + 1) % gameState.playerStates.Length,
                            targetPlayer: null
                        );
                    }
                    else
                    {
                        onSelect = Model.Logic.ActionOutCome(action,targetedPlayer:null);
                    }

                    break;
                default:
                    throw new ArgumentException($"Failed to handle case of {nameof(action)}: {action.actionType}.");
            }
            return new Choice()
            {
                title = action.name,
                description = action.description,
                justification = GenerateChoiceJustification(action,playerState, sourceCardOption),
                onChosen = onSelect,
            };
        }
    }
}