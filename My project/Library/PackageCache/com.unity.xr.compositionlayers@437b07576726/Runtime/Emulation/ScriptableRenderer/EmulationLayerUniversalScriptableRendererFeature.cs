#if UNITY_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;

namespace Unity.XR.CompositionLayers.Emulation
{
    [System.Obsolete("EmulationLayerUniversalScriptableRendererFeature is not supported now. Please delete this feature manually.", false)]
    public class EmulationLayerUniversalScriptableRendererFeature : ScriptableRendererFeature
    {
        static bool s_logged;

        public override void Create()
        {
            if(!s_logged)
            {
                s_logged = true;
                UnityEngine.Debug.LogWarning("EmulationLayerUniversalScriptableRendererFeature is not supported now. Please delete this feature manually.");
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
        }
    }
}
#endif
