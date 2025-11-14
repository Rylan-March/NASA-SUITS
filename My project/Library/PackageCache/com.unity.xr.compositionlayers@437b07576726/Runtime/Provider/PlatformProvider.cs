using System;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Provider
{
    /// <summary>
    /// The default implementation that defines the API for an PlatformProvider.
    /// </summary>
    public abstract class PlatformProvider
    {
        /// <summary>
        /// HDR Params.
        /// </summary>
        public struct HDRParams
        {
            /// <summary>
            /// The color gamut used for HDR.
            /// </summary>
            public ColorGamut ColorGamut;

            /// <summary>
            /// The nits value representing paper white in HDR.
            /// </summary>
            public float NitsForPaperWhite;

            /// <summary>
            /// The maximum display nits for HDR.
            /// </summary>
            public float MaxDisplayNits;

            /// <summary>
            /// Gets the default HDR parameters.
            /// </summary>
            /// <returns>The default <see cref="HDRParams"/> instance.</returns>
            public static HDRParams GetDefault()
            {
                return new HDRParams() { ColorGamut = ColorGamut.sRGB };
            }
        }

        /// <summary>
        /// Default coordinate system string.
        /// </summary>
        public const string DefaultCoordinateSystem = "World";

        static readonly BlendType[] s_SupportedBlendTypes = new BlendType[] { BlendType.Alpha, BlendType.Premultiply };
        static readonly string[] s_SupportedCoordinateSystems = new string[] { DefaultCoordinateSystem };

        /// <summary>
        /// Supporting XRLoader.
        /// </summary>
        public abstract Type XRLoaderType { get; }

        /// <summary>
        /// Supporting LayerProvider.
        /// </summary>
        public abstract Type LayerProviderType { get; }

        /// <summary>
        /// Supported LayerData types. null means all supported.
        /// </summary>
        public virtual Type[] SupportedLayerDataTypes { get => null; }

        /// <summary>
        /// PlatformLayerData type. This property is null if it's unsupported.
        /// </summary>
        public virtual Type PlatformLayerDataType { get => null; }

        /// <summary>
        /// Supported BlendType values. null means all supported.
        /// </summary>
        public virtual BlendType[] SupportedBlendTypes { get => s_SupportedBlendTypes; }

        /// <summary>
        /// Supports underlay layers.
        /// </summary>
        public virtual bool IsSupportedUnderlayLayers { get => true; }

        /// <summary>
        /// Supports HDR on the target platform.
        /// </summary>
        /// <remarks>
        /// This information is referenced from HDRTonemapping. If this value is false, HDRTonemapping is disabled on target platform.
        /// </remarks>
        public virtual bool IsSupportedHDR { get => false; }

        /// <summary>
        /// Prefered HDR Params on the target platform. (Optional)
        /// </summary>
        /// <remarks>
        /// This information is referenced from HDRTonemapping. This class can give hints to set proper settings.
        /// </remarks>
        /// <returns> New HDR parameters with default settings. </returns>
        public virtual HDRParams GetPreferredHDRParams()
        {
            return HDRParams.GetDefault();
        }

        /// <summary>
        /// Supported coordinate system names.
        /// </summary>
        public virtual string[] SupportedCoordinateSystems { get => s_SupportedCoordinateSystems; }

        /// <summary>
        /// Get selected coordinate system name for the target layer.
        /// </summary>
        /// <param name="compositionLayer">Target composition layer.</param>
        /// <returns>Coordinate system name. This name matches any of the SupportedCoordinateSystems.</returns>
        public virtual string GetSelectedCoordinateSystem(CompositionLayer compositionLayer)
        {
            return DefaultCoordinateSystem;
        }

        /// <summary>
        /// Check the target layer whether to use a custom transform.
        /// </summary>
        /// <param name="compositionLayer">Target composition layer.</param>
        /// <returns>The flag whether custom transforming is supported.</returns>
        public virtual bool IsEnabledCustomTransform(CompositionLayer compositionLayer)
        {
            return false;
        }

        /// <summary>
        /// Get custom transform data from the target layer.
        /// </summary>
        /// <param name="compositionLayer">Target composition layer.</param>
        /// <param name="cameraData">Camera data for custom transforming.</param>
        /// <returns>Custom transform data. It contains matrix and matrixType.</returns>
        public virtual CustomTransformData GetCustomTransformData(CompositionLayer compositionLayer, CustomTransformCameraData cameraData)
        {
            return CustomTransformData.Default;
        }
    }

    /// <summary>
    /// Default PlatformProvider implementation for generic  platforms.
    /// </summary>
    internal class DefaultPlatformProvider : PlatformProvider
    {
        Type m_LayerProviderType;

        /// <summary>
        /// Default constructor. This functions is used only for emulated.
        /// </summary>
        public DefaultPlatformProvider()
            : this(null)
        { }

        /// <summary>
        /// Constuctor with the type of ILayerProvider.
        /// </summary>
        /// <param name="layerProviderType">Type of class inherited from ILayerProvider</param>
        public DefaultPlatformProvider(Type layerProviderType)
        {
            m_LayerProviderType = layerProviderType;
        }

        /// <summary>
        /// Supporting XRLoader.
        /// </summary>
        public override Type XRLoaderType { get => null; }

        /// <summary>
        /// Supporting LayerProvider.
        /// </summary>
        public override Type LayerProviderType { get => m_LayerProviderType; }
    }
}
