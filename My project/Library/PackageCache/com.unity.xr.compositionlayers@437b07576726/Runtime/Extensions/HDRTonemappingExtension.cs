using System;
using UnityEngine;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Extensions
{
    /// <summary>
    /// Subclass of <see cref="CompositionLayerExtension" /> to support
    /// color scaling and biasing for the <see cref="CompositionLayer"/> instance
    /// on the same game object.
    ///
    /// Support for this component is up the the instance of <see cref="ILayerProvider" />
    /// currently assigned to the <see cref="Unity.XR.CompositionLayers.Services.CompositionLayerManager" />.
    ///
    /// If this extension is not added to a layer game object, it is expected that
    /// the provider will assume no color scale/bias is to be applied.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Composition Layers/Extensions/HDR Tonemapping")]
    [CompositionLayersHelpURL(typeof(HDRTonemappingExtension))]
    public class HDRTonemappingExtension : CompositionLayerExtension
    {
        [SerializeField]
        [Tooltip("The value used to color gamut for the source texture.")]
        ColorGamut m_ColorGamut = ColorGamut.sRGB;

        [SerializeField]
        [Tooltip("The value used to describe nits for paper white for the source texture.")]
        int m_NitsForPaperWhite = 160;

        [SerializeField]
        [Tooltip("The value used to describe max display nits for the source texture.")]
        int m_MaxDisplayNits = 160;

        /// <summary>
        /// Gets the target of the extension, which is a layer.
        /// </summary>
        public override ExtensionTarget Target { get { return ExtensionTarget.Layer; } }

        /// <summary>
        /// Retrieves a pointer to the native structure.
        /// </summary>
        /// <returns>A null pointer as this extension does not use a native structure.</returns>
        public override unsafe void* GetNativeStructPtr() { return null; }

        /// <summary>
        /// The value used to color gamut for the source texture.
        /// </summary>
        public ColorGamut ColorGamut
        {
            get => m_ColorGamut;
            set => m_ColorGamut = UpdateValue(m_ColorGamut, value);
        }

        /// <summary>
        /// The value used to describe nits for paper white for the source texture.
        /// </summary>
        public int NitsForPaperWhite
        {
            get => m_NitsForPaperWhite;
            set => m_NitsForPaperWhite = UpdateValue(m_NitsForPaperWhite, value);
        }

        /// <summary>
        /// The value used to describe max display nits for the source texture.
        /// </summary>
        public int MaxDisplayNits
        {
            get => m_MaxDisplayNits;
            set => m_MaxDisplayNits = UpdateValue(m_MaxDisplayNits, value);
        }
    }
}
