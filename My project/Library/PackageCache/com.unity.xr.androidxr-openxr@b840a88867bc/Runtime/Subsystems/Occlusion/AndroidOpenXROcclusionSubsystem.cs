using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Android;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR implementation of the <see cref="XROcclusionSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXROcclusionSubsystem : XROcclusionSubsystem
    {
        internal const string k_SubsystemId = "Android-Occlusion";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XROcclusionSubsystemDescriptor.Register(
                new XROcclusionSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(AndroidOpenXRProvider),
                    subsystemTypeOverride = typeof(AndroidOpenXROcclusionSubsystem),
                    environmentDepthTemporalSmoothingSupportedDelegate = () => Supported.Supported,
                    environmentDepthImageSupportedDelegate = () => Supported.Supported,
                    environmentDepthConfidenceImageSupportedDelegate = () => Supported.Supported
                }
            );
        }

        class AndroidOpenXRProvider : Provider
        {
            const int k_Views = 2;
            Texture2DArray[] m_DepthTextures;
            RenderTexture[] m_DepthRTs;
            Texture2DArray[] m_DepthConfidenceTextures;
            RenderTexture[] m_DepthConfidenceRTs;
            bool m_EnableTemporalSmoothing = true;
            bool m_PendingTemporalSmoothingChange = false;
            int m_LastReadyIndex;

            enum UpdateResult
            {
                Failed = -1,
                UpdateSuccess,
                NoUpdateNeeded,
                RebuiltSwapchain
            }

            /// <summary>
            /// The shader keyword toggling depth linearization logic for occlusion
            /// </summary>
            const string k_LinearDepthKeyword = "XR_LINEAR_DEPTH";

            readonly XRShaderKeywords m_XRShaderKeywords =
                new(new ReadOnlyList<string>(new List<string> { k_LinearDepthKeyword }), null);

            struct AndroidDepthFrameInterchange
            {
                public Int64 exposureTimestamp;
                public XRFov fovLeft;
                public XRFov fovRight;
                public Pose poseLeft;
                public Pose poseRight;
                public IntPtr depthBuffer;
                public IntPtr confidenceBuffer;
                public UInt32 swapchainIndex;
            }

            /// <summary>
            /// The shader property name for the environment depth texture.
            /// </summary>
            const string k_EnvironmentDepthTexturePropertyName = "_EnvironmentDepthTexture";
            const string k_EnvironmentConfidenceTexturePropertyName =
                "_EnvironmentConfidenceTexture";

            /// <summary>
            /// The shader property name identifier for the environment depth texture.
            /// </summary>
            static readonly int k_EnvironmentDepthTexturePropertyId = Shader.PropertyToID(
                k_EnvironmentDepthTexturePropertyName
            );

            static readonly int k_EnvironmentConfidenceTexturePropertyId = Shader.PropertyToID(
                k_EnvironmentConfidenceTexturePropertyName
            );

            static readonly XRTextureDescriptor k_DefaultXRTextureDescriptor =
                new XRTextureDescriptor(
                    nativeTexture: IntPtr.Zero,
                    width: 0,
                    height: 0,
                    mipmapCount: 0,
                    format: (TextureFormat)0,
                    propertyNameId: -1,
                    depth: 0,
                    textureType: XRTextureType.None
                );

            bool m_Started = false;

            UInt32 m_Width = 0;

            UInt32 m_Height = 0;

            int viewElementCount => (int)(m_Width * m_Height);

            int bufferElementCount => viewElementCount * k_Views;

            AndroidDepthFrameInterchange m_LastImage;

            int m_FrameCountAtLastUpdate;

            /// <summary>
            /// Used to prevent redundant copies from being done
            /// </summary>
            Int64 m_OcclusionTimestampAtLastUpdate;

            /// <summary>
            /// Used to prevent redundant copies from being done
            /// </summary>
            Int64 m_ConfidenceTimestampAtLastUpdate;

            /// <summary>
            /// Whether temporal smoothing should be applied to the environment depth image. Query for support with
            /// <see cref="XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported"/>.
            /// </summary>
            /// <value>When `true`, temporal smoothing is applied to the environment depth image. Otherwise, no temporal smoothing is applied.</value>
            public override bool environmentDepthTemporalSmoothingRequested
            {
                get => m_EnableTemporalSmoothing;
                set
                {
                    if (NativeApi.GetUseSmoothing() != value && NativeApi.SetUseSmoothing(value))
                    { // on success, update value
                        m_EnableTemporalSmoothing = value;
                        m_PendingTemporalSmoothingChange = m_Started;
                    }
                }
            }

            /// <summary>
            /// Property to be implemented by the provider to get whether temporal smoothing is currently applied to the
            /// environment depth image.
            /// </summary>
            public override bool environmentDepthTemporalSmoothingEnabled =>
                m_EnableTemporalSmoothing;

            protected override bool TryInitialize()
            {
                if (
                    OpenXRRuntime.IsExtensionEnabled(
                        Constants.OpenXRExtensions.k_XR_ANDROID_depth_texture
                    )
                )
                {
                    NativeApi.Create();
                    return true;
                }

                return false;
            }

            public override void Destroy()
            {
                NativeApi.Destroy();
            }

            public override void Start()
            {
                if (m_Started)
                {
                    return;
                }

#if UNITY_ANDROID
                if (
                    !Permission.HasUserAuthorizedPermission(
                        Constants.Permissions.k_SceneUnderstandingFinePermission
                    )
                )
                {
                    Debug.LogWarning(
                        $"Depth occlusion requires system permission {Constants.Permissions.k_SceneUnderstandingFinePermission}, but permission was not granted."
                    );
                }
#endif

                // need to set the property before creating the swapchain in NativeApi.Start()
                NativeApi.SetUseSmoothing(m_EnableTemporalSmoothing);

                if (!NativeApi.Start())
                {
                    Debug.LogError("Failed to start AndroidXR Depth API");
                    return;
                }

                NativeApi.GetDepthTextureDimensions(out m_Width, out m_Height);
                var bufferCount = (int)NativeApi.GetBufferCount();
                if (bufferCount == 0)
                {
                    Debug.LogError("Failed to start AndroidXR Depth API: swapchain has 0 capacity");
                    return;
                }

                m_DepthTextures = new Texture2DArray[bufferCount];
                m_DepthRTs = new RenderTexture[bufferCount];
                m_DepthConfidenceTextures = new Texture2DArray[bufferCount];
                m_DepthConfidenceRTs = new RenderTexture[bufferCount];
                for (int i = 0; i < bufferCount; i++)
                {
                    m_DepthTextures[i] = new(
                        (int)m_Width,
                        (int)m_Height,
                        k_Views,
                        TextureFormat.RFloat,
                        false
                    );
                    m_DepthRTs[i] = new(
                        new RenderTextureDescriptor
                        {
                            width = (int)m_Width,
                            height = (int)m_Height,
                            graphicsFormat = GraphicsFormat.R32_SFloat,
                            dimension = TextureDimension.Tex2DArray,
                            volumeDepth = 2,
                            msaaSamples = 1,
                        }
                    );
                    if (!m_DepthRTs[i].Create())
                    {
                        Debug.LogError("Failed to create depth RenderTexture");
                        return;
                    }

                    m_DepthConfidenceTextures[i] = new(
                        (int)m_Width,
                        (int)m_Height,
                        k_Views,
                        TextureFormat.R8,
                        false
                    );
                    m_DepthConfidenceRTs[i] = new(
                        new RenderTextureDescriptor
                        {
                            width = (int)m_Width,
                            height = (int)m_Height,
                            graphicsFormat = GraphicsFormat.R8_UInt,
                            dimension = TextureDimension.Tex2DArray,
                            volumeDepth = 2,
                            msaaSamples = 1,
                        }
                    );
                    if (!m_DepthConfidenceRTs[i].Create())
                    {
                        Debug.LogError("Failed to create depth confidence RenderTexture");
                        return;
                    }
                }

                m_Started = true;
            }

            public override void Stop()
            {
                NativeApi.Stop();
                m_Started = false;
            }

            void Restart()
            {
                Stop();
                Start();
            }

            /// <summary>
            /// Method to be implemented by the provider to get the occlusion texture descriptors associated with the
            /// current AR frame.
            /// </summary>
            /// <param name="defaultDescriptor">The default descriptor value.</param>
            /// <param name="allocator">The allocator to use when creating the returned <c>NativeArray</c>.</param>
            /// <returns>An array of the occlusion texture descriptors.</returns>
            public override NativeArray<XRTextureDescriptor> GetTextureDescriptors(
                XRTextureDescriptor defaultDescriptor,
                Allocator allocator
            )
            {
                if (!m_Started || !UpdateDepthImage() || !UpdateDepthConfidenceImage())
                {
                    return new NativeArray<XRTextureDescriptor>(0, allocator);
                }

                var descriptors = new NativeArray<XRTextureDescriptor>(2, allocator);

                var depthHandle = GCHandle.Alloc(m_DepthRTs[m_LastReadyIndex]);
                descriptors[0] = new XRTextureDescriptor(
                    nativeTexture: GCHandle.ToIntPtr(depthHandle),
                    width: (int)m_Width,
                    height: (int)m_Height,
                    mipmapCount: 0,
                    format: TextureFormat.RFloat,
                    propertyNameId: k_EnvironmentDepthTexturePropertyId,
                    depth: 2,
                    textureType: XRTextureType.ColorRenderTextureRef
                );

                var confidenceHandle = GCHandle.Alloc(m_DepthConfidenceRTs[m_LastReadyIndex]);
                descriptors[1] = new XRTextureDescriptor(
                    nativeTexture: GCHandle.ToIntPtr(confidenceHandle),
                    width: (int)m_Width,
                    height: (int)m_Height,
                    mipmapCount: 0,
                    format: TextureFormat.R8,
                    propertyNameId: k_EnvironmentConfidenceTexturePropertyId,
                    depth: 2,
                    textureType: XRTextureType.ColorRenderTextureRef
                );

                return descriptors;
            }

            /// <summary>
            /// Method to be implemented by the provider to get the environment depth confidence texture descriptor.
            /// </summary>
            /// <param name="environmentDepthConfidenceDescriptor">The environment depth texture descriptor to be
            /// populated, if available.</param>
            /// <returns>
            /// <c>true</c> if the environment depth confidence texture descriptor is available and is returned.
            /// Otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown if the implementation does not support environment
            /// depth confidence texture.</exception>
            public override bool TryGetEnvironmentDepthConfidence(
                out XRTextureDescriptor environmentDepthConfidenceDescriptor
            )
            {
                environmentDepthConfidenceDescriptor = k_DefaultXRTextureDescriptor;

                if (!m_Started || !UpdateDepthConfidenceImage())
                {
                    return false;
                }

                var confidenceHandle = GCHandle.Alloc(m_DepthConfidenceRTs[m_LastReadyIndex]);
                environmentDepthConfidenceDescriptor = new XRTextureDescriptor(
                    nativeTexture: GCHandle.ToIntPtr(confidenceHandle),
                    width: (int)m_Width,
                    height: (int)m_Height,
                    mipmapCount: 0,
                    format: TextureFormat.R8,
                    propertyNameId: k_EnvironmentConfidenceTexturePropertyId,
                    depth: 2,
                    textureType: XRTextureType.ColorRenderTextureRef
                );

                return true;
            }

            /// <summary>
            /// Get the camera's occlusion frame for the subsystem.
            /// </summary>
            /// <param name="frame">The current camera's occlusion frame returned by the method.</param>
            /// <returns><see langword="true"/> if the method successfully got a frame. Otherwise,
            /// <see langword="false"/>.</returns>
            public override XRResultStatus TryGetFrame(
                Allocator allocator,
                out XROcclusionFrame frame
            )
            {
                if (!m_Started || !UpdateDepthImage())
                {
                    frame = new XROcclusionFrame();
                    return false;
                }

                var mainCamera = Camera.main;
                var zNear = mainCamera?.nearClipPlane ?? 0;
                var zFar = mainCamera?.farClipPlane ?? 0;

                frame = new XROcclusionFrame(
                    properties: XROcclusionFrameProperties.Timestamp
                        | XROcclusionFrameProperties.Poses
                        | XROcclusionFrameProperties.Fovs
                        | XROcclusionFrameProperties.NearFarPlanes,
                    timestamp: m_LastImage.exposureTimestamp,
                    nearFarPlanes: new XRNearFarPlanes(zNear, zFar),
                    poses: new NativeArray<Pose>(
                        new[] { m_LastImage.poseLeft, m_LastImage.poseRight },
                        allocator
                    ),
                    fovs: new NativeArray<XRFov>(
                        new[] { m_LastImage.fovLeft, m_LastImage.fovRight },
                        allocator
                    )
                );
                return true;
            }

            [Obsolete(
                "ShaderKeywords struct is deprecated as of AR Foundation 6.1. Use GetShaderKeywords2() method instead, which provides XRShaderKeywords."
            )]
            public override ShaderKeywords GetShaderKeywords()
            {
                return new ShaderKeywords(
                    new ReadOnlyCollection<string>(new[] { k_LinearDepthKeyword })
                );
            }

            public override XRShaderKeywords GetShaderKeywords2()
            {
                return m_XRShaderKeywords;
            }

            UpdateResult TryUpdate(ref Int64 previousTimestamp)
            {
                if (m_PendingTemporalSmoothingChange)
                {
                    m_PendingTemporalSmoothingChange = false;
                    Restart();
                    return UpdateResult.RebuiltSwapchain;
                }

                if (m_FrameCountAtLastUpdate != Time.frameCount)
                {
                    if (!NativeApi.TryGetEnvironmentDepth(out m_LastImage))
                    { // don't attempt on failed update
                        return UpdateResult.Failed;
                    }
                    m_FrameCountAtLastUpdate = Time.frameCount;
                }

                if (m_LastImage.exposureTimestamp == previousTimestamp)
                { // already current despite new frameCount
                    return UpdateResult.NoUpdateNeeded;
                }

                previousTimestamp = m_LastImage.exposureTimestamp;
                return UpdateResult.UpdateSuccess;
            }

            bool UpdateDepthImage()
            {
                switch (TryUpdate(ref m_OcclusionTimestampAtLastUpdate))
                {
                    case UpdateResult.Failed:
                        return false;
                    case UpdateResult.RebuiltSwapchain:
                        return false; // not an error but didn't update.
                    case UpdateResult.NoUpdateNeeded:
                        return true;
                }

                if (m_LastImage.depthBuffer == IntPtr.Zero)
                { // no texture update yet
                    return false;
                }
                unsafe
                {
                    using (
                        var depthBuffer =
                            NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                                (void*)m_LastImage.depthBuffer,
                                bufferElementCount,
                                Allocator.None
                            )
                    )
                    {
                        var si = (int)m_LastImage.swapchainIndex;
                        for (int i = 0; i < k_Views; i++)
                        {
                            var view = depthBuffer.GetSubArray(
                                i * viewElementCount,
                                viewElementCount
                            );
                            m_DepthTextures[si].SetPixelData(view, 0, i, 0);
                        }
                        m_DepthTextures[si].Apply();
                        for (int i = 0; i < k_Views; i++)
                        {
                            Graphics.Blit(m_DepthTextures[si], m_DepthRTs[si], i, i);
                        }
                        m_LastReadyIndex = si;
                        return true;
                    }
                }
            }

            bool UpdateDepthConfidenceImage()
            {
                switch (TryUpdate(ref m_OcclusionTimestampAtLastUpdate))
                {
                    case UpdateResult.Failed:
                        return false;
                    case UpdateResult.RebuiltSwapchain:
                        return false; // not an error but didn't update.
                    case UpdateResult.NoUpdateNeeded:
                        return true;
                }

                if (m_LastImage.confidenceBuffer == IntPtr.Zero)
                { // no texture update yet
                    return false;
                }
                unsafe
                {
                    using (
                        var srcBuffer =
                            NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                                (void*)m_LastImage.confidenceBuffer,
                                bufferElementCount,
                                Allocator.None
                            )
                    )
                    {
                        var si = (int)m_LastImage.swapchainIndex;
                        for (int i = 0; i < k_Views; i++)
                        {
                            var srcView = srcBuffer.GetSubArray(
                                i * viewElementCount,
                                viewElementCount
                            );
                            m_DepthConfidenceTextures[si].SetPixelData(srcView, 0, i, 0);
                        }
                        m_DepthConfidenceTextures[si].Apply();
                        for (int i = 0; i < k_Views; i++)
                        {
                            Graphics.Blit(
                                m_DepthConfidenceTextures[si],
                                m_DepthConfidenceRTs[si],
                                i,
                                i
                            );
                        }
                        m_LastReadyIndex = si;
                        return true;
                    }
                }
            }

            static class NativeApi
            {
                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_Create"
                )]
                public static extern void Create();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_Destroy"
                )]
                public static extern void Destroy();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_Start"
                )]
                [return: MarshalAs(UnmanagedType.U1)]
                public static extern bool Start();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_Stop"
                )]
                public static extern void Stop();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_GetUseSmoothing"
                )]
                [return: MarshalAs(UnmanagedType.U1)]
                public static extern bool GetUseSmoothing();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_SetUseSmoothing"
                )]
                [return: MarshalAs(UnmanagedType.U1)]
                public static extern bool SetUseSmoothing(
                    [MarshalAs(UnmanagedType.I1)] bool useSmoothing
                );

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_GetDepthTextureDimensions"
                )]
                public static extern void GetDepthTextureDimensions(
                    out UInt32 width,
                    out UInt32 height
                );

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_GetBufferCount"
                )]
                public static extern uint GetBufferCount();

                [DllImport(
                    Constants.k_ARFoundationLibrary,
                    EntryPoint = "UnityOpenXRAndroid_Occlusion_TryGetEnvironmentDepth"
                )]
                [return: MarshalAs(UnmanagedType.U1)]
                public static extern bool TryGetEnvironmentDepth(
                    out AndroidDepthFrameInterchange environmentDepthDescriptor
                );
            }
        }
    }

    static class RenderTextureExtensions
    {
        struct RenderBufferPartial
        {
            IntPtr hdrSettings;
            IntPtr resolveSurface;
            int memLabelOwner;
            public int textureID;
        }

        internal static int GetColorTextureID(this RenderTexture rt)
        {
            var renderBufferPtr = rt.colorBuffer.GetNativeRenderBufferPtr();
            if (renderBufferPtr == IntPtr.Zero)
            {
                return 0;
            }
            unsafe
            {
                var renderBuffer = Marshal.PtrToStructure<RenderBufferPartial>(renderBufferPtr);
                return renderBuffer.textureID;
            }
        }
    }
}
