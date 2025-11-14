using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// This class is used to enable PropertyField() for PlatformLayerData.
    /// </summary>
    internal class PlatformLayerDataHolder : ScriptableObject
    {
        internal const string k_PlatformLayerDataName = "m_PlatformLayerData";

        [SerializeReference]
        internal PlatformLayerData m_PlatformLayerData;
    }
}
