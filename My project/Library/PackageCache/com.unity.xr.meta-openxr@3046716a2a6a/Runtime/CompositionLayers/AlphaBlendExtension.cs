using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.XR.CompositionLayers.Provider;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions
{
    /// <summary>
    /// Subclass of <see cref="CompositionLayerExtension" /> to support
    /// color scaling and biasing for the <see cref="CompositionLayer"/> instance
    /// on the same GameObject.
    ///
    /// Support for this component is up the instance of <see cref="ILayerProvider" />
    /// currently assigned to the <see cref="Unity.XR.CompositionLayers.Services.CompositionLayerManager" />.
    ///
    /// If this extension is not added to a layer GameObject, the provider
    /// will not apply color scale/bias.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    class AlphaBlendExtension : CompositionLayerExtension
    {
        const uint XR_TYPE_COMPOSITION_LAYER_ALPHA_BLEND_FB = 1000041001;

        /// <summary>
        /// Options for which type of object this extension should be associated with.
        /// </summary>
        public override ExtensionTarget Target => ExtensionTarget.Layer;

        NativeArray<Native.XrCompositionLayerAlphaBlendFB> m_NativeArray;

        ///<summary>
        /// Return a pointer to this extension's native struct.
        /// </summary>
        /// <returns>the pointer to colorScaleBias extension's native struct.</returns>
        public override unsafe void* GetNativeStructPtr()
        {
            var openXRStruct = new Native.XrCompositionLayerAlphaBlendFB(
                XR_TYPE_COMPOSITION_LAYER_ALPHA_BLEND_FB,
                null, Native.XrBlendFactorFB.XR_BLEND_FACTOR_ONE_FB,
                Native.XrBlendFactorFB.XR_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA_FB,
                Native.XrBlendFactorFB.XR_BLEND_FACTOR_SRC_ALPHA_FB,
                Native.XrBlendFactorFB.XR_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA_FB);

            if (!m_NativeArray.IsCreated)
                m_NativeArray = new NativeArray<Native.XrCompositionLayerAlphaBlendFB>(1, Allocator.Persistent);

            m_NativeArray[0] = openXRStruct;
            return m_NativeArray.GetUnsafePtr();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_NativeArray.IsCreated)
                m_NativeArray.Dispose();
        }

        static class Native
        {
            [StructLayout(LayoutKind.Sequential)]
            internal unsafe struct XrCompositionLayerAlphaBlendFB
            {
                public XrCompositionLayerAlphaBlendFB(uint type, void* next, XrBlendFactorFB srcFactorColor, XrBlendFactorFB dstFactorColor, XrBlendFactorFB srcFactorAlpha, XrBlendFactorFB dstFactorAlpha)
                {
                    this.type = type;
                    this.next = next;
                    this.srcFactorColor = srcFactorColor;
                    this.dstFactorColor = dstFactorColor;
                    this.srcFactorAlpha = srcFactorAlpha;
                    this.dstFactorAlpha = dstFactorAlpha;
                }

                uint type;
                void* next;
                XrBlendFactorFB srcFactorColor;
                XrBlendFactorFB dstFactorColor;
                XrBlendFactorFB srcFactorAlpha;
                XrBlendFactorFB dstFactorAlpha;
            }

            internal enum XrBlendFactorFB
            {
                XR_BLEND_FACTOR_ZERO_FB = 0,
                XR_BLEND_FACTOR_ONE_FB = 1,
                XR_BLEND_FACTOR_SRC_ALPHA_FB = 2,
                XR_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA_FB = 3,
                XR_BLEND_FACTOR_DST_ALPHA_FB = 4,
                XR_BLEND_FACTOR_ONE_MINUS_DST_ALPHA_FB = 5,
                XR_BLEND_FACTOR_MAX_ENUM_FB = 0x7FFFFFFF
            }
        }
    }
}
