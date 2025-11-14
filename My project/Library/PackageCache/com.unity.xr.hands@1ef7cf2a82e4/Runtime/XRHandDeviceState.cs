using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// State for input device representing XR hand gestures.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 179)]
    struct XRHandDeviceState : IInputStateTypeInfo
    {
        /// <summary>
        /// Memory format identifier for <see cref="XRHandDeviceState"/>.
        /// </summary>
        /// <seealso cref="InputStateBlock.format"/>
        public static FourCC formatId => new FourCC('X', 'R', 'H', 'D');

        /// <summary>
        /// Data format identifier of the state.
        /// </summary>
        public FourCC format => formatId;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the grip position and rotation.
        /// </summary>
        [InputControl(usage = "GripTrackingState", layout = "Integer", offset = 0)]
        [FieldOffset(0)]
        public int gripTrackingState;

        /// <summary>
        /// Position of the grip pose.
        /// </summary>
        [InputControl(usage = "GripPosition", offset = 4)]
        [FieldOffset(4)]
        public Vector3 gripPosition;

        /// <summary>
        /// Rotation of the grip pose.
        /// </summary>
        [InputControl(usage = "GripRotation", offset = 16)]
        [FieldOffset(16)]
        public Quaternion gripRotation;

        /// <summary>
        /// Value corresponding to the grip pose.
        /// </summary>
        [InputControl(usage = "GraspValue", layout = "Axis", offset = 32)]
        [FieldOffset(32)]
        public float graspValue;

        /// <summary>
        /// Informs whether or not the user is making a fist.
        /// </summary>
        [InputControl(usage = "GraspFirm", layout = "Button", offset = 36)]
        [FieldOffset(36)]
        public bool graspFirm;

        /// <summary>
        /// Informs whether the grasp values are ready.
        /// </summary>
        [InputControl(usage = "GraspReady", layout = "Button", offset = 37)]
        [FieldOffset(37)]
        public bool graspReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the poke position and rotation.
        /// </summary>
        [InputControl(usage = "pokeTrackingState", layout = "Integer", offset = 38)]
        [FieldOffset(38)]
        public int pokeTrackingState;

        /// <summary>
        /// Position of the poke pose.
        /// </summary>
        [InputControl(usage = "PokePosition", offset = 42)]
        [FieldOffset(42)]
        public Vector3 pokePosition;

        /// <summary>
        /// Rotation of the poke pose.
        /// </summary>
        [InputControl(usage = "PokeRotation", offset = 54)]
        [FieldOffset(54)]
        public Quaternion pokeRotation;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the pinch position and rotation.
        /// </summary>
        [InputControl(usage = "pinchTrackingState", layout = "Integer", offset = 70)]
        [FieldOffset(70)]
        public int pinchTrackingState;

        /// <summary>
        /// Position of the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchPosition", offset = 74)]
        [FieldOffset(74)]
        public Vector3 pinchPosition;

        /// <summary>
        /// Rotation of the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchRotation", offset = 86)]
        [FieldOffset(86)]
        public Quaternion pinchRotation;

        /// <summary>
        /// Value corresponding to the pinch pose.
        /// </summary>
        [InputControl(usage = "PinchValue", layout = "Axis", offset = 102)]
        [FieldOffset(102)]
        public float pinchValue;

        /// <summary>
        /// Informs whether or not the user actively pinching.
        /// </summary>
        [InputControl(usage = "PinchTouched", layout = "Button", offset = 106)]
        [FieldOffset(106)]
        public bool pinchTouched;

        /// <summary>
        /// Informs whether the pinch pose is currently ready.
        /// </summary>
        [InputControl(usage = "PinchReady", layout = "Button", offset = 107)]
        [FieldOffset(107)]
        public bool pinchReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the aim position and rotation.
        /// </summary>
        [InputControl(usage = "aimTrackingState", layout = "Integer", offset = 108)]
        [FieldOffset(108)]
        public int aimTrackingState;

        /// <summary>
        /// Position of the aim pose.
        /// </summary>
        [InputControl(usage = "AimPosition", alias = "pointerPosition", offset = 112)]
        [FieldOffset(112)]
        public Vector3 aimPosition;

        /// <summary>
        /// Rotation of the aim pose.
        /// </summary>
        [InputControl(usage = "AimRotation", alias = "pointerRotation", offset = 124)]
        [FieldOffset(124)]
        public Quaternion aimRotation;

        /// <summary>
        /// Value corresponding to activiation by the aim pose.
        /// </summary>
        [InputControl(usage = "PointerActivateValue", layout = "Axis", alias = "pointerActivateValue", offset = 140)]
        [FieldOffset(140)]
        public float aimActivateValue;

        /// <summary>
        /// Informs whether or not aim is activated.
        /// </summary>
        [InputControl(usage = "PointerActivated", alias = "pointerActivated", layout = "Button", offset = 144)]
        [FieldOffset(144)]
        public bool aimActivated;

        /// <summary>
        /// Informs whether the aim activation values are ready.
        /// </summary>
        [InputControl(usage = "PointerActivateReady", layout = "Button", offset = 145)]
        [FieldOffset(145)]
        public bool aimActivateReady;

        /// <summary>
        /// <see cref="InputTrackingState"/> for the device position and rotation.
        /// </summary>
        [InputControl(usage = "TrackingState", layout = "Integer", offset = 146)]
        [FieldOffset(146)]
        public int trackingState;

        /// <summary>
        /// Informs to the developer whether the device is currently being tracked.
        /// </summary>
        [InputControl(usage = "IsTracked", layout = "Button", offset = 150)]
        [FieldOffset(150)]
        public bool isTracked;

        /// <summary>
        /// Position of the device.
        /// </summary>
        [InputControl(usage = "DevicePosition", offset = 151)]
        [FieldOffset(151)]
        public Vector3 devicePosition;

        /// <summary>
        /// Rotation of this device.
        /// </summary>
        [InputControl(usage = "DeviceRotation", offset = 163)]
        [FieldOffset(163)]
        public Quaternion deviceRotation;
    }
}
