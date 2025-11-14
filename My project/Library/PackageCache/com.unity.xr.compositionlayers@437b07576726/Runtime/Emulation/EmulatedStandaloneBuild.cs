#if !UNITY_EDITOR && UNITY_STANDALONE
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Emulation;
using UnityEngine;

namespace UnityEditor.XR.CompositionLayers.Emulation
{
    internal class EmulatedStandaloneBuildLoader
    {
        /// <summary>
        /// Loads Composition Layers Emulation in Standalone Build
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void CompositionLayersEmulationStandaloneLoader()
        {
                InitializeCompositionLayerFunctions();
                EmulatedLayerProvider.DisconnectEmulatedLayerProvider();
                EmulatedLayerProvider.ConnectEmulatedLayerProvider();
        }

        /// <summary>
        /// Initializes the Composition Layer Emulation Boolean Functions
        /// </summary>
        static void InitializeCompositionLayerFunctions()
        {
            var compositionLayersSettings = CompositionLayersRuntimeSettings.Instance;
            if (compositionLayersSettings == null)
                compositionLayersSettings= ScriptableObject.CreateInstance<CompositionLayersRuntimeSettings>();

            EmulatedCompositionLayerUtils.GetEmulationInStandalone = () => compositionLayersSettings.EmulationInStandalone;
        }
    }
}
#endif
