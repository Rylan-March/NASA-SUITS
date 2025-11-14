using System;
using System.Collections.Generic;
using Unity.Collections;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Processing;
using UnityEngine.XR.Hands.ProviderImplementation;
using UnityEngine.XR.Hands.Capture;

namespace UnityEditor.XR.Hands.Capture
{
    class XRHandCapturePlayback
    {
        const int k_FingerCount = 5;
        const float k_DefaultTolerance = 0.15f;
        const XRFingerShapeTypes k_DefaultSupportedFingerShapeTypes =
            XRFingerShapeTypes.FullCurl | XRFingerShapeTypes.Spread | XRFingerShapeTypes.Pinch;
        const string k_DefaultSavePath = "Assets/HandRecordings/";

        const string k_PrefKeyPrefix = "XRHandCapturePlayback_";
        const string k_PrefKeyRecordingSavePath = k_PrefKeyPrefix + "RecordingSavePath";
        const string k_PrefKeyLowerTolerance = k_PrefKeyPrefix + "LowerTolerance";
        const string k_PrefKeyUpperTolerance = k_PrefKeyPrefix + "UpperTolerance";
        const string k_PrefKeyShouldDrawMeshes = k_PrefKeyPrefix + "ShouldDrawMeshes";
        const string k_PrefKeyShouldDrawJoints = k_PrefKeyPrefix + "ShouldDrawJoints";
        const string k_PrefKeyLeftHandPrefabGUID = k_PrefKeyPrefix + "LeftHandPrefabGUID";
        const string k_PrefKeyRightHandPrefabGUID = k_PrefKeyPrefix + "RightHandPrefabGUID";
        const string k_PrefKeyHandMaterialGUID = k_PrefKeyPrefix + "HandMaterialGUID";
        const string k_PrefKeyJointPrefabGUID = k_PrefKeyPrefix + "JointPrefabGUID";
        const string k_PrefKeySelectedRecordingGUID = k_PrefKeyPrefix + "SelectedRecordingGUID";
        const string k_PrefKeySelectedFrameID = k_PrefKeyPrefix + "SelectedFrameID";
        const string k_PrefKeyHandShapeToOverwriteGUID = k_PrefKeyPrefix + "HandShapeToOverwriteGUID";

        // "Left Hand Tracking" prefab from HandVisualizer sample
        const string k_DefaultLeftHandPrefabGUID = "b3ed8a0a703ebd34a9e44ed3d9f1fcf6";
        // "Right Hand Tracking" prefab from HandVisualizer sample
        const string k_DefaultRightHandPrefabGUID = "3f7511fbc40ae7a4b89c3298a3de199d";
        // "HandsDefaultMaterial" material from HandVisualizer sample
        const string k_DefaultHandMaterialGUID = "613690cd962241049a0ec289a6ff835e";
        // "Joint" prefab from HandVisualizer sample
        const string k_DefaultJointPrefabGUID = "254b742d65a15d14b9df756ae77de868";

        GameObject m_LeftHandPrefab;
        GameObject m_RightHandPrefab;
        Material m_HandMeshMaterial;
        GameObject m_JointPrefab;
        bool m_ShouldDrawMeshes;
        bool m_ShouldDrawJoints;
        HandGameObjects m_LeftHandGameObjects;
        HandGameObjects m_RightHandGameObjects;
        Transform m_HandObjectParentTransform;
        XRHand m_LeftHand;
        XRHand m_RightHand;
        XRFingerShape[] m_LeftFingerShapes = new XRFingerShape[k_FingerCount];
        XRFingerShape[] m_RightFingerShapes = new XRFingerShape[k_FingerCount];
        List<XRFingerShapeCondition> m_LeftFingerShapeConditions = new List<XRFingerShapeCondition>();
        List<XRFingerShapeCondition> m_RightFingerShapeConditions = new List<XRFingerShapeCondition>();
        XRHandShape m_LeftHandShape;
        XRHandShape m_RightHandShape;
        string m_RecordingsAssetSavePath;
        float m_FingerShapeLowerTolerance;
        float m_FingerShapeUpperTolerance;
        XRHandCaptureSequence m_SelectedRecording;
        int m_SelectedFrameID;
        XRHandShape m_HandShapeToOverwrite;
        XRHandCaptureImporter m_RecordingImporter;
        internal event Action recordingChanged;
        internal event Action<int> frameChanged;

        static XRHandCapturePlayback s_Instance;

        internal GameObject leftHandPrefab
        {
            get => m_LeftHandPrefab;
            set => m_LeftHandPrefab = value;
        }
        internal GameObject rightHandPrefab
        {
            get => m_RightHandPrefab;
            set => m_RightHandPrefab = value;
        }

        internal Material handMeshMaterial
        {
            get => m_HandMeshMaterial;
            set => m_HandMeshMaterial = value;
        }
        internal GameObject jointPrefab
        {
            get => m_JointPrefab;
            set => m_JointPrefab = value;
        }

        internal bool shouldDrawMeshes
        {
            get => m_ShouldDrawMeshes;
            set
            {
                if (m_ShouldDrawMeshes == value)
                    return;

                m_ShouldDrawMeshes = value;
                UpdateRenderingVisibility();
            }
        }

        internal bool shouldDrawJoints
        {
            get => m_ShouldDrawJoints;
            set
            {
                if (m_ShouldDrawJoints == value)
                    return;

                m_ShouldDrawJoints = value;
                UpdateRenderingVisibility();
            }
        }

        internal string recordingsAssetSavePath
        {
            get => m_RecordingsAssetSavePath;
            set => m_RecordingsAssetSavePath = value;
        }

        internal float fingerShapeLowerTolerance
        {
            get => m_FingerShapeLowerTolerance;
            set => m_FingerShapeLowerTolerance = value;
        }

        internal float fingerShapeUpperTolerance
        {
            get => m_FingerShapeUpperTolerance;
            set => m_FingerShapeUpperTolerance = value;
        }

        internal XRHandCaptureSequence selectedRecording
        {
            get => m_SelectedRecording;
            set
            {
                if (m_SelectedRecording != value)
                {
                    m_SelectedRecording = value;
                    recordingChanged?.Invoke();
                }
            }
        }

        internal int selectedFrameID
        {
            get => m_SelectedFrameID;
            set
            {
                if (m_SelectedFrameID != value)
                {
                    m_SelectedFrameID = value;
                    frameChanged?.Invoke(value);
                }
            }
        }

        internal XRHandShape leftHandShape => m_LeftHandShape;
        internal XRHandShape rightHandShape => m_RightHandShape;

        internal XRHandCaptureImporter recordingImporter => m_RecordingImporter;

        internal XRHandShape handShapeToOverwrite
        {
            get => m_HandShapeToOverwrite;
            set => m_HandShapeToOverwrite = value;
        }

        XRHandCapturePlayback()
        {
            m_RecordingsAssetSavePath = k_DefaultSavePath;
            m_FingerShapeLowerTolerance = k_DefaultTolerance;
            m_FingerShapeUpperTolerance = k_DefaultTolerance;
            m_ShouldDrawMeshes = true;
            m_ShouldDrawJoints = true;
            m_SelectedFrameID = 0;
#if UNITY_ANDROID
            var fileService = new AndroidDeviceFileService();
            m_RecordingImporter = new XRHandCaptureImporter(fileService);
#else
            m_RecordingImporter = null;
#endif
            InitializeXRHands();
        }

        internal static XRHandCapturePlayback GetInstance()
        {
            if (s_Instance == null)
            {
                s_Instance = new XRHandCapturePlayback();
                s_Instance.Initialize();
            }
            return s_Instance;
        }

        void LoadPreferences()
        {
            // Load all preferences
            m_RecordingsAssetSavePath = EditorPrefs.GetString(k_PrefKeyRecordingSavePath);
            if (string.IsNullOrEmpty(m_RecordingsAssetSavePath))
                m_RecordingsAssetSavePath = k_DefaultSavePath;
            m_FingerShapeLowerTolerance = EditorPrefs.GetFloat(k_PrefKeyLowerTolerance, k_DefaultTolerance);
            m_FingerShapeUpperTolerance = EditorPrefs.GetFloat(k_PrefKeyUpperTolerance, k_DefaultTolerance);
            m_ShouldDrawMeshes = EditorPrefs.GetBool(k_PrefKeyShouldDrawMeshes, true);
            m_ShouldDrawJoints = EditorPrefs.GetBool(k_PrefKeyShouldDrawJoints, true);
            m_SelectedFrameID = EditorPrefs.GetInt(k_PrefKeySelectedFrameID, 0);

            // Load references to assets
            m_LeftHandPrefab = LoadAssetFromGUID<GameObject>(k_PrefKeyLeftHandPrefabGUID, k_DefaultLeftHandPrefabGUID);
            m_RightHandPrefab = LoadAssetFromGUID<GameObject>(k_PrefKeyRightHandPrefabGUID, k_DefaultRightHandPrefabGUID);
            m_HandMeshMaterial = LoadAssetFromGUID<Material>(k_PrefKeyHandMaterialGUID, k_DefaultHandMaterialGUID);
            m_JointPrefab = LoadAssetFromGUID<GameObject>(k_PrefKeyJointPrefabGUID, k_DefaultJointPrefabGUID);
            m_SelectedRecording = LoadAssetFromGUID<XRHandCaptureSequence>(k_PrefKeySelectedRecordingGUID);
            m_HandShapeToOverwrite = LoadAssetFromGUID<XRHandShape>(k_PrefKeyHandShapeToOverwriteGUID);
        }

        void SavePreferences()
        {
            // Save all preferences
            EditorPrefs.SetString(k_PrefKeyRecordingSavePath, m_RecordingsAssetSavePath);
            EditorPrefs.SetFloat(k_PrefKeyLowerTolerance, m_FingerShapeLowerTolerance);
            EditorPrefs.SetFloat(k_PrefKeyUpperTolerance, m_FingerShapeUpperTolerance);
            EditorPrefs.SetBool(k_PrefKeyShouldDrawMeshes, m_ShouldDrawMeshes);
            EditorPrefs.SetBool(k_PrefKeyShouldDrawJoints, m_ShouldDrawJoints);
            EditorPrefs.SetInt(k_PrefKeySelectedFrameID, m_SelectedFrameID);

            // Save asset GUIDs
            EditorPrefs.SetString(k_PrefKeyLeftHandPrefabGUID, GetAssetGUID(m_LeftHandPrefab));
            EditorPrefs.SetString(k_PrefKeyRightHandPrefabGUID, GetAssetGUID(m_RightHandPrefab));
            EditorPrefs.SetString(k_PrefKeyHandMaterialGUID, GetAssetGUID(m_HandMeshMaterial));
            EditorPrefs.SetString(k_PrefKeyJointPrefabGUID, GetAssetGUID(m_JointPrefab));
            EditorPrefs.SetString(k_PrefKeySelectedRecordingGUID, GetAssetGUID(m_SelectedRecording));
            EditorPrefs.SetString(k_PrefKeyHandShapeToOverwriteGUID, GetAssetGUID(m_HandShapeToOverwrite));
        }

        static T LoadAssetFromGUID<T>(string guidPrefKey, string defaultGUID = null) where T : UnityEngine.Object
        {
            string guid = EditorPrefs.GetString(guidPrefKey);

            // If no GUID was stored in prefs, fall back to the provided default GUID.
            if (string.IsNullOrEmpty(guid))
                guid = defaultGUID;

            if (string.IsNullOrEmpty(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        static string GetAssetGUID(in UnityEngine.Object asset)
        {
            if (asset == null)
                return string.Empty;

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return AssetDatabase.AssetPathToGUID(path);
        }

        internal void Initialize()
        {
            LoadPreferences();
        }

        internal void OnDestroy()
        {
            SavePreferences();

            m_LeftHandGameObjects?.OnDestroy();
            m_RightHandGameObjects?.OnDestroy();
            m_LeftHandGameObjects = null;
            m_RightHandGameObjects = null;

            if (m_LeftHandShape != null)
                ScriptableObject.DestroyImmediate(m_LeftHandShape);
            if (m_RightHandShape != null)
                ScriptableObject.DestroyImmediate(m_RightHandShape);

            m_LeftHand.Dispose();
            m_RightHand.Dispose();

            s_Instance = null;
        }

        void InitializeXRHands()
        {
            m_LeftHand = new XRHand(Handedness.Left, Allocator.Persistent);
            m_RightHand = new XRHand(Handedness.Right, Allocator.Persistent);

            var leftHandJoints = m_LeftHand.GetRawJointArray();
            var rightHandJoints = m_RightHand.GetRawJointArray();

            for (var jointID = XRHandJointID.BeginMarker; jointID < XRHandJointID.EndMarker; ++jointID)
            {
                leftHandJoints[jointID.ToIndex()] = XRHandProviderUtility.CreateJoint(
                    Handedness.Left,
                    XRHandJointTrackingState.Pose,
                    jointID,
                    Pose.identity);

                rightHandJoints[jointID.ToIndex()] = XRHandProviderUtility.CreateJoint(
                    Handedness.Right,
                    XRHandJointTrackingState.Pose,
                    jointID,
                    Pose.identity);
            }
        }

        internal void UpdateHandShapes()
        {
            if (!IsFrameIDValid(m_SelectedFrameID))
                return;

            // Compute finger shape conditions for both hands
            ComputeFingerShapesAndConditions(m_SelectedFrameID);

            // Update the hand shapes with the computed finger shape conditions
            m_LeftHandShape = CreateOrUpdateHandShape(m_LeftHandShape, m_LeftFingerShapeConditions);
            m_RightHandShape = CreateOrUpdateHandShape(m_RightHandShape, m_RightFingerShapeConditions);
        }

        static XRHandShape CreateOrUpdateHandShape(XRHandShape existingShape, List<XRFingerShapeCondition> conditions)
        {
            if (existingShape == null)
            {
                existingShape = ScriptableObject.CreateInstance<XRHandShape>();
            }

            existingShape.fingerShapeConditions = conditions;

            // Mark for saving
            EditorUtility.SetDirty(existingShape);

            return existingShape;
        }

        void UpdateRenderingVisibility()
        {
            if (!IsFrameIDValid(m_SelectedFrameID))
                return;

            var frameData = m_SelectedRecording.frames[m_SelectedFrameID];

            if (m_LeftHandGameObjects != null)
            {
                m_LeftHandGameObjects.ToggleDrawMesh(m_ShouldDrawMeshes && frameData.isLeftHandTracked);
                m_LeftHandGameObjects.ToggleDebugDrawJoints(m_ShouldDrawJoints && frameData.isLeftHandTracked);
            }

            if (m_RightHandGameObjects != null)
            {
                m_RightHandGameObjects.ToggleDrawMesh(m_ShouldDrawMeshes && frameData.isRightHandTracked);
                m_RightHandGameObjects.ToggleDebugDrawJoints(m_ShouldDrawJoints && frameData.isRightHandTracked);
            }
        }

        internal bool IsRecordingDataAvailable()
        {
            return m_SelectedRecording != null && m_SelectedRecording.frames != null && m_SelectedRecording.frames.Count > 0;
        }

        internal bool IsFrameIDValid(int frameID)
        {
            return IsRecordingDataAvailable() && frameID >= 0 && frameID < m_SelectedRecording.frames.Count;
        }

        void SyncHandDataToFrame(int frameID)
        {
            if (!IsFrameIDValid(frameID))
                return;

            var frame = m_SelectedRecording.frames[frameID];

            // Update hand joints using the selected frame
            UpdateHandJoints(ref m_LeftHand, frame, Handedness.Left);
            UpdateHandJoints(ref m_RightHand, frame, Handedness.Right);
        }

        static void UpdateHandJoints(ref XRHand hand, in XRHandCaptureFrame frame, Handedness handedness)
        {
            bool isFrameDataValid = frame.IsHandTracked(handedness);

            if (!isFrameDataValid)
                return;

            var dstHandJoints = hand.GetRawJointArray();

            for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            {
                XRHandJoint joint = dstHandJoints[jointIndex];
                frame.TryGetJoint(out XRHandJoint srcJoint, handedness, XRHandJointIDUtility.FromIndex(jointIndex));
                Pose pose = srcJoint.TryGetPose(out var srcPose) ? srcPose : Pose.identity;
                joint.SetPose(pose);
                dstHandJoints[jointIndex] = joint;
            }
        }

        XRFingerShapeCondition.Target CreateTarget(XRFingerShapeType fingerShapeType, float desired)
        {
            var target = new XRFingerShapeCondition.Target();
            target.shapeType = fingerShapeType;
            target.desired = desired;
            target.lowerTolerance = m_FingerShapeLowerTolerance;
            target.upperTolerance = m_FingerShapeUpperTolerance;
            return target;
        }

        List<XRFingerShapeCondition.Target> CreateFingerShapeTargetList(XRFingerShape fingerShape,
            XRFingerShapeTypes supportedTypes)
        {
            List<XRFingerShapeCondition.Target> targetList = new List<XRFingerShapeCondition.Target>();

            if ((supportedTypes & XRFingerShapeTypes.FullCurl) != 0 &&
                fingerShape.TryGetFullCurl(out var fullCurl))
            {
                targetList.Add(CreateTarget(XRFingerShapeType.FullCurl, fullCurl));
            }

            if ((supportedTypes & XRFingerShapeTypes.BaseCurl) != 0 &&
                fingerShape.TryGetBaseCurl(out var baseCurl))
            {
                targetList.Add(CreateTarget(XRFingerShapeType.BaseCurl, baseCurl));
            }

            if ((supportedTypes & XRFingerShapeTypes.TipCurl) != 0 &&
                fingerShape.TryGetTipCurl(out var tipCurl))
            {
                targetList.Add(CreateTarget(XRFingerShapeType.TipCurl, tipCurl));
            }

            if ((supportedTypes & XRFingerShapeTypes.Pinch) != 0 &&
                fingerShape.TryGetPinch(out var pinch))
            {
                targetList.Add(CreateTarget(XRFingerShapeType.Pinch, pinch));
            }

            if ((supportedTypes & XRFingerShapeTypes.Spread) != 0 &&
                fingerShape.TryGetSpread(out var spread))
            {
                targetList.Add(CreateTarget(XRFingerShapeType.Spread, spread));
            }

            return targetList;
        }

        XRFingerShapeCondition CreateFingerShapeCondition(XRFingerShape fingerShape, XRHandFingerID fingerId)
        {
            List<XRFingerShapeCondition.Target> targetList =
                CreateFingerShapeTargetList(fingerShape, k_DefaultSupportedFingerShapeTypes);

            XRFingerShapeCondition.Target[] targets = targetList.ToArray();

            var fingerShapeCondition = new XRFingerShapeCondition();
            fingerShapeCondition.fingerID = fingerId;
            fingerShapeCondition.targets = targets;

            return fingerShapeCondition;
        }

        void ComputeFingerShapesAndConditions(int frameId)
        {
            if (!IsFrameIDValid(frameId))
                return;

            // Sync the hand data to the selected frame
            SyncHandDataToFrame(frameId);

            // Calculate and update the finger shapes
            CalculateFingerShapes(in m_LeftHand, ref m_LeftFingerShapes);
            CalculateFingerShapes(in m_RightHand, ref m_RightFingerShapes);

            // Create the finger shape conditions using the calculated finger shapes
            CreateFingerShapeConditions();
        }

        static void CalculateFingerShapes(in XRHand hand, ref XRFingerShape[] fingerShapes)
        {
            for (var fingerIndex = (int)XRHandFingerID.Thumb; fingerIndex <= (int)XRHandFingerID.Little; ++fingerIndex)
            {
                fingerShapes[fingerIndex] = hand.CalculateFingerShapeUncached((XRHandFingerID)fingerIndex, XRFingerShapeTypes.All);
            }
        }

        internal void CreateFingerShapeConditions()
        {
            m_LeftFingerShapeConditions.Clear();
            m_RightFingerShapeConditions.Clear();

            for (var fingerID = XRHandFingerID.Thumb; fingerID <= XRHandFingerID.Little; ++fingerID)
            {
                // Create left finger shape targets
                var leftFingerShapeCondition = CreateFingerShapeCondition(m_LeftFingerShapes[(int)fingerID], fingerID);
                m_LeftFingerShapeConditions.Add(leftFingerShapeCondition);

                // Create right finger shape targets
                var rightFingerShapeCondition = CreateFingerShapeCondition(m_RightFingerShapes[(int)fingerID], fingerID);
                m_RightFingerShapeConditions.Add(rightFingerShapeCondition);
            }
        }

        internal void RenderFrame(int frameId)
        {
            if (!IsFrameIDValid(frameId))
                return;

            EnsureHandGameObjectsInitialized();

            // Update XRHand data to sync with the selected frame
            SyncHandDataToFrame(frameId);

            // Update the hand visuals in Scene View to reflect the current frame's data
            SyncVisualsToFrame(frameId);
        }

        void EnsureHandGameObjectsInitialized()
        {
            try
            {
                if (m_LeftHandGameObjects == null)
                {
                    m_LeftHandGameObjects = new HandGameObjects(
                        Handedness.Left,
                        m_HandObjectParentTransform,
                        m_LeftHandPrefab,
                        m_HandMeshMaterial,
                        m_JointPrefab);
                }

                if (m_RightHandGameObjects == null)
                {
                    m_RightHandGameObjects = new HandGameObjects(
                        Handedness.Right,
                        m_HandObjectParentTransform,
                        m_RightHandPrefab,
                        m_HandMeshMaterial,
                        m_JointPrefab);
                }
            }
            catch (MissingComponentException e)
            {
                Debug.LogException(e);
            }
        }

        void SyncVisualsToFrame(int frameId)
        {
            try
            {
                if (!IsFrameIDValid(frameId))
                    return;
                var frameData = m_SelectedRecording.frames[frameId];

                // Update the transforms of the visual joints
                var isLeftHandTracked = frameData.IsHandTracked(Handedness.Left);
                var isRightHandTracked = frameData.IsHandTracked(Handedness.Right);

                m_LeftHandGameObjects.UpdateJoints(m_LeftHand, isLeftHandTracked);
                m_RightHandGameObjects.UpdateJoints(m_RightHand, isRightHandTracked);

                UpdateRenderingVisibility();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
