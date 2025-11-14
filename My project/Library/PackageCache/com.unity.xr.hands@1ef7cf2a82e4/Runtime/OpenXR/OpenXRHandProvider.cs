#if UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.OpenXR.Meshing;
using UnityEngine.XR.Hands.ProviderImplementation;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_OPENXR_PACKAGE_1_8
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

namespace UnityEngine.XR.Hands.OpenXR
{
    /// <summary>
    /// Hand tracking provider for the OpenXR platform.
    /// </summary>
    public unsafe class OpenXRHandProvider : XRHandSubsystemProvider
    {
        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Start"/>.
        /// </summary>
        public override void Start() {}

        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Stop"/>.
        /// </summary>
        public override void Stop() {}

        /// <summary>
        /// See <see cref="UnityEngine.SubsystemsImplementation.SubsystemProvider{T}.Destroy"/>.
        /// </summary>
        public override void Destroy() => NativeApi.Destroy();

        /// <inheritdoc/>
        public override void GetHandLayout(NativeArray<bool> handJointsInLayout)
        {
            if (!NativeApi.TryInitialize())
            {
                Debug.LogError("OpenXR hand provider failed to initialize - no data will be tracked or surfaced!");
                return;
            }

            handJointsInLayout[XRHandJointID.Palm.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.Wrist.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.ThumbMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.ThumbTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.IndexMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.IndexTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.MiddleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.MiddleTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.RingMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.RingTip.ToIndex()] = true;

            handJointsInLayout[XRHandJointID.LittleMetacarpal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleProximal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleIntermediate.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleDistal.ToIndex()] = true;
            handJointsInLayout[XRHandJointID.LittleTip.ToIndex()] = true;

            m_IsValid = true;
        }

        /// <inheritdoc/>
        public override XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
            XRHandSubsystem.UpdateType updateType,
            ref Pose leftHandRootPose,
            NativeArray<XRHandJoint> leftHandJoints,
            ref Pose rightHandRootPose,
            NativeArray<XRHandJoint> rightHandJoints)
        {
            if (!m_IsValid)
                return XRHandSubsystem.UpdateSuccessFlags.None;

            return NativeApi.TryUpdateHands(
                updateType,
                ref leftHandRootPose,
                leftHandJoints.GetUnsafePtr(),
                ref rightHandRootPose,
                rightHandJoints.GetUnsafePtr());
        }

        /// <inheritdoc/>
        public override bool canSurfaceCommonPoseData
        {
            get
            {
                if (m_IsHandInteractionProfileEnabled)
                    return true;

                m_IsHandInteractionProfileEnabled =
#if UNITY_OPENXR_PACKAGE_1_8
                    OpenXRRuntime.IsExtensionEnabled(HandInteractionProfile.extensionString);
#else
                    false;
#endif
                return m_IsHandInteractionProfileEnabled;
            }
        }
        bool m_IsHandInteractionProfileEnabled;

        /// <inheritdoc/>
        public override bool TryGetAimPose(Handedness handedness, out Pose aimPose)
        {
            aimPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.aimPosition, out var position))
                aimPose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.aimRotation, out var rotation))
                aimPose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.isAimPoseTracked, out var isTracked) && isTracked;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetAimActivateValue(Handedness handedness, out float aimActivateValue)
        {
            aimActivateValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.aimActivateValue, out aimActivateValue);
            return handDevice.TryGetFeatureValue(Usages.isAimActivateValueReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetGraspValue(Handedness handedness, out float graspValue)
        {
            graspValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.graspValue, out graspValue);
            return handDevice.TryGetFeatureValue(Usages.isGraspValueReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetGripPose(Handedness handedness, out Pose gripPose)
        {
            gripPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.gripPosition, out var position))
                gripPose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.gripRotation, out var rotation))
                gripPose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.isGripPoseTracked, out var isTracked) && isTracked;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPinchPose(Handedness handedness, out Pose pinchPose)
        {
            pinchPose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.pinchPosition, out var position))
                pinchPose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.pinchRotation, out var rotation))
                pinchPose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.isPinchPoseTracked, out var isTracked) && isTracked;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPinchValue(Handedness handedness, out float pinchValue)
        {
            pinchValue = 0f;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            handDevice.TryGetFeatureValue(Usages.pinchValue, out pinchValue);
            return handDevice.TryGetFeatureValue(Usages.isPinchValueReady, out var isReady) && isReady;
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool TryGetPokePose(Handedness handedness, out Pose pokePose)
        {
            pokePose = Pose.identity;
#if UNITY_OPENXR_PACKAGE_1_8
            if (!TryGetHandDevice(handedness, out var handDevice))
                return false;

            if (handDevice.TryGetFeatureValue(Usages.pokePosition, out var position))
                pokePose.position = position;

            if (handDevice.TryGetFeatureValue(Usages.pokeRotation, out var rotation))
                pokePose.rotation = rotation;

            return handDevice.TryGetFeatureValue(Usages.isPokePoseTracked, out var isTracked) && isTracked;
#else
            return false;
#endif
        }

        /// <summary>
        /// The <see cref="OpenXRHandProvider"/> calls into this when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <value>
        /// This is only useful for developers exposing hand mesh data for their
        /// platform. If you are a user making a game or app, you do not need to
        /// worry about this.
        /// </value>
        public IOpenXRHandMeshDataSupplier handMeshDataSupplier { get; set; }

        /// <summary>
        /// Attempt to retrieve hand mesh data from the platform. Only called when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <param name="result">
        /// Output data for hand meshes.
        /// </param>
        /// <param name="queryParams">
        /// Input data for hand meshes.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and either hand has
        /// valid data. Otherwise, returns <see langword="false"/>.
        /// </returns>
        public override bool TryGetMeshData(ref XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams)
            => handMeshDataSupplier != null ? handMeshDataSupplier.TryGetMeshData(ref result, ref queryParams) : false;

        /// <inheritdoc/>
        public override XRDetectedHandMeshLayout detectedHandMeshLayout => NativeApi.GetDetectedHandMeshLayout();

#if UNITY_OPENXR_PACKAGE_1_8
        static class Usages
        {
            internal static readonly InputFeatureUsage<bool> isAimPoseTracked = new InputFeatureUsage<bool>("PointerIsTracked");
            internal static readonly InputFeatureUsage<Vector3> aimPosition = new InputFeatureUsage<Vector3>("PointerPosition");
            internal static readonly InputFeatureUsage<Quaternion> aimRotation = new InputFeatureUsage<Quaternion>("PointerRotation");

            internal static readonly InputFeatureUsage<bool> isAimActivateValueReady = new InputFeatureUsage<bool>("PointerActivateReady");
            internal static readonly InputFeatureUsage<float> aimActivateValue = new InputFeatureUsage<float>("PointerActivateValue");

            internal static readonly InputFeatureUsage<bool> isGraspValueReady = new InputFeatureUsage<bool>("GraspReady");
            internal static readonly InputFeatureUsage<float> graspValue = new InputFeatureUsage<float>("GraspValue");

            internal static readonly InputFeatureUsage<bool> isGripPoseTracked = new InputFeatureUsage<bool>("DeviceIsTracked");
            internal static readonly InputFeatureUsage<Vector3> gripPosition = new InputFeatureUsage<Vector3>("DevicePosition");
            internal static readonly InputFeatureUsage<Quaternion> gripRotation = new InputFeatureUsage<Quaternion>("DeviceRotation");

            internal static readonly InputFeatureUsage<bool> isPinchPoseTracked = new InputFeatureUsage<bool>("PinchIsTracked");
            internal static readonly InputFeatureUsage<Vector3> pinchPosition = new InputFeatureUsage<Vector3>("PinchPosition");
            internal static readonly InputFeatureUsage<Quaternion> pinchRotation = new InputFeatureUsage<Quaternion>("PinchRotation");

            internal static readonly InputFeatureUsage<bool> isPinchValueReady = new InputFeatureUsage<bool>("PinchReady");
            internal static readonly InputFeatureUsage<float> pinchValue = new InputFeatureUsage<float>("PinchValue");

            internal static readonly InputFeatureUsage<bool> isPokePoseTracked = new InputFeatureUsage<bool>("PokeIsTracked");
            internal static readonly InputFeatureUsage<Vector3> pokePosition = new InputFeatureUsage<Vector3>("PokePosition");
            internal static readonly InputFeatureUsage<Quaternion> pokeRotation = new InputFeatureUsage<Quaternion>("PokeRotation");
        }

        InputDevice m_LeftHandInteractionDevice;
        InputDevice m_RightHandInteractionDevice;
        static readonly List<InputDevice> s_DevicesReuse = new List<InputDevice>();
        bool TryGetHandDevice(Handedness handedness, out InputDevice device)
        {
            if (handedness == Handedness.Left && m_LeftHandInteractionDevice.isValid)
            {
                device = m_LeftHandInteractionDevice;
                return true;
            }
            else if (handedness == Handedness.Right && m_RightHandInteractionDevice.isValid)
            {
                device = m_RightHandInteractionDevice;
                return true;
            }

            InputDevices.GetDevicesWithCharacteristics(
                handedness == Handedness.Left
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right,
                s_DevicesReuse);

            for (int deviceIndex = 0; deviceIndex < s_DevicesReuse.Count; ++deviceIndex)
            {
                if (s_DevicesReuse[deviceIndex].name != k_HandInteractionDeviceName)
                    continue;

                device = s_DevicesReuse[deviceIndex];

                if (handedness == Handedness.Left)
                    m_LeftHandInteractionDevice = device;
                else
                    m_RightHandInteractionDevice = device;

                return true;
            }

            device = default;
            return false;
        }
#endif

        bool m_IsValid;

        static internal string id { get; private set; }

        static OpenXRHandProvider() => id = "OpenXR Hands";

        const string k_HandInteractionDeviceName = "Hand Interaction OpenXR";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Register()
        {
            var settings = OpenXRSettings.Instance;
            if (settings == null)
                return;

            var feature = settings.GetFeature<HandTracking>();
            if (feature != null && feature.enabled)
            {
#if UNITY_OPENXR_PACKAGE_1_8
                var profile = OpenXRSettings.Instance.GetFeature<HandInteractionProfile>();
                bool commonPosesEnabled = profile != null && profile.enabled;
#else
                bool commonPosesEnabled = false;
#endif

                var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
                {
                    id = id,
                    providerType = typeof(OpenXRHandProvider),
                    supportsAimPose = commonPosesEnabled,
                    supportsAimActivateValue = commonPosesEnabled,
                    supportsGraspValue = commonPosesEnabled,
                    supportsGripPose = commonPosesEnabled,
                    supportsPinchPose = commonPosesEnabled,
                    supportsPinchValue = commonPosesEnabled,
                    supportsPokePose = commonPosesEnabled,
                };
                XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
            }
        }

        static class NativeApi
        {
            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_TryInitialize")]
            internal static extern bool TryInitialize();

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_Destroy")]
            internal static extern void Destroy();

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_TryUpdateHands")]
            internal static unsafe extern XRHandSubsystem.UpdateSuccessFlags TryUpdateHands(
                XRHandSubsystem.UpdateType updateType,
                ref Pose leftRootPose,
                void* leftHandJoints,
                ref Pose rightRootPose,
                void* rightHandJoints);

            [DllImport(HandTracking.k_LibraryName, EntryPoint = "UnityOpenXRHands_GetDetectedHandMeshLayout")]
            internal static extern XRDetectedHandMeshLayout GetDetectedHandMeshLayout();
        }
    }
}

#endif // UNITY_OPENXR_PACKAGE || PACKAGE_DOCS_GENERATION
