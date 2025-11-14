#if ENABLE_VR || UNITY_GAMECORE || PACKAGE_DOCS_GENERATION

using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.Hands.Configuration;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// An <see cref="InputDevice"/> that surfaces common controller data
    /// populated by hand joint poses. Devices will only be created if
    /// hand-tracking is enabled in the build settings for the target platform.
    /// </summary>
    /// <remarks>
    /// The <see cref="TrackedDevice.devicePosition"/> and
    /// <see cref="TrackedDevice.deviceRotation"/> inherited from <see cref="TrackedDevice"/>
    /// represent the wrist pose.
    ///
    /// Use the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) in the scene to position and orient the device
    /// and gesture poses properly. If you are using this data to set the Transform of a GameObject in
    /// the scene hierarchy, you can set the local position and rotation of the Transform and make
    /// it a child of the <c>CameraOffset</c> object below the <c>XROrigin</c>. Otherwise, you can use the
    /// Transform of the <c>CameraOffset</c> to transform the data into world space.
    /// </remarks>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve, InputControlLayout(stateType = typeof(XRHandDeviceState), displayName = "XR Hand Device", commonUsages = new[] { "LeftHand", "RightHand" })]
    public class XRHandDevice : TrackedDevice
    {
        /// <summary>
        /// Left-hand <see cref="InputDevice"/> that contains <see cref="InputControl"/>s that surface
        /// common hand data populated by hand joint poses.
        /// </summary>
        public static XRHandDevice leftHand { get; internal set; }

        /// <summary>
        /// Right-hand <see cref="InputDevice"/> that contains <see cref="InputControl"/>s that surface
        /// common hand data populated by hand joint poses.
        /// </summary>
        public static XRHandDevice rightHand { get; internal set; }

        /// <summary>
        /// Tracking status of the grip position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl gripTrackingState { get; private set; }

        /// <summary>
        /// Position of the grip pose, representing the palm.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control gripPosition { get; private set; }

        /// <summary>
        /// Rotation of the grip pose, representing the palm.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl gripRotation { get; private set; }

        /// <summary>
        /// [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that indicates the extent to which a user is making a fist.
        /// </summary>
        public AxisControl graspValue { get; private set; }

        /// <summary>
        /// Whether the hand performing the grasp action is properly tracked by the hand tracking device
        /// and it is observed to be ready to perform or is performing the grasp action.
        /// </summary>
        public ButtonControl graspReady { get; private set; }

        /// <summary>
        /// Tracking status of the poke position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl pokeTrackingState { get; private set; }

        /// <summary>
        /// Position of the poke pose, representing the index finger's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control pokePosition { get; private set; }

        /// <summary>
        /// Rotation of the poke pose, representing the index finger's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl pokeRotation { get; private set; }

        /// <summary>
        /// Tracking status of the pinch position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl pinchTrackingState { get; private set; }

        /// <summary>
        /// Position of the pinch pose, representing the thumb's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control pinchPosition { get; private set; }

        /// <summary>
        /// Rotation of the pinch pose, representing the thumb's tip.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl pinchRotation { get; private set; }

        /// <summary>
        /// [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the extent
        /// to which the user is bringing their finger and thumb together to perform a "pinch" gesture.
        /// </summary>
        public AxisControl pinchValue { get; private set; }

        /// <summary>
        /// Whether the fingers used to perform the "pinch" gesture are properly tracked by the hand tracking device
        /// and the hand shape is observed to be ready to perform or is performing a "pinch" gesture.
        /// </summary>
        public ButtonControl pinchReady { get; private set; }

        /// <summary>
        /// Tracking status of the aim position and rotation. See <see cref="InputTrackingState"/> for more.
        /// </summary>
        public IntegerControl aimTrackingState { get; private set; }

        /// <summary>
        /// Position of the aim pose, representing an aiming ray cast to a target.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the position will be in the correct position in the scene relative to the user.
        /// </summary>
        public Vector3Control aimPosition { get; private set; }

        /// <summary>
        /// Rotation of the aim pose, representing an aiming ray cast to a target.
        /// When transformed relative to the [XROrigin](xref:Unity.XR.CoreUtils.XROrigin),
        /// the rotation will be in the correct orientation in the scene relative to the user.
        /// </summary>
        public QuaternionControl aimRotation { get; private set; }

        /// <summary>
        /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that indicates the extent to which a
        /// user activated the aim action on the target that the user is pointing at with the aim pose.
        /// </summary>
        public AxisControl aimActivateValue { get; private set; }

        /// Whether the fingers to perform the aim activate gesture are properly tracked by the hand tracking device
        /// and the hand shape is observed to be ready to perform or is performing an aim activate gesture.
        /// </summary>
        public ButtonControl aimActivateReady { get; private set; }

        Action<XRHandSubsystem, XRHandSubsystem.UpdateSuccessFlags, XRHandSubsystem.UpdateType> m_UpdateBehavior;

        XRHandDeviceState m_DeviceState;

        /// <summary>
        /// Perform final initialization tasks after the control hierarchy has been put into place.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            gripPosition = GetChildControl<Vector3Control>("gripPosition");
            gripRotation = GetChildControl<QuaternionControl>("gripRotation");
            gripTrackingState = GetChildControl<IntegerControl>("gripTrackingState");
            graspValue = GetChildControl<AxisControl>("graspValue");
            graspReady = GetChildControl<ButtonControl>("graspReady");
            pokePosition = GetChildControl<Vector3Control>("pokePosition");
            pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
            pokeTrackingState = GetChildControl<IntegerControl>("pokeTrackingState");
            pinchTrackingState = GetChildControl<IntegerControl>("pinchTrackingState");
            pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
            pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");
            pinchValue = GetChildControl<AxisControl>("pinchValue");
            pinchReady = GetChildControl<ButtonControl>("pinchReady");
            aimTrackingState = GetChildControl<IntegerControl>("aimTrackingState");
            aimPosition = GetChildControl<Vector3Control>("aimPosition");
            aimRotation = GetChildControl<QuaternionControl>("aimRotation");
            aimActivateValue = GetChildControl<AxisControl>("aimActivateValue");
            aimActivateReady = GetChildControl<ButtonControl>("aimActivateReady");

            m_DeviceState.gripRotation = Quaternion.identity;
            m_DeviceState.pokeRotation = Quaternion.identity;
            m_DeviceState.pinchRotation = Quaternion.identity;
            m_DeviceState.aimRotation = Quaternion.identity;

            var deviceDescriptor = XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.LeftHand);
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                    InputSystem.InputSystem.SetDeviceUsage(this, InputSystem.CommonUsages.RightHand);
            }
        }

        internal static XRHandDevice Create(
            XRHandSubsystem subsystem,
            Handedness handedness,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            var extraCharacteristics = handedness == Handedness.Left ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;

            var desc = new InputDeviceDescription
            {
                product = k_DeviceProductName,
                capabilities = new XRDeviceDescriptor
                {
                    characteristics = InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | extraCharacteristics,
                    inputFeatures = new List<XRFeatureDescriptor>
                    {
                        new XRFeatureDescriptor
                        {
                            name = "grip_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grip_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grip_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grasp_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "grasp_ready",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "poke_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "poke_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "poke_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "pinch_ready",
                            featureType = FeatureType.Binary
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_position",
                            featureType = FeatureType.Axis3D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_rotation",
                            featureType = FeatureType.Rotation
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_tracking_state",
                            featureType = FeatureType.DiscreteStates
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_activate_value",
                            featureType = FeatureType.Axis1D
                        },
                        new XRFeatureDescriptor
                        {
                            name = "aim_activate_ready",
                            featureType = FeatureType.Binary
                        },
                    }
                }.ToJson()
            };
            var handDevice = InputSystem.InputSystem.AddDevice(desc) as XRHandDevice;
            if (handDevice != null)
            {
                subsystem.updatedHands += handDevice.OnUpdatedHands;
                subsystem.configurationUpdated += handDevice.OnXRHandSubsystemConfigUpdated;

                handDevice.m_Handedness = handedness;
                handDevice.m_UpdateBehavior = handDevice.OnUpdatedHandsLegacy;

                handDevice.OnDevicePoseSourceUpdated(subsystem.handSubsystemConfiguration.xrHandDevicePoseSource);
                handDevice.OnUpdatedHands(subsystem, updateSuccessFlags, updateType);
            }

            return handDevice;
        }

        void OnXRHandSubsystemConfigUpdated(XRHandSubsystemConfigurationUpdatedEventArgs args)
        {
            OnDevicePoseSourceUpdated(args.newConfiguration.xrHandDevicePoseSource);
        }

        void OnDevicePoseSourceUpdated(XRHandDevicePoseSource newPoseSource)
        {
            if (newPoseSource == XRHandDevicePoseSource.CommonGestures)
                m_UpdateBehavior = OnUpdatedHandsCommonGesture;
            else
                m_UpdateBehavior = OnUpdatedHandsLegacy;
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            m_UpdateBehavior(subsystem, updateSuccessFlags, updateType);
        }

        void OnUpdatedHandsLegacy(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            XRHand hand;
            bool isValid;
            if (m_Handedness == Handedness.Left)
            {
                hand = subsystem.leftHand;
                var success = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }
            else
            {
                hand = subsystem.rightHand;
                var success = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }

            if (!m_WasValid && !isValid)
                return;

            if (m_WasValid && !isValid)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, false);
                InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.None);
                InputSystem.InputSystem.QueueDeltaStateEvent(gripTrackingState, InputTrackingState.None);
                InputSystem.InputSystem.QueueDeltaStateEvent(pokeTrackingState, InputTrackingState.None);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchTrackingState, InputTrackingState.None);
                m_WasValid = false;
                return;
            }

            if (!m_WasValid && isValid)
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(isTracked, true);
                InputSystem.InputSystem.QueueDeltaStateEvent(trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                InputSystem.InputSystem.QueueDeltaStateEvent(gripTrackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                InputSystem.InputSystem.QueueDeltaStateEvent(pokeTrackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchTrackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                m_WasValid = true;
            }

            if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(devicePosition, wristPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(deviceRotation, wristPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(gripPosition, palmPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(gripRotation, palmPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var indexTipPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(pokePosition, indexTipPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(pokeRotation, indexTipPose.rotation);
            }

            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumbTipPose))
            {
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchPosition, thumbTipPose.position);
                InputSystem.InputSystem.QueueDeltaStateEvent(pinchRotation, thumbTipPose.rotation);
            }
        }

        void OnUpdatedHandsCommonGesture(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            XRCommonHandGestures commonGestures;
            bool isValid;
            if (m_Handedness == Handedness.Left)
            {
                commonGestures = subsystem.leftHandCommonGestures;
                var success = XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose | XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }
            else
            {
                commonGestures = subsystem.rightHandCommonGestures;
                var success = XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose | XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }

            if (!m_WasValid && !isValid)
                return;

            if (m_WasValid && !isValid)
            {
                m_DeviceState.isTracked = false;
                m_DeviceState.trackingState = (int) InputTrackingState.None;
                m_DeviceState.aimTrackingState = (int) InputTrackingState.None;
                m_DeviceState.gripTrackingState = (int) InputTrackingState.None;
                m_DeviceState.pokeTrackingState = (int) InputTrackingState.None;
                m_DeviceState.pinchTrackingState = (int) InputTrackingState.None;
                m_WasValid = false;
                return;
            }

            const int poseFullyTracked = (int)(InputTrackingState.Position | InputTrackingState.Rotation);

            if (!m_WasValid && isValid)
            {
                m_DeviceState.isTracked = true;
                m_DeviceState.trackingState = poseFullyTracked;
                m_WasValid = true;
            }

            if (commonGestures.TryGetGripPose(out var gripPose))
            {
                m_DeviceState.devicePosition = gripPose.position;
                m_DeviceState.deviceRotation = gripPose.rotation;

                m_DeviceState.gripPosition = gripPose.position;
                m_DeviceState.gripRotation = gripPose.rotation;
                m_DeviceState.gripTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.gripTrackingState = (int)InputTrackingState.None;
            }

            if (commonGestures.TryGetGraspValue(out var currentGraspValue))
            {
                m_DeviceState.graspValue = currentGraspValue;
                m_DeviceState.graspReady = true;
            }
            else
            {
                m_DeviceState.graspValue = 0f;
                m_DeviceState.graspReady = false;
            }

            if (commonGestures.TryGetPokePose(out var pokePose))
            {
                m_DeviceState.pokePosition = pokePose.position;
                m_DeviceState.pokeRotation = pokePose.rotation;
                m_DeviceState.pokeTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.pokeTrackingState = (int)InputTrackingState.None;
            }

            if (commonGestures.TryGetPinchPose(out var pinchPose))
            {
                m_DeviceState.pinchPosition = pinchPose.position;
                m_DeviceState.pinchRotation = pinchPose.rotation;
                m_DeviceState.pinchTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.pinchTrackingState = (int)InputTrackingState.None;
            }

            if (commonGestures.TryGetPinchValue(out var currentPinchValue))
            {
                m_DeviceState.pinchValue = currentPinchValue;
                m_DeviceState.pinchReady = true;
            }
            else
            {
                m_DeviceState.pinchValue = 0f;
                m_DeviceState.pinchReady = false;
            }

            if (commonGestures.TryGetAimPose(out var aimPose))
            {
                m_DeviceState.aimPosition = aimPose.position;
                m_DeviceState.aimRotation = aimPose.rotation;
                m_DeviceState.aimTrackingState = poseFullyTracked;
            }
            else
            {
                m_DeviceState.aimTrackingState = (int)InputTrackingState.None;
            }

            if (commonGestures.TryGetAimActivateValue(out var currentAimValue))
            {
                m_DeviceState.aimActivateValue = currentAimValue;
                m_DeviceState.aimActivateReady = true;
            }
            else
            {
                m_DeviceState.aimActivateValue = 0f;
                m_DeviceState.aimActivateReady = false;
            }

            InputSystem.InputSystem.QueueStateEvent(this, m_DeviceState);
        }

        static XRHandDevice() => Initialize();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.InputSystem.RegisterLayout<XRHandDevice>(
                matches: new InputDeviceMatcher()
                .WithProduct(k_DeviceProductName));
#endif // ENABLE_INPUT_SYSTEM
        }

        const string k_DeviceProductName = "XRHandDevice";

        Handedness m_Handedness;
        bool m_WasValid;
    }
}

#endif // ENABLE_VR || UNITY_GAMECORE || PACKAGE_DOCS_GENERATION
