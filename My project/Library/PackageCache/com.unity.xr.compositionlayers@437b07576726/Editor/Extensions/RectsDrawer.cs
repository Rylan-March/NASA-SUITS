using System;
using Unity.XR.CompositionLayers.Layers.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    /// <summary>
    /// Base custom drawer for rects.
    /// </summary>
    abstract class RectsDrawer
    {
        internal class RectData
        {
            public string title;
            public SerializedProperty prop;
            public bool visible;
            public Texture tex;
            public Rect moveRect;
            public Rect sizeRect;
            public bool isTrackingMouse;
        }

        const int k_TextureEditorWidth = 100;
        const int k_TextureEditorHeight = 175;
        const int k_TexturePreviewWidth = 100;
        const int k_RectInset = 5;

        TexturesExtension m_TexturesExtension;
        SerializedProperty m_InEditorEmulationProperty;
        Vector2 m_CurrentMousePosition = Vector2.zero;
        Material m_RectEditMaterial;

        Texture m_DefaultTexture;

        RectData[] m_EyeRectData = { new(), new() };

        SerializedObject m_SerializedObject;
        string m_RightEyeName;
        string m_LeftEyeName;
        string m_Title;
        static readonly int k_RectBounds = Shader.PropertyToID("_RectBounds");

        protected SerializedObject serializedObject => m_SerializedObject;
        protected string rightEyeName => m_RightEyeName;
        protected string leftEyeName => m_LeftEyeName;
        protected RectData[] eyeRectData => m_EyeRectData;
        protected TexturesExtension texturesExtension => m_TexturesExtension;

        static class Content
        {
            public static GUIContent defaultImage = EditorGUIUtility.IconContent("scenevis_visible_hover@2x");
        }

        static class Styles
        {
            public static GUIStyle centeredText;
        }

        protected RectsDrawer(SerializedObject serializedObject, string title, string leftEyeName, string rightEyeName)
        {
            m_SerializedObject = serializedObject;
            m_Title = title;
            m_LeftEyeName = leftEyeName;
            m_RightEyeName = rightEyeName;
        }

        static void InitStyles()
        {
            if (Styles.centeredText == null)
            {
                Styles.centeredText = new GUIStyle(EditorStyles.label);
                Styles.centeredText.alignment = TextAnchor.MiddleCenter;
            }
        }

        internal void Initialize()
        {
            if (!m_RectEditMaterial)
            {
                m_RectEditMaterial = new Material(Shader.Find("Unlit/XRCompositionLayers/Editor/Rects"));
            }

            var assets = AssetDatabase.FindAssets("1K_UV_checker");
            if (assets.Length > 0)
            {
                var assetGuid = assets[0];
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                m_DefaultTexture = tex ? tex : Content.defaultImage.image;
            }

            m_TexturesExtension = serializedObject.targetObject as TexturesExtension;

            m_InEditorEmulationProperty = serializedObject.FindProperty("m_InEditorEmulation");

            m_EyeRectData[0].prop = serializedObject.FindProperty(leftEyeName);
            m_EyeRectData[0].tex = m_DefaultTexture;
            m_EyeRectData[0].visible = true;

            m_EyeRectData[1].prop = serializedObject.FindProperty(rightEyeName);
            m_EyeRectData[1].tex = m_DefaultTexture;
            m_EyeRectData[1].visible = true;
        }

        protected virtual void PopulateShaderProperties(Material material, int index, RectData rectData)
        {
            var propRect = rectData.prop.rectValue;
            material.SetVector(k_RectBounds, new Vector4(propRect.x, propRect.y, propRect.width, propRect.height));
        }

        void RenderTextureEditor(int index, RectData rectData)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(k_TextureEditorWidth), GUILayout.Height(k_TextureEditorHeight));
            EditorGUILayout.LabelField(rectData.title, Styles.centeredText, GUILayout.Width(k_TextureEditorWidth), GUILayout.ExpandWidth(false));
            var r = EditorGUILayout.GetControlRect(GUILayout.Width(k_TexturePreviewWidth), GUILayout.Height(k_TexturePreviewWidth));

            GUI.Box(r, string.Empty);
            if (rectData.visible)
            {
                r.x += k_RectInset;
                r.y += k_RectInset;
                r.width -= (2 * k_RectInset);
                r.height -= (2 * k_RectInset);

                PopulateShaderProperties(m_RectEditMaterial, index, rectData);
                EditorGUI.DrawPreviewTexture(r, rectData.tex, m_RectEditMaterial);

                var e = Event.current;
                switch (e.type)
                {
                    case EventType.MouseDown:
                        m_CurrentMousePosition = e.mousePosition;
                        if (r.Contains(m_CurrentMousePosition))
                        {
                            rectData.isTrackingMouse = true;
                        }
                        break;

                    case EventType.MouseDrag:
                        if (rectData.isTrackingMouse)
                        {
                            var newMousePosition = e.mousePosition;
                            if (r.Contains(newMousePosition))
                            {
                                var delta = newMousePosition - m_CurrentMousePosition;

                                if (rectData.sizeRect.Contains(m_CurrentMousePosition))
                                {
                                    rectData.sizeRect.x += delta.x;
                                    rectData.sizeRect.y += delta.y;

                                    rectData.moveRect.width += delta.x;
                                    rectData.moveRect.height += delta.y;
                                }
                                else if (rectData.moveRect.Contains(m_CurrentMousePosition))
                                {
                                    rectData.moveRect.x += delta.x;
                                    rectData.moveRect.y += delta.y;

                                    rectData.sizeRect.x += delta.x;
                                    rectData.sizeRect.y += delta.y;
                                }

                                var newRect = rectData.prop.rectValue;

                                newRect.x = (rectData.moveRect.x - r.x) / r.width;
                                newRect.y = (rectData.moveRect.y - r.y) / r.height;
                                newRect.width = rectData.moveRect.width / r.width;
                                newRect.height = rectData.moveRect.height / r.height;

                                rectData.prop.rectValue = CompositionLayerEditorUtils.Clamp01(newRect);

                                m_CurrentMousePosition = newMousePosition;
                            }
                        }
                        break;

                    case EventType.MouseUp:
                        rectData.isTrackingMouse = false;
                        m_CurrentMousePosition = Vector2.zero;
                        break;

                    case EventType.Repaint:
                        var left = r.x + (rectData.prop.rectValue.x * r.width);
                        var top = r.y + (rectData.prop.rectValue.y * r.height);
                        var width = (rectData.prop.rectValue.width * r.width);
                        var height = (rectData.prop.rectValue.height * r.height);

                        var right = left + width;
                        var bottom = top + height;

                        rectData.moveRect = new Rect(left, top, width, height);
                        rectData.sizeRect = new Rect(right - 2 * k_RectInset, bottom - 2 * k_RectInset, 2 * k_RectInset, 2 * k_RectInset);

                        EditorGUIUtility.AddCursorRect(rectData.moveRect, MouseCursor.MoveArrow);
                        EditorGUIUtility.AddCursorRect(rectData.sizeRect, MouseCursor.ResizeUpLeft);
                        break;
                }

                rectData.prop.rectValue = EditorGUILayout.RectField(rectData.prop.rectValue, GUILayout.Width(100));
                rectData.prop.rectValue = CompositionLayerEditorUtils.Clamp01(rectData.prop.rectValue);
            }
            else
            {
                EditorGUI.LabelField(r, "No texture.", Styles.centeredText);
            }

            EditorGUILayout.EndVertical();
        }

        internal void DrawGUI()
        {
            InitStyles();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(65f);
            EditorGUILayout.PrefixLabel(m_Title, EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            m_EyeRectData[0].tex = m_TexturesExtension.LeftTexture;
            m_EyeRectData[0].visible = m_EyeRectData[0].tex && (m_InEditorEmulationProperty.intValue == 0);
            m_EyeRectData[1].tex = m_TexturesExtension.RightTexture;
            m_EyeRectData[1].visible = m_EyeRectData[1].tex && (m_InEditorEmulationProperty.intValue == 1);

            GUILayout.Space(-4);

            RenderTextureEditor(m_InEditorEmulationProperty.intValue, m_EyeRectData[m_InEditorEmulationProperty.intValue]);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is CompositionLayerExtension layerExtension)
                        layerExtension.ReportStateChange?.Invoke();
                }
            }
        }
    }
}
