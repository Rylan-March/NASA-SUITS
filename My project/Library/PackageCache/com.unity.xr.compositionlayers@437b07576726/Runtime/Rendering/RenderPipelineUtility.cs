using UnityEngine.Rendering;

namespace Unity.XR.CompositionLayers.Rendering
{
    /// <summary>
    /// Provides utility methods for determining the active render pipeline.
    /// </summary>
    public static class RenderPipelineUtility
    {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
        /// <summary>
        /// Checks if the current render pipeline is the Universal Render Pipeline (URP).
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current render pipeline is URP. Otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsUniversalRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
                return false;
#if UNITY_2023_2_OR_NEWER
            return typeof(UnityEngine.Rendering.Universal.UniversalRenderPipeline).IsAssignableFrom(GraphicsSettings.currentRenderPipeline.pipelineType);
#else
            return GraphicsSettings.currentRenderPipeline.renderPipelineShaderTag == "UniversalPipeline";
#endif
        }
#endif

#if UNITY_RENDER_PIPELINES_HDRENDER
        /// <summary>
        /// Checks if the current render pipeline is the High Definition Render Pipeline (HDRP).
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the current render pipeline is HDRP. Otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsHDRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
                return false;
#if UNITY_2023_2_OR_NEWER
            return typeof(UnityEngine.Rendering.HighDefinition.HDRenderPipeline).IsAssignableFrom(GraphicsSettings.currentRenderPipeline.pipelineType);
#else
            return GraphicsSettings.currentRenderPipeline.renderPipelineShaderTag == "HDRenderPipeline";
#endif
        }
#endif
    }
}
