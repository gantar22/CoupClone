using System;
using System.Collections.Generic;
using System.Linq;
using Model.Cards;

namespace Model.State
{
    /* In a bigger board/card game project I'd probably not want to hard code the state data
     * in fields like this, preferring to instead use key value pairs or some internal ECS structure.
     * That'd be much more resilient to design changes, but less readable in the case of such a small game.
     */
    [Serializable]
    public class GameState
    {
        public PlayerState[] playerStates; // Each player's playerID is and index into this list.
        public int currentPlayersTurn; // Index into m_PlayerStates.
        // Design Invariants:
        // The treasury always has 50 - sum(player coins) coins.
        // The court deck always has exactly those cards that aren't in any player's hand.
        public int treasuryCoinCount;
        public List<CardId> courtDeck;

        public GameState Clone() // In a more performance critical game I'd be a bit picky about allocation, but we'll just make copies freely for now.
        {
            return new GameState()
            {
                playerStates = playerStates.ToArray(),
                currentPlayersTurn = currentPlayersTurn,
                treasuryCoinCount = treasuryCoinCount,
                courtDeck = courtDeck.ToList(),
            };
        }
    }

    public abstract class GameStateEdit // using this like a discriminated union / quick and dirty visitor pattern
    {    
        public abstract class PlayerStateEdit : GameStateEdit
        {
            public int playerIndex;        
            
            public class CoinCount : PlayerStateEdit
            {
                public int coinCount;

                public CoinCount(int playerIndex, int coinCount)
                {
                    this.playerIndex = playerIndex;
                    this.coinCount = coinCount;
                }
            }
    
            public class RevealCard : PlayerStateEdit
            {
                public int cardIndex;

                public RevealCard(int playerIndex, int cardIndex)
                {
                    this.playerIndex = playerIndex;
                    this.cardIndex = cardIndex;
                }
            }
    
            public class AddCard : PlayerStateEdit
            {
                public CardId cardId;
                
                public AddCard(int playerIndex, CardId cardId)
                {
                    this.playerIndex = playerIndex;
                    this.cardId = cardId;
                }
            }

            public class RemoveCard : PlayerStateEdit
            {
                public int cardIndexToRemove;
                
                public RemoveCard(int playerIndex, int cardIndexToRemove)
                {
                    this.playerIndex = playerIndex;
                    this.cardIndexToRemove = cardIndexToRemove;
                }
            }
        }

        public class TreasuryCoinCount : GameStateEdit
        {
            public int newCoinCount;
            
            public TreasuryCoinCount(int newCoinCount)
            {
                this.newCoinCount = newCoinCount;
            }
        }
    
        public class CourtDeck : GameStateEdit
        {
            public List<CardId> newDeck;
            
            public CourtDeck(List<CardId> newDeck)
            {
                this.newDeck = newDeck;
            }
        }
    }
}