using System;
using System.Collections.Generic;
using Model.Actions;
using Model.Cards;
using Util;

namespace Model.State
{
    public struct Choice
    {
        public enum Justification
        {
            Free,
            Forced,
            UseCard,
            Bluff,
        }
        public struct Result
        {
            public List<GameStateEdit> edits;
            public string resultText;
            public Phase? newPhase; // empty represents new turn
        }
        
        public string title;
        public string description;
        public Justification justification;
        public ResultArguments onChosen;
    }

    public abstract class ResultArguments// acting as sum-type / discr union, could use visitor pattern, but don't want the boiler plate
    {
        public class GenericResult : ResultArguments // used for cases where we want to edit the result in advance
        {
            public Choice.Result result;
        }
        public class DecisionToChallengeAction : ResultArguments
        { // handle the continuation when generating choices based off of challenging player index
            public int decidingPlayer;
            public CardId claimedCard;
            public ActionData action; // we need to store enough info here to be able to generate ResultArguments for the action to resolve.
            public int? targetPlayer; // we could just store the args here, but i'd like to have the raw info if we need to use it
        }

        public class ActionChallenged : ResultArguments
        {
            public int challengingPlayer;
            public ActionData challengedAction;
            public CardId claimedCard;
            public int? targetPlayer;
        }

        public class ActionChallengeSucceeds : ResultArguments
        {
            public int revealedCardIndex; // revealed by the current player, not the challenging player
            public CardId claimedCard;
        }

        public class ActionChallengeFails : ResultArguments
        {
            public int revealedCardIndex;
            public int playerThatChallenged;
            public ActionData proposedAction;
            public int? targetPlayer;
        }
        
        public class DecisionToBlock : ResultArguments
        {
            public int targetedPlayer;
            public ActionData action;
        }

        public class TargetPicking : ResultArguments
        {
            public ActionData action;
            public Optional<CardId> sourceCard;
        }
        
        #region Resolved Action Results

        public class PlayerMustLoseInfluenceDueToAction : ResultArguments
        {
            public int targetPlayer;
        }

        public class PlayerLosesInfluenceDueToAction : ResultArguments
        {
            public int targetPlayer;
            public int cardIndex;
        }
        
        public class ExchangeCards : ResultArguments
        {
            public int cardTotal;
        }

        public class DiscardedFromExchange : ResultArguments
        {
            public int cardIndex;
            public int cardsLeftToDiscard;
        }
        
        public class StealCoins : ResultArguments
        {
            public int targetPlayer;
            public int amount;
        }
        
        public class TakeCoins : ResultArguments
        {
            public int amount;
        }
        
        #endregion // Resolved Action Results

        public class BlockAttempted : ResultArguments
        {
            public int targetedPlayer;
            public ActionData actionToBlock;
            public CardId cardClaimedToBlock;
        }
        
        public class BlockChallenged : ResultArguments
        {
            public int challengedBlocker;
            public ActionData actionBeingBlocked;
            public CardId cardBeingChallenged;
        }

        public class BlockChallengeFails : ResultArguments
        {
            public int blockingPlayer;
            public ActionData actionBlocked;
            public int revealedCardIndex;
        }
        
        public class BlockChallengeSucceeds : ResultArguments
        {
            public int challengedBlocker;
            public ActionData actionNotBlocked;
            public CardId cardClaimedToBlock;
            public int revealedCardIndex;
        }
        
        public class BlockNotChallenged : ResultArguments
        {
            public int blockingPlayer;
        }
    }
    
    public struct Phase
    {
        public string text;
        public int choosingPlayer;
        public IEnumerable<Choice> choices;
    }
}