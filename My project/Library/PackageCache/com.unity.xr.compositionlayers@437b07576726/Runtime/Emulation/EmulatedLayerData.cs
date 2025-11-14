using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.XR.CompositionLayers.Emulation
{
    /// <summary>
    /// Base class for emulating <see cref="LayerData"/> objects. To have a <see cref="LayerData"/> object emulated in
    /// the Editor every <see cref="LayerData"/> must have a corresponding EmulatedLayerData class with a
    /// <see cref="EmulatedLayerDataTypeAttribute"/>.
    /// Class for rendering emulated layer data with a command buffer. The EmulatedLayerProvider handles sorting
    /// and managing the command buffers applied to the cameras.
    /// </summary>
    internal abstract class EmulatedLayerData : IDisposable
    {
        public const string k_CustomRectsMaterialKeyword = "CUSTOM_RECTS_ON";
        public const string k_SourceTextureMaterialKeyword = "SOURCE_TEXTURE_ON";
        public const string k_ColorScaleMaterialKeyword = "COLOR_SCALE_BIAS_ON";

        public static readonly int k_MainTex = Shader.PropertyToID("_MainTex");
        static readonly int k_Cubemap = Shader.PropertyToID("_Cubemap");
        static readonly int k_ColorScale = Shader.PropertyToID("_ColorScale");
        static readonly int k_ColorBias = Shader.PropertyToID("_ColorBias");
        static readonly int k_SourceRect = Shader.PropertyToID("_SourceRect");
        static readonly int k_DestRect = Shader.PropertyToID("_DestRect");
        static readonly int k_SourceNitsForPaperWhite = Shader.PropertyToID("_SourceNitsForPaperWhite");
        static readonly int k_SourceColorGamut = Shader.PropertyToID("_SourceColorGamut");
        static readonly int k_SourceMaxDisplayNits = Shader.PropertyToID("_SourceMaxDisplayNits");
        static readonly int k_NitsForPaperWhite = Shader.PropertyToID("_NitsForPaperWhite");
        static readonly int k_ColorGamut = Shader.PropertyToID("_ColorGamut");
        static readonly int k_MaxDisplayNits = Shader.PropertyToID("_MaxDisplayNits");
        protected static readonly int k_TransformMatrix = Shader.PropertyToID("_TransformMatrix");
        protected static readonly int k_TransformMatrixType = Shader.PropertyToID("_TransformMatrixType");

        protected CommandBufferTemp m_CommandBufferTemp;
        protected CommandBufferTemp m_CommandBufferTempSceneView;

        bool m_IsInitialized;
        EmulatedCompositionLayer m_EmulatedCompositionLayer;
        private Texture m_ExternalTexturePlaceholder;
        private string m_TypeName = string.Empty;

        //------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Is the <see cref="EmulatedLayerData"/> initialized and ready to use.
        /// </summary>
        public bool IsInitialized
        {
            get => m_IsInitialized;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Is the emulation of the <see cref="LayerData"/> supported with your current configuration on a given
        /// instance of <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera"><see cref="Camera"/> instance to check if the rendering of the
        /// <see cref="EmulatedLayerData"/></param> is supported on.
        /// <returns><c>true</c> if emulation the <see cref="LayerData"/> type is supported in your configuration.</returns>
        public abstract bool IsSupported(Camera camera);

        //------------------------------------------------------------------------------------------------------------------------------------------------
        // Accessors
        //------------------------------------------------------------------------------------------------------------------------------------------------

        protected CompositionLayer CompositionLayer
        {
            get => m_EmulatedCompositionLayer?.CompositionLayer;
        }

        protected LayerData LayerData
        {
            get
            {
                if (m_EmulatedCompositionLayer?.CompositionLayer == null)
                    return null;

                return m_EmulatedCompositionLayer.CompositionLayer.LayerData;
            }
        }

        protected Transform Transform
        {
            get
            {
                if (m_EmulatedCompositionLayer?.CompositionLayer == null)
                    return null;

                return m_EmulatedCompositionLayer.CompositionLayer.transform;
            }
        }

        protected List<CompositionLayerExtension> LayerExtensions
        {
            get => m_EmulatedCompositionLayer?.LayerExtensions;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the shader for layer rendering.
        /// </summary>
        Shader GetShader()
        {
            var shader = Shader.Find(CompositionLayerConstants.UberShader);
            if (shader == null)
            {
                Debug.LogWarning($"Can't find {CompositionLayerConstants.UberShader}. Treat this layer as invisible.");
            }

            return shader;
        }

        /// <summary>
        /// Get the layer type keyword for Uber.shader.
        /// This function will be called only once for creating material.
        /// </summary>
        /// <returns>Shader keyword for layer type. See also Uber.shader. null means undrawing layer.</returns>
        protected abstract string GetShaderLayerTypeKeyword();

        /// <summary>
        /// Material instance used to blit the emulated composition layer to the camera.
        /// </summary>
        protected Material EmulationMaterial;

        //------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Used to set the initial values and setup for the <see cref="EmulatedLayerData"/> so it can be used in the
        /// Emulated Composition Layer rendering.
        /// </summary>
        protected internal void InitializeLayerData(EmulatedCompositionLayer emulatedCompositionLayer)
        {
            if (emulatedCompositionLayer?.CompositionLayer?.LayerData == null)
            {
                return;
            }

            m_EmulatedCompositionLayer = emulatedCompositionLayer;
            m_TypeName = GetType().Name;
            m_IsInitialized = true;

            var layerType = GetShaderLayerTypeKeyword();
            if (layerType != null)
            {
                var shader = GetShader();
                if (shader != null)
                {
                    EmulationMaterial = new Material(shader);
                    EmulationMaterial.EnableKeyword(layerType);
                    EmulationMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    EmulationMaterial.SetMatrix(k_TransformMatrix, Matrix4x4.identity);
                }
            }
        }

        /// <summary>
        /// Updates this instance's <see cref="EmulatedLayerData"/> with values from the <paramref name="layer"/>.
        /// </summary>
        /// <param name="layer">The source <see cref="CompositionLayer"/> object for this instance.</param>
        /// <param name="extensions">The <see cref="CompositionLayerExtension"/> components on the <paramref name="layer"/> object.</param>
        protected internal virtual void UpdateEmulatedLayerData()
        {
            m_CommandBufferTemp.IsInvalidated = true;
            m_CommandBufferTempSceneView.IsInvalidated = true;
            UpdateMaterial();
        }

        void UpdateMaterial()
        {
            var material = EmulationMaterial;
            if (material == null)
            {
                return;
            }

            var layerData = LayerData;
            if (layerData != null)
            {
                switch (layerData.BlendType)
                {
                    case BlendType.Alpha:
                        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        material.SetInt("_AlphaDstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        break;
                    case BlendType.Premultiply:
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        material.SetInt("_AlphaDstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        break;
                    case BlendType.Additive:
                        material.SetInt("_SrcBlend", (int)BlendMode.One);
                        material.SetInt("_DstBlend", (int)BlendMode.One);
                        material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        material.SetInt("_AlphaDstBlend", (int)BlendMode.One);
                        break;
                    default:
                        break;
                }
            }

            var layerExtensions = LayerExtensions;
            var isSupportedHDR = PlatformManager.IsSupportedHDR; // for supporting the pass-thtough rendering.
            var hasHDRTonemappingExtension = false;
            if (layerExtensions != null)
            {
                var hasTexture = false;
                var hasCustomRects = false;
                var hasColorScaleBiasExtension = false;

                foreach (var extension in CompositionLayer.Extensions)
                {
                    switch (extension)
                    {
                        case TexturesExtension texturesExtension:
                        {
                            int eyeIndex = 0;
                            Texture texture = null;
#if UNITY_EDITOR
                            if (texturesExtension.sourceTexture == TexturesExtension.SourceTextureEnum.AndroidSurface)
                            {
                                var layerDataType = CompositionLayer.LayerData.GetType();
                                if ((layerDataType == typeof(QuadLayerData)) ||
                                    (layerDataType == typeof(CylinderLayerData)))
                                {
                                    string externalTexImagePath = "Packages/com.unity.xr.compositionlayers/Editor/Textures/AndroidSurfacePlaceholder.png";
                                    m_ExternalTexturePlaceholder = AssetDatabase.LoadAssetAtPath<Texture2D>(externalTexImagePath);
                                    texture = m_ExternalTexturePlaceholder;
                                    hasTexture = texture != null;
                                    if (hasTexture)
                                        EmulationMaterial.SetTexture(k_MainTex, texture);
                                    break;
                                }

                                if (texturesExtension.CustomRects)
                                {
                                    EmulationMaterial.SetVector(k_SourceRect, ToVector4YFlipped(eyeIndex == 0 ? texturesExtension.LeftEyeSourceRect : texturesExtension.RightEyeSourceRect));
                                    EmulationMaterial.SetVector(k_DestRect, ToVector4YFlipped(eyeIndex == 0 ? texturesExtension.LeftEyeDestinationRect : texturesExtension.RightEyeDestinationRect));
                                    hasCustomRects = true;
                                }

                                break;
                            }
#endif
                            if ((texturesExtension.RightTexture != null) && (texturesExtension.InEditorEmulation == 1) &&
                                (CompositionLayer.LayerData.GetType() == typeof(ProjectionLayerData)))
                            {
                                eyeIndex = 1;
                                texture = texturesExtension.RightTexture;
                            }
                            else if (texturesExtension.LeftTexture != null)
                            {
                                eyeIndex = 0;
                                texture = texturesExtension.LeftTexture;
                            }

                            hasTexture = texture != null;
                            if (texture != null)
                            {
                                if (GetShaderLayerTypeKeyword() == "COMPOSITION_LAYERTYPE_CUBEMAP" && texture is Cubemap)
                                {
                                    EmulationMaterial.SetTexture(k_Cubemap, texture);
                                }
                                else if (texture is not Cubemap)
                                {
                                    EmulationMaterial.SetTexture(k_MainTex, texture);
                                }
                            }

                            if (texturesExtension.CustomRects)
                            {
                                var srcRect = eyeIndex == 0 ? texturesExtension.LeftEyeSourceRect : texturesExtension.RightEyeSourceRect;
                                var dstRect = eyeIndex == 0 ? texturesExtension.LeftEyeDestinationRect : texturesExtension.RightEyeDestinationRect;

                                EmulationMaterial.SetVector(k_SourceRect, ToVector4YFlipped(srcRect));
                                EmulationMaterial.SetVector(k_DestRect, ToVector4YFlipped(dstRect));
                                hasCustomRects = true;
                            }

                            break;
                        }

                        case ColorScaleBiasExtension colorScaleBiasExtension:
                            material.SetVector(k_ColorScale, ToColor(colorScaleBiasExtension.Scale));
                            material.SetVector(k_ColorBias, ToColor(colorScaleBiasExtension.Bias));
                            hasColorScaleBiasExtension = true;
                            break;

                        case HDRTonemappingExtension hdrTonemappingExtension:
                            if (isSupportedHDR && hdrTonemappingExtension.enabled) // Need to check isSupportedHDR for the pass-through rendring.
                            {
                                material.SetInteger(k_SourceColorGamut, (int)hdrTonemappingExtension.ColorGamut);
                                material.SetFloat(k_SourceNitsForPaperWhite, hdrTonemappingExtension.NitsForPaperWhite);
                                material.SetFloat(k_SourceMaxDisplayNits, hdrTonemappingExtension.MaxDisplayNits);
                                hasHDRTonemappingExtension = true;
                            }
                            break;
                    }
                }

                EnableMaterialKeyword(material, k_CustomRectsMaterialKeyword, hasCustomRects);
                EnableMaterialKeyword(material, k_SourceTextureMaterialKeyword, hasTexture);
                EnableMaterialKeyword(material, k_ColorScaleMaterialKeyword, hasColorScaleBiasExtension);
            }

            if (!hasHDRTonemappingExtension)
            {
                material.SetInteger(k_SourceColorGamut, -1);
                material.SetFloat(k_SourceNitsForPaperWhite, 160.0f);
                material.SetFloat(k_SourceMaxDisplayNits, 160.0f);
            }

            HDROutputSettings mainDisplayHdrSettings = HDROutputSettings.main;
            if (isSupportedHDR && mainDisplayHdrSettings.active) // Need to check isSupportedHDR for the pass-through rendering.
            {
                material.SetInteger(k_ColorGamut, (int)mainDisplayHdrSettings.displayColorGamut);
                material.SetFloat(k_NitsForPaperWhite, mainDisplayHdrSettings.paperWhiteNits);
                material.SetFloat(k_MaxDisplayNits, mainDisplayHdrSettings.maxToneMapLuminance);
            }
            else
            {
                material.SetInteger(k_ColorGamut, -1);
                material.SetFloat(k_NitsForPaperWhite, 160.0f);
                material.SetFloat(k_MaxDisplayNits, 160.0f);
            }
        }

        /// <summary>
        /// Used to clean up and remove any component or objects the <see cref="EmulatedLayerData"/> has created.
        /// </summary>
        public virtual void Dispose()
        {
            m_CommandBufferTemp.Release();
            m_CommandBufferTempSceneView.Release();

            if (EmulationMaterial != null)
                UnityObjectUtils.Destroy(EmulationMaterial);

            EmulationMaterial = null;

            m_EmulatedCompositionLayer = null;
            m_TypeName = string.Empty;
            m_IsInitialized = false;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        // Command Buffers
        //------------------------------------------------------------------------------------------------------------------------------------------------

        public struct CommandArgs
        {
            public bool IsSceneView;

            public CommandArgs(Camera camera)
            {
                IsSceneView = (camera.cameraType == CameraType.SceneView);
            }
        }

        protected struct CommandBufferTemp
        {
            public CommandBuffer CommandBuffer;
            public bool IsInvalidated;

            public bool IsRequiredToCreate
            {
                get
                {
                    return CommandBuffer == null || IsInvalidated;
                }
            }

            public CommandBuffer Create(string bufferName)
            {
                if (CommandBuffer == null)
                {
#if UNITY_RENDER_PIPELINES_CORE
                    CommandBuffer = CommandBufferPool.Get(bufferName);
#else
                    CommandBuffer = new CommandBuffer()
                    {
                        name = bufferName
                    };
#endif
                }
                else
                {
                    CommandBuffer.Clear();
                }

                IsInvalidated = false;
                return CommandBuffer;
            }

            public void Release()
            {
                if (CommandBuffer != null)
                {
#if UNITY_RENDER_PIPELINES_CORE
                    CommandBufferPool.Release(CommandBuffer);
#else
                    CommandBuffer.Release();
#endif
                }

                CommandBuffer = null;
                IsInvalidated = false;
            }
        }

        public struct RenderContext
        {
            public CommandBuffer m_CommandBuffer;
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
            public RasterCommandBuffer m_RasterCommandBuffer;
#endif

            public RenderContext(CommandBuffer commandBuffer)
            {
                m_CommandBuffer = commandBuffer;
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
                m_RasterCommandBuffer = null;
#endif
            }

            public static implicit operator RenderContext(CommandBuffer commandBuffer)
            {
                return new RenderContext(commandBuffer);
            }

#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
            public RenderContext(RasterCommandBuffer rasterCommandBuffer)
            {
                m_CommandBuffer = null;
                m_RasterCommandBuffer = rasterCommandBuffer;
            }

            public static implicit operator RenderContext(RasterCommandBuffer rasterCommandBuffer)
            {
                return new RenderContext(rasterCommandBuffer);
            }
#endif

            public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int subMeshIndex, int shaderPass, MaterialPropertyBlock properties)
            {
                if (m_CommandBuffer != null)
                    m_CommandBuffer.DrawMesh(mesh, matrix, material, subMeshIndex, shaderPass, properties);
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
                else if (m_RasterCommandBuffer != null)
                    m_RasterCommandBuffer.DrawMesh(mesh, matrix, material, subMeshIndex, shaderPass, properties);
#endif
            }

            public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor)
            {
                if (m_CommandBuffer != null)
                    m_CommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor);
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
                else if (m_RasterCommandBuffer != null)
                    m_RasterCommandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor);
#endif
            }
        }

        protected abstract void PrepareCommands();

        protected abstract void AddCommands(RenderContext renderContext, CommandArgs commandArgs);

        /// <summary>
        /// Update the <see cref="RenderContext"/> that is used for rendering the <see cref="EmulatedLayerData"/> for
        /// the <paramref name="camera"/>.
        /// </summary>
        /// <param name="camera">The <see cref="Camera"/> that the command buffer is being setup for.</param>
        public CommandBuffer UpdateCommandBuffer(CommandArgs commandArgs)
        {
#if UNITY_EDITOR
            if (commandArgs.IsSceneView)
            {
                return UpdateCommandBufferInternal(ref m_CommandBufferTempSceneView, commandArgs);
            }
#endif
            return UpdateCommandBufferInternal(ref m_CommandBufferTemp, commandArgs);
        }

        CommandBuffer UpdateCommandBufferInternal(ref CommandBufferTemp commandBufferTemp, CommandArgs commandArgs)
        {
            PrepareCommands();

            if (commandBufferTemp.IsRequiredToCreate)
            {
                var commandBuffer = commandBufferTemp.Create(m_TypeName);
                AddCommands(commandBuffer, commandArgs);
            }

            return commandBufferTemp.CommandBuffer;
        }

        public void AddToCommandBuffer(RenderContext renderContext, CommandArgs commandArgs)
        {
#if UNITY_EDITOR
            // Skip rendering if the layer is hidden in the scene view.
            if (SceneVisibilityManager.instance.IsHidden(CompositionLayer.gameObject) && commandArgs.IsSceneView)
                return;
#endif
            PrepareCommands();
            AddCommands(renderContext, commandArgs);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        // Internal utilities
        //------------------------------------------------------------------------------------------------------------------------------------------------

        protected static Vector4 ToVector4(Rect rect)
        {
            // To match the behaviour on RectsDrawer.
            return new Vector4(rect.x, rect.y, rect.width, rect.height);
        }

        protected static Vector4 ToVector4XYFlipped(Rect rect)
        {
            // To match the behaviour on RectsDrawer.
            return new Vector4(1.0f - (rect.x + rect.width), 1.0f - (rect.y + rect.height), rect.width, rect.height);
        }

        protected static Vector4 ToVector4YFlipped(Rect rect)
        {
            // To match the behaviour on RectsDrawer.
            return new Vector4(rect.x, 1.0f - (rect.y + rect.height), rect.width, rect.height);
        }

        protected static Color ToColor(Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        protected static void EnableMaterialKeyword(Material material, string keyword, bool enabled)
        {
            if (material == null)
            {
                Debug.LogWarning("Can not enable the material as it isn't created yet");
                return;
            }

            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
