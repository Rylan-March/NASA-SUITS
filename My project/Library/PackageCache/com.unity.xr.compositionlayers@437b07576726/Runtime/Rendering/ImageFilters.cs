using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

namespace Unity.XR.CompositionLayers.Rendering
{
    /// <summary>
    /// Provides image filtering and blitting functionalities for composition layers in XR applications.
    /// </summary>
    public static class ImageFilters
    {
        /// <summary>
        /// Defines the alpha blending modes.
        /// </summary>
        public enum AlphaMode
        {
            /// <summary>
            /// Opaque alpha mode.
            /// </summary>
            Opaque,

            /// <summary>
            /// Alpha blending mode.
            /// </summary>
            Alpha,

            /// <summary>
            /// Premultiplied alpha blending mode.
            /// </summary>
            Premultiply,
        };

        /// <summary>
        /// Enumerates the supported render pipelines.
        /// </summary>
        public enum RenderPipeline
        {
            /// <summary>
            /// Built-in render pipeline.
            /// </summary>
            Builtin,

            /// <summary>
            /// Universal Render Pipeline.
            /// </summary>
            Universal,
            /// <summary>
            /// High Definition Render Pipeline.
            /// </summary>
            HighDefinition,
        }

        /// <summary>
        /// Represents HDR parameters for image processing.
        /// </summary>
        public struct HDRParams
        {
            const float defaultNitsForPaperWhite = 160.0f;
            const float defaultMaxDisplayNits = 160.0f;

            /// <summary>
            /// Indicates whether HDR encoding is enabled.
            /// </summary>
            public bool hdrEncoded;

            /// <summary>
            /// The color gamut used.
            /// </summary>
            public ColorGamut colorGamut;

            /// <summary>
            /// The nits value for paper white.
            /// </summary>
            public float nitsForPaperWhite;

            /// <summary>
            /// The maximum display nits.
            /// </summary>
            public float maxDisplayNits;

            /// <summary>
            /// Gets an inactive HDR parameters instance with default values.
            /// </summary>
            public static HDRParams inactiveHdrParams
            {
                get
                {
                    return new HDRParams
                    {
                        hdrEncoded = false,
                        colorGamut = ColorGamut.sRGB,
                        nitsForPaperWhite = defaultNitsForPaperWhite,
                        maxDisplayNits = defaultMaxDisplayNits,
                    };
                }
            }

            HDRParams(HDROutputSettings hdrSettings)
            {
                if (hdrSettings != null && hdrSettings.active)
                {
                    hdrEncoded = true;
                    colorGamut = hdrSettings.displayColorGamut;
                    nitsForPaperWhite = hdrSettings.paperWhiteNits;
                    maxDisplayNits = hdrSettings.maxFullFrameToneMapLuminance;
                }
                else
                {
                    hdrEncoded = false;
                    colorGamut = ColorGamut.sRGB;
                    nitsForPaperWhite = defaultNitsForPaperWhite;
                    maxDisplayNits = defaultMaxDisplayNits;
                }
            }

            /// <summary>
            /// Implicitly converts an <see cref="HDROutputSettings"/> to <see cref="HDRParams"/>.
            /// </summary>
            /// <param name="hdrSettings">The HDR output settings to convert.</param>
            /// <returns> New HDR parameters for image processing.</returns>
            public static implicit operator HDRParams(HDROutputSettings hdrSettings)
            {
                return new HDRParams(hdrSettings);
            }

            /// <summary>
            /// Returns a string representation of the HDR parameters.
            /// </summary>
            /// <returns>A string describing the HDR parameters.</returns>
            public override string ToString()
            {
                return $"hdrEncoded {hdrEncoded} colorGamut {colorGamut} nitsForPaperWhite {nitsForPaperWhite} maxDisplayNits {maxDisplayNits}";
            }
        }

        /// <summary>
        /// Parameters for blitting textures.
        /// </summary>
        public struct BlitParams
        {
            /// <summary>
            /// The source texture to blit.
            /// </summary>
            public Texture sourceTexture;

            /// <summary>
            /// The array slice of the source texture.
            /// </summary>
            public int sourceTextureArraySlice;

            /// <summary>
            /// The source rectangle.
            /// </summary>
            public Rect sourceRect;

            /// <summary>
            /// The destination rectangle.
            /// </summary>
            public Rect destRect;

            /// <summary>
            /// The HDR parameters for the source texture.
            /// </summary>
            public HDRParams sourceHdrParams;

            /// <summary>
            /// The alpha blending mode.
            /// </summary>
            public AlphaMode alphaMode;

            /// <summary>
            /// The foveated rendering information.
            /// </summary>
            public IntPtr foveatedRenderingInfo;

            /// <summary>
            /// The render pipeline used.
            /// </summary>
            public RenderPipeline renderPipeline;

            /// <summary>
            /// Initializes a new instance of the <see cref="BlitParams"/> struct with the specified source texture.
            /// </summary>
            /// <param name="sourceTexture">The source texture to blit.</param>
            public BlitParams(Texture sourceTexture)
                : this(sourceTexture, 0)
            { }

            /// <summary>
            /// Initializes a new instance of the <see cref="BlitParams"/> struct with the specified source texture and array slice.
            /// </summary>
            /// <param name="sourceTexture">The source texture to blit.</param>
            /// <param name="sourceTextureArraySlice">The array slice of the source texture.</param>
            public BlitParams(Texture sourceTexture, int sourceTextureArraySlice)
            {
                this.sourceTexture = sourceTexture;
                this.sourceTextureArraySlice = sourceTextureArraySlice;

                sourceRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                destRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

                sourceHdrParams = HDRParams.inactiveHdrParams;

                alphaMode = AlphaMode.Opaque;

                foveatedRenderingInfo = IntPtr.Zero;

                renderPipeline = RenderPipeline.Builtin;
            }

            /// <summary>
            /// Returns a string representation of the blit parameters.
            /// </summary>
            /// <returns>A string describing the blit parameters.</returns>
            public override string ToString()
            {
                return $"sourceTexture {sourceTexture} sourceTextureArraySlice {sourceTextureArraySlice} sourceRect {sourceRect} destRect {destRect} sourceHdrParams {sourceHdrParams} alphaMode {alphaMode} foveatedRenderingInfo {foveatedRenderingInfo}";
            }
        }

        /// <summary>
        /// Parameters for the blit target.
        /// </summary>
        public struct TargetParams
        {
            /// <summary>
            /// The render target texture.
            /// </summary>
            public RenderTexture renderTarget;

            /// <summary>
            /// The target display index.
            /// </summary>
            public int targetDisplay;

            /// <summary>
            /// The HDR parameters for the target.
            /// </summary>
            public HDRParams hdrParams;

            /// <summary>
            /// Initializes a new instance of the <see cref="TargetParams"/> struct with the specified render target.
            /// </summary>
            /// <param name="renderTarget">The render target texture.</param>
            public TargetParams(RenderTexture renderTarget)
            {
                this.renderTarget = renderTarget;
                this.targetDisplay = -1;

                hdrParams = HDRParams.inactiveHdrParams;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TargetParams"/> struct with the specified target display.
            /// </summary>
            /// <param name="targetDisplay">The target display index.</param>
            public TargetParams(int targetDisplay)
            {
                this.renderTarget = null;
                this.targetDisplay = targetDisplay;

                var displays = HDROutputSettings.displays;
                if (displays != null && targetDisplay >= 0 && targetDisplay < displays.Length)
                {
                    hdrParams = HDROutputSettings.displays[targetDisplay];
                }
                else
                {
                    hdrParams = HDRParams.inactiveHdrParams;
                }
            }

            /// <summary>
            /// Returns a string representation of the target parameters.
            /// </summary>
            /// <returns>A string describing the target parameters.</returns>
            public override string ToString()
            {
                return $"renderTarget {renderTarget} targetDisplay {targetDisplay} hdrParams {hdrParams}";
            }
        }

        /// <summary>
        /// Parameters for mirror view rendering.
        /// </summary>
        public struct MirrorViewParams
        {
            /// <summary>
            /// The XR display subsystem.
            /// </summary>
            public XRDisplaySubsystem displaySubsystem;

            /// <summary>
            /// The main camera.
            /// </summary>
            public Camera mainCamera;

            /// <summary>
            /// The mirror view camera.
            /// </summary>
            public Camera mirrorViewCamera;

            /// <summary>
            /// The blit mode.
            /// </summary>
            public int blitMode;

            /// <summary>
            /// The alpha blending mode.
            /// </summary>
            public AlphaMode alphaMode;

            /// <summary>
            /// The render pipeline used.
            /// </summary>
            public RenderPipeline renderPipeline;
        }

        const string ShaderName = "Unlit/XRCompositionLayers/BlitCopyHDR";

        static Material s_Material = null;
        static MaterialPropertyBlock s_MaterialProperty = new MaterialPropertyBlock();
        static CommandBuffer s_CommandBuffer = null;
        static int s_BatchingDepth = 0;

        static readonly int k_MainTex = Shader.PropertyToID("_MainTex");
        static readonly int k_MainTexArraySlice = Shader.PropertyToID("_MainTex_ArraySlice");
        static readonly int k_SourceRect = Shader.PropertyToID("_SourceRect");
        static readonly int k_DestRect = Shader.PropertyToID("_DestRect");

        static readonly int k_SourceNitsForPaperWhite = Shader.PropertyToID("_SourceNitsForPaperWhite");
        static readonly int k_SourceMaxDisplayNits = Shader.PropertyToID("_SourceMaxDisplayNits");
        static readonly int k_SourceColorGamut = Shader.PropertyToID("_SourceColorGamut");
        static readonly int k_NitsForPaperWhite = Shader.PropertyToID("_NitsForPaperWhite");
        static readonly int k_ColorGamut = Shader.PropertyToID("_ColorGamut");
        static readonly int k_MaxDisplayNits = Shader.PropertyToID("_MaxDisplayNits");

        static readonly int k_SrcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int k_DstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int k_AlphaSrcBlend = Shader.PropertyToID("_AlphaSrcBlend");
        static readonly int k_AlphaDstBlend = Shader.PropertyToID("_AlphaDstBlend");

        /// <summary>
        /// Begins a batch of image operations.
        /// </summary>
        public static void BeginBatch()
        {
            if (s_BatchingDepth == 0)
            {
                if (s_CommandBuffer == null)
                    s_CommandBuffer = new CommandBuffer();
                else
                    s_CommandBuffer.Clear();
            }

            ++s_BatchingDepth;
        }

        /// <summary>
        /// Ends a batch of image operations and executes the command buffer.
        /// </summary>
        public static void EndBatch()
        {
            if (s_BatchingDepth > 0)
            {
                if (--s_BatchingDepth == 0)
                {
                    if (s_CommandBuffer != null && s_CommandBuffer.sizeInBytes > 0)
                    {
                        Graphics.ExecuteCommandBuffer(s_CommandBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Blits the specified source texture to the target using the given parameters.
        /// </summary>
        /// <param name="blitParam">Parameters for the blit operation.</param>
        /// <param name="targetParams">Parameters for the blit target.</param>
        public static void Blit(in BlitParams blitParam, in TargetParams targetParams)
        {
            BeginBatch();
            Blit(s_CommandBuffer, blitParam, targetParams);
            EndBatch();
        }

        /// <summary>
        /// Blits the specified source texture to the target display using the given parameters.
        /// </summary>
        /// <param name="cmd">The command buffer to use.</param>
        /// <param name="blitParam">Parameters for the blit operation.</param>
        /// <param name="targetDisplay">The target display index.</param>
        public static void Blit(CommandBuffer cmd, in BlitParams blitParam, int targetDisplay)
        {
            var targetParams = new TargetParams(targetDisplay);
            BlitInternal(cmd, blitParam, targetParams);
        }

        /// <summary>
        /// Blits the specified source texture to the target using the given parameters.
        /// </summary>
        /// <param name="cmd">The command buffer to use.</param>
        /// <param name="blitParam">Parameters for the blit operation.</param>
        /// <param name="targetParams">Parameters for the blit target.</param>
        public static void Blit(CommandBuffer cmd, in BlitParams blitParam, in TargetParams targetParams)
        {
            BlitInternal(cmd, blitParam, targetParams);
        }

        /// <summary>
        /// Blits the mirror view using the specified parameters.
        /// </summary>
        /// <param name="cmd">The command buffer to use.</param>
        /// <param name="mirrorViewParams">Parameters for the mirror view blit operation.</param>
        static void BlitInternal(CommandBuffer cmd, in BlitParams blitParam, in TargetParams targetParams)
        {
            if (blitParam.sourceTexture == null)
                return;

            if (targetParams.renderTarget == null && targetParams.targetDisplay < 0)
                return;

            var mat = InitializeMaterial();
            if (mat == null)
                return;

            Vector4 sourceRect = new Vector4(blitParam.sourceRect.width, blitParam.sourceRect.height, blitParam.sourceRect.x, blitParam.sourceRect.y);
            Vector4 destRect = new Vector4(blitParam.destRect.width, blitParam.destRect.height, blitParam.destRect.x, blitParam.destRect.y);

            if (ShouldYFlipTexture(blitParam.sourceTexture))
            {
                sourceRect.y = -sourceRect.y;
                sourceRect.w += blitParam.sourceRect.height;
            }

            int sourceColorGamut = blitParam.sourceHdrParams.hdrEncoded ? (int)blitParam.sourceHdrParams.colorGamut : -1;
            int colorGamut = targetParams.hdrParams.hdrEncoded ? (int)targetParams.hdrParams.colorGamut : -1;

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                if (blitParam.renderPipeline == RenderPipeline.Universal)
                {
                    // See also: UniversalCameraData.requireSrgbConversion
                    // Note: Need to convert from gamma to linear manually for linear color space when sourceTexture is fromatted as R8G8B8A8_UNorm or B8G8R8A8_UNorm. (Reproducible on URP)
                    bool isSRGBRead = !blitParam.sourceTexture.isDataSRGB &&
                                          (blitParam.sourceTexture.graphicsFormat == GraphicsFormat.R8G8B8A8_UNorm ||
                                           blitParam.sourceTexture.graphicsFormat == GraphicsFormat.B8G8R8A8_UNorm);

                    if (isSRGBRead)
                    {
                        if (sourceColorGamut < 0)
                        {
                            sourceColorGamut = (int)ColorGamut.sRGB;
                        }
                    }
                }
            }

            s_MaterialProperty.SetTexture(k_MainTex, blitParam.sourceTexture);
            s_MaterialProperty.SetInteger(k_MainTexArraySlice, blitParam.sourceTextureArraySlice);
            s_MaterialProperty.SetVector(k_SourceRect, sourceRect);
            s_MaterialProperty.SetVector(k_DestRect, destRect);

            s_MaterialProperty.SetInteger(k_SourceColorGamut, sourceColorGamut);
            s_MaterialProperty.SetFloat(k_SourceNitsForPaperWhite, blitParam.sourceHdrParams.nitsForPaperWhite);
            s_MaterialProperty.SetFloat(k_SourceMaxDisplayNits, blitParam.sourceHdrParams.maxDisplayNits);

            s_MaterialProperty.SetInteger(k_ColorGamut, colorGamut);
            s_MaterialProperty.SetFloat(k_NitsForPaperWhite, targetParams.hdrParams.nitsForPaperWhite);
            s_MaterialProperty.SetFloat(k_MaxDisplayNits, targetParams.hdrParams.maxDisplayNits);

            switch (blitParam.alphaMode)
            {
                case AlphaMode.Alpha:
                    SetBlend(s_MaterialProperty, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, BlendMode.One, BlendMode.OneMinusSrcAlpha);
                    break;
                case AlphaMode.Premultiply:
                    SetBlend(s_MaterialProperty, BlendMode.One, BlendMode.OneMinusSrcAlpha, BlendMode.One, BlendMode.OneMinusSrcAlpha);
                    break;
                default:
                    SetBlend(s_MaterialProperty, BlendMode.One, BlendMode.Zero, BlendMode.One, BlendMode.Zero);
                    break;
            }

            if (targetParams.renderTarget != null)
                cmd.SetRenderTarget(targetParams.renderTarget);

#if UNITY_RENDER_PIPELINES_CORE
            bool isEnabledFoveatedRendering = XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster) && blitParam.foveatedRenderingInfo != IntPtr.Zero;
            if (isEnabledFoveatedRendering)
            {
                cmd.ConfigureFoveatedRendering(blitParam.foveatedRenderingInfo);
                cmd.EnableShaderKeyword("_FOVEATED_RENDERING_NON_UNIFORM_RASTER");
            }
            else
            {
                cmd.DisableShaderKeyword("_FOVEATED_RENDERING_NON_UNIFORM_RASTER");
            }
#endif

            if (blitParam.sourceTexture.dimension != TextureDimension.Tex2DArray)
                cmd.EnableShaderKeyword("COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY");

            cmd.DrawProcedural(Matrix4x4.identity, mat, 0, MeshTopology.Quads, 4, 1, s_MaterialProperty);

            if (blitParam.sourceTexture.dimension != TextureDimension.Tex2DArray)
                cmd.DisableShaderKeyword("COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY");

#if UNITY_RENDER_PIPELINES_CORE
            if (isEnabledFoveatedRendering)
                cmd.DisableShaderKeyword("_FOVEATED_RENDERING_NON_UNIFORM_RASTER");
#endif

            if (targetParams.renderTarget != null)
                cmd.SetRenderTarget(-1);
        }

        static void SetBlend(MaterialPropertyBlock materialProperty, BlendMode srcBlend, BlendMode dstBlend, BlendMode alphaSrcBlend, BlendMode alphaDstBlend)
        {
            materialProperty.SetInt(k_SrcBlend, (int)srcBlend);
            materialProperty.SetInt(k_DstBlend, (int)dstBlend);
            materialProperty.SetInt(k_AlphaSrcBlend, (int)alphaSrcBlend);
            materialProperty.SetInt(k_AlphaDstBlend, (int)alphaDstBlend);
        }

        static bool ShouldYFlipTexture(Texture texture)
        {
            if (texture == null)
                return false;

            var sourceTextureRT = texture as RenderTexture;
            if (sourceTextureRT == null)
                return false;

            if ((sourceTextureRT.descriptor.flags & RenderTextureCreationFlags.AllowVerticalFlip) == 0 && SystemInfo.graphicsUVStartsAtTop)
                return true;

            return false;
        }

        static Material InitializeMaterial()
        {
            if (s_Material != null)
                return s_Material;

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"{ShaderName} isn't found.");
                return null;
            }

            s_Material = new Material(shader);
            return s_Material;
        }

        /// <summary>
        /// Blits the mirror view using the specified parameters.
        /// </summary>
        /// <param name="cmd">The command buffer to use.</param>
        /// <param name="mirrorViewParams">Parameters for the mirror view blit operation.</param>
        public static void BlitMirrorView(CommandBuffer cmd, in MirrorViewParams mirrorViewParams)
        {
            if (cmd == null || mirrorViewParams.displaySubsystem == null || mirrorViewParams.mainCamera == null || mirrorViewParams.mirrorViewCamera == null || mirrorViewParams.blitMode == XRMirrorViewBlitMode.None)
            {
                return;
            }

            RenderTexture tempRT = null;
#if UNITY_EDITOR
            // Note: The screen size for tempRT = null(back buffer) in Editor will be full resolution, not game view. This workaround avoids this issue.
            tempRT = GetTempRenderTexture(mirrorViewParams.mirrorViewCamera.pixelWidth, mirrorViewParams.mirrorViewCamera.pixelHeight);
#endif

            if (!mirrorViewParams.displaySubsystem.GetMirrorViewBlitDesc(tempRT, out var blitDesc, mirrorViewParams.blitMode))
            {
                return;
            }

            bool viewportEnabled = IsViewportEnabled(blitDesc);

            if (mirrorViewParams.renderPipeline == RenderPipeline.HighDefinition)
            {
                cmd.EnableShaderKeyword("COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL");
            }

            for (int blitParamIndex = 0; blitParamIndex < blitDesc.blitParamsCount; ++blitParamIndex)
            {
                int stereoEyeIndex = GetStereoEyeIndex(mirrorViewParams.blitMode, blitParamIndex);

                blitDesc.GetBlitParameter(blitParamIndex, out XRDisplaySubsystem.XRBlitParams blitParameter);

                if (blitParameter.srcTex == null)
                    continue;

                if (viewportEnabled)
                    EnableViewport(cmd, mirrorViewParams.mirrorViewCamera, blitParameter.destRect);

                float scaledCameraTargetWidth = (float)blitParameter.srcTex.width;
                float scaledCameraTargetHeight = (float)blitParameter.srcTex.height;
                if (mirrorViewParams.mainCamera.allowDynamicResolution)
                {
                    scaledCameraTargetWidth *= ScalableBufferManager.widthScaleFactor;
                    scaledCameraTargetHeight *= ScalableBufferManager.heightScaleFactor;
                }

                cmd.SetGlobalInteger("_CompositionLayers_StereoEyeIndex", stereoEyeIndex);
                cmd.SetGlobalVector("_CompositionLayers_ScreenSize", new Vector4(scaledCameraTargetWidth, scaledCameraTargetHeight, 1.0f / scaledCameraTargetWidth, 1.0f / scaledCameraTargetHeight));

                {
                    var renderParameter = GetRenderParameter(mirrorViewParams, stereoEyeIndex);
                    var view = renderParameter.view;
                    var projection = GetWindowMatrix(blitParameter.srcRect) * renderParameter.projection;

                    if (mirrorViewParams.renderPipeline == RenderPipeline.HighDefinition)
                    {
                        // Note: HDRP will overwrite view & projection matrices. (Use the contant buffer for optimization.)
                        projection = GL.GetGPUProjectionMatrix(projection, false);
                        var projectionParams = GetProjectionParams(projection);
                        cmd.SetGlobalMatrix("_CompositionLayers_ViewMatrix", view);
                        cmd.SetGlobalMatrix("_CompositionLayers_ProjectionMatrix", projection);
                        cmd.SetGlobalVector("_CompositionLayers_ProjectionParams", projectionParams);
                    }
                    else
                    {
                        cmd.SetViewProjectionMatrices(view, projection);
                    }
                }

                MirrorViewLayerProvider.Instance?.AddToCommandBuffer(cmd, MirrorViewLayerProvider.LayerOrderType.Underlay);

                // Rendering mirror view.
                {
                    var blitParams = new BlitParams(blitParameter.srcTex, blitParameter.srcTexArraySlice);

                    blitParams.sourceRect = blitParameter.srcRect;
                    blitParams.destRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

                    blitParams.sourceHdrParams = mirrorViewParams.displaySubsystem.hdrOutputSettings;
                    blitParams.foveatedRenderingInfo = blitParameter.foveatedRenderingInfo;

                    blitParams.alphaMode = mirrorViewParams.alphaMode;
                    blitParams.renderPipeline = mirrorViewParams.renderPipeline;

                    Blit(cmd, blitParams, mirrorViewParams.mirrorViewCamera.targetDisplay);
                }

                MirrorViewLayerProvider.Instance?.AddToCommandBuffer(cmd, MirrorViewLayerProvider.LayerOrderType.Overlay);
            }

            if (mirrorViewParams.renderPipeline == RenderPipeline.HighDefinition)
            {
                cmd.DisableShaderKeyword("COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL");
            }

            if (viewportEnabled)
                DisableViewport(cmd, mirrorViewParams.mirrorViewCamera);
        }

#if UNITY_EDITOR
        static RenderTexture s_CachedTempRenderTexture;

        static RenderTexture GetTempRenderTexture(int width, int height)
        {
            if (s_CachedTempRenderTexture != null && s_CachedTempRenderTexture.width == width && s_CachedTempRenderTexture.height == height)
                return s_CachedTempRenderTexture;

            if (s_CachedTempRenderTexture != null)
                RenderTexture.Destroy(s_CachedTempRenderTexture);

            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0, 1, RenderTextureReadWrite.Default);
            var renderTexture = new RenderTexture(desc);
            renderTexture.Create();
            s_CachedTempRenderTexture = renderTexture;
            return s_CachedTempRenderTexture;
        }
#endif

        static Vector4 GetProjectionParams(in Matrix4x4 projectionMatrix)
        {
            var invProjMatrix = projectionMatrix.inverse;
            bool flipProj = invProjMatrix.MultiplyPoint(new Vector3(0.0f, 1.0f, 0.0f)).y < 0.0f;
            return new Vector4(flipProj ? -1.0f : 1.0f, 0.0f, 0.0f, 0.0f); // projectionParams.yzw aren't used yet.
        }

        static XRDisplaySubsystem.XRRenderParameter GetRenderParameter(in MirrorViewParams mirrorViewParams, int stereoEyeIndex)
        {
            int blitParamIndex = 0;
            int renderPassCount = mirrorViewParams.displaySubsystem.GetRenderPassCount();
            for (int renderPassIndex = 0; renderPassIndex < renderPassCount; ++renderPassIndex)
            {
                mirrorViewParams.displaySubsystem.GetRenderPass(renderPassIndex, out var renderPass);
                for (int renderParameterIndex = 0, renderParameterCount = renderPass.GetRenderParameterCount(); renderParameterIndex < renderParameterCount; ++renderParameterIndex, ++blitParamIndex)
                {
                    if (blitParamIndex == stereoEyeIndex)
                    {
                        renderPass.GetRenderParameter(mirrorViewParams.mainCamera, renderParameterIndex, out var renderParameter);
                        return renderParameter;
                    }
                }
            }

            return new XRDisplaySubsystem.XRRenderParameter();
        }

        static int GetStereoEyeIndex(int blitMode, int blitParamIndex)
        {
            if (blitMode == XRMirrorViewBlitMode.LeftEye)
                return 0;
            if (blitMode == XRMirrorViewBlitMode.RightEye)
                return 1;

            return blitParamIndex;
        }

        static void EnableViewport(CommandBuffer cmd, Camera camera, Rect viewport)
        {
            cmd.SetViewport(GetPixelRect(camera, viewport));
        }

        static void DisableViewport(CommandBuffer cmd, Camera camera)
        {
            cmd.SetViewport(GetPixelRect(camera));
        }

        static bool IsViewportEnabled(in XRDisplaySubsystem.XRMirrorViewBlitDesc blitDesc)
        {
            for (int blitParamIndex = 0; blitParamIndex < blitDesc.blitParamsCount; ++blitParamIndex)
            {
                blitDesc.GetBlitParameter(blitParamIndex, out var blitParam);
                if (IsViewportEnabled(blitParam.destRect))
                    return true;
            }

            return false;
        }

        static bool IsViewportEnabled(Rect rect)
        {
            if (Mathf.Approximately(rect.x, 0.0f) &&
                Mathf.Approximately(rect.y, 0.0f) &&
                Mathf.Approximately(rect.width, 1.0f) &&
                Mathf.Approximately(rect.height, 1.0f))
                return false;
            else
                return true;
        }

        static Rect GetPixelRect(Camera camera)
        {
            var pixelRect = camera.pixelRect;
            pixelRect.width = camera.pixelWidth;
            pixelRect.height = camera.pixelHeight;
            return camera.pixelRect;
        }

        static Rect GetPixelRect(Camera camera, Rect rect)
        {
            var pixelRect = camera.pixelRect;
            pixelRect.width = camera.pixelWidth;
            pixelRect.height = camera.pixelHeight;
            return new Rect(rect.x * pixelRect.width + pixelRect.x, rect.y * pixelRect.height + pixelRect.y, rect.width * pixelRect.width, rect.height * pixelRect.height);
        }

        static Matrix4x4 GetWindowMatrix(Rect viewport)
        {
            float viewportCenterX = (viewport.x + viewport.width * 0.5f) - 0.5f;
            float viewportCenterY = (viewport.y + viewport.height * 0.5f) - 0.5f;
            float windowW = SafeDiv(1.0f, viewport.width);
            float windowH = SafeDiv(1.0f, viewport.height);
            float windowX = (-viewportCenterX * 2.0f) * windowW;
            float windowY = (viewportCenterY * 2.0f) * windowH;

            return Matrix4x4.TRS(new Vector3(windowX, windowY, 0.0f), Quaternion.identity, new Vector3(windowW, windowH, 1.0f));
        }

        static float SafeDiv(float x, float y)
        {
            return (y > 1e-7f || y < -1e-7f) ? x / y : 0.0f;
        }
    }
}
