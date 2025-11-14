using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Emulation;
using UnityEngine;

namespace UnityEditor.XR.CompositionLayers.Editor.Emulation
{
    [InitializeOnLoad]
    static class CompositionLayersEmulationLoader
    {
        static CompositionLayersEmulationLoader()
        {
            ConnectCompositionLayerFunctions();
            CompositionLayersPreferences.RefreshEmulationSettings();
        }

        internal static void ConnectCompositionLayerFunctions()
        {
            var compositionLayersPreferences = CompositionLayersPreferences.Instance;
            if (compositionLayersPreferences == null)
                compositionLayersPreferences = ScriptableObject.CreateInstance<CompositionLayersPreferences>();

            EmulatedCompositionLayerUtils.GetEmulationInScene = () => compositionLayersPreferences.EnableEmulationInScene;
            EmulatedCompositionLayerUtils.GetEmulationInPlayMode = () => compositionLayersPreferences.EmulationInPlayMode;

        }
    }
}
