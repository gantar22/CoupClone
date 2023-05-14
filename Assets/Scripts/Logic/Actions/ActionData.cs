using System;
using UnityEngine;
using Logic.Cards;

namespace Logic.Actions
{
    [Serializable]
    public enum ActionType
    {
        TakeCoins,
        StealCoins,
        ExchangeCards,
        PlayerLosesInfluence
    }
    
    [Serializable]
    public struct ActionData
    {
        public string name;
        [TextArea(2,4)]
        public string description;
        public ActionType actionType;
        public int coinCost;
        public CardId[] cardsThatCanCounterThis;
        public int amount;
        [Tooltip("If this is set, this action must be taken if the coin cost can be paid.")]
        public Util.OptionalInt obligatoryCoinCost;
    }
}