using System.Linq;
using UnityEngine;

namespace Logic.Cards
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Coup/Cards/Database")]
    public class CardDatabase : ScriptableObject
    {
        [SerializeField] private CardData[] m_Cards;

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