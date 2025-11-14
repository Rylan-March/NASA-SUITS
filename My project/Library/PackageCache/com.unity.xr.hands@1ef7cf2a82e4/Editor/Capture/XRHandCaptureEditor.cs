using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.XR.Hands.Analytics;
using UnityEditor.XR.Hands.Gestures;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Gestures;
// Some UI Elements classes like IntegerField are in a different namespace for older Unity versions
#if !UNITY_2022_3_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace UnityEditor.XR.Hands.Capture
{
    [CustomEditor(typeof(XRHandCapturePlayback))]
    class XRHandCaptureEditor : Editor
    {
        XRHandCapturePlayback m_CapturePlayback;

        Foldout m_HandMeshSettings;
        Foldout m_ImportRecordingSettings;
        Foldout m_SelectFrameSettings;
        Foldout m_HandShapeOverwriteSettings;

        SliderInt m_FrameSelectionSlider;
        IntegerField m_FrameIDField;
        Label m_TimestampLabel;
        VisualElement m_FrameSelectionContainer;
        VisualElement m_XRHandShapeInspector;
        Editor m_EmbeddedHandShapeEditor;

        const string k_TimestampDefaultText = "00:00:0000";
        const string k_LeftHandText = "Left";
        const string k_RightHandText = "Right";

        const string k_VisualizationSettingsTitle = "Hand Visualization";
        const string k_ImportRecordingTitle = "Step 1 - Import Recordings to Unity";
        const string k_SelectFrameTitle = "Step 2 - Select Frame To Create Hand Shape";
        const string k_OverwriteShapeTitle = "Step 3 - Select Hand Shape To Overwrite";

        const string k_LeftHandPrefabLabel = "Left Hand Prefab";
        const string k_RightHandPrefabLabel = "Right Hand Prefab";
        const string k_HandMeshMaterialLabel = "Hand Mesh Material";
        const string k_JointPrefabLabel = "Joint Prefab";
        const string k_DrawMeshesToggleLabel = "Should Draw Meshes";
        const string k_DrawJointsToggleLabel = "Should Draw Joints";

        const string k_ImportPathLabel = "Import To";
        const string k_ImportButtonText = "Import From Connected Headset";
        const string k_SelectRecordingLabel = "Recording";

        const string k_HandednessDropdownLabel = "Hand";
        const string k_FrameSelectionDescriptionText = "Recording replay happens in Scene view";
        const string k_FrameSelectionSliderLabel = "Timestamp\nmm:ss:frame";
        const string k_FrameIDLabel = "Frame #";

        const string k_LowerToleranceLabel = "Finger Value Lower Threshold";
        const string k_UpperToleranceLabel = "Finger Value Upper Threshold";
        const string k_ComputeFingerValueButtonText = "Compute Finger Values";

        const string k_HandShapeToOverwriteLabel = "Hand Shape";
        const string k_OverwriteHandShapeButtonText = "Overwrite with Computed Shape";

        bool m_IsLeftHandSelected = true; // Default to select left hand for hand shape generation

        void OnEnable()
        {
            m_CapturePlayback = XRHandCapturePlayback.GetInstance();
            m_CapturePlayback.recordingChanged += OnRecordingChanged;
            m_CapturePlayback.frameChanged += OnFrameChanged;
        }

        void OnDisable()
        {
            if (m_CapturePlayback != null)
            {
                m_CapturePlayback.recordingChanged -= OnRecordingChanged;
                m_CapturePlayback.frameChanged -= OnFrameChanged;
            }

            if (m_EmbeddedHandShapeEditor != null)
                DestroyImmediate(m_EmbeddedHandShapeEditor);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Step 0: Configure Hand Mesh Visualization Settings
            m_HandMeshSettings = CreateHandMeshVisualizationSettingsSection();
            root.Add(m_HandMeshSettings);

            // Step 1: Import Recording
            m_ImportRecordingSettings = CreateImportRecordingSection(k_ImportRecordingTitle);
            root.Add(m_ImportRecordingSettings);

            // Step 2: Select Frame
            m_SelectFrameSettings = CreateSelectFrameSection(k_SelectFrameTitle);
            root.Add(m_SelectFrameSettings);

            // Step 3: Select XRHandShape asset to overwrite and save
            m_HandShapeOverwriteSettings = CreateHandShapeToOverwriteSection(k_OverwriteShapeTitle);
            root.Add(m_HandShapeOverwriteSettings);

            // Render the last selected frame if available
            if (m_CapturePlayback.IsRecordingDataAvailable())
            {
                OnFrameChanged(m_CapturePlayback.selectedFrameID);
            }

            return root;
        }

        Foldout CreateHandMeshVisualizationSettingsSection()
        {
            var handMeshSettings = EditorUIUtils.CreateFoldout(k_VisualizationSettingsTitle);

            var leftHandPrefabField = EditorUIUtils.CreateObjectField(k_LeftHandPrefabLabel, typeof(GameObject));
            leftHandPrefabField.value = m_CapturePlayback.leftHandPrefab;
            leftHandPrefabField.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.leftHandPrefab = evt.newValue as GameObject);

            var rightHandPrefabField = EditorUIUtils.CreateObjectField(k_RightHandPrefabLabel, typeof(GameObject));
            rightHandPrefabField.value = m_CapturePlayback.rightHandPrefab;
            rightHandPrefabField.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.rightHandPrefab = evt.newValue as GameObject);

            var materialField = EditorUIUtils.CreateObjectField(k_HandMeshMaterialLabel, typeof(Material));
            materialField.value = m_CapturePlayback.handMeshMaterial;
            materialField.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.handMeshMaterial = evt.newValue as Material);

            var jointPrefabField = EditorUIUtils.CreateObjectField(k_JointPrefabLabel, typeof(GameObject));
            jointPrefabField.value = m_CapturePlayback.jointPrefab;
            jointPrefabField.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.jointPrefab = evt.newValue as GameObject);

            // Create Toggle controls for boolean properties
            var drawMeshesToggle = new Toggle(k_DrawMeshesToggleLabel) { value = m_CapturePlayback.shouldDrawMeshes };
            drawMeshesToggle.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.shouldDrawMeshes = evt.newValue);

            var drawJointsToggle = new Toggle(k_DrawJointsToggleLabel) { value = m_CapturePlayback.shouldDrawJoints };
            drawJointsToggle.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.shouldDrawJoints = evt.newValue);

            // Add all controls to the foldout
            handMeshSettings.Add(leftHandPrefabField);
            handMeshSettings.Add(rightHandPrefabField);
            handMeshSettings.Add(materialField);
            handMeshSettings.Add(drawMeshesToggle);
            handMeshSettings.Add(jointPrefabField);
            handMeshSettings.Add(drawJointsToggle);

            return handMeshSettings;
        }

        Foldout CreateImportRecordingSection(string title)
        {
            var importRecordingSettings = EditorUIUtils.CreateFoldout(title);

            var importPathField = new TextField(k_ImportPathLabel);
            importPathField.style.marginBottom = 5;
            importPathField.style.marginRight = 5;
            importPathField.value = m_CapturePlayback.recordingsAssetSavePath;
            importPathField.RegisterValueChangedCallback(evt =>
                m_CapturePlayback.recordingsAssetSavePath = evt.newValue);
            importRecordingSettings.Add(importPathField);

            var pullAllRecordingsButtonUI = EditorUIUtils.CreateButton(k_ImportButtonText, () =>
            {
                ImportRecordings();

                // Refresh to show the newly imported files
                AssetDatabase.Refresh();
            });

            importRecordingSettings.Add(pullAllRecordingsButtonUI);

            if (m_CapturePlayback.recordingImporter == null)
            {
                pullAllRecordingsButtonUI.SetEnabled(false);
                var warningBox = new HelpBox("Importing from connected headset is currently supported only on Android devices. " +
                    "Please ensure you have the Android Build Support Module installed and the build target set to Android. ",
                    HelpBoxMessageType.Warning);
                importRecordingSettings.Add(warningBox);
            }

            return importRecordingSettings;
        }

        Foldout CreateSelectFrameSection(string title)
        {
            var selectFrameSettings = EditorUIUtils.CreateFoldout(title);

            // Add a field to pick the recording to work with
            var selectedRecordingField =
                EditorUIUtils.CreateObjectField(k_SelectRecordingLabel, typeof(XRHandCaptureSequence));
            selectedRecordingField.value = m_CapturePlayback.selectedRecording;
            selectedRecordingField.RegisterValueChangedCallback(evt =>
            {
                m_CapturePlayback.selectedRecording = evt.newValue as XRHandCaptureSequence;
            });
            selectFrameSettings.Add(selectedRecordingField);

            // Create a container for all the frame selection controls so that they can be toggled together
            m_FrameSelectionContainer = CreateFrameSelectionContainer();
            selectFrameSettings.Add(m_FrameSelectionContainer);

            return selectFrameSettings;
        }

        VisualElement CreateFrameSelectionContainer()
        {
            var frameSelectionContainer = new VisualElement();

            frameSelectionContainer.SetEnabled(m_CapturePlayback.IsRecordingDataAvailable());

            // Create a dropdown menu for selecting left or right hand
            var handSelectionDropdown = new DropdownField(k_HandednessDropdownLabel,
                new List<string> { k_LeftHandText, k_RightHandText }, 0);
            handSelectionDropdown.style.marginBottom = 10;

            handSelectionDropdown.RegisterValueChangedCallback(evt =>
            {
                m_IsLeftHandSelected = evt.newValue == k_LeftHandText;
                RefreshXRHandShapeInspector();
            });

            frameSelectionContainer.Add(handSelectionDropdown);

            var description = EditorUIUtils.CreateDescriptionLabel(k_FrameSelectionDescriptionText);
            frameSelectionContainer.Add(description);

            var sliderTimeStampContainer = new VisualElement();
            sliderTimeStampContainer.style.flexDirection = FlexDirection.Row;

            m_FrameSelectionSlider = new SliderInt(k_FrameSelectionSliderLabel, 0, 0);
            m_FrameSelectionSlider.style.flexGrow = 1;
            ResetFrameSelectionSliderRange();

            m_TimestampLabel = new Label(k_TimestampDefaultText);
            m_TimestampLabel.style.minWidth = 70;
            m_TimestampLabel.style.marginLeft = 8;
            m_TimestampLabel.style.marginBottom = 8;
            m_TimestampLabel.style.marginRight = 8;
            m_TimestampLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            m_FrameIDField = new IntegerField(k_FrameIDLabel);
            m_FrameIDField.style.marginBottom = 8;
            m_FrameIDField.style.marginRight = 5;
            m_FrameIDField.value = m_FrameSelectionSlider.value;

            m_FrameSelectionSlider.RegisterValueChangedCallback(evt =>
            {
                // Ensure the new frame ID is within bounds
                int validFrameID = Mathf.Clamp(
                    evt.newValue, 0, m_CapturePlayback.selectedRecording.frames.Count - 1);

                m_CapturePlayback.selectedFrameID = validFrameID;
            });

            m_FrameIDField.RegisterValueChangedCallback(evt =>
            {
                // Ensure the new frame ID entered is within bounds
                int validFrameID = Mathf.Clamp(
                    evt.newValue, 0, m_CapturePlayback.selectedRecording.frames.Count - 1);

                m_CapturePlayback.selectedFrameID = validFrameID;
            });

            sliderTimeStampContainer.Add(m_FrameSelectionSlider);
            sliderTimeStampContainer.Add(m_TimestampLabel);

            frameSelectionContainer.Add(sliderTimeStampContainer);
            frameSelectionContainer.Add(m_FrameIDField);

            // Add input fields for finger shape condition tolerances
            var lowerToleranceField = new FloatField(k_LowerToleranceLabel)
            {
                value = m_CapturePlayback.fingerShapeLowerTolerance,
                style = { marginBottom = 5, marginRight = 5 }
            };

            lowerToleranceField.RegisterValueChangedCallback(evt =>
            {
                float clampedValue = Mathf.Clamp(evt.newValue, 0.0f, 1.0f);
                if (!Mathf.Approximately(clampedValue, evt.newValue))
                    lowerToleranceField.SetValueWithoutNotify(clampedValue);

                m_CapturePlayback.fingerShapeLowerTolerance = clampedValue;
                m_CapturePlayback.CreateFingerShapeConditions();
                RefreshXRHandShapeInspector(true);
            });
            frameSelectionContainer.Add(lowerToleranceField);

            var upperToleranceField = new FloatField(k_UpperToleranceLabel)
            {
                value = m_CapturePlayback.fingerShapeUpperTolerance,
                style = { marginBottom = 5, marginRight = 5 }
            };
            upperToleranceField.RegisterValueChangedCallback(evt =>
            {
                float clampedValue = Mathf.Clamp(evt.newValue, 0.0f, 1.0f);
                if (!Mathf.Approximately(clampedValue, evt.newValue))
                    upperToleranceField.SetValueWithoutNotify(clampedValue);

                m_CapturePlayback.fingerShapeUpperTolerance = clampedValue;
                m_CapturePlayback.CreateFingerShapeConditions();
                RefreshXRHandShapeInspector(true);
            });
            frameSelectionContainer.Add(upperToleranceField);

            var computeFingerValueButton =
                EditorUIUtils.CreateButton(k_ComputeFingerValueButtonText, () => OnSelectFrameClicked());

            frameSelectionContainer.Add(computeFingerValueButton);

            // Embed XRHandShapeInspector window to the container
            m_XRHandShapeInspector = new VisualElement { name = "XRHandShapeInspector" };
            m_XRHandShapeInspector.style.display = DisplayStyle.None;
            frameSelectionContainer.Add(m_XRHandShapeInspector);
            RefreshXRHandShapeInspector();

            return frameSelectionContainer;
        }

        Foldout CreateHandShapeToOverwriteSection(string title)
        {
            var handShapeAssetSettings = EditorUIUtils.CreateFoldout(title);
            handShapeAssetSettings.SetEnabled(m_CapturePlayback.IsRecordingDataAvailable());

            var handShapeOverwriteAssetField = EditorUIUtils.CreateObjectField(
                k_HandShapeToOverwriteLabel, typeof(XRHandShape));
            handShapeOverwriteAssetField.value = m_CapturePlayback.handShapeToOverwrite;

            var overwriteHandShapeButton = EditorUIUtils.CreateButton(
                k_OverwriteHandShapeButtonText, SaveComputedShapeToTarget);
            overwriteHandShapeButton.SetEnabled(m_CapturePlayback.handShapeToOverwrite != null);

            handShapeOverwriteAssetField.RegisterValueChangedCallback(evt =>
            {
                m_CapturePlayback.handShapeToOverwrite = evt.newValue as XRHandShape;
                overwriteHandShapeButton.SetEnabled(m_CapturePlayback.handShapeToOverwrite != null);
            });

            handShapeAssetSettings.Add(handShapeOverwriteAssetField);

            handShapeAssetSettings.Add(overwriteHandShapeButton);

            return handShapeAssetSettings;
        }

        void OnRecordingChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            // Enable or disable the frame selection container based on whether a recording is selected
            bool isRecordingAvailable = m_CapturePlayback != null && m_CapturePlayback.IsRecordingDataAvailable();
            m_FrameSelectionContainer.SetEnabled(isRecordingAvailable);

            // Reset the frame ID to 0 when the recording data changes
            OnFrameChanged(0);

            // Reset the frame selection slider range
            ResetFrameSelectionSliderRange();

            // If recording data is not available, clear the inspector and hide it
            if (!isRecordingAvailable)
            {
                m_XRHandShapeInspector.Clear();
                m_XRHandShapeInspector.style.display = DisplayStyle.None;
            }

            m_HandShapeOverwriteSettings.SetEnabled(isRecordingAvailable);
        }

        void OnFrameChanged(int newFrameID)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            m_CapturePlayback.RenderFrame(newFrameID);
            m_CapturePlayback.UpdateHandShapes();

            m_FrameSelectionSlider.SetValueWithoutNotify(newFrameID);
            m_FrameIDField.SetValueWithoutNotify(newFrameID);
            UpdateTimestampDisplay(newFrameID);
        }

        void OnSelectFrameClicked()
        {
            m_CapturePlayback.UpdateHandShapes();

            RefreshXRHandShapeInspector(true);
        }

        static string FormatTimestamp(float timeInSeconds, int frameID)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);

            return $"{minutes:00}:{seconds:00}:{frameID:0000}";
        }

        void UpdateTimestampDisplay(int frameID)
        {
            // Timestamp format: mm:ss:frameID
            if (!ValidateFrameAccess(frameID))
            {
                m_TimestampLabel.text = k_TimestampDefaultText;
                return;
            }

            var frame = m_CapturePlayback.selectedRecording.frames[frameID];
            m_TimestampLabel.text = FormatTimestamp(frame.timestamp, frameID);
        }

        void ResetFrameSelectionSliderRange()
        {
            bool hasData = m_CapturePlayback.IsRecordingDataAvailable();
            m_FrameSelectionSlider.highValue = hasData ? m_CapturePlayback.selectedRecording.frames.Count - 1 : 0;
            m_FrameSelectionSlider.value = hasData ? m_CapturePlayback.selectedFrameID : 0;
        }

        void SaveComputedShapeToTarget()
        {
            if (m_CapturePlayback.handShapeToOverwrite == null)
            {
                Debug.LogError("HandShapeToOverwrite is not set.");
                return;
            }

            XRHandShape newHandShape =
                m_IsLeftHandSelected ? m_CapturePlayback.leftHandShape : m_CapturePlayback.rightHandShape;

            if (newHandShape == null)
            {
                Debug.LogError("No hand shape available to save.");
                return;
            }

            // Ask for confirmation before overwriting Hand Shape Asset Target
            bool shouldOverwrite = EditorUtility.DisplayDialog(
                "Overwrite Hand Shape Asset",
                $"Overwrite '{m_CapturePlayback.handShapeToOverwrite.name}' with " +
                $"selected {(m_IsLeftHandSelected ? k_LeftHandText : k_RightHandText)} hand shape from frame {m_CapturePlayback.selectedFrameID}?",
                "Save", "Cancel");

            if (!shouldOverwrite)
                return;

            EditorUtility.DisplayProgressBar("Hold on...", "Overwriting hand shape...", 0.5f);
            try
            {
                // Copy the finger shape conditions from the computed hand shape to the target asset
                m_CapturePlayback.handShapeToOverwrite.fingerShapeConditions =
                    new List<XRFingerShapeCondition>(newHandShape.fingerShapeConditions);

                // Mark the target asset as dirty and save it
                EditorUtility.SetDirty(m_CapturePlayback.handShapeToOverwrite);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#if ENABLE_CLOUD_SERVICES_ANALYTICS
                 var analyticsData = new XRHandCaptureAnalyticsData();
                 analyticsData.newHandShapeSaved = true;
                 analyticsData.Send();
#endif
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        void RefreshXRHandShapeInspector(bool shouldExpandFingerValues = false)
        {
            if (m_XRHandShapeInspector == null || !m_CapturePlayback.IsRecordingDataAvailable())
                return;

            // Clear existing inspector
            m_XRHandShapeInspector.Clear();

            if (m_EmbeddedHandShapeEditor != null)
                DestroyImmediate(m_EmbeddedHandShapeEditor);

            // Retrieve the updated XRHandShape objects
            XRHandShape leftHandShape = m_CapturePlayback.leftHandShape;
            XRHandShape rightHandShape = m_CapturePlayback.rightHandShape;

            // If no hand shapes exist, keep the layout intact but hide content
            if (leftHandShape == null && rightHandShape == null)
            {
                m_XRHandShapeInspector.style.display = DisplayStyle.None;
                return;
            }

            var title = $"{(m_IsLeftHandSelected ? k_LeftHandText : k_RightHandText)} Hand | Frame #{m_CapturePlayback.selectedFrameID}";

            if (m_IsLeftHandSelected && leftHandShape != null)
            {
                m_EmbeddedHandShapeEditor = CreateEditor(leftHandShape);
                m_XRHandShapeInspector.Add(CreateEmbeddedInspector(m_EmbeddedHandShapeEditor, title, shouldExpandFingerValues));
            }
            else if (!m_IsLeftHandSelected && rightHandShape != null)
            {
                m_EmbeddedHandShapeEditor = CreateEditor(rightHandShape); // Store the new editor
                m_XRHandShapeInspector.Add(CreateEmbeddedInspector(m_EmbeddedHandShapeEditor, title, shouldExpandFingerValues));
            }

            m_XRHandShapeInspector.style.display = DisplayStyle.Flex;
        }

        static VisualElement CreateEmbeddedInspector(Editor embeddedEditor, string title, bool shouldExpand = false)
        {
            var container = new Foldout
            {
                text = title,
                value = shouldExpand
            };

            if (embeddedEditor is XRHandShapeEditor customEditor)
            {
                container.Add(customEditor.CreateInspectorGUI());
            }
            return container;
        }

        bool ValidateFrameAccess(int frameID)
        {
            return m_CapturePlayback.IsRecordingDataAvailable() && m_CapturePlayback.IsFrameIDValid(frameID);
        }

        void ImportRecordings()
        {
            if (m_CapturePlayback.recordingImporter == null)
                return;

            List<XRHandCaptureSequence> recordings = new List<XRHandCaptureSequence>();
            try
            {
                if (!m_CapturePlayback.recordingImporter.TryGetAllCaptureSequences(recordings))
                {
                    EditorUtility.DisplayDialog("Import Error", "Failed to retrieve recordings from device.", "OK");
                    return;
                }

                if (recordings.Count == 0)
                {
                    EditorUtility.DisplayDialog("Import Result", "No recordings found on the device.", "OK");
                    return;
                }

                if (!Directory.Exists(m_CapturePlayback.recordingsAssetSavePath))
                    Directory.CreateDirectory(m_CapturePlayback.recordingsAssetSavePath);

                // Create recording assets with unique names
                for (var i = 0; i < recordings.Count; i++)
                {
                    var recording = recordings[i];

                    string uniqueName = GetUniqueAssetName(recording.name, m_CapturePlayback.recordingsAssetSavePath);
                    recording.name = uniqueName;

                    string assetPath = Path.Combine(m_CapturePlayback.recordingsAssetSavePath, uniqueName + ".asset");
                    AssetDatabase.CreateAsset(recording, assetPath);

                    float progress = (float)(i + 1) / recordings.Count;
                    EditorUtility.DisplayProgressBar("Importing Recordings", $"Importing {recording.name}...", progress);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

#if ENABLE_CLOUD_SERVICES_ANALYTICS
                var analyticsData = new XRHandCaptureAnalyticsData();
                analyticsData.recordingsImported = true;
                analyticsData.Send();
#endif
            }
            catch (Exception ex)
            {
                // Catch any exceptions from the data retrieval layer and display a dialog
                Debug.LogError($"An error occurred during import: {ex.Message}");
                EditorUtility.DisplayDialog("Import Error", $"An error occurred during import: {ex.Message}", "OK");
            }
            finally
            {
                // Clear the progress bar when the operation is complete
                EditorUtility.ClearProgressBar();
            }
        }

        static string GetUniqueAssetName(string originalName, string savePath)
        {
            string currentName = originalName;
            string assetPath = Path.Combine(savePath, currentName + ".asset");

            if (AssetDatabase.LoadAssetAtPath<XRHandCaptureSequence>(assetPath) == null)
            {
                return currentName;
            }

            int suffix = 1;
            do
            {
                currentName = $"{originalName} {suffix}";
                assetPath = Path.Combine(savePath, currentName + ".asset");
                suffix++;
            } while (AssetDatabase.LoadAssetAtPath<XRHandCaptureSequence>(assetPath) != null);

            return currentName;
        }
    }
}
