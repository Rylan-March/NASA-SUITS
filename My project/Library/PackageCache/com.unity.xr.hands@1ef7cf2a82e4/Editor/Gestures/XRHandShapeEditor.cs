using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEditor.XR.Hands.Gestures
{
    /// <summary>
    /// Custom editor for setting up XR hand shapes
    /// </summary>
    [CustomEditor(typeof(XRHandShape))]
    class XRHandShapeEditor : Editor
    {
        const int k_FingerCount = 5;

        const float k_MinToleranceLimit = 0f;
        const float k_MaxToleranceLimit = 1f;
        const float k_OffsetToleranceLimit = 0.0125f;

        // Style-related constants
        const float k_AddRemoveButtonWidth = 16f;
        const float k_AddButtonMarginLeft = 3f;
        const float k_StyleFingerShapeListItemPaddingCorrection = 12f;
        const float k_FingerShapeContainerBottomSpacerWidth = 40f;
        const float k_FingerShapeContainerBottomSpacerMarginRight = 24f;
        const float k_FingerShapeListItemStyleContainerMarginBottom = 2f;
        const float k_FingerShapeListItemStyleContainerMarginLeft = -k_StyleFingerShapeListItemPaddingCorrection;
        const float k_FingerShapeListItemStyleContainerMarginRight = 10f;
        const float k_FingerShapeListItemStyleContainerPadding = 6f;
        const float k_FingerShapeListItemStyleContainerPaddingWithMargin = k_FingerShapeListItemStyleContainerPadding + k_StyleDefaultMarginSize;
        const float k_LowerToleranceFieldMarginRight = 3f;
        const float k_LowerToleranceFieldWidth = 178f;
        const float k_StyleAllowFlexGrow = 1f;
        const float k_StyleDefaultBorderRadius = 4f;
        const float k_StyleDefaultBorderWidth = 1f;
        const float k_StyleDefaultButtonFontSize = 18f;
        const float k_StyleDefaultElementHeight = 20f;
        const float k_StyleDefaultFieldMinWidth = 42f;
        const float k_StyleDefaultMarginSize = 4f;
        const float k_StyleDefaultTitleFontSize = 12f;
        const float k_StyleOffsetMarginSize = -1f;
        const float k_StyleVerticalBufferSpace = 4f;
        const float k_StyleZeroBorderWidth = 0f;
        const float k_StyleZeroMargin = 0f;
        const float k_StyleContainerMinWidth = 280f;
        const float k_ThresholdRangeSliderMaxWidth = 1600f;

        const string k_MainParentContainerLabelText = "Finger Shapes";
        const string k_AddShapeButtonLabelText = "+";
        const string k_RemoveShapeButtonLabelText = "-";
        const string k_ShapeLabelText = "Shape";
        const string k_ShapeTypeBindingPathLabelText = "shapeType";
        const string k_TargetRangeStringFormatText = "F3";
        const string k_TargetRangeLabelText = "Target";
        const string k_ThresholdLabelText = "Threshold";

        static readonly Color k_StyleContainerBackgroundColorDarkTheme = new Color(0.2f, 0.2f, 0.2f, 1f);
        static readonly Color k_StyleContainerBackgroundColorLightTheme = new Color(0.7f, 0.7f, 0.7f, 1f);
        static readonly Color k_StyleContainerBackgroundBorderColorLightTheme = new Color(0.575f, 0.575f, 0.575f, 1f);
        static readonly Color k_StyleListContainerBackgroundColorDarkTheme = new Color(0.275f, 0.275f, 0.275f, 1f);
        static readonly Color k_StyleListContainerBackgroundColorLightTheme = new Color(0.625f, 0.625f, 0.625f, 1f);
        static readonly Color k_StyleButtonBackgroundColor = new Color(0.275f, 0.275f, 0.275f, 0f);

        VisualElement m_StyleContainer;

        List<XRFingerShapeCondition> m_FingerShapeConditions;

        /// <summary>
        /// See <see cref="Editor"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (target == null)
                return;

            var targetDataDuplicate = (XRHandShape)target;
            m_FingerShapeConditions = targetDataDuplicate.fingerShapeConditions;
            var fingerShapeConditionCountEmptyCheck = m_FingerShapeConditions.Count;
            if (fingerShapeConditionCountEmptyCheck == 0 || fingerShapeConditionCountEmptyCheck == k_FingerCount)
            {
                // This ensures the asset is saved right after creation.
                // The delayCall allows for newly created handShapes to properly serialize
                // However, the delayCall does not allow for proper serialization of existing handShapes that didn't have 5 fingerShapeConditions previously assigned
                EditorApplication.delayCall += () =>
                {
                    SetupEmptyFingerShapeConditions();
                };
            }
            else if (fingerShapeConditionCountEmptyCheck > 0 && fingerShapeConditionCountEmptyCheck < k_FingerCount)
            {
                SetupEmptyFingerShapeConditions();
            }
        }

        /// <summary>
        /// Create the custom inspector UI for XRHandShapeEditor
        /// </summary>
        /// <returns>Returns the root VisualElement housing the custom inspector UI</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var targetData = target as XRHandShape;
            var serializedObject = new SerializedObject(target);

            var titleLabel = new Label(k_MainParentContainerLabelText);
            titleLabel.style.fontSize = k_StyleDefaultTitleFontSize;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            titleLabel.style.marginTop = k_StyleDefaultMarginSize;
            titleLabel.style.marginBottom = k_StyleVerticalBufferSpace;

            root.Add(titleLabel);

            // Styling the background and border for the finger shapes
            m_StyleContainer = new VisualElement();
            m_StyleContainer.style.backgroundColor = EditorGUIUtility.isProSkin ? k_StyleContainerBackgroundColorDarkTheme : k_StyleContainerBackgroundColorLightTheme;
            m_StyleContainer.style.borderBottomColor = m_StyleContainer.style.borderTopColor =
                m_StyleContainer.style.borderLeftColor = m_StyleContainer.style.borderRightColor = EditorGUIUtility.isProSkin ? Color.black : k_StyleContainerBackgroundBorderColorLightTheme;
            m_StyleContainer.style.borderBottomWidth = m_StyleContainer.style.borderTopWidth =
                m_StyleContainer.style.borderLeftWidth = m_StyleContainer.style.borderRightWidth = k_StyleDefaultBorderWidth;

            // Set the border-radius to make corners rounded
            m_StyleContainer.style.borderBottomLeftRadius = m_StyleContainer.style.borderBottomRightRadius =
                m_StyleContainer.style.borderTopLeftRadius = m_StyleContainer.style.borderTopRightRadius = new StyleLength(k_StyleDefaultBorderRadius);

            m_StyleContainer.style.overflow = Overflow.Visible;
            m_StyleContainer.style.minWidth = k_StyleContainerMinWidth;

            m_FingerShapeConditions = targetData.fingerShapeConditions;

            // Populate missing FingerShapeConditions, and assign any empty conditions a default fingerShape
            for (int fingerShapeConditionsCount = 0; fingerShapeConditionsCount < m_FingerShapeConditions.Count; ++fingerShapeConditionsCount)
            {
                var index = fingerShapeConditionsCount;
                var fingerShapeCondition = targetData.fingerShapeConditions[index];

                var assignedShapeCountForThisFinger = 0;
                for (int i = 0; i < fingerShapeCondition.targets.Length; ++i)
                {
                    if (fingerShapeCondition.targets[i].shapeType != XRFingerShapeType.Unspecified)
                        ++assignedShapeCountForThisFinger;
                }

                // The foldout wrapper that each fingerShapeCondition target will be added to
                var fingerFoldoutTopLevelContainer = new Foldout
                {
                    // append arbitrary "(-)" characters when no shape is assigned, as per design
                    text = fingerShapeCondition.fingerID + " (" + (assignedShapeCountForThisFinger > 0 ? assignedShapeCountForThisFinger : "-") + ")",
                    value = true // Start expanded
                };

                fingerFoldoutTopLevelContainer.style.paddingLeft = k_StyleFingerShapeListItemPaddingCorrection;

                var fingerShapeListItemStyleContainers = new List<VisualElement>();

                // Append the bottom add/remove shape buttons container after the individual finger shapes are drawn
                var addRemoveBottomHorizontalLayoutContainer = new VisualElement();

                addRemoveBottomHorizontalLayoutContainer.style.flexDirection = FlexDirection.Row;
                addRemoveBottomHorizontalLayoutContainer.style.marginBottom = k_StyleDefaultMarginSize;
                addRemoveBottomHorizontalLayoutContainer.style.marginTop = k_StyleOffsetMarginSize;
                addRemoveBottomHorizontalLayoutContainer.style.height = k_StyleDefaultElementHeight;

                var addRemoveButtonBottomSpacer = new VisualElement();
                addRemoveButtonBottomSpacer.style.height = k_StyleDefaultElementHeight;
                addRemoveButtonBottomSpacer.style.flexGrow = k_StyleAllowFlexGrow;
                addRemoveBottomHorizontalLayoutContainer.Add(addRemoveButtonBottomSpacer);

                var fingerShapeContainerBottomSpacer = new VisualElement();
                fingerShapeContainerBottomSpacer.style.backgroundColor = EditorGUIUtility.isProSkin ? k_StyleListContainerBackgroundColorDarkTheme : k_StyleListContainerBackgroundColorLightTheme;
                fingerShapeContainerBottomSpacer.style.height = k_StyleDefaultElementHeight;
                fingerShapeContainerBottomSpacer.style.width = k_FingerShapeContainerBottomSpacerWidth;
                fingerShapeContainerBottomSpacer.style.marginRight = k_FingerShapeContainerBottomSpacerMarginRight;
                fingerShapeContainerBottomSpacer.style.borderBottomWidth = fingerShapeContainerBottomSpacer.style.borderTopWidth =
                    fingerShapeContainerBottomSpacer.style.borderLeftWidth = fingerShapeContainerBottomSpacer.style.borderRightWidth = k_StyleZeroBorderWidth;
                fingerShapeContainerBottomSpacer.style.borderBottomLeftRadius = fingerShapeContainerBottomSpacer.style.borderBottomRightRadius = new StyleLength(k_StyleDefaultBorderRadius);

                var addRemoveButtonHorizontalLayout = new VisualElement();
                addRemoveButtonHorizontalLayout.style.flexDirection = FlexDirection.Row;
                addRemoveButtonHorizontalLayout.style.marginBottom = k_StyleDefaultMarginSize;
                addRemoveButtonHorizontalLayout.style.height = k_StyleDefaultElementHeight;

                var addShapeButton = new Button
                {
                    text = k_AddShapeButtonLabelText
                };

                var removeShapeButton = new Button
                {
                    text = k_RemoveShapeButtonLabelText
                };

                // Assign click logic separate from the button declaration in order to be able to pass button reference into contained functions
                addShapeButton.clicked += () =>
                {
                    var newItem = new XRFingerShapeCondition.Target();
                    newItem.shapeType = XRFingerShapeType.Unspecified;

                    fingerShapeCondition = targetData.fingerShapeConditions[index];

                    XRFingerShapeCondition.Target[] newArray = new XRFingerShapeCondition.Target[fingerShapeCondition.targets.Length + 1];
                    Array.Copy(fingerShapeCondition.targets, newArray, fingerShapeCondition.targets.Length);
                    newArray[newArray.Length - 1] = newItem;

                    fingerShapeCondition.targets = newArray;

                    serializedObject.ApplyModifiedProperties();

                    root.Add(m_StyleContainer);
                    root.Bind(serializedObject);

                    serializedObject.Update();
                    Repaint();

                    fingerFoldoutTopLevelContainer.Remove(addRemoveBottomHorizontalLayoutContainer);

                    var fingerShapeListItemStyleContainersBeforeEditing = new List<VisualElement>();

                    foreach (var container in fingerShapeListItemStyleContainers)
                    {
                        fingerShapeListItemStyleContainersBeforeEditing.Add(container);
                    }

                    foreach (var styleContainer in fingerShapeListItemStyleContainersBeforeEditing)
                    {
                        styleContainer.Clear();

                        fingerFoldoutTopLevelContainer.Remove(styleContainer);
                    }

                    CreateFingerShapeList(targetData.fingerShapeConditions, index, fingerShapeListItemStyleContainers, fingerFoldoutTopLevelContainer, serializedObject, index, addRemoveButtonHorizontalLayout, addShapeButton, removeShapeButton, fingerShapeContainerBottomSpacer, addRemoveBottomHorizontalLayoutContainer);

                    foreach (var container in fingerShapeListItemStyleContainersBeforeEditing)
                    {
                        fingerShapeListItemStyleContainers.Remove(container);
                    }

                    assignedShapeCountForThisFinger = 0;
                    for (int i = 0; i < targetData.fingerShapeConditions[index].targets.Length; ++i)
                    {
                        if (targetData.fingerShapeConditions[index].targets[i].shapeType != XRFingerShapeType.Unspecified)
                            ++assignedShapeCountForThisFinger;
                    }

                    fingerFoldoutTopLevelContainer.text = fingerShapeCondition.fingerID + " (" + assignedShapeCountForThisFinger + ")";

                    fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);
                };

                addShapeButton.style.backgroundColor = k_StyleButtonBackgroundColor;
                addShapeButton.style.width = k_AddRemoveButtonWidth;
                addShapeButton.style.height = k_StyleDefaultElementHeight;
                addShapeButton.style.borderBottomWidth = addShapeButton.style.borderTopWidth =
                    addShapeButton.style.borderLeftWidth = addShapeButton.style.borderRightWidth = k_StyleZeroBorderWidth;
                addShapeButton.style.fontSize = k_StyleDefaultButtonFontSize;
                addShapeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                addShapeButton.style.marginTop = k_StyleOffsetMarginSize;
                addShapeButton.style.marginBottom = k_StyleDefaultMarginSize;
                addShapeButton.style.marginLeft = k_AddButtonMarginLeft;

                // Assign click logic separate from the button declaration in order to be able to pass button reference into contained functions
                removeShapeButton.clicked += () =>
                {
                    fingerShapeCondition = targetData.fingerShapeConditions[index];

                    XRFingerShapeCondition.Target[] newArray = new XRFingerShapeCondition.Target[fingerShapeCondition.targets.Length - 1];

                    Array.Copy(fingerShapeCondition.targets, 0, newArray, 0, fingerShapeCondition.targets.Length - 1);

                    fingerShapeCondition.targets = newArray;

                    serializedObject.ApplyModifiedProperties();

                    root.Add(m_StyleContainer);
                    root.Bind(serializedObject);

                    serializedObject.Update();
                    Repaint();

                    fingerFoldoutTopLevelContainer.Remove(addRemoveBottomHorizontalLayoutContainer);

                    var fingerShapeListItemStyleContainersBeforeEditing = new List<VisualElement>();

                    foreach (var container in fingerShapeListItemStyleContainers)
                    {
                        fingerShapeListItemStyleContainersBeforeEditing.Add(container);
                    }

                    foreach (var styleContainer in fingerShapeListItemStyleContainersBeforeEditing)
                    {
                        styleContainer.Clear();

                        fingerFoldoutTopLevelContainer.Remove(styleContainer);
                    }

                    CreateFingerShapeList(targetData.fingerShapeConditions, index, fingerShapeListItemStyleContainers, fingerFoldoutTopLevelContainer, serializedObject, index, addRemoveButtonHorizontalLayout, addShapeButton, removeShapeButton, fingerShapeContainerBottomSpacer, addRemoveBottomHorizontalLayoutContainer);

                    foreach (var container in fingerShapeListItemStyleContainersBeforeEditing)
                    {
                        fingerShapeListItemStyleContainers.Remove(container);
                    }

                    assignedShapeCountForThisFinger = 0;
                    for (int i = 0; i < fingerShapeCondition.targets.Length; ++i)
                    {
                        if (fingerShapeCondition.targets[i].shapeType != XRFingerShapeType.Unspecified)
                            ++assignedShapeCountForThisFinger;
                    }

                    //If fingerShape are empty AFTER removing what may be the last one, repopulate the empty shape selection option
                    if (assignedShapeCountForThisFinger == 0)
                    {
                        var newFingerShapeTargetToPopulateEmptyFingerShape = new XRFingerShapeCondition.Target();
                        newFingerShapeTargetToPopulateEmptyFingerShape.shapeType = XRFingerShapeType.Unspecified;

                        XRFingerShapeCondition.Target[] newArrayToPopulateEmptyFingerShapes = new XRFingerShapeCondition.Target[1];

                        newArrayToPopulateEmptyFingerShapes[0] = newFingerShapeTargetToPopulateEmptyFingerShape;

                        fingerShapeCondition.targets = newArrayToPopulateEmptyFingerShapes;

                        CreateFingerShapeList(targetData.fingerShapeConditions, index, fingerShapeListItemStyleContainers, fingerFoldoutTopLevelContainer, serializedObject, index, addRemoveButtonHorizontalLayout, addShapeButton, removeShapeButton, fingerShapeContainerBottomSpacer, addRemoveBottomHorizontalLayoutContainer);

                        // Hide add/remove UI when there are no shapes assigned.  UI will be shown after new shape is assigned
                        addRemoveButtonHorizontalLayout.Remove(addShapeButton);
                        addRemoveButtonHorizontalLayout.Remove(removeShapeButton);
                        fingerShapeContainerBottomSpacer.Remove(addRemoveButtonHorizontalLayout);
                        addRemoveBottomHorizontalLayoutContainer.Remove(fingerShapeContainerBottomSpacer);
                        fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);
                    }

                    fingerFoldoutTopLevelContainer.text = fingerShapeCondition.fingerID + " (" + (assignedShapeCountForThisFinger > 0 ? assignedShapeCountForThisFinger : "-") + ")";

                    fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);
                };

                removeShapeButton.style.backgroundColor = k_StyleButtonBackgroundColor;
                removeShapeButton.style.width = k_AddRemoveButtonWidth;
                removeShapeButton.style.height = k_StyleDefaultElementHeight;
                removeShapeButton.style.borderBottomWidth = removeShapeButton.style.borderTopWidth =
                    removeShapeButton.style.borderLeftWidth = removeShapeButton.style.borderRightWidth = k_StyleZeroBorderWidth;
                removeShapeButton.style.fontSize = k_StyleDefaultButtonFontSize;
                removeShapeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                removeShapeButton.style.marginTop = k_StyleOffsetMarginSize;
                removeShapeButton.style.marginBottom = k_StyleDefaultMarginSize;
                removeShapeButton.style.marginLeft = k_StyleOffsetMarginSize;

                CreateFingerShapeList(targetData.fingerShapeConditions, fingerShapeConditionsCount, fingerShapeListItemStyleContainers, fingerFoldoutTopLevelContainer, serializedObject, index, addRemoveButtonHorizontalLayout, addShapeButton, removeShapeButton, fingerShapeContainerBottomSpacer, addRemoveBottomHorizontalLayoutContainer);

                // Initially display add/remove shape UI if there was no previously assigned shape before selecting/assigning a new shape
                if (assignedShapeCountForThisFinger > 0)
                {
                    addRemoveButtonHorizontalLayout.Add(addShapeButton);
                    addRemoveButtonHorizontalLayout.Add(removeShapeButton);
                    fingerShapeContainerBottomSpacer.Add(addRemoveButtonHorizontalLayout);
                    addRemoveBottomHorizontalLayoutContainer.Add(fingerShapeContainerBottomSpacer);
                }

                fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);

                m_StyleContainer.Add(fingerFoldoutTopLevelContainer);
            }

            var containerBottomSpacer = new VisualElement();
            containerBottomSpacer.style.height = k_StyleDefaultElementHeight;
            m_StyleContainer.Add(containerBottomSpacer);

            root.Add(m_StyleContainer);
            root.Bind(serializedObject);

            root.TrackSerializedObjectValue(serializedObject, OnTargetPropertyChanged);

            return root;
        }

        /// <summary>
        /// Create the UI elements that display the individual fingerShapes within the parent UI container
        /// </summary>
        /// <param name="fingerShapeConditions">The fingerShapeConditions that house the individual conditions whose data will be utilized to create UI</param>
        /// <param name="fingerShapeConditionsCount">The index of the fingerShapeCondition whose data will be represented by the UI</param>
        /// <param name="fingerShapeListItemStyleContainers">The UI containers that house the fingerShape UI that will be added to them in this function</param>
        /// <param name="fingerFoldoutTopLevelContainer">The main parent fingerShape container that new UI elements will be added to</param>
        /// <param name="serializedObject">The serialized object to update when underlying parameter values are changed</param>
        /// <param name="index">The index of the fingerShapeCondition that new fingerShape UI elements will be added to</param>
        /// <param name="addRemoveButtonHorizontalLayout">The UI element that allows for adding/removing fingerShapes that is shown/hidden based on certain conditions</param>
        /// <param name="addShapeButton">The button to add from the addRemoveButtonHorizontalLayout based on certain conditions</param>
        /// <param name="removeShapeButton">The button to remove from the addRemoveButtonHorizontalLayout based on certain conditions</param>
        /// <param name="fingerShapeContainerBottomSpacer">The UI element used to vertically space fingerShapeCondition UI</param>
        /// <param name="addRemoveBottomHorizontalLayoutContainer">The UI container to show/hide based on a shape being assigned to a fingerShapeCondition</param>
        void CreateFingerShapeList(
            List<XRFingerShapeCondition> fingerShapeConditions, int fingerShapeConditionsCount,
            List<VisualElement> fingerShapeListItemStyleContainers, Foldout fingerFoldoutTopLevelContainer,
            SerializedObject serializedObject, int index, VisualElement addRemoveButtonHorizontalLayout,
            Button addShapeButton, Button removeShapeButton, VisualElement fingerShapeContainerBottomSpacer,
            VisualElement addRemoveBottomHorizontalLayoutContainer)
        {
            for (int fingerShapeConditionTargetCount = 0; fingerShapeConditionTargetCount < fingerShapeConditions[fingerShapeConditionsCount].targets.Length; ++fingerShapeConditionTargetCount)
            {
                var shapeData = (XRHandShape)target;
                var targetData = target as XRHandShape;
                var fingerShapeCondition = targetData.fingerShapeConditions[index];

                var targetPosition = fingerShapeConditionTargetCount;
                var fingerShapeConditionTarget = fingerShapeCondition.targets[targetPosition];
                var shapeEnumField = new EnumField(k_ShapeLabelText, XRFingerShapeType.Unspecified)
                {
                    bindingPath = k_ShapeTypeBindingPathLabelText,
                    value = fingerShapeConditionTarget.shapeType
                };

                shapeEnumField.style.marginTop = k_StyleDefaultMarginSize;
                shapeEnumField.style.marginBottom = k_StyleDefaultMarginSize;

                var shapeAssigned = fingerShapeCondition.targets[targetPosition].shapeType != XRFingerShapeType.Unspecified;

                var assignedShapeCountForThisFinger = 0;

                // Styling the background and border for the ListView
                var fingerShapeListItemStyleContainer = new VisualElement();
                fingerShapeListItemStyleContainer.style.backgroundColor = EditorGUIUtility.isProSkin ? k_StyleListContainerBackgroundColorDarkTheme : k_StyleListContainerBackgroundColorLightTheme;
                fingerShapeListItemStyleContainer.style.borderBottomWidth = fingerShapeListItemStyleContainer.style.borderTopWidth =
                    fingerShapeListItemStyleContainer.style.borderLeftWidth = fingerShapeListItemStyleContainer.style.borderRightWidth = k_StyleZeroBorderWidth;
                fingerShapeListItemStyleContainer.style.borderBottomLeftRadius = fingerShapeListItemStyleContainer.style.borderBottomRightRadius =
                    fingerShapeListItemStyleContainer.style.borderTopLeftRadius = fingerShapeListItemStyleContainer.style.borderTopRightRadius = new StyleLength(k_StyleDefaultBorderRadius);
                fingerShapeListItemStyleContainer.style.marginLeft = k_FingerShapeListItemStyleContainerMarginLeft;
                fingerShapeListItemStyleContainer.style.marginRight = k_FingerShapeListItemStyleContainerMarginRight;
                fingerShapeListItemStyleContainer.style.marginBottom = k_FingerShapeListItemStyleContainerMarginBottom;
                fingerShapeListItemStyleContainer.style.paddingLeft = k_FingerShapeListItemStyleContainerPaddingWithMargin;
                fingerShapeListItemStyleContainer.style.paddingRight = k_FingerShapeListItemStyleContainerPadding;

                fingerShapeListItemStyleContainer.Add(shapeEnumField);

                fingerShapeListItemStyleContainers.Add(fingerShapeListItemStyleContainer); // Add to collection in order to manually refresh when adding/removing shapes

                fingerFoldoutTopLevelContainer.Add(fingerShapeListItemStyleContainer);

                var targetRangeField = new FloatField()
                {
                    value = fingerShapeCondition.targets[targetPosition].desired,
                    formatString = k_TargetRangeStringFormatText
                };

                targetRangeField.RegisterValueChangedCallback(evt =>
                {
                    var fingerShapeCondition = targetData.fingerShapeConditions[index];
                    fingerShapeCondition.targets[targetPosition].desired = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });

                targetRangeField.style.marginTop = k_StyleDefaultMarginSize;
                targetRangeField.style.marginBottom = k_StyleDefaultMarginSize;
                targetRangeField.style.minWidth = k_StyleDefaultFieldMinWidth; // Ensure the label has enough space
                targetRangeField.style.unityTextAlign = TextAnchor.MiddleLeft;

                var targetRangeHorizontalLayout = new VisualElement();
                targetRangeHorizontalLayout.style.flexDirection = FlexDirection.Row;

                Slider targetRangeSlider = new Slider(k_TargetRangeLabelText, k_MinToleranceLimit, k_MaxToleranceLimit)
                {
                    value = fingerShapeCondition.targets[targetPosition].desired,
                    direction = SliderDirection.Horizontal,
                    showInputField = false, // Hide the default input field
                };

                targetRangeSlider.RegisterValueChangedCallback(evt =>
                {
                    targetRangeField.value = evt.newValue;
                });

                targetRangeSlider.style.flexGrow = k_StyleAllowFlexGrow; // Allow the slider to expand
                targetRangeSlider.style.marginRight = k_StyleZeroMargin; // Prevent space between added between slider and label
                targetRangeSlider.style.marginTop = k_StyleDefaultMarginSize;
                targetRangeSlider.style.marginBottom = k_StyleDefaultMarginSize;

                targetRangeHorizontalLayout.Add(targetRangeSlider);
                targetRangeHorizontalLayout.Add(targetRangeField);

                var thresholdRangeHorizontalLayout = new VisualElement();
                thresholdRangeHorizontalLayout.style.flexDirection = FlexDirection.Row;

                var lowerToleranceField = new FloatField(k_ThresholdLabelText) { value = fingerShapeCondition.targets[targetPosition].lowerTolerance };
                var upperToleranceField = new FloatField() { value = fingerShapeCondition.targets[targetPosition].upperTolerance };

                float minThresholdVal = fingerShapeCondition.targets[targetPosition].lowerTolerance;
                float maxThresholdVal = fingerShapeCondition.targets[targetPosition].upperTolerance;

                var thresholdRangeMinMaxSliderContainer = new IMGUIContainer(() =>
                {
                    GUILayout.Space(k_StyleVerticalBufferSpace);

                    EditorGUILayout.MinMaxSlider(
                        ref minThresholdVal, ref maxThresholdVal, k_MinToleranceLimit, k_MaxToleranceLimit,
                        GUILayout.MaxWidth(k_ThresholdRangeSliderMaxWidth)
                        );

                    if (GUI.changed)
                    {
                        lowerToleranceField.value = minThresholdVal;
                        upperToleranceField.value = maxThresholdVal;
                    }
                });

                lowerToleranceField.RegisterValueChangedCallback(evt =>
                {
                    var fingerShapeCondition = targetData.fingerShapeConditions[index];

                    var truncatedValue = (float)TruncateDecimal((decimal)evt.newValue, 3);
                    var newValue = Mathf.Clamp(truncatedValue, k_MinToleranceLimit, k_MaxToleranceLimit);

                    // Force the lower tolerance value to be less than the upper tolerance value
                    if (newValue > fingerShapeCondition.targets[targetPosition].upperTolerance)
                    {
                        newValue = Mathf.Clamp(truncatedValue, k_MinToleranceLimit, maxThresholdVal - k_OffsetToleranceLimit);
                        lowerToleranceField.value = newValue;
                        return;
                    }

                    fingerShapeCondition.targets[targetPosition].lowerTolerance = newValue;

                    serializedObject.ApplyModifiedProperties();

                    // Manually refresh the tolerance field value in order to force the UI to update if user attempts to scroll the value beyond allowed ranges
                    var valueWasOutOfRange = evt.newValue < k_MinToleranceLimit || evt.newValue > maxThresholdVal;
                    if (valueWasOutOfRange)
                        lowerToleranceField.value = newValue;

                    minThresholdVal = newValue;
                    thresholdRangeMinMaxSliderContainer.MarkDirtyRepaint();
                });

                lowerToleranceField.style.marginTop = k_StyleDefaultMarginSize;
                lowerToleranceField.style.marginBottom = k_StyleDefaultMarginSize;
                lowerToleranceField.style.marginRight = k_LowerToleranceFieldMarginRight; // Padding between the min/max slider
                lowerToleranceField.style.width = k_LowerToleranceFieldWidth;

                upperToleranceField.RegisterValueChangedCallback(evt =>
                {
                    var fingerShapeCondition = targetData.fingerShapeConditions[index];

                    // Truncate upper/lower tolerance float values so they don't draw beyond the bounds of the input fields
                    var truncatedValue = (float)TruncateDecimal((decimal)evt.newValue, 3);

                    var newValue = Mathf.Clamp(truncatedValue, k_MinToleranceLimit, k_MaxToleranceLimit);

                    // Force the upper tolerance value to be greater than the lower tolerance value
                    if (newValue < fingerShapeCondition.targets[targetPosition].lowerTolerance)
                    {
                        newValue = Mathf.Clamp(truncatedValue, k_MinToleranceLimit, minThresholdVal + k_OffsetToleranceLimit);
                        upperToleranceField.value = newValue;
                        return;
                    }

                    fingerShapeCondition.targets[targetPosition].upperTolerance = newValue;

                    serializedObject.ApplyModifiedProperties();

                    var valueWasOutOfRange = evt.newValue > k_MaxToleranceLimit || evt.newValue < minThresholdVal;
                    if (valueWasOutOfRange) // Perform check to limit recursion if value is within allowed range
                        upperToleranceField.value = newValue;

                    maxThresholdVal = newValue;
                    thresholdRangeMinMaxSliderContainer.MarkDirtyRepaint();
                });

                upperToleranceField.style.marginTop = k_StyleDefaultMarginSize;
                upperToleranceField.style.marginBottom = k_StyleDefaultMarginSize;
                upperToleranceField.style.width = k_StyleDefaultFieldMinWidth;

                thresholdRangeHorizontalLayout.Add(lowerToleranceField);
                thresholdRangeHorizontalLayout.Add(thresholdRangeMinMaxSliderContainer);
                thresholdRangeHorizontalLayout.Add(upperToleranceField);

                var shapeWasPreviouslyUnassigned = fingerShapeCondition.targets[targetPosition].shapeType == XRFingerShapeType.Unspecified;

                // Correct invalid target range values if a shape is assigned
                if (!shapeWasPreviouslyUnassigned)
                {
                    var valueWasOutOfRange = minThresholdVal > maxThresholdVal || maxThresholdVal < minThresholdVal;
                    if (valueWasOutOfRange)
                    {
                        // Invert the values, in case they were set manually in an improper format; 0/1 could be used, but utilizing the previously set values
                        var inverseUpperTolerance = fingerShapeCondition.targets[targetPosition].upperTolerance;
                        var inverseLowerTolerance = fingerShapeCondition.targets[targetPosition].lowerTolerance;

                        var newValue = inverseLowerTolerance;
                        fingerShapeCondition.targets[targetPosition].upperTolerance = newValue;
                        serializedObject.ApplyModifiedProperties();

                        maxThresholdVal = newValue;
                        thresholdRangeMinMaxSliderContainer.MarkDirtyRepaint();

                        newValue = inverseUpperTolerance;
                        fingerShapeCondition.targets[targetPosition].lowerTolerance = newValue;
                        serializedObject.ApplyModifiedProperties();

                        minThresholdVal = newValue;
                        thresholdRangeMinMaxSliderContainer.MarkDirtyRepaint();
                    }
                }

                shapeEnumField.RegisterValueChangedCallback(evt =>
                {
                    var targetData = target as XRHandShape;
                    shapeData.UpdateFingerShapeType((XRFingerShapeType)evt.newValue, index, targetPosition);

                    serializedObject.ApplyModifiedProperties();

                    shapeAssigned = (XRFingerShapeType)evt.newValue != XRFingerShapeType.Unspecified;

                    if (!shapeAssigned && !shapeWasPreviouslyUnassigned)
                    {
                        assignedShapeCountForThisFinger = 0;
                        for (int i = 0; i < targetData.fingerShapeConditions[index].targets.Length; ++i)
                        {
                            if (targetData.fingerShapeConditions[index].targets[i].shapeType != XRFingerShapeType.Unspecified)
                                ++assignedShapeCountForThisFinger;
                        }

                        shapeWasPreviouslyUnassigned = true;

                        // Hide add/remove shape UI if there are no longer assigned shapes for this finger
                        if (assignedShapeCountForThisFinger == 0)
                        {
                            addRemoveButtonHorizontalLayout.Remove(addShapeButton);
                            addRemoveButtonHorizontalLayout.Remove(removeShapeButton);
                            fingerShapeContainerBottomSpacer.Remove(addRemoveButtonHorizontalLayout);
                            addRemoveBottomHorizontalLayoutContainer.Remove(fingerShapeContainerBottomSpacer);
                        }

                        fingerFoldoutTopLevelContainer.text = fingerShapeCondition.fingerID + " (" + (assignedShapeCountForThisFinger > 0 ? assignedShapeCountForThisFinger : "-") + ")";

                        fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);
                    }
                    else if (shapeAssigned && shapeWasPreviouslyUnassigned)
                    {
                        assignedShapeCountForThisFinger = 0;
                        for (int i = 0; i < targetData.fingerShapeConditions[index].targets.Length; ++i)
                        {
                            if (targetData.fingerShapeConditions[index].targets[i].shapeType != XRFingerShapeType.Unspecified)
                                ++assignedShapeCountForThisFinger;
                        }

                        shapeAssigned = true;

                        shapeWasPreviouslyUnassigned = false;

                        // Display add/remove shape UI if there was no previously assigned shape before selecting/assigning a new shape
                        if (shapeAssigned || assignedShapeCountForThisFinger == 1)
                        {
                            addRemoveButtonHorizontalLayout.Add(addShapeButton);
                            addRemoveButtonHorizontalLayout.Add(removeShapeButton);
                            fingerShapeContainerBottomSpacer.Add(addRemoveButtonHorizontalLayout);
                            addRemoveBottomHorizontalLayoutContainer.Add(fingerShapeContainerBottomSpacer);

                            fingerFoldoutTopLevelContainer.text = fingerShapeCondition.fingerID + " (" + (assignedShapeCountForThisFinger > 0 ? assignedShapeCountForThisFinger : "-") + ")";

                            fingerFoldoutTopLevelContainer.Add(addRemoveBottomHorizontalLayoutContainer);
                        }
                    }

                    // Show/hide range slider based on valid shape assignment
                    if (shapeAssigned)
                    {
                        fingerShapeListItemStyleContainer.Add(targetRangeHorizontalLayout);
                        fingerShapeListItemStyleContainer.Add(thresholdRangeHorizontalLayout);
                    }
                    else
                    {
                        fingerShapeListItemStyleContainer.Remove(targetRangeHorizontalLayout);
                        fingerShapeListItemStyleContainer.Remove(thresholdRangeHorizontalLayout);
                    }
                });

                if (shapeAssigned)
                {
                    fingerShapeListItemStyleContainer.Add(targetRangeHorizontalLayout);
                    fingerShapeListItemStyleContainer.Add(thresholdRangeHorizontalLayout);
                }
            }
        }

        /// <summary>
        /// Populate missing fingerShape entries for a new or existing handShape with fewer than 5 fingerShapeConditions
        /// </summary>
        void SetupEmptyFingerShapeConditions()
        {
            if (target == null)
                return;

            var targetDataDuplicate = (XRHandShape)target;

            m_FingerShapeConditions = targetDataDuplicate.fingerShapeConditions;
            var fingerShapeConditionCountEmptyCheck = m_FingerShapeConditions.Count;

            // Populate missing FingerShapeConditions, and assign any empty conditions a default fingerShape
            if (fingerShapeConditionCountEmptyCheck < 5)
            {
                var unassignedFingerIDs = new List<XRHandFingerID>();
                unassignedFingerIDs.Add(XRHandFingerID.Index);
                unassignedFingerIDs.Add(XRHandFingerID.Little);
                unassignedFingerIDs.Add(XRHandFingerID.Middle);
                unassignedFingerIDs.Add(XRHandFingerID.Ring);
                unassignedFingerIDs.Add(XRHandFingerID.Thumb);

                // Prevent duplicate fingerShapeIDs from being assigned by removing existing IDs from the above collection
                if (m_FingerShapeConditions.Count > 1)
                {
                    foreach (var fingerShapeCondition in m_FingerShapeConditions)
                    {
                        unassignedFingerIDs.Remove(fingerShapeCondition.fingerID);
                    }
                }

                var tempFingerShapeConditions = new List<XRFingerShapeCondition>();

                for (int i = 0; i < k_FingerCount; ++i)
                {
                    if (i + 1 > fingerShapeConditionCountEmptyCheck || m_FingerShapeConditions[i] == null)
                    {
                        var newEmptyFingerShapeCondition = new XRFingerShapeCondition();

                        var newFingerShapeTargetToPopulateEmptyFingerShape =
                            new XRFingerShapeCondition.Target();
                        newFingerShapeTargetToPopulateEmptyFingerShape.shapeType =
                            XRFingerShapeType.Unspecified;

                        XRFingerShapeCondition.Target[] newArrayToPopulateEmptyFingerShapes =
                            new XRFingerShapeCondition.Target[1];

                        newArrayToPopulateEmptyFingerShapes[0] =
                            newFingerShapeTargetToPopulateEmptyFingerShape;

                        newEmptyFingerShapeCondition.targets = newArrayToPopulateEmptyFingerShapes;

                        newEmptyFingerShapeCondition.fingerID = unassignedFingerIDs.Last();
                        unassignedFingerIDs.Remove(unassignedFingerIDs.Last());

                        tempFingerShapeConditions.Add(newEmptyFingerShapeCondition);
                    }
                    else
                    {
                        tempFingerShapeConditions.Add(m_FingerShapeConditions[i]);
                    }
                }

                targetDataDuplicate.fingerShapeConditions = tempFingerShapeConditions;
                m_FingerShapeConditions = tempFingerShapeConditions;

                serializedObject.ApplyModifiedProperties();
            }
        }

        void OnTargetPropertyChanged(SerializedObject _)
        {
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Truncates upper/lower tolerance float values so they don't draw beyond the bounds of the input fields
        /// </summary>
        /// <param name="value">The value to truncate</param>
        /// <param name="precision">Limit the returned value to the decimal precision defined</param>
        /// <returns></returns>
        decimal TruncateDecimal(decimal value, int precision)
        {
            decimal stepPrecision = (decimal)Math.Pow(10, precision);
            decimal initialTruncatedValue = Math.Truncate(stepPrecision * value);
            return initialTruncatedValue / stepPrecision;
        }
    }
}
