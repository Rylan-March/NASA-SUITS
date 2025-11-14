using System;
using System.Diagnostics;
using Unity.XR.CompositionLayers.Layers.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="ColorScaleBiasExtension" /> component.
    /// </summary>
    [CustomEditor(typeof(ColorScaleBiasExtension))]
    class ColorScaleBiasEditor : CompositionLayerExtensionEditor
    {
        static class Content
        {
            internal static GUIContent scaleLabel = new("Scale");
            internal static GUIContent biasLabel = new("Bias");
            internal static GUIContent resetLabel = new("Reset");
        }

        SerializedProperty m_ColorScale;
        SerializedProperty m_ColorBias;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ColorScale = serializedObject.FindProperty("m_Scale");
            m_ColorBias = serializedObject.FindProperty("m_Bias");
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            m_ColorScale.vector4Value = EditorGUILayout.ColorField(Content.scaleLabel, m_ColorScale.vector4Value, true, true, false);
            m_ColorScale.vector4Value = CompositionLayerEditorUtils.Clamp01(m_ColorScale.vector4Value);

            m_ColorBias.vector4Value = EditorGUILayout.ColorField(Content.biasLabel, m_ColorBias.vector4Value, true, true, false);
            m_ColorBias.vector4Value = CompositionLayerEditorUtils.Clamp01(m_ColorBias.vector4Value);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin.left = (int)EditorGUIUtility.labelWidth + 5;

            if (GUILayout.Button(Content.resetLabel, buttonStyle))
            {
                m_ColorScale.vector4Value = new Vector4(1f, 1f, 1f, 1f);
                m_ColorScale.vector4Value = CompositionLayerEditorUtils.Clamp01(m_ColorScale.vector4Value);

                m_ColorBias.vector4Value = new Vector4(0f, 0f, 0f, 0f);
                m_ColorBias.vector4Value = CompositionLayerEditorUtils.Clamp01(m_ColorBias.vector4Value);
            }

            EditorGUILayout.EndVertical();

            if (serializedObject.hasModifiedProperties)
                ApplyChangesWithReportStateChange();
        }
    }
}
