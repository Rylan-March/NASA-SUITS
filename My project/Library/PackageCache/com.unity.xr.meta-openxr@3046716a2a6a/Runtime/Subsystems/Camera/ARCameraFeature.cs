using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
#if MODULE_URP_ENABLED
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation passthrough support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation camera support on Meta Quest devices",
        DocumentationLink = Constants.DocsUrls.k_CameraUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARCameraFeature : MetaOpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-camera";

        internal const string displayName = "Meta Quest: Camera (Passthrough)";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        const string k_OpenXRRequestedExtensions = k_XR_FB_passthrough;

        static List<XRCameraSubsystemDescriptor> s_CameraDescriptors = new();

        /// <summary>
        /// Called after `xrCreateInstance`. Override this method to validate that any necessary OpenXR extensions were
        /// successfully enabled
        /// (<a href="xref:UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled(System.String)">OpenXRRuntime.IsExtensionEnabled</a>)
        /// and that any required system properties are supported. If this method returns <see langword="false"/>,
        /// the feature's [enabled](xref:UnityEngine.XR.OpenXR.Features.OpenXRFeature.enabled) property is set to <see langword="false"/>.
        /// </summary>
        /// <param name="xrInstance">Handle of the native `xrInstance`.</param>
        /// <returns><see langword="true"/> if this feature successfully initialized. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// If this feature is a required feature of an enabled feature set, returning <see langword="false"/> here
        /// causes the `OpenXRLoader` to fail, and XR Plug-in Management will fall back to another loader if enabled.
        /// </remarks>
        /// <seealso href="xref:openxr-features#enabling-openxr-spec-extension-strings">Enabling OpenXR spec extension strings</seealso>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            return
                OpenXRUtils.IsExtensionEnabled(k_XR_FB_passthrough) &&
                IsCapabilitySupported(SystemCapability.Passthrough, xrInstance, displayName, typeof(XRCameraSubsystem));
        }

        /// <summary>
        /// Instantiates OpenXR Meta Camera subsystem instances, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(
                s_CameraDescriptors,
                MetaOpenXRCameraSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the camera subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRCameraSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARCameraFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            var additionalRules = new[]
            {
#if MODULE_URP_ENABLED
                new ValidationRule(this)
                {
                    message = "Vulkan supports the most setting configurations to enable Passthrough on Meta Quest when using URP.",
                    checkPredicate = () =>
                    {
                        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                        {
                            var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                            return graphicsApis.Length > 0 && graphicsApis[0] == GraphicsDeviceType.Vulkan;
                        }
                        return true;
                    },
                    fixItAutomatic = true,
                    fixItMessage = "Go to <b>Project Settings</b> > <b>Player</b> > Android tab." +
                        " Under <b>Graphics APIs</b>, add <b>Vulkan</b> as the topmost API in the list.",
                    fixIt = () =>
                    {
                        var currentGraphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        int apiLength = currentGraphicsApis.Length;
                        apiLength += Array.Exists(currentGraphicsApis, element => element == GraphicsDeviceType.Vulkan) ? 0 : 1;

                        // Copy the user's graphics APIs into a new array where Vulkan is the first element
                        GraphicsDeviceType[] correctGraphicsApis = new GraphicsDeviceType[apiLength];
                        correctGraphicsApis[0] = GraphicsDeviceType.Vulkan;
                        var dstIndex = 1;
                        for (var srcIndex = 0; srcIndex < currentGraphicsApis.Length; ++srcIndex)
                        {
                            if (currentGraphicsApis[srcIndex] != GraphicsDeviceType.Vulkan)
                            {
                                correctGraphicsApis[dstIndex] = currentGraphicsApis[srcIndex];
                                ++dstIndex;
                            }
                        }
                        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, correctGraphicsApis);
                    },
                    error = false,
                },
#endif
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
                }
            };

            rules.AddRange(additionalRules);
            rules.Add(ValidationRuleFactory.CreateARSessionValidationRule(this, targetGroup));
        }
#endif
    }
}
