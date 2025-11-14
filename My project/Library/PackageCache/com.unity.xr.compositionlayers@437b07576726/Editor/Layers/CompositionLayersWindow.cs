using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Editor;
using Unity.XR.CompositionLayers.Services;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    [EditorWindowTitle(
        title = k_WindowTitle,
        icon = "Packages/com.unity.xr.compositionlayers/Editor/Icons/LayerOrder.png",
        useTypeNameAsIconName = false)]
    class CompositionLayersWindow : EditorWindow
    {
        const string k_WindowTitle = "Composition Layers";
        ReorderableList m_CompositionLayersList;

        Dictionary<string, Texture2D> m_LayerIcons = new();
        Texture2D m_DefaultIcon;
        Vector2 m_ScrollView;
        GUIContent m_OrderLabelContent = new();
        bool m_Renaming;
        Rect m_RenamingRect;

        [MenuItem("Window/XR/Composition Layers")]
        public static void InitWindow()
        {
            var window = GetWindow<CompositionLayersWindow>(k_WindowTitle);
            window.titleContent.image = GUIHelpers.LoadIcon(CompositionLayerConstants.IconPath, "LayerOrder");
            window.Show();
        }

        void OnEnable()
        {
            titleContent.text = k_WindowTitle;
            titleContent.image = GUIHelpers.LoadIcon(CompositionLayerConstants.IconPath, "LayerOrder");
            autoRepaintOnSceneChange = true;

            m_LayerIcons.Clear();
            foreach (var descriptor in CompositionLayerUtils.GetAllLayerDescriptors())
            {
                m_LayerIcons.Add(descriptor.TypeFullName, CompositionLayerEditorUtils.GetListViewIcon(descriptor.TypeFullName));
            }

            m_DefaultIcon = CompositionLayerEditorUtils.GetInspectorIcon("LayerGeneric");

            CompositionLayerEditorUtils.GetKnownOccupiedLayersFromManager();

            if (m_CompositionLayersList == null)
            {
                CompositionLayerEditorUtils.GetKnownOccupiedLayersFromManager();
                m_CompositionLayersList = new ReorderableList(CompositionLayerEditorUtils.SortedLayers,
                    typeof(CompositionLayer), true, true, false, false);
                m_CompositionLayersList.onReorderCallback = OnReorderCallback;
                m_CompositionLayersList.drawElementCallback = DrawElementCallback;
                m_CompositionLayersList.drawHeaderCallback = DrawHeaderCallback;
                m_CompositionLayersList.drawFooterCallback = DrawFooterCallback;
                m_CompositionLayersList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            }

            CompositionLayerManager.OccupiedLayersUpdated += Repaint;
        }

        void OnDisable()
        {
            CompositionLayerManager.OccupiedLayersUpdated -= Repaint;
        }

        static void OnReorderCallback(ReorderableList list)
        {
            var reorderedList = CompositionLayerEditorUtils.SortedLayers;

            CompositionLayerManager.Instance.OccupiedLayers.Clear();
            var pivot = reorderedList.IndexOf(CompositionLayerManager.Instance.DefaultSceneCompositionLayer);
            for (var i = 0; i < reorderedList.Count; i++)
            {
                var compositionLayer = reorderedList[i];
                var order = i - pivot;
                CompositionLayerEditorUtils.SetOrderInEditor(compositionLayer, compositionLayer.Order, order);
            }

            CompositionLayerManager.Instance.FindAllLayersInScene();
        }

        void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Layer Order");
            var current = Event.current;
            if (current is { isMouse: true, type: EventType.MouseDown } && rect.Contains(current.mousePosition))
                DisableRenaming();
        }

        void DrawFooterCallback(Rect rect)
        {
            var current = Event.current;
            if (current is { isMouse: true, type: EventType.MouseDown } && rect.Contains(current.mousePosition))
                DisableRenaming();
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.yMin += 1;
            rect.yMax -= 1;

            m_OrderLabelContent.text = "-00";
            var boldRight = new GUIStyle(EditorStyles.boldLabel);
            boldRight.alignment = TextAnchor.MiddleRight;
            boldRight.CalcMinMaxWidth(m_OrderLabelContent, out var minWidth, out _);
            var orderRect = new Rect(rect.xMax - minWidth, rect.y, minWidth, rect.height);

            rect.xMax -= minWidth;

            var compositionLayer = CompositionLayerEditorUtils.SortedLayers[index];
            if (compositionLayer == null)
            {
                CompositionLayerManager.Instance.OccupiedLayersDirty = true;
                EditorApplication.QueuePlayerLoopUpdate();
                Repaint();
                return;
            }

            var isDefault = compositionLayer.LayerData?.GetType() == typeof(DefaultLayerData);
            using (new EditorGUI.DisabledScope(!compositionLayer.isActiveAndEnabled))
            {
                var layerName = isDefault ? "Default Scene Layer" : compositionLayer.gameObject.name;
                var layerTypeName = compositionLayer.LayerData != null ? compositionLayer.LayerData.GetType().FullName : string.Empty;
                if (string.IsNullOrEmpty(layerTypeName))
                    layerTypeName = "";

                if (!m_LayerIcons.TryGetValue(layerTypeName, out var layerIcon))
                    layerIcon = m_DefaultIcon;

                if (m_Renaming && m_RenamingRect == rect)
                {
                    var labelContent = new GUIContent(layerIcon);
                    EditorStyles.label.CalcMinMaxWidth(labelContent, out var iconMinWidth, out _);
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, iconMinWidth, rect.height), labelContent);
                    using var chang = new EditorGUI.ChangeCheckScope();
                    var newLayerName = EditorGUI.DelayedTextField(new Rect(rect.x + iconMinWidth, rect.y,
                        rect.width - iconMinWidth, rect.height), layerName);

                    if (chang.changed)
                    {
                        var go = compositionLayer.gameObject;
                        Undo.RecordObject(go, $"Rename {go.name}");
                        go.name = newLayerName;
                        // Need to disable renaming after pressing enter and setting the name.
                        DisableRenaming();
                    }
                }
                else
                {
                    var labelContent = new GUIContent(layerName, layerIcon);
                    EditorGUI.LabelField(rect, labelContent, EditorStyles.boldLabel);
                }

                EditorGUI.LabelField(orderRect, compositionLayer.Order.ToString(), boldRight);
            }

            var current = Event.current;

            switch (current)
            {
                // Ping the composition layer GameObject
                case { isMouse: true, clickCount: 1, type: EventType.MouseDown } when rect.Contains(current.mousePosition):
                {
                    if (rect.Contains(current.mousePosition))
                    {
                        EditorGUIUtility.PingObject(compositionLayer);
                        Selection.activeGameObject = compositionLayer.gameObject;
                        if (isDefault && compositionLayer.gameObject.hideFlags.HasFlag(HideFlags.HideAndDontSave))
                            compositionLayer.gameObject.hideFlags = HideFlags.None;
                    }

                    if (m_Renaming && m_RenamingRect != rect)
                    {
                        DisableRenaming();
                    }

                    break;
                }

                // Rename the composition layer GameObject
                case { isMouse: true, clickCount: 2, type: EventType.MouseDown } when rect.Contains(current.mousePosition):
                {
                    if (rect.Contains(current.mousePosition) && !isDefault)
                    {
                        m_Renaming = true;
                        m_RenamingRect = rect;
                    }

                    if (m_Renaming && m_RenamingRect != rect)
                    {
                        DisableRenaming();
                    }

                    break;
                }

                case { type: EventType.MouseDrag } when !m_RenamingRect.Contains(current.mousePosition):
                {
                    DisableRenaming();
                    break;
                }
            }
        }

        void DisableRenaming()
        {
            m_Renaming = false;
            m_RenamingRect = Rect.zero;
        }

        void OnGUI()
        {
            CompositionLayerEditorUtils.GetKnownOccupiedLayersFromManager();

            using var scrollViewScope = new EditorGUILayout.ScrollViewScope(m_ScrollView);
            m_CompositionLayersList.DoLayoutList();
            var rect = GUILayoutUtility.GetLastRect();

            var current = Event.current;
            if (current is { isMouse: true, type: EventType.MouseDown } && !rect.Contains(current.mousePosition))
                DisableRenaming();

            m_ScrollView = scrollViewScope.scrollPosition;
        }

        void OnLostFocus()
        {
            DisableRenaming();
        }
    }
}
