#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HDRENDER
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace Unity.XR.CompositionLayers.Emulation
{
    internal static class EmulationLayerScriptableRendererManager
    {
        static List<EmulatedLayerData> m_UnderlayLayers = new List<EmulatedLayerData>();
        static List<EmulatedLayerData> m_OverlayLayers = new List<EmulatedLayerData>();

        internal static List<EmulatedLayerData> GetEmulatedLayerDataList(bool isOverlay)
        {
            return isOverlay ? m_OverlayLayers : m_UnderlayLayers;
        }

        internal static void Add(EmulatedLayerData layerData, int order)
        {
            if (order >= 0)
            {
                m_OverlayLayers.Add(layerData);
            }
            else
            {
                m_UnderlayLayers.Add(layerData);
            }
        }

        internal static void Clear()
        {
            m_OverlayLayers.Clear();
            m_UnderlayLayers.Clear();
        }
    }
}
#endif
