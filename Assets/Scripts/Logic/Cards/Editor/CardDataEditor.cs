using System;
using UnityEditor;

namespace Logic.Cards.Editor
{
    //[CustomEditor(typeof(CardData))]
    /*public class CardDataEditor : UnityEditor.Editor
    {
        SerializedProperty m_Id;
        SerializedProperty m_HasAction;
        SerializedProperty m_Action;
        
        private void OnEnable()
        {
            m_Id = serializedObject.FindProperty("m_Id");
            m_HasAction = serializedObject.FindProperty("m_HasAction");
            m_Action = serializedObject.FindProperty("m_Action");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Id);
            EditorGUILayout.PropertyField(m_HasAction);
            if (m_HasAction.boolValue)
            {
                EditorGUILayout.PropertyField(m_Action);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
    */
}