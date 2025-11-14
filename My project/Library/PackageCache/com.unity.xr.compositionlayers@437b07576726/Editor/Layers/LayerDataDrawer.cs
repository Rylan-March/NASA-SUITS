using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Services.Editor;
using Unity.XR.CompositionLayers.Layers.Internal.Editor;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// Base custom <see cref="PropertyDrawer"/> for <see cref="LayerData"/> to be displayed in an editor inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerData), true)]
    internal class LayerDataDrawer : PropertyDrawer
    {
        internal const string DisplayAnglePreferenceToolTip = "You can change the way angles are displayed in 'Preferences > " +
            "Composition Layer Preferences > Display Angles As' dropdown.";

        internal const string k_UnsupportedBlendTypeMessage = "This blend type isn't supported on {0}.";

        internal const string k_BlendTypeDisplayName = "Blend Type";
        internal const string k_BlendTypeTooltip = "Blend type for drawing. Most platforms support Alpha & Premultiply.";

        protected const string k_BlendTypePropertyName = "m_BlendType";

        /// <inheritdoc cref="PropertyDrawer"/>
        /// <remarks>Using IMGUI for this due to issues with re-creating the ui element in the inspector when the layer
        /// data type changes.</remarks>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue == null)
                return; // Fix crash on obsolete properties.

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(position, label);
                EditorGUI.indentLevel++;

                if (property.hasVisibleChildren)
                {
                    var enumerator = property.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var currentProperty = enumerator.Current as SerializedProperty;
                        if (currentProperty == null)
                            continue;

                        if (currentProperty.name == k_BlendTypePropertyName)
                            BlendTypePropertyField(currentProperty);
                        else
                            EditorGUILayout.PropertyField(currentProperty);
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("No modifiable properties");
                    EditorGUI.EndDisabledGroup();
                }

                if (change.changed)
                    ApplyChangesWithReportStateChange(property);

                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
            }
        }

        /// <summary>
        /// Used to <see cref="SerializedObject.ApplyModifiedProperties"/> then
        /// <see cref="LayerData.ReportStateChange"/> so any changes made in the editor are reported
        /// back to the <see cref="Unity.XR.CompositionLayers.Services.CompositionLayerManager"/>
        /// </summary>
        /// <param name="property">Property the <see cref="PropertyDrawer"/> is drawing.</param>
        protected static void ApplyChangesWithReportStateChange(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();
            foreach (var targetObject in serializedObject.targetObjects)
            {
                if (targetObject is CompositionLayer layer)
                    layer.ReportStateChange();
            }
        }

        /// <summary>
        /// Drawing custom BlendType enum popup on EditorGUILayout.
        /// </summary>
        /// <param name="blendTypeProperty">SerializedProperty for m_BlendType.</param>
        protected static void BlendTypePropertyField(SerializedProperty blendTypeProperty)
        {
            if (blendTypeProperty == null)
                return;

            blendTypeProperty.intValue = (int)(BlendType)EditorGUILayout.EnumPopup(
                new GUIContent(k_BlendTypeDisplayName, k_BlendTypeTooltip),
                (BlendType)blendTypeProperty.intValue,
                IsSelectableBlendType,
                false);

            DrawUnsupportedBlendTypeMesssage((BlendType)blendTypeProperty.intValue);
        }

        static bool IsSelectableBlendType(Enum value)
        {
            var activeProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activeProviders == null || activeProviders.Count == 0)
                return true;

            foreach (var activeProvider in activeProviders)
            {
                var supportedBlendTypes = activeProvider.SupportedBlendTypes;
                if (supportedBlendTypes == null || supportedBlendTypes.Contains((BlendType)value))
                    return true;
            }

            return false;
        }

        static void DrawUnsupportedBlendTypeMesssage(BlendType blendType)
        {
            var activeProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activeProviders == null || activeProviders.Count == 0)
                return;

            // Checking for each platform.
            foreach (var activeProvider in activeProviders)
            {
                var supportedBlendTypes = activeProvider.SupportedBlendTypes;
                if (supportedBlendTypes == null || supportedBlendTypes.Contains(blendType))
                    continue;

                UIHelper.GUIWarning(k_UnsupportedBlendTypeMessage, activeProvider);
            }
        }
    }
}
