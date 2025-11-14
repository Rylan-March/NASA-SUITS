#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
using System;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
     /// <summary>
    /// Use this OpenXR feature to enable colocation discovery on Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = m_DisplayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "Enables you to advertise messages and discover colocated OpenXR Meta devices",
        DocumentationLink = Constants.DocsUrls.k_ColocationDiscoveryUrl,
        OpenxrExtensionStrings = k_RequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "1.0.0")]
#endif
    public class ColocationDiscoveryFeature : MetaOpenXRFeature
    {
        /// <summary>
        /// UI display name of this feature.
        /// </summary>
        const string m_DisplayName = "Meta Quest: Colocation Discovery";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.meta-colocation-discovery";

        /// <summary>
        /// Notifies when colocation advertisement state changes.
        /// </summary>
        public event EventHandler<Result<ColocationState>> advertisementStateChanged
        {
            add => ColocationAdvertisement.stateChanged += value;
            remove => ColocationAdvertisement.stateChanged -= value;
        }

        /// <summary>
        /// Notifies when colocation discovery state changes.
        /// </summary>
        public event EventHandler<Result<ColocationState>> discoveryStateChanged
        {
            add => ColocationDiscovery.stateChanged += value;
            remove => ColocationDiscovery.stateChanged -= value;
        }

        /// <summary>
        /// Notifies when a message was discovered while colocation discovery is <see cref="ColocationState.Active"/>.
        /// The <see cref="ColocationDiscoveryMessage.data"/> is allocated with `Allocator.Temp` and is automatically
        /// disposed of at the end of the frame.
        /// </summary>
        public event EventHandler<ColocationDiscoveryMessage> messageDiscovered
        {
            add => ColocationDiscovery.messageDiscovered += value;
            remove => ColocationDiscovery.messageDiscovered -= value;
        }

        /// <summary>
        /// The current Colocation Advertisement <see cref="ColocationState"/>.
        /// </summary>
        public ColocationState advertisementState => ColocationAdvertisement.advertisementState;

        /// <summary>
        /// The current Colocation Discovery <see cref="ColocationState"/>.
        /// </summary>
        public ColocationState discoveryState => ColocationDiscovery.discoveryState;

        /// <summary>
        /// The ID of the active Colocation Advertisement.
        /// </summary>
        public SerializableGuid advertisementId => ColocationAdvertisement.advertisementId;

        const string k_RequestedExtensions = k_XR_META_colocation_discovery;

        /// <summary>
        /// Attempts to start advertising, or broadcasting, the <paramref name="message"/> to colocated users of your
        /// app that have Colocation Discovery active. If successful, the <see cref="advertisementState"/> will
        /// transition to <see cref="ColocationState.Starting"/>. Subscribe to <see cref="advertisementStateChanged"/>
        /// to know when advertisement has started and is active or if advertisement fails to start. Colocation Advertisement
        /// can only be started if the current <see cref="advertisementState"/> is <see cref="ColocationState.Inactive"/>.
        /// Calling `TryStartAdvertisementAsync` will return <see cref="XRResultStatus.StatusCode.ValidationFailure"/>
        /// if the current advertisement state is not `Inactive`.
        /// </summary>
        /// <param name="message">The message to advertise. The max buffer size is 1024 bytes.</param>
        /// <returns>The result of the request to start advertisement and the advertisement ID.</returns>
        public Awaitable<Result<SerializableGuid>> TryStartAdvertisementAsync(Span<byte> message)
        {
            return ColocationAdvertisement.TryStartAsync(message);
        }

        /// <summary>
        /// Attempts to stop colocation advertisement. If successful, the <see cref="advertisementState"/> will transition
        /// to <see cref="ColocationState.Stopping"/>. Subscribe to <see cref="advertisementStateChanged"/> to know when
        /// advertisement has stopped and is inactive or if advertisement fails to stop. Colocation Advertisement
        /// can only be stopped if the current <see cref="advertisementState"/> is <see cref="ColocationState.Active"/>.
        /// Calling `TryStopAdvertisementAsync` will return <see cref="XRResultStatus.StatusCode.ValidationFailure"/>
        /// if the current advertisement state is not `Inactive`.
        /// </summary>
        /// <returns>The Awaitable of the async operation. You are responsible to <see langword="await"/> this.</returns>
        public Awaitable<XRResultStatus> TryStopAdvertisementAsync()
        {
            if (!enabled)
                return AwaitableUtils.CompletedAwaitable(new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess));

            return ColocationAdvertisement.TryStopAsync();
        }

        /// <summary>
        /// Attempts to start discovery of any colocated users of your app that is actively advertising a message. If
        /// successful, the <see cref="discoveryState"/> will transition to <see cref="ColocationState.Starting"/>.
        /// Subscribe to <see cref="discoveryStateChanged"/> to know when discovery has started and is active or if
        /// discovery fails to start. Subscribe to <see cref="messageDiscovered"/> to know when a message is discovered.
        /// Colocation Discovery can only be started if the current <see cref="discoveryState"/> is
        /// <see cref="ColocationState.Inactive"/>. Calling `TryStartDiscoveryAsync` will return
        /// <see cref="XRResultStatus.StatusCode.ValidationFailure"/> if the current advertisement state is not `Inactive`.
        /// </summary>
        /// <returns>The result of the request to start discovery.</returns>
        public Awaitable<XRResultStatus> TryStartDiscoveryAsync()
        {
            return ColocationDiscovery.TryStartAsync();
        }

        /// <summary>
        /// Attempts to Stops colocation discovery. If successful, the <see cref="discoveryState"/> will transition to
        /// <see cref="ColocationState.Stopping"/>. Subscribe to <see cref="discoveryStateChanged"/> to know when
        /// discovery has stopped and is inactive. Colocation Discovery can only be stopped if the current
        /// <see cref="discoveryState"/> is <see cref="ColocationState.Active"/>. Calling `TryStartDiscoveryAsync` will
        /// return <see cref="XRResultStatus.StatusCode.ValidationFailure"/> if the current advertisement state is not
        /// `Inactive`.
        /// </summary>
        /// <returns>The Awaitable of the async operation. You are responsible to <see langword="await"/> this.</returns>
        public Awaitable<XRResultStatus> TryStopDiscoveryAsync()
        {
            if (!enabled)
                return AwaitableUtils.CompletedAwaitable(new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess));

            return ColocationDiscovery.TryStopAsync();
        }

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
            var enableFeature =
                OpenXRRuntime.IsExtensionEnabled(k_XR_META_colocation_discovery) &&
                IsCapabilitySupported(SystemCapability.ColocationDiscovery, xrInstance, "Colocation Discovery");

            if (!enableFeature)
                return false;

            ColocationAdvertisement.CreateNative();
            ColocationDiscovery.Create();
            return true;
        }

        /// <summary>
        /// Called before xrDestroyInstance. Destroys the Colocation Advertisement and Discovery providers.
        /// </summary>
        /// <param name="xrInstance">Handle of the xrInstance</param>
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            ColocationAdvertisement.CancelRequestsAndDestroyNative();
            ColocationDiscovery.CancelRequestsAndDestroyNative();
        }
    }
}
