#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Util.Editor
{
    [CustomPropertyDrawer(typeof(OptionalInt))]
    public class OptionalPropertyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            var propPos = position;
            propPos.width = position.width - 20;

            var enabledPos = position;
            enabledPos.width = 20;
            enabledPos.x = (position.x + position.width) - 16f;

            var enabledProp = property.FindPropertyRelative("hasValue");
            if (enabledProp.boolValue)
            {
                var valueProp = property.FindPropertyRelative("value");
                EditorGUI.PropertyField(propPos, valueProp, label, true);
            }
            else
            {
                EditorGUI.LabelField(propPos, label);
            }
            enabledProp.boolValue = GUI.Toggle(enabledPos, enabledProp.boolValue, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var enabledProp = property.FindPropertyRelative("hasValue");
            return enabledProp.boolValue ? EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label, true) : EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif