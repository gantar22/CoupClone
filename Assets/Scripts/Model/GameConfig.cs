using Model.Actions;
using Model.Cards;
using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Coup/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private ActionDatabase m_ActionDatabase = default;
        public ActionDatabase actionDatabase => m_ActionDatabase;
        
        [SerializeField] private CardDatabase m_CardDatabase = default;
        public CardDatabase cardDatabase => m_CardDatabase;
        
        [SerializeField] string[] m_AIPlayerNames = default;
        public string[] aiPlayerNames => m_AIPlayerNames;
        
        [SerializeField] private int m_CardsPerPlayer = 2;
        public int cardsPerPlayer => m_CardsPerPlayer;

        [SerializeField] private int m_CoinTotal = 50;
        public int coinTotal => m_CoinTotal;
        [SerializeField] private int m_CardRepeatCount = 3;
        public int cardRepeatCount => m_CardRepeatCount;
        [SerializeField, Range(0, 1)] private float m_AIBluffProbability = 0.15f;
        public float aiBluffProbability => m_AIBluffProbability;
        [SerializeField] private float m_TurnWaitTime = 2f;
        public float turnWaitTime => m_TurnWaitTime;
    }
}