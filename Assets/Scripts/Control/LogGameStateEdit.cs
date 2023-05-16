using System;
using Model;
using Model.State;

namespace Control
{
    public static class LogGameStateEdit
    {
        public static string GetLogFromEdit(GameState gameState,GameConfig gameConfig, GameStateEdit edit)
        {
            switch (edit)
            {
                case GameStateEdit.CourtDeck courtDeck:
                    return "Court deck was shuffled";
                case GameStateEdit.PlayerStateEdit.AddCard addCard:
                    return $"{gameState.playerStates[addCard.playerIndex].playerName} gained a card";
                case GameStateEdit.PlayerStateEdit.CoinCount coinCount:
                    var oldCoinCount = gameState.playerStates[coinCount.playerIndex].coinCount;
                    var newCoinCount = coinCount.coinCount;
                    if (oldCoinCount > newCoinCount)
                    {
                        return
                            $"{gameState.playerStates[coinCount.playerIndex].playerName} lost {oldCoinCount - newCoinCount} coin(s)";
                    } 
                    
                    if (oldCoinCount == newCoinCount)
                        return "";

                    return $"{gameState.playerStates[coinCount.playerIndex].playerName} gained {newCoinCount - oldCoinCount} coin(s)";
                case GameStateEdit.PlayerStateEdit.RemoveCard removeCard:
                    return $"{gameState.playerStates[removeCard.playerIndex].playerName} lost a card";
                case GameStateEdit.PlayerStateEdit.RevealCard revealCard:
                    var revealedCard = gameState.playerStates[revealCard.playerIndex].cards[revealCard.cardIndex].id;
                    return $"{gameState.playerStates[revealCard.playerIndex].playerName} revealed a {gameConfig.cardDatabase.GetCard(revealedCard).cardName}";
                case GameStateEdit.TreasuryCoinCount treasuryCoinCount:
                    oldCoinCount = gameState.treasuryCoinCount;
                    newCoinCount = treasuryCoinCount.newCoinCount;
                    if (oldCoinCount > newCoinCount)
                    {
                        return
                            $"Treasury lost {oldCoinCount - newCoinCount} coin(s)";
                    }

                    if (oldCoinCount == newCoinCount)
                        return "";

                    return $"Treasury gained {newCoinCount - oldCoinCount} coin(s)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(edit));
            }
        }
    }
}