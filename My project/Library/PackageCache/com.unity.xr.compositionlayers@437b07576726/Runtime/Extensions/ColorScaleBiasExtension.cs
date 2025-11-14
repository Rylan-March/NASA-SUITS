using System;
using UnityEngine;
using Unity.XR.CompositionLayers.Provider;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

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
    [AddComponentMenu("XR/Composition Layers/Extensions/Color Scale and Bias")]
    [CompositionLayersHelpURL(typeof(ColorScaleBiasExtension))]
    public class ColorScaleBiasExtension : CompositionLayerExtension
    {
        const uint XR_KHR_composition_layer_color_scale_bias = 1000034000;

        /// <summary>
        /// Options for which type of object this extension should be associated with.
        /// </summary>
        public override ExtensionTarget Target => ExtensionTarget.Layer;

        [SerializeField]
        [Tooltip("The value used to scale a given color by.")]
        Vector4 m_Scale = Vector4.one;

        [SerializeField]
        [Tooltip("The value used to bias a given color by.")]
        Vector4 m_Bias = Vector4.zero;

        NativeArray<Native.XrCompositionLayerColorScaleBiasKHR> m_NativeArray;

        /// <summary>
        /// The value used to scale a given color by.
        /// </summary>
        public Vector4 Scale
        {
            get => m_Scale;
            set => m_Scale = UpdateValue(m_Scale, value);
        }

        /// <summary>
        /// The value used to bias a given color by.
        /// </summary>
        public Vector4 Bias
        {
            get => m_Bias;
            set => m_Bias = UpdateValue(m_Bias, value);
        }

        ///<summary>
        /// Return a pointer to this extension's native struct.
        /// </summary>
        /// <returns>the pointer to colorScaleBias extension's native struct.</returns>
        public override unsafe void* GetNativeStructPtr()
        {
            var openXRStruct = new Native.XrCompositionLayerColorScaleBiasKHR(XR_KHR_composition_layer_color_scale_bias, null, m_Scale, m_Bias);

            if (!m_NativeArray.IsCreated)
                m_NativeArray = new NativeArray<Native.XrCompositionLayerColorScaleBiasKHR>(1, Allocator.Persistent);

            m_NativeArray[0] = openXRStruct;
            return m_NativeArray.GetUnsafePtr();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_NativeArray.IsCreated)
                m_NativeArray.Dispose();
        }

        private static class Native
        {
            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct XrCompositionLayerColorScaleBiasKHR
            {
                public XrCompositionLayerColorScaleBiasKHR(uint type, void* next, Vector4 colorScale, Vector4 colorBias)
                {
                    this.type = type;
                    this.next = next;
                    this.colorScale = colorScale;
                    this.colorBias = colorBias;
                }

                private uint type;
                private void* next;
                private Vector4 colorScale;
                private Vector4 colorBias;
            }
        }
    }
}
