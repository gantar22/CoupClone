using System;
using Model.State;

namespace Control
{
    public static class PerformGameStateEdit
    {
        public static GameState PerformEdit(GameState gameState, GameStateEdit edit)
        {
            var result = gameState.Clone();
            switch (edit)
            {
                case GameStateEdit.CourtDeck courtDeck:
                    result.courtDeck = courtDeck.newDeck;
                    break;
                case GameStateEdit.PlayerStateEdit.AddCard addCard:
                    result.playerStates[addCard.playerIndex].cards.Add((addCard.cardId, true));
                    break;
                case GameStateEdit.PlayerStateEdit.CoinCount coinCount:
                    result.playerStates[coinCount.playerIndex].coinCount = coinCount.coinCount;
                    break;
                case GameStateEdit.PlayerStateEdit.RemoveCard removeCard:
                    result.playerStates[removeCard.playerIndex].cards.RemoveAt(removeCard.cardIndexToRemove);
                    break;
                case GameStateEdit.PlayerStateEdit.RevealCard revealCard:
                    result.playerStates[revealCard.playerIndex].cards[revealCard.cardIndex] = (result.playerStates[revealCard.playerIndex].cards[revealCard.cardIndex].id, false);
                    break;
                case GameStateEdit.TreasuryCoinCount treasuryCoinCount:
                    result.treasuryCoinCount = treasuryCoinCount.newCoinCount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edit));
            }
            return result;
        }
    }
}