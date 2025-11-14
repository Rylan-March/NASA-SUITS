using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services.Editor;
using Unity.XR.CompositionLayers.Layers.Internal.Editor;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    internal static class CoordinateSystemUtils
    {
        public static void UpdateHelpBox(HelpBox helpBox, CompositionLayer compositionLayer)
        {
            if (helpBox == null)
                return;

            if (!UpdateHelpBoxInternal(helpBox, compositionLayer))
            {
                helpBox.style.display = DisplayStyle.None;
                return;
            }
        }

        static bool UpdateHelpBoxInternal(HelpBox helpBox, CompositionLayer compositionLayer)
        {
            if (compositionLayer == null)
                return false;

            if (!IsTransformingSupported(compositionLayer.LayerData))
                return false;

            var activePlatformProvider = PlatformManager.ActivePlatformProvider;
            if (activePlatformProvider == null)
                return false;

            var selectedCoordinateSystem = activePlatformProvider.GetSelectedCoordinateSystem(compositionLayer);
            var unsupportedPlatformNames = GetAllPlatformNamesUnsupportingCoordinateSystem(selectedCoordinateSystem);

            var displayTexts = new List<string>();
            bool isMessageTypeWarning = unsupportedPlatformNames.Count > 0;
            if (unsupportedPlatformNames.Count > 0 || selectedCoordinateSystem != PlatformProvider.DefaultCoordinateSystem)
            {
                displayTexts.Add($"Current coordinate system is {UIHelper.GetDisplayName(selectedCoordinateSystem)}.");
            }

            if (unsupportedPlatformNames.Count > 0)
            {
                var platformNames = UIHelper.ConcatEnumeratedNames(unsupportedPlatformNames);
                displayTexts.Add($"{UIHelper.GetDisplayName(selectedCoordinateSystem)} cooordinate system isn't supported on {platformNames}.");
            }

            if (displayTexts.Count > 0)
            {
                helpBox.style.display = DisplayStyle.Flex;
                helpBox.text = UIHelper.ConcatMultiLineTexts(displayTexts);
                helpBox.messageType = isMessageTypeWarning ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check to support transforming on target layer data.
        /// </summary>
        /// <param name="layerData">Target layer data.</param>
        /// <returns>The flag whether transforming is supported.</returns>
        static bool IsTransformingSupported(LayerData layerData)
        {
            var layerDesc = layerData != null ? CompositionLayerUtils.GetLayerDescriptor(layerData.GetType()) : LayerDataDescriptor.Empty;
            return layerDesc.SupportTransform;
        }

        /// <summary>
        /// Get all platform names that unsupport target coordinate system.
        /// </summary>
        /// <param name="selectedCoordinateSystem">Target coordinate system.</param>
        /// <returns>The platform name list.</returns>
        static List<string> GetAllPlatformNamesUnsupportingCoordinateSystem(string selectedCoordinateSystem)
        {
            var unsupportedPlatformNames = new List<string>();

            var activePlatformProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activePlatformProviders != null)
            {
                foreach (var provider in activePlatformProviders)
                {
                    if (!provider.SupportedCoordinateSystems.Contains(selectedCoordinateSystem) && !provider.IsInternal())
                    {
                        unsupportedPlatformNames.Add(UIHelper.GetDisplayName(provider));
                    }
                }
            }

            return unsupportedPlatformNames;
        }
    }
}
