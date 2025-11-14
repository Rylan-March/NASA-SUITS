using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using System.Collections.Generic;
#endif
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Use this OpenXR feature to control boundary visibility on Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "Enables you to suppress the visibility of the boundary so that users can move freely when passthrough is enabled",
        DocumentationLink = Constants.DocsUrls.k_BoundaryVisibilityUrl,
        OpenxrExtensionStrings = k_XR_META_boundary_visibility,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class BoundaryVisibilityFeature : MetaOpenXRFeature
    {
        const string displayName = "Meta Quest: Boundary Visibility";

        /// <summary>
        /// Integer value of <see cref="XrResult"/> which indicates that the runtime did not accept the request to
        /// suppress the boundary visibility. This is expected when the app is not rendering passthrough.
        /// </summary>
        /// <value>Equal to `XR_BOUNDARY_VISIBILITY_SUPPRESSION_NOT_ALLOWED_META`.</value>
        /// <remarks>
        /// Cast your <see cref="XrResult"/> to `int` and compare with this value to determine if the runtime
        /// did not accept your request to suppress the boundary visibility.
        /// </remarks>
        public const int XR_BOUNDARY_VISIBILITY_SUPPRESSION_NOT_ALLOWED_META = 1000528000;

        static BoundaryVisibilityFeature s_Instance;

        [SerializeField, Tooltip("If enabled, this feature will attempt to suppress boundary visibility immediately after XR is initialized.")]
        bool m_SuppressVisibility;

        /// <summary>
        /// Invoked whenever the OpenXR Meta runtime's boundary visibility has changed.
        /// Visibility may change in response to <see cref="TryRequestBoundaryVisibility"/> or at
        /// the discretion of Meta's OpenXR runtime, such as if your app fully obscures the
        /// physical environment.
        /// </summary>
        /// <value>The new boundary visibility.</value>
        public event EventHandler<XrBoundaryVisibility> boundaryVisibilityChanged;

        /// <summary>
        /// Get the current boundary visibility.
        /// </summary>
        /// <value>The current visibility.</value>
        public XrBoundaryVisibility currentVisibility { get; private set; }

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        /// <value>The feature id.</value>
        public const string featureId = "com.unity.openxr.feature.meta-boundary-visibility";

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
            if (!OpenXRUtils.IsExtensionEnabled(k_XR_META_boundary_visibility, displayName) ||
                !IsCapabilitySupported(SystemCapability.BoundaryVisibility, xrInstance, displayName))
                return false;

            s_Instance = this;
            NativeApi.Create(s_BoundaryVisibilityCallback);

            if (m_SuppressVisibility)
                SuppressVisibilityFireAndForget();

            return true;
        }

        async void SuppressVisibilityFireAndForget()
        {
            try
            {
                XrResult result = XrResult.Success;

                // Allow up to ~12 seconds of attempts.
                // Suppression cannot succeed until splash screen is hidden and passthrough is visible.
                for (uint retryCount = 120; retryCount > 0; retryCount--)
                {
                    result = TryRequestBoundaryVisibility(XrBoundaryVisibility.VisibilitySuppressed);
                    if (result.IsUnqualifiedSuccess())
                        return;

                    await Awaitable.WaitForSecondsAsync(.1f);
                }

                Debug.LogError($"Boundary Visibility Feature timed out and was unable to suppress boundary visibility. Result code: {result}");
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        /// <summary>
        /// Attempts to request a change to the current boundary visibility. If the request returns
        /// `XR_SUCCESS`, Meta's OpenXR runtime is expected to subsequently change the boundary
        /// visibility and invoke the <see cref="boundaryVisibilityChanged"/> event.
        /// </summary>
        /// <param name="visibility">The desired boundary visibility.</param>
        /// <returns>The result that indicates whether the operation succeeded.</returns>
        /// <remarks>
        /// This method can return multiple possible success codes, most notably:
        /// * `XR_SUCCESS` if the request was successfully made.
        /// * `XR_BOUNDARY_VISIBILITY_SUPPRESSION_NOT_ALLOWED_META` if the runtime did not
        /// accept the request to suppress the boundary visibility.
        /// </remarks>
        public XrResult TryRequestBoundaryVisibility(XrBoundaryVisibility visibility)
        {
            return NativeApi.TryRequestBoundaryVisibility(visibility);
        }

        delegate void BoundaryVisibilityChangedDelegate(XrBoundaryVisibility visibility);

        static readonly IntPtr s_BoundaryVisibilityCallback =
                Marshal.GetFunctionPointerForDelegate((BoundaryVisibilityChangedDelegate)OnBoundaryVisibilityChanged);

        [MonoPInvokeCallback(typeof(BoundaryVisibilityChangedDelegate))]
        static void OnBoundaryVisibilityChanged(XrBoundaryVisibility visibility)
        {
            s_Instance.currentVisibility = visibility;
            s_Instance?.boundaryVisibilityChanged?.Invoke(s_Instance, visibility);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for Boundary Visibility feature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(new(this)
            {
                message = $"Your app can only suppress boundary visibility while rendering Passthrough. You should enable and use <b>{ARCameraFeature.displayName}</b> as a dependency of this feature.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    var cameraFeature = settings.GetFeature<ARCameraFeature>();
                    return !enabled || cameraFeature == null || (enabled && cameraFeature.enabled);
                },
                fixItAutomatic = true,
                fixItMessage = $"Go to <b>Project Settings</b> > <b>XR Plug-in Management</b> > <b>OpenXR</b> > <b>{targetGroup}</b> tab. In the list of OpenXR Features, enable <b>{ARCameraFeature.displayName}</b>.",
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                    var cameraFeature = settings.GetFeature<ARCameraFeature>();
                    cameraFeature.enabled = true;
                },
                error = false
            });
        }
#endif

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_BoundaryVisibility_Create")]
            public static extern void Create(IntPtr boundaryVisibilityChangedCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_BoundaryVisibility_TryRequestBoundaryVisibility")]
            public static extern XrResult TryRequestBoundaryVisibility(XrBoundaryVisibility visibility);
        }
    }
}
