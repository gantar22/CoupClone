using Logic.Actions;
using Logic.Cards;
using UnityEngine;

namespace Logic
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Coup/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private ActionDatabase m_ActionDatabase = default;
        public ActionDatabase actionDatabase => m_ActionDatabase;
        
        [SerializeField] private CardDatabase m_CardDatabase = default;
        public CardDatabase cardDatabase => m_CardDatabase;
        
        [SerializeField] private int m_PlayerCount = 5;
        public int playerCount => m_PlayerCount;
        
        [SerializeField] private int m_CardsPerPlayer = 2;
        public int cardsPerPlayer => m_CardsPerPlayer;

        [SerializeField] private int m_CoinTotal = 50;
        public int coinTotal => m_CoinTotal;
    }
}