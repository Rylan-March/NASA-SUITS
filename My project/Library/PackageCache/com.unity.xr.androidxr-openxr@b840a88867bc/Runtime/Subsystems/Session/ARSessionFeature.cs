using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if XR_HANDS_1_5_0_OR_NEWER
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
#endif // XR_HANDS_1_5_0_OR_NEWER

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR Foundation session support via OpenXR for Android XR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: AR Session",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation support on Android XR devices. Required as a dependency of any other AR feature.",
        DocumentationLink = Constants.DocsUrls.k_SessionUrl,
        OpenxrExtensionStrings = "",
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARSessionFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-session";

        static readonly List<XRSessionSubsystemDescriptor> s_SessionDescriptors = new();

        /// <summary>
        /// Called after xrCreateSession.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            AndroidOpenXRSessionSubsystem.instance?.RefreshSessionId();

#if XR_HANDS_1_5_0_OR_NEWER
            XRFingerShapeMath.SetFingerShapeConfiguration(XRHandFingerID.Thumb, FingerShapeConfigs.k_Thumb);
            XRFingerShapeMath.SetFingerShapeConfiguration(XRHandFingerID.Index, FingerShapeConfigs.k_Index);
            XRFingerShapeMath.SetFingerShapeConfiguration(XRHandFingerID.Middle, FingerShapeConfigs.k_Middle);
            XRFingerShapeMath.SetFingerShapeConfiguration(XRHandFingerID.Ring, FingerShapeConfigs.k_Ring);
            XRFingerShapeMath.SetFingerShapeConfiguration(XRHandFingerID.Little, FingerShapeConfigs.k_Little);
#endif // XR_HANDS_1_5_0_OR_NEWER
        }

        /// <summary>
        /// Called when the OpenXR loader receives the `XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED` event
        /// from the runtime signaling that the XrSessionState has changed.
        /// </summary>
        /// <param name="oldState">Previous state</param>
        /// <param name="newState">New state</param>
        protected override void OnSessionStateChange(int oldState, int newState) => AndroidOpenXRSessionSubsystem.instance?.OnSessionStateChange(oldState, newState);

        /// <summary>
        /// Instantiates Android OpenXR Session subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(
                s_SessionDescriptors,
                AndroidOpenXRSessionSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the session subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRSessionSubsystem>();
        }

#if XR_HANDS_1_5_0_OR_NEWER
        static class FingerShapeConfigs
        {
            static internal readonly XRFingerShapeConfiguration k_Thumb = new XRFingerShapeConfiguration
            {
                minimumFullCurlDegrees1 = 120f,
                maximumFullCurlDegrees1 = 180f,
                minimumFullCurlDegrees2 = 120f,
                maximumFullCurlDegrees2 = 180f,
                minimumFullCurlDegrees3 = -1f,
                maximumFullCurlDegrees3 = -1f,

                minimumBaseCurlDegrees = 2f,
                maximumBaseCurlDegrees = 65f,

                minimumTipCurlDegrees1 = 120f,
                maximumTipCurlDegrees1 = 180f,
                minimumTipCurlDegrees2 = 120f,
                maximumTipCurlDegrees2 = 180f,

                minimumPinchDistance = -1f,
                maximumPinchDistance = -1f,

                minimumSpreadDegrees = 3f,
                maximumSpreadDegrees = 55f,
            };

            static internal readonly XRFingerShapeConfiguration k_Index = new XRFingerShapeConfiguration
            {
                minimumFullCurlDegrees1 = 90f,
                maximumFullCurlDegrees1 = 180f,
                minimumFullCurlDegrees2 = 90f,
                maximumFullCurlDegrees2 = 180f,
                minimumFullCurlDegrees3 = -1f,
                maximumFullCurlDegrees3 = -1f,

                minimumBaseCurlDegrees = 90f,
                maximumBaseCurlDegrees = 180f,

                minimumTipCurlDegrees1 = 90f,
                maximumTipCurlDegrees1 = 180f,
                minimumTipCurlDegrees2 = 120f,
                maximumTipCurlDegrees2 = 180f,

                minimumPinchDistance = 0.01f,
                maximumPinchDistance = 0.045f,

                minimumSpreadDegrees = 3f,
                maximumSpreadDegrees = 20f,
            };

            static internal readonly XRFingerShapeConfiguration k_Middle = new XRFingerShapeConfiguration
            {
                minimumFullCurlDegrees1 = 90f,
                maximumFullCurlDegrees1 = 180f,
                minimumFullCurlDegrees2 = 90f,
                maximumFullCurlDegrees2 = 180f,
                minimumFullCurlDegrees3 = -1f,
                maximumFullCurlDegrees3 = -1f,

                minimumBaseCurlDegrees = 90f,
                maximumBaseCurlDegrees = 180f,

                minimumTipCurlDegrees1 = 90f,
                maximumTipCurlDegrees1 = 180f,
                minimumTipCurlDegrees2 = 110f,
                maximumTipCurlDegrees2 = 180f,

                minimumPinchDistance = 0.01f,
                maximumPinchDistance = 0.045f,

                minimumSpreadDegrees = 1f,
                maximumSpreadDegrees = 10f,
            };

            static internal readonly XRFingerShapeConfiguration k_Ring = new XRFingerShapeConfiguration
            {
                minimumFullCurlDegrees1 = 90f,
                maximumFullCurlDegrees1 = 180f,
                minimumFullCurlDegrees2 = 90f,
                maximumFullCurlDegrees2 = 180f,
                minimumFullCurlDegrees3 = -1f,
                maximumFullCurlDegrees3 = -1f,

                minimumBaseCurlDegrees = 90f,
                maximumBaseCurlDegrees = 180f,

                minimumTipCurlDegrees1 = 90f,
                maximumTipCurlDegrees1 = 180f,
                minimumTipCurlDegrees2 = 110f,
                maximumTipCurlDegrees2 = 180f,

                minimumPinchDistance = 0.01f,
                maximumPinchDistance = 0.045f,

                minimumSpreadDegrees = 1f,
                maximumSpreadDegrees = 18f,
            };

            static internal readonly XRFingerShapeConfiguration k_Little = new XRFingerShapeConfiguration
            {
                minimumFullCurlDegrees1 = 90f,
                maximumFullCurlDegrees1 = 180f,
                minimumFullCurlDegrees2 = 90f,
                maximumFullCurlDegrees2 = 180f,
                minimumFullCurlDegrees3 = -1f,
                maximumFullCurlDegrees3 = -1f,

                minimumBaseCurlDegrees = 90f,
                maximumBaseCurlDegrees = 180f,

                minimumTipCurlDegrees1 = 90f,
                maximumTipCurlDegrees1 = 180f,
                minimumTipCurlDegrees2 = 110f,
                maximumTipCurlDegrees2 = 180f,

                minimumPinchDistance = 0.01f,
                maximumPinchDistance = 0.045f,

                minimumSpreadDegrees = -1f,
                maximumSpreadDegrees = -1f,
            };
        }
#endif // XR_HANDS_1_5_0_OR_NEWER
    }
}
