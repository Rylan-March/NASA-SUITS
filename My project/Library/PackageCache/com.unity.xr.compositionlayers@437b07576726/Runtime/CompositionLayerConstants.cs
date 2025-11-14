using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers
{
    /// <summary>
    /// Contains constant values useful for working with composition layers.
    /// </summary>
    internal static class CompositionLayerConstants
    {
        internal const string UberShader = "Unlit/XRCompositionLayers/Uber";

        internal const string IconPath = "Packages/com.unity.xr.compositionlayers/Editor/Icons/";

        /// <summary>
        /// Name of the in GameObject created to provide Monobehaviour callbacks for the <see cref="UnityEngine.XR.CompositionLayers.Services.CompositionLayerManager"/>
        /// </summary>
        internal const string SceneManagerName = "~Compositon Manager";

        /// <summary>
        /// Name of the in default scene composition layer GameObject. <see cref="CompositionLayerManager.DefaultSceneCompositionLayer"/>
        /// </summary>
        internal const string DefaultSceneLayerName = "Default Scene Layer";
    }
}
