using System.Collections;
using System.Collections.Generic;
using Logic;
using UnityEngine;

namespace Logic.Actions
{
    [CreateAssetMenu(fileName = "ActionDatabase", menuName = "Coup/Actions/ActionDatabase")]
    public class ActionDatabase : ScriptableObject
    {
        [SerializeField] private ActionData[] m_DefaultActions = default;
        public ActionData[] defaultActions => m_DefaultActions;
    }
}
