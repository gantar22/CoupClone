using System;
using System.Collections.Generic;
using Logic.Cards;

namespace Logic.State
{
    /* In a bigger board/card game project I'd probably not want to hard code the state data
     * in fields like this, preferring to instead use key value pairs or some internal ECS structure.
     * That'd be much more resilient to design changes, but less readable in the case of such a small game.
     */
    [Serializable]
    public class GameState
    {
        public PlayerState[] m_PlayerStates; // Each player's playerID is and index into this list.
        public int currentPlayersTurn; // Index into m_PlayerStates.
        // Design Invariants:
        // The treasury always has 50 - sum(player coins) coins.
        // The court deck always has exactly those cards that aren't in any player's hand.
        public int treasuryCoinCount;
        public List<CardId> courtDeck;
    }
}
