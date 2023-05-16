using UnityEngine;

namespace UI
{
    public class PlayerTurnUI : MonoBehaviour
    {
        [SerializeField] private GameObject m_Icon = default;

        public void SetPlayersTurn(bool isPlayersTurn)
        {
            m_Icon.SetActive(isPlayersTurn);
        }
    }
}
