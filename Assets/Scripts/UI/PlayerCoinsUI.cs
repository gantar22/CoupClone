using UnityEngine;

namespace UI
{
    public class PlayerCoinsUI : MonoBehaviour
    {
        [SerializeField] GameObject m_CoinTemplate = default;
        [SerializeField] Transform m_CoinContainer = default;
        [SerializeField] TMPro.TMP_Text m_CoinCountText = default;
        
        public void SetCoinCount(int coinCount)
        {
            foreach (Transform child in m_CoinContainer)
            {
                Destroy(child.gameObject);  // todo: object pooling
            }

            for (int i = 0; i < coinCount; i++)
            {
                Instantiate(m_CoinTemplate, m_CoinContainer);
            }
            
            m_CoinCountText.text = $"Your Coins: {coinCount}";
        }
    }
}
