using UnityEngine;

namespace UI
{
    public class BoardStatusUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text m_StatusText = default;
    
        public void SetStatus(int treasuryCount, int courtDeckSize)
        {
            m_StatusText.text = $"Board Info:\nTreasury Count: {treasuryCount}\nCourt Deck Size: {courtDeckSize}";
        }
    }
}
