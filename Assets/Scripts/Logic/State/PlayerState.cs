using System;
using System.Collections.Generic;
using Logic.Cards;

namespace Logic.State
{
    /*
     * Holds the state of a players inventory at the beginning of a turn.
     */
    [Serializable]
    public struct PlayerState
    {
        public int coinCount;
        // By using an array here instead of 2 fields, we're not enforcing
        // that each player has 2 cards or even that all
        // players have the same number of cards, but the
        // game logic should be able to handle those cases.
        public (CardId id, bool isFaceDown)[] cards;
    }
}
