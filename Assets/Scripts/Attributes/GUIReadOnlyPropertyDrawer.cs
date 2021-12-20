#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Attributes
{
    [CustomPropertyDrawer(typeof(GUIReadOnlyAttribute))]
    public class GUIReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            //base.OnGUI(position, property, label);
            GUI.enabled = true;
        }
    }
}
#endif
