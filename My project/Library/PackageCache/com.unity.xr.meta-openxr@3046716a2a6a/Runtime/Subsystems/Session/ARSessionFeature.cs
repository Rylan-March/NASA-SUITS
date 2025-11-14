using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation session support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation support on Meta Quest devices. Required as a dependency of any other AR feature.",
        DocumentationLink = Constants.DocsUrls.k_SessionUrl,
        OpenxrExtensionStrings = Constants.OpenXRExtensions.k_XR_FB_scene_capture,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARSessionFeature : MetaOpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-session";

        internal const string displayName = "Meta Quest: Session";

        static List<XRSessionSubsystemDescriptor> s_SessionDescriptors = new();

        /// <summary>
        /// Called when the OpenXR loader receives the `XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED` event
        /// from the runtime signaling that the XrSessionState has changed.
        /// </summary>
        /// <param name="oldState">Previous state</param>
        /// <param name="newState">New state</param>
        protected override void OnSessionStateChange(int oldState, int newState)
        {
            MetaOpenXRSessionSubsystem.instance?.OnSessionStateChange(oldState, newState);
        }

        /// <summary>
        /// Creates the <see cref="MetaOpenXRSessionSubsystem"/>, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(
                s_SessionDescriptors,
                MetaOpenXRSessionSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the session subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRSessionSubsystem>();
        }
    }
}
