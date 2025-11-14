using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The OpenXR Meta implementation of <see cref="XROcclusionSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXROcclusionSubsystem : XROcclusionSubsystem
    {
        internal const string k_SubsystemId = "Meta-Occlusion";

        const int XR_ERROR_NOT_PERMITTED_PASSTHROUGH_FB = -1000118003;
        const string k_ScenePermissionError = "To use occlusion, your app must first be granted XR_FB_scene permission."
            + " Do not enable the AR Occlusion Manager component until permissions is granted."
            + " Refer to the Unity OpenXR: Meta package documentation for more information.";

        const int XR_ENVIRONMENT_DEPTH_NOT_AVAILABLE_META = 1000291000;

        /// <summary>
        /// Get whether hand removal is supported by the OpenXR runtime.
        /// </summary>
        /// <value>
        /// * `Unknown` if the OpenXR instance has not yet been created.
        /// * `Unsupported` if hand removal is not supported.
        /// * `Supported` if hand removal is supported.
        /// </value>
        public Supported isHandRemovalSupported =>
            GetCachedSystemCapabilities()[SystemCapability.HandRemoval].isSupported;

        /// <summary>
        /// Get whether hand removal is currently enabled.
        /// </summary>
        /// <remarks>
        /// This value is always `false` while the subsystem is not yet running.
        /// </remarks>
        public bool isHandRemovalEnabled => running && ((MetaOpenXROcclusionProvider)provider).isHandRemovalEnabled;

        /// <summary>
        /// Attempts to set whether hand removal is enabled. Enable hand removal if your project uses a separate
        /// occlusion method for hands.
        /// </summary>
        /// <param name="enableHandRemoval">Indicates whether to enable hand removal.</param>
        /// <returns>
        /// * `Success` if setting hand removal succeeded with no qualifications.
        /// * `FeatureUnsupported` if the device does not support hand removal.
        /// * Otherwise, read the result value for more information.
        /// </returns>
        public XrResult TrySetHandRemovalEnabled(bool enableHandRemoval)
            => ((MetaOpenXROcclusionProvider)provider).TrySetHandRemovalEnabled(enableHandRemoval);

        class MetaOpenXROcclusionProvider : Provider
        {
            internal bool isHandRemovalEnabled { get; private set; }

            const string k_EnvironmentDepthTextureName = "_EnvironmentDepthTexture";
            int m_EnvironmentDepthTextureId;
            bool m_PermissionDenied;

            protected override bool TryInitialize()
            {
                m_EnvironmentDepthTextureId = Shader.PropertyToID(k_EnvironmentDepthTextureName);
                NativeApi.Create();
                return true;
            }

            public override bool TryGetAllTextureDescriptorsInSwapchain(
                out NativeArray<NativeArray<XRTextureDescriptor>> swapchainDescriptors)
            {
                unsafe
                {
                    var swapchainSize = NativeApi.GetSwapchainSize();
                    swapchainDescriptors =
                        new NativeArray<NativeArray<XRTextureDescriptor>>((int)swapchainSize, Allocator.Temp);

                    var stagingArray = new NativeArray<XRTextureDescriptor>((int)swapchainSize, Allocator.Temp);
                    for (var i = 0; i < swapchainSize; i++)
                    {
                        stagingArray[i] = new XRTextureDescriptor(m_EnvironmentDepthTextureId);
                    }

                    var status = NativeApi.TryGetSwapchainTextureDescriptors(stagingArray.GetUnsafePtr());

                    for (var i = 0; i < swapchainSize; ++i)
                    {
                        var frameArray = new NativeArray<XRTextureDescriptor>(1, Allocator.Temp);
                        frameArray[0] = stagingArray[i];
                        swapchainDescriptors[i] = frameArray;
                    }

                    return status;
                }
            }

            internal XrResult TrySetHandRemovalEnabled(bool enableHandRemoval)
            {
                // Hand removal has no effect until the native provider is started.
                // If this method is called before then, just save the requested value and wait until Start.
                if (!running)
                {
                    isHandRemovalEnabled = enableHandRemoval;
                    return XrResult.Success;
                }

                var result = NativeApi.SetHandRemovalEnabled(enableHandRemoval);
                if (result.IsSuccess())
                    isHandRemovalEnabled = enableHandRemoval;

                return result;
            }

            public override void Start()
            {
                // Natively, initializing the occlusion provider can fail due to lack of required permissions.
                // We wait to initialize until the last possible moment to give the user time to request permission,
                // but SubsystemWithProvider.Start isn't allowed to fail, so at this point we must fire and forget.
                TryStart();
            }

            XRResultStatus TryStart()
            {
                var result = NativeApi.TryStart(SystemInfo.graphicsDeviceType, isHandRemovalEnabled);
                if (result.IsSuccess())
                {
                    m_PermissionDenied = false;
                    return result;
                }
                if (result.statusCode == StatusCode.ProviderUninitialized)
                {
                    Debug.LogError(Constants.k_ErrorProviderUninitialized);
                }
                else if (result.nativeStatusCode == XR_ERROR_NOT_PERMITTED_PASSTHROUGH_FB)
                {
                    Debug.LogError(k_ScenePermissionError);
                    m_PermissionDenied = true;
                }
                else if (result.IsError() && result.statusCode == StatusCode.PlatformError)
                {
                    Debug.LogError($"Occlusion subsystem failed to start with error: {(XrResult)result.nativeStatusCode}");
                }
                else if (result.IsError())
                {
                    Debug.LogError("Occlusion subsystem failed to start. Check logs for more information.");
                }

                return result;
            }

            public override void Stop()
            {
                if (m_PermissionDenied)
                    return;

                NativeApi.Stop();
            }

            public override void Destroy()
            {
                if (m_PermissionDenied)
                    return;

                NativeApi.Destroy();
            }

            public override XRResultStatus TryGetFrame(Allocator allocator, out XROcclusionFrame frame)
            {
                if (m_PermissionDenied)
                {
                    Debug.LogError(k_ScenePermissionError);
                    frame = default;
                    return new XRResultStatus(StatusCode.ProviderNotStarted);
                }

                var poses = new NativeArray<Pose>(2, allocator);
                var fovs = new NativeArray<XRFov>(2, allocator);
                XROcclusionFrameNative nativeFrame = default;

                unsafe
                {
                    nativeFrame.poses = poses.GetUnsafePtr();
                    nativeFrame.fovs = fovs.GetUnsafePtr();
                }

                var result = NativeApi.TryGetFrame(ref nativeFrame);
                frame = new XROcclusionFrame(
                    properties: nativeFrame.properties,
                    timestamp: nativeFrame.timestampNs,
                    nearFarPlanes: nativeFrame.nearFarPlanes,
                    poses: poses,
                    fovs: fovs
                );

                // Environment depth not available is a success code, but this API needs to treat it as an error
                return result.nativeStatusCode == XR_ENVIRONMENT_DEPTH_NOT_AVAILABLE_META
                        ? new XRResultStatus(StatusCode.ProviderNotStarted)
                        : result;
            }

            public override NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator)
            {
                if (m_PermissionDenied)
                {
                    Debug.LogError(k_ScenePermissionError);
                    return default;
                }

                XRTextureDescriptor descriptor = new(m_EnvironmentDepthTextureId);
                if (!NativeApi.TryGetTextureDescriptorForFrame(ref descriptor))
                    return default;

                var textureDescriptors = new NativeArray<XRTextureDescriptor>(1, allocator);
                textureDescriptors[0] = descriptor;
                return textureDescriptors;
            }

            public override bool TryGetEnvironmentDepth(out XRTextureDescriptor environmentDepthDescriptor)
            {
                if (m_PermissionDenied)
                {
                    Debug.LogError(k_ScenePermissionError);
                    environmentDepthDescriptor = default;
                    return false;
                }

                XRTextureDescriptor descriptor = new XRTextureDescriptor(m_EnvironmentDepthTextureId);
                var result = NativeApi.TryGetTextureDescriptorForFrame(ref descriptor);
                environmentDepthDescriptor = descriptor;
                return result;
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                var occlusionSubsystemCinfo = new XROcclusionSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXROcclusionProvider),
                    subsystemTypeOverride = typeof(MetaOpenXROcclusionSubsystem),
                    humanSegmentationStencilImageSupportedDelegate = null,
                    humanSegmentationDepthImageSupportedDelegate = null,
                    environmentDepthTemporalSmoothingSupportedDelegate = null,
                    environmentDepthImageSupportedDelegate = () =>
                        GetCachedSystemCapabilities()[SystemCapability.EnvironmentDepth].isSupported,
                    environmentDepthConfidenceImageSupportedDelegate = null,
                };

                XROcclusionSubsystemDescriptor.Register(occlusionSubsystemCinfo);
            }

            static unsafe class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_Create")]
                internal static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_TryStart")]
                internal static extern XRResultStatus TryStart(
                    GraphicsDeviceType graphisAPI, [MarshalAs(UnmanagedType.U1)]bool enableHandRemoval);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_TrySetHandRemovalEnabled")]
                internal static extern XrResult SetHandRemovalEnabled([MarshalAs(UnmanagedType.U1)]bool enableHandRemoval);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_GetSwapchainSize")]
                internal static extern uint GetSwapchainSize();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_TryGetSwapchainTextureDescriptors")]
                internal static extern XRResultStatus TryGetSwapchainTextureDescriptors(void* descriptors);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_TryGetFrame")]
                internal static extern XRResultStatus TryGetFrame(ref XROcclusionFrameNative frame);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_TryGetTextureDescriptorForFrame")]
                internal static extern XRResultStatus TryGetTextureDescriptorForFrame(ref XRTextureDescriptor descriptor);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_Stop")]
                internal static extern void Stop();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_Occlusion_Destroy")]
                internal static extern void Destroy();
            }
        }
    }
}
