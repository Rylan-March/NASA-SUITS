using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR Foundation face support via OpenXR for Android XR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: AR Face",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation face detection support on Android XR devices",
        DocumentationLink = Constants.DocsUrls.k_FacesUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARFaceFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature an id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-face";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_faces;

        static readonly List<XRFaceSubsystemDescriptor> s_FaceDescriptors = new();

        /// <summary>
        /// Instantiates Android OpenXR Face subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRFaceSubsystemDescriptor, XRFaceSubsystem>(
                s_FaceDescriptors,
                AndroidOpenXRFaceSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the face subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRFaceSubsystem>();
        }

        /// <summary>
        /// Called when the XR session state changes
        /// </summary>
        /// <param name="oldState">Previous state</param>
        /// <param name="newState">New state</param>
        protected override void OnSessionStateChange(int oldState, int newState)
        {
            NativeApi.OnSessionStateChange(oldState, newState);
        }

        /// <summary>
        /// Called after xrSessionBegin.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionBegin(ulong xrSession)
        {
            NativeApi.OnSessionBegin(xrSession);
        }

        /// <summary>
        /// Called before xrEndSession.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionEnd(ulong xrSession)
        {
            NativeApi.OnSessionEnd(xrSession);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_OnSessionBegin")]
            public static extern void OnSessionBegin(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_OnSessionEnd")]
            public static extern void OnSessionEnd(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_OnSessionStateChange")]
            public static extern void OnSessionStateChange(int oldState, int newState);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARFaceFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
