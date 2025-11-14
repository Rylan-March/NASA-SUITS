using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR Foundation passthrough support via OpenXR for Android XR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: AR Camera",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation camera support on Android XR devices.",
        DocumentationLink = Constants.DocsUrls.k_CameraUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARCameraFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature an id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-camera";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_passthrough_camera_state + " " +
            Constants.OpenXRExtensions.k_XR_ANDROID_light_estimation;

        static List<XRCameraSubsystemDescriptor> s_CameraDescriptors = new();

        static bool m_PassthroughEnabled = false;

        /// <summary>
        /// Instantiates Android OpenXR Session subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(
                s_CameraDescriptors,
                AndroidOpenXRCameraSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the session subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRCameraSubsystem>();
        }

        /// <summary>
        /// Realigns environment blend mode with current passthrough state if they are misaligned after the mode was changed.
        /// </summary>
        /// <param name="xrEnvironmentBlendMode">New environment blend mode value</param>
        protected override void OnEnvironmentBlendModeChange(XrEnvironmentBlendMode xrEnvironmentBlendMode)
        {
            if (m_PassthroughEnabled && xrEnvironmentBlendMode != XrEnvironmentBlendMode.AlphaBlend)
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);

            else if (!m_PassthroughEnabled && xrEnvironmentBlendMode != XrEnvironmentBlendMode.Opaque)
                SetEnvironmentBlendMode(XrEnvironmentBlendMode.Opaque);
        }

        /// <summary>
        /// Controls passthrough activity by setting the environment blend mode.
        /// </summary>
        internal static void SetPassthrough(bool active)
        {
            m_PassthroughEnabled = active;
            SetEnvironmentBlendMode(active ? XrEnvironmentBlendMode.AlphaBlend : XrEnvironmentBlendMode.Opaque);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARCameraFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            var AdditionalRules = new ValidationRule[]
            {
                new ValidationRule(this)
                {
                    message = "Passthrough requires Camera clear flags set to solid color with alpha value zero.",
                    checkPredicate = () =>
                    {
                        var xrOrigin = FindAnyObjectByType<XROrigin>();
                        if (xrOrigin == null || !xrOrigin.enabled) return true;

                        var camera = xrOrigin.Camera;
                        if (camera == null || camera.GetComponent<ARCameraManager>() == null) return true;

                        return camera.clearFlags == CameraClearFlags.SolidColor && Mathf.Approximately(camera.backgroundColor.a, 0);
                    },
                    fixItAutomatic = true,
                    fixItMessage = "Set your XR Origin camera's Clear Flags to solid color with alpha value zero.",
                    fixIt = () =>
                    {
                        var xrOrigin = FindAnyObjectByType<XROrigin>();
                        if (xrOrigin != null || xrOrigin.enabled)
                        {
                            var camera = xrOrigin.Camera;
                            if (camera != null || camera.GetComponent<ARCameraManager>() != null)
                            {
                                camera.clearFlags = CameraClearFlags.SolidColor;
                                Color clearColor = camera.backgroundColor;
                                clearColor.a = 0;
                                camera.backgroundColor = clearColor;
                            }
                        }
                    },
                    error = false
                },
                new ValidationRule(this)
                {
                    message = "AR Camera Manager component should be enabled for Passthrough to function correctly.",
                    checkPredicate = () =>
                    {
                        var cameraManager = FindAnyObjectByType<ARCameraManager>();
                        return cameraManager != null && cameraManager.enabled;
                    },
                    fixItAutomatic = true,
                    fixItMessage = "Find the object with ARCameraManager component and enable it.",
                    fixIt = () =>
                    {
                        var cameraManager = FindAnyObjectByType<ARCameraManager>();
                        if (cameraManager != null)
                            cameraManager.enabled = true;
                    },
                    error = false
                }
            };

            rules.AddRange(AdditionalRules);
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
