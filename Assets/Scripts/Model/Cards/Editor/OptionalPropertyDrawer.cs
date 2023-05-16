#if UNITY_EDITOR
using UnityEditor;
using Util;

namespace Model.Cards.Editor
{
    
    [CustomPropertyDrawer(typeof(OptionalActionData))]
    public class OptionalPropertyDrawer : Util.Editor.OptionalPropertyDrawer { }
}
#endif