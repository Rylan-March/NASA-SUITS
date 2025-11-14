using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="TexturesExtension" /> component.
    /// </summary>
    [CustomEditor(typeof(TexturesExtension))]
    class TexturesExtensionEditor : CompositionLayerExtensionEditor
    {
        static List<Type> s_SupportedRectLayerTypes = new List<Type>
        {
            typeof(QuadLayerData), typeof(CylinderLayerData), typeof(ProjectionLayerData)
        };

        SerializedProperty m_LeftTextureProperty;
        SerializedProperty m_RightTextureProperty;
        SerializedProperty m_InEditorEmulationProperty;
        SerializedProperty m_SourceTextureProperty;
        SerializedProperty m_TargetEyeProperty;
        SerializedProperty m_CustomRectProperty;
        SerializedProperty m_CropToAspectProperty;

        SourceRectsDrawer m_SourceRectsDrawer;
        DestinationRectsDrawer m_DestinationRectsDrawer;

        CompositionLayer m_CompositionLayer;

        string[] k_InEditorEmulationOptions = new string[] {"Left Eye", "Right Eye"};

        /// <inheritdoc cref="Editor"/>>
        protected override void OnEnable()
        {
            base.OnEnable();

            m_LeftTextureProperty = serializedObject.FindProperty("m_LeftTexture");
            m_RightTextureProperty = serializedObject.FindProperty("m_RightTexture");
            m_InEditorEmulationProperty = serializedObject.FindProperty("m_InEditorEmulation");
            m_SourceTextureProperty = serializedObject.FindProperty("m_SourceTexture");
            m_TargetEyeProperty = serializedObject.FindProperty("m_TargetEye");
            m_CustomRectProperty = serializedObject.FindProperty("m_CustomRects");
            m_CropToAspectProperty = serializedObject.FindProperty("m_CropToAspect");

            m_SourceRectsDrawer = new SourceRectsDrawer(serializedObject);
            m_SourceRectsDrawer.Initialize();

            m_DestinationRectsDrawer = new DestinationRectsDrawer(serializedObject);
            m_DestinationRectsDrawer.Initialize();

            var extension = target as CompositionLayerExtension;
            var go = extension.gameObject;
            m_CompositionLayer = go.GetComponent<CompositionLayer>();

            // Ensure that if the source texture is added without Layer Data, we add a default one.
            if (m_CompositionLayer.LayerData == null)
            {
                m_CompositionLayer.ChangeLayerDataType(typeof(QuadLayerData));
                m_CompositionLayer.AddSuggestedExtensions();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_SourceTextureProperty, new GUIContent("Source"));
            serializedObject.ApplyModifiedProperties();

            var selectedSourceTexture = (TexturesExtension.SourceTextureEnum)m_SourceTextureProperty.enumValueIndex;
            if (selectedSourceTexture == TexturesExtension.SourceTextureEnum.AndroidSurface)
            {
                DrawExternalTextureUI();
            }
            else
            {
                DrawLocalTextureUI();

                if (m_CustomRectProperty.boolValue)
                {
                    DrawHelpBox();

                    m_SourceRectsDrawer.DrawGUI();

                    if (m_CompositionLayer != null
                        && m_CompositionLayer.LayerData.GetType() != typeof(ProjectionLayerData))
                    {
                        m_DestinationRectsDrawer.DrawGUI();
                    }
                }
            }
        }

        void DrawExternalTextureUI()
        {
            SerializedProperty resolutionProperty = serializedObject.FindProperty("m_Resolution");
            float originalLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Resolution");
            EditorGUIUtility.labelWidth = 15f;
            resolutionProperty.vector2Value = new Vector2(
                EditorGUILayout.FloatField("W", resolutionProperty.vector2Value.x),
                EditorGUILayout.FloatField("H", resolutionProperty.vector2Value.y)
            );
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = originalLabelWidth;
            EditorGUILayout.PropertyField(m_CropToAspectProperty, new GUIContent("Maintain Aspect Ratio"));


            serializedObject.ApplyModifiedProperties();

            if (m_CompositionLayer == null || m_CompositionLayer.LayerData == null)
                return;

            var layerDataType = m_CompositionLayer.LayerData.GetType();
            var layerDataName = CompositionLayerUtils.GetLayerDescriptor(layerDataType).Name;
            if ((layerDataType != typeof(QuadLayerData)) && (layerDataType != typeof(CylinderLayerData)))
                EditorGUILayout.HelpBox($"{layerDataName} type does not support the use of Android Surface.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawLocalTextureUI()
        {
            if (m_CompositionLayer == null || m_CompositionLayer.LayerData == null)
                return;

            var layerDataType = m_CompositionLayer.LayerData.GetType();
            var selectedTargetEye = (TexturesExtension.TargetEyeEnum)m_TargetEyeProperty.enumValueIndex;

            EditorGUI.BeginChangeCheck();

            if ((layerDataType == typeof(ProjectionLayerData)))
                selectedTargetEye = TexturesExtension.TargetEyeEnum.Individual;
            else
                selectedTargetEye = TexturesExtension.TargetEyeEnum.Both;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            if (selectedTargetEye == TexturesExtension.TargetEyeEnum.Both)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" "); // Used to align Rect with Custom Rects with other properties

                Rect textureRect = EditorGUILayout.GetControlRect(GUILayout.Height(64), GUILayout.Width(64));
                var type = layerDataType == typeof(CubeProjectionLayerData) ? typeof(Cubemap) : typeof(Texture);
                var newTexture = EditorGUI.ObjectField(textureRect, m_LeftTextureProperty.objectReferenceValue, type, true);
                if (!IsValidTexture(newTexture as Texture))
                {
                    m_LeftTextureProperty.objectReferenceValue = null;
                    m_RightTextureProperty.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (newTexture != m_LeftTextureProperty.objectReferenceValue)
                {
                    m_LeftTextureProperty.objectReferenceValue = newTexture;
                    m_RightTextureProperty.objectReferenceValue = m_LeftTextureProperty.objectReferenceValue;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (selectedTargetEye == TexturesExtension.TargetEyeEnum.Individual)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");
                GUILayout.Space(2);

                var type = layerDataType == typeof(CubeProjectionLayerData) ? typeof(Cubemap) : typeof(Texture);
                Rect leftTextureRect = EditorGUILayout.GetControlRect(GUILayout.Height(64), GUILayout.Width(64));
                var newLeftTexture = EditorGUI.ObjectField(leftTextureRect, m_LeftTextureProperty.objectReferenceValue, type, true);
                if (!IsValidTexture(newLeftTexture as Texture))
                {
                    m_LeftTextureProperty.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (newLeftTexture != m_LeftTextureProperty.objectReferenceValue) m_LeftTextureProperty.objectReferenceValue = newLeftTexture;

                GUILayout.FlexibleSpace();

                Rect rightTextureRect = EditorGUILayout.GetControlRect(GUILayout.Height(64), GUILayout.Width(64));
                var newRightTexture = EditorGUI.ObjectField(rightTextureRect, m_RightTextureProperty.objectReferenceValue, type, true);
                if (!IsValidTexture(newRightTexture as Texture))
                {
                    m_RightTextureProperty.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (newRightTexture != m_RightTextureProperty.objectReferenceValue) m_RightTextureProperty.objectReferenceValue = newRightTexture;

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("In-Editor Emulation");
                GUILayout.Space(2);
                for (int i = 0; i < k_InEditorEmulationOptions.Length; i++)
                {
                    bool isSelected = m_InEditorEmulationProperty.intValue == i;
                    if (GUILayout.Toggle(isSelected, k_InEditorEmulationOptions[i], EditorStyles.radioButton))
                    {
                        m_InEditorEmulationProperty.intValue = i;
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            if(layerDataType == typeof(QuadLayerData) || layerDataType == typeof(CylinderLayerData))
                EditorGUILayout.PropertyField(m_CropToAspectProperty, new GUIContent("Maintain Aspect Ratio"));

            if(layerDataType != typeof(CubeProjectionLayerData) && layerDataType != typeof(EquirectMeshLayerData))
                EditorGUILayout.PropertyField(m_CustomRectProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is CompositionLayerExtension layerExtension)
                        layerExtension.ReportStateChange();
                }
            }
        }

        void DrawHelpBox()
        {
            if (m_CompositionLayer.LayerData == null)
                return;

            var layerDataType = m_CompositionLayer.LayerData.GetType();
            if (s_SupportedRectLayerTypes.Contains(layerDataType))
                return;

            var layerDataName = CompositionLayerUtils.GetLayerDescriptor(layerDataType).Name;

            EditorGUILayout.HelpBox($"{layerDataName} type may not support the use of Custom Rects.", MessageType.Info);
        }

        private bool IsValidTexture(Texture texture)
        {
            if(texture == null)
            {
                return true;
            }
            if (m_CompositionLayer.LayerData.GetType() != typeof(CubeProjectionLayerData) && texture is Cubemap)
            {
                Debug.LogError("Cubemap textures are not supported for this layer type.");
                return false;
            }
            else if (m_CompositionLayer.LayerData.GetType() == typeof(CubeProjectionLayerData) && !(texture is Cubemap))
            {
                Debug.LogError("Cube Projection Layer requires a Cubemap texture.");
                return false;
            }

            return true;
        }
    }
}
