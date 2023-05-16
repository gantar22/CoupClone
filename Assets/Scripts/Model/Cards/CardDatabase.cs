using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.Cards
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Coup/Cards/Database")]
    public class CardDatabase : ScriptableObject
    {
        [SerializeField] private CardData[] m_Cards;
        private Dictionary<CardId, CardData> m_CardMap = null;
        public Dictionary<CardId, CardData> cardMap
        {
            get
            {
              if(m_CardMap == null)
                  m_CardMap = m_Cards.ToDictionary(c => c.id, c => c);
              return m_CardMap;
            }
        }

        public CardData GetCard(CardId id) => cardMap[id];

        private void ValidateCards()
        {            
            void ValidateIdExistence()
            {
                if (m_Cards.Any(c => c.id == null))
                {
                    Debug.LogError("CardDatabase: Card with null id found!");
                }
            }
            void ValidateUniqueIds()
            {
                var ids = m_Cards.Select(c => c.id).ToArray();
                var uniqueIds = ids.Distinct().ToArray();
                if (ids.Length != uniqueIds.Length)
                {
                    Debug.LogError("CardDatabase: Duplicate card ids found!");
                }
            }


         
            ValidateIdExistence();
            ValidateUniqueIds();
        }
    }
}