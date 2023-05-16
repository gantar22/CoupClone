using Model.Actions;
using UnityEngine;

namespace Model.Cards
{
    [CreateAssetMenu(fileName = "Card", menuName = "Coup/Cards/Card")]
    public class CardData : ScriptableObject
    {
        [SerializeField] private CardId m_Id = null;
        public CardId id => m_Id;
        
        [SerializeField] private string m_CardName = null;
        public string cardName => m_CardName;
        [SerializeField] Color m_BaseColor = Color.black;
        public Color baseColor => m_BaseColor;
        [SerializeField] private OptionalActionData m_Action = new OptionalActionData(); // represents null by default
        public ActionData? action => m_Action.TryGetValue(out var outAction) ? (ActionData?)outAction : default;
    }
}