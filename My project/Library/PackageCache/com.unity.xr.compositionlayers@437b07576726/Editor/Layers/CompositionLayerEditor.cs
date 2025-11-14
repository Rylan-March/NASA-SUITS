using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Services.Editor;
using Unity.XR.CompositionLayers.Layers.Internal.Editor;
using Button = UnityEngine.UIElements.Button;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    [CustomEditor(typeof(CompositionLayer))]
    class CompositionLayerEditor : UnityEditor.Editor
    {
        SerializedProperty m_OrderProperty;
        SerializedProperty m_LayerDataProperty;

        PopupField<string> m_LayerDataTypeName;

        VisualElement m_RootElement;
        HelpBox m_HelpBoxElement;
        HelpBox m_InstructionElement;
        HelpBox m_HelpBoxForCoordinateSystemElement;
        VisualElement m_LayerDataElement;
        VisualElement m_LayerDataWatcherElement;

        VisualElement m_UnsupportedUnderlayInserting;
        VisualElement m_UnsupportedLayerDataInserting;

        VisualElement m_PlatformLayerDataInserting;
        List<PlatformLayerDataDrawer> m_PlatformLayerDataDrawers;

        SerializedObject m_LayerDataObject;
        static readonly List<Type> k_ExtensionTypes = new();

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void OnDestroy()
        {
            if (m_PlatformLayerDataDrawers != null)
            {
                foreach (var platformLayerDataDrawer in m_PlatformLayerDataDrawers)
                {
                    platformLayerDataDrawer.Dispose();
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            m_RootElement = new VisualElement();
            m_HelpBoxElement = new HelpBox();
            m_HelpBoxElement.style.marginRight = 0;
            m_RootElement.Add(m_HelpBoxElement);

            var m_layerOrderRow = new VisualElement();
            m_layerOrderRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            m_RootElement.Add(m_layerOrderRow);

            m_OrderProperty = serializedObject.FindProperty("m_Order");
            var orderElement = new IntegerField("Layer Order");
            orderElement.ToggleInClassList("unity-base-field__aligned");
            orderElement.style.flexGrow = 1;
            orderElement.tooltip = m_OrderProperty.tooltip;
            // need to set the initial value before bind to avoid getting change callback. This can happen when reopening the inspector.
            orderElement.value = m_OrderProperty.intValue;
            orderElement.bindingPath = m_OrderProperty.propertyPath;
            orderElement.isDelayed = true;
            orderElement.RegisterValueChangedCallback(OnOrderChanged);
            m_layerOrderRow.Add(orderElement);
            m_RootElement.TrackPropertyValue(m_OrderProperty, OnLayerOrderChange);

            // Warning message for unsupported underlay on specific platforms.
            m_UnsupportedUnderlayInserting = new VisualElement();
            m_RootElement.Add(m_UnsupportedUnderlayInserting);

            var managerButtonElement = new Button(OnManagerButtonClick);
            var managerButtonLabel = new Label("Manage");
            managerButtonElement.Add(managerButtonLabel);
            managerButtonElement.style.marginRight = 0;
            m_layerOrderRow.Add(managerButtonElement);

            m_LayerDataProperty = serializedObject.FindProperty("m_LayerData");

            var layerData = m_LayerDataProperty.managedReferenceValue as LayerData;
            var typeFullName = layerData != null ? layerData.GetType().FullName : string.Empty;
            var index = CompositionLayerEditorUtils.LayerNames.IndexOf(typeFullName);

            var compositionLayer = serializedObject.targetObject as CompositionLayer;
            var isInteractableUI = compositionLayer.GetComponent<UIInteraction.InteractableUIMirror>() != null;
            var layerDropdownNames = isInteractableUI ? CompositionLayerEditorUtils.UILayerNames : CompositionLayerEditorUtils.LayerNames;

            m_LayerDataTypeName = new PopupField<string>(
                "Layer Type",
                layerDropdownNames,
                index,
                CompositionLayerEditorUtils.GetFormattedLayerName,
                CompositionLayerEditorUtils.GetFormattedLayerName);

            if (layerData != null)
            {
                switch (layerData)
                {
                    case DefaultLayerData:
                        orderElement.SetEnabled(false);
                        m_LayerDataTypeName.SetEnabled(false);
                        break;

                    case ProjectionLayerRigData:
                        m_LayerDataTypeName.SetEnabled(false);
                        break;

                    default:
                        m_LayerDataTypeName.SetEnabled(true);
                        break;
                }
            }

            m_LayerDataTypeName.RegisterValueChangedCallback(OnLayerTypeChange);
            m_RootElement.Add(m_LayerDataTypeName);

            // Warning message for unsupported layer types on specific platforms.
            m_UnsupportedLayerDataInserting = new VisualElement();
            m_RootElement.Add(m_UnsupportedLayerDataInserting);

            {
                m_PlatformLayerDataInserting = new VisualElement();

                var platformLayerDataList = PlatformLayerDataUtil.GetActivePlatformLayerDataList(compositionLayer);
                if (platformLayerDataList != null)
                {
                    m_PlatformLayerDataDrawers = new List<PlatformLayerDataDrawer>();
                    foreach (var platformLayerData in platformLayerDataList)
                    {
                        var platformLayerDataDrawer = new PlatformLayerDataDrawer(compositionLayer, platformLayerData);
                        m_PlatformLayerDataDrawers.Add(platformLayerDataDrawer);
                        m_PlatformLayerDataInserting.Add(platformLayerDataDrawer.RootElement);
                    }
                }
            }

            UpdateValidationMessages(layerData);

            SyncLayerDataToPopupField(layerData);

            m_HelpBoxForCoordinateSystemElement = new HelpBox();
            m_LayerDataElement = new PropertyField(m_LayerDataProperty);
            m_LayerDataTypeName.ToggleInClassList("unity-base-field__aligned");
            m_RootElement.Add(m_LayerDataElement);
            m_RootElement.Add(m_PlatformLayerDataInserting);
            m_RootElement.Add(m_HelpBoxElement);
            m_RootElement.Add(m_HelpBoxForCoordinateSystemElement);

            m_RootElement.MarkDirtyRepaint();

            // Ensure `ReportStateChange` is set on layer data
            foreach (var targetObject in targets)
            {
                if (targetObject is CompositionLayer compLayer && compLayer.LayerData != null)
                {
                    compLayer.LayerData.ReportStateChange = compLayer.ReportStateChange;
                }
            }

            m_InstructionElement = new HelpBox();
            m_InstructionElement.style.marginRight = 0;
            m_RootElement.Add(m_InstructionElement);

            UpdateHelpBox();
            UpdateHelpBoxForCoordinateSystem();

            return m_RootElement;
        }

        void UpdateValidationMessages(LayerData layerData)
        {
            UpdateUnsupportedUnderlayMessage();
            UpdateUnsupportedLayerDataType(layerData);
        }

        void UpdateUnsupportedUnderlayMessage()
        {
            m_UnsupportedUnderlayInserting.Clear();

            if (m_OrderProperty == null || m_OrderProperty.intValue >= 0)
                return;

            var activeProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activeProviders == null)
                return;

            var inserting = m_UnsupportedUnderlayInserting;
            foreach (var activeProvider in activeProviders)
            {
                if (!activeProvider.IsSupportedUnderlayLayers)
                {
                    inserting.Add(UIHelper.UIWarning(CompositionLayerEditorUtils.k_UnsupportedUnderlayMessage, activeProvider));
                }
            }
        }

        void UpdateUnsupportedLayerDataType(LayerData layerData)
        {
            m_UnsupportedLayerDataInserting.Clear();

            var activeProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activeProviders == null)
                return;

            var inserting = m_UnsupportedLayerDataInserting;
            foreach (var activeProvider in activeProviders)
            {
                if (!activeProvider.IsSupportedLayerData(layerData))
                {
                    inserting.Add(UIHelper.UIWarning(CompositionLayerEditorUtils.k_UnsupportedLayerTypeMessage, activeProvider));
                }
            }
        }

        void OnLayerDataChange(SerializedProperty layerDataProperty)
        {
            var layerData = layerDataProperty.managedReferenceValue as LayerData;
            var layerId = layerData != null ? layerData.GetType().FullName : string.Empty;
            UpdateLayerDataDrawer(layerId);
        }

        void OnLayerTypeChange(ChangeEvent<string> evt)
        {
            if (evt.previousValue == evt.newValue)
                return;

            var layerId = evt.newValue;
            var previousLayerId = evt.previousValue;

            UpdateLayerDataDrawer(layerId);
        }

        void UpdateLayerDataDrawer(string layerId)
        {
            ChangeLayerType(layerId);

            UpdateHelpBox();
            UpdateHelpBoxForCoordinateSystem();
            m_RootElement.MarkDirtyRepaint();
            m_LayerDataElement.MarkDirtyRepaint();

            if (m_PlatformLayerDataDrawers != null)
            {
                foreach (var platformLayerDataDrawer in m_PlatformLayerDataDrawers)
                {
                    platformLayerDataDrawer.OnUpdatedCurrentLayerData();
                }
            }

            CheckSuggestedComponents();
        }

        void ChangeLayerType(string layerId)
        {
            LayerData layerData = LayerIdToLayerData(layerId);

            m_LayerDataTypeName.value = layerId;
            m_LayerDataProperty.managedReferenceValue = layerData;

            ApplyChangesWithReportStateChange();
        }

        LayerData LayerIdToLayerData(string layerId)
        {
            LayerData layerData = null;
            if (!string.IsNullOrEmpty(layerId))
            {
                layerData = m_LayerDataProperty.managedReferenceValue as LayerData;
                if (layerData == null || layerData.GetType().FullName != layerId)
                {
                    layerData = CompositionLayerUtils.CreateLayerData(layerId);
                    m_LayerDataTypeName.value = layerId;
                    m_LayerDataProperty.managedReferenceValue = layerData;
                    // Note: Setter of m_LayerDataProperty.managedReferenceValue will cause the crash when m_LayerData has been obsoleted.
                    (target as CompositionLayer).LayerData = layerData;
                    UpdateValidationMessages(layerData);
                    ApplyChangesWithReportStateChange();
                }
            }

            return layerData;
        }

        void OnOrderChanged(ChangeEvent<int> evt)
        {
            var compositionLayer = target as CompositionLayer;
            if (compositionLayer == null)
                return;

            if (CompositionLayerManager.ManagerActive && CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(
                evt.newValue, out var occupiedLayer) && occupiedLayer != compositionLayer)
                CompositionLayerUtils.LogLayerOrderCannotBeSet(compositionLayer, evt.newValue);

            CompositionLayerEditorUtils.SetOrderInEditor(compositionLayer, evt.previousValue, evt.newValue);
            ApplyChangesWithReportStateChange();
        }

        void UpdateHelpBox()
        {
            if (m_HelpBoxElement == null || m_InstructionElement == null)
                return;

            var mainCamera = CompositionLayerManager.mainCameraCache;

            // Check that layer data is set to valid type
            var layerData = m_LayerDataProperty.managedReferenceValue as LayerData;
            if (layerData == null)
            {
                m_HelpBoxElement.style.display = DisplayStyle.Flex;
                m_HelpBoxElement.text = "Please select a layer type.";
                m_HelpBoxElement.messageType = HelpBoxMessageType.Info;
                m_InstructionElement.style.display = DisplayStyle.None;
                return;
            }

            var fullName = layerData != null ? layerData.GetType().FullName : string.Empty;
            var index = string.IsNullOrEmpty(fullName) ? -1 : CompositionLayerEditorUtils.LayerNames.IndexOf(fullName);
            var layerDataType = layerData.GetType();

            if (layerDataType == typeof(DefaultLayerData))
            {
                // Show the help Box
                m_HelpBoxElement.style.display = DisplayStyle.Flex;
                m_HelpBoxElement.text = "Scene objects are rendered on this layer by default";
                m_HelpBoxElement.messageType = HelpBoxMessageType.Info;
            }
            else if (index < 0 && layerDataType != typeof(ProjectionLayerRigData))
            {
                // Show the help Box
                m_HelpBoxElement.style.display = DisplayStyle.Flex;

                var msg = "Please select a layer type to continue.";
                if (!string.IsNullOrEmpty(fullName))
                {
                    msg = "Currently selected layer type is unknown. Make a new selection to override the current type.";
                }

                m_HelpBoxElement.text = msg;
                m_HelpBoxElement.messageType = HelpBoxMessageType.Info;

            }
            // Check if camera background will block underlay layers
            else if (m_OrderProperty.intValue < 0 && mainCamera.clearFlags == CameraClearFlags.Skybox)
            {
                var camera = mainCamera;
                if (camera == null)
                    camera = FindFirstObjectByType<Camera>();

                if (camera == null || camera.clearFlags == CameraClearFlags.Skybox)
                {
                    // Show the help Box
                    m_HelpBoxElement.style.display = DisplayStyle.Flex;
                    m_HelpBoxElement.text = "Cameras with clear flags set to Skybox may obscure this layer.";
                    m_HelpBoxElement.messageType = HelpBoxMessageType.Warning;
                }
            }
            else
            {
                m_HelpBoxElement.style.display = DisplayStyle.None;
            }

            if (layerData?.GetType().Name == "ProjectionLayerRigData")
            {
                m_InstructionElement.style.display = DisplayStyle.Flex;
                m_InstructionElement.text = "Add GameObjects to this Projection Layer by changing their User Layer to match this Projection Layer's name.";
                m_InstructionElement.messageType = HelpBoxMessageType.Info;
            }
            else
            {
                m_InstructionElement.style.display = DisplayStyle.None;
            }
        }

        void UpdateHelpBoxForCoordinateSystem()
        {
            CoordinateSystemUtils.UpdateHelpBox(m_HelpBoxForCoordinateSystemElement, target as CompositionLayer);
        }

        void CheckSuggestedComponents()
        {
            var layerComponent = serializedObject.targetObject as CompositionLayer;
            if (layerComponent == null)
                return;

            GetMissingSuggestedExtensionTypes(layerComponent);

            if (k_ExtensionTypes.Count < 1)
                return;

            var msg = "The selected layer type would like to add the following suggested component extensions to this " +
                "Game Object. Would you like them to be added now?\n\n";

            foreach (var missingType in k_ExtensionTypes)
            {
                msg += $"{missingType.Name}\n";
            }

            if (EditorUtility.DisplayDialog("Suggested Layer Extensions", msg, "Yes", "No"))
            {
                Undo.IncrementCurrentGroup();
                Undo.RecordObject(layerComponent.gameObject, "Add Suggested Layer Extensions");
                foreach (var extensionType in k_ExtensionTypes)
                {
                    Undo.AddComponent(layerComponent.gameObject, extensionType);
                }
            }
        }

        static void OnManagerButtonClick()
        {
            CompositionLayersWindow.InitWindow();
        }

        static void GetMissingSuggestedExtensionTypes(CompositionLayer layer)
        {
            k_ExtensionTypes.Clear();

            if (layer.LayerData == null)
                return;

            var desc = CompositionLayerUtils.GetLayerDescriptor(layer.LayerData.GetType());

            if (desc.Equals(default) || desc.Equals(LayerDataDescriptor.Empty)
                || desc.SuggestedExtensions.Length == 0)
                return;

            foreach (var extensionType in desc.SuggestedExtensions)
            {
                if (layer.gameObject.GetComponent(extensionType) == null)
                    k_ExtensionTypes.Add(extensionType);
            }
        }

        void ApplyChangesWithReportStateChange()
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var targetObject in serializedObject.targetObjects)
            {
                if (targetObject is CompositionLayer compositionLayer)
                    compositionLayer.ReportStateChange();
            }
        }

        void OnValidate()
        {
            if (m_LayerDataProperty == null)
                return;

            var layerData = m_LayerDataProperty.managedReferenceValue as LayerData;
            SyncLayerDataToPopupField(layerData);
        }

        void SyncLayerDataToPopupField(LayerData layerData)
        {
            var typeFullName = layerData != null ? layerData.GetType().FullName : string.Empty;

            if (string.IsNullOrEmpty(typeFullName))
            {
                m_LayerDataProperty.managedReferenceValue = null;
                m_LayerDataTypeName.index = -1;
                m_LayerDataTypeName.value = string.Empty;
                serializedObject.Update();
                return;
            }

            if (m_LayerDataProperty.managedReferenceValue == null
                || m_LayerDataProperty.managedReferenceValue.GetType().FullName != typeFullName)
                ChangeLayerType(typeFullName);

            var index = CompositionLayerEditorUtils.LayerNames.IndexOf(typeFullName);

            m_LayerDataTypeName.index = index;
            m_LayerDataTypeName.value = typeFullName;
        }

        void SyncLayerDataToPopupField()
        {
            if (m_LayerDataProperty == null || m_LayerDataElement == null)
                return;

            var layerData = m_LayerDataProperty.managedReferenceValue as LayerData;
            SyncLayerDataToPopupField(layerData);
        }

        void OnLayerOrderChange(SerializedProperty layerOrderProperty)
        {
            UpdateUnsupportedUnderlayMessage();
            UpdateHelpBox();
            UpdateHelpBoxForCoordinateSystem();
        }

        void UndoRedoPerformed()
        {
            // Redo actions are not tripping the value changed in the element
            // and don't seem to be applied till the end of an editor frame
            EditorApplication.delayCall += SyncLayerDataToPopupField;

            if (m_PlatformLayerDataDrawers != null)
            {
                foreach (var platformLayerDataDrawer in m_PlatformLayerDataDrawers)
                {
                    platformLayerDataDrawer.UndoRedoPerformed();
                }
            }
        }
    }
}
