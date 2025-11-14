using UnityEditor;
using UnityEngine;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    [CustomPropertyDrawer(typeof(QuadLayerData))]
    class QuadLayerDataDrawer : LayerDataDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue == null)
                return; // Fix crash on obsolete properties.

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(position, label);
                EditorGUI.indentLevel++;

                BlendTypePropertyField(property.FindPropertyRelative(k_BlendTypePropertyName));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_Size"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("m_ApplyTransformScale"));

                if (change.changed)
                    ApplyChangesWithReportStateChange(property);

                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
            }
        }
    }
}
