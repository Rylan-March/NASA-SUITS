using System;
using System.Linq;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Services.Editor;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// Supports Get/Read/Write PlatformLayerData for CompositionLayer.
    /// </summary>
    internal static class PlatformLayerDataUtil
    {
        // Public functions
        //--------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get all active PlatformLayerData from CompositionLayer.
        /// All unsupported PlatformLayerData will be removed automatically.
        /// If supported PlatformLayerData is contained in CompositionLayer, it's deserialized. If not, it's created.
        /// </summary>
        /// <param name="compositionLayer">Target CompositionLayer.</param>
        /// <returns>All supported PlatformLayerData.</returns>
        public static List<PlatformLayerData> GetActivePlatformLayerDataList(CompositionLayer compositionLayer)
        {
            if (compositionLayer == null)
                return null;

            List<PlatformLayerData> platformLayerDataList = GetPlatformLayerDataList(compositionLayer);

            var activePlatformLayerDataTypes = GetActivePlatformLayerDataTypes();
            if (activePlatformLayerDataTypes == null)
            {
                platformLayerDataList.Clear();
                return platformLayerDataList;
            }

            // Remove inactive platform layer data list.
            platformLayerDataList.RemoveAll(x => !activePlatformLayerDataTypes.Contains(x.GetType()));

            var sortedPlatformLayerDataList = new List<PlatformLayerData>(activePlatformLayerDataTypes.Count);

            foreach (var platformLayerDataType in activePlatformLayerDataTypes)
            {
                var platformLayerData = platformLayerDataList.Find(x => x.GetType() == platformLayerDataType);
                if (platformLayerData == null)
                    platformLayerData = Activator.CreateInstance(platformLayerDataType) as PlatformLayerData;

                sortedPlatformLayerDataList.Add(platformLayerData);
            }

            return sortedPlatformLayerDataList;
        }

        /// <summary>
        /// For writing PlatformLayerData in CompositionLayer.
        /// This function is used for updating properties in PlatformLayerData.
        /// </summary>
        /// <param name="compositionLayer">Target CompositionLayer.</param>
        /// <param name="platformLayerData">Target PlatformLayerData.</param>
        public static void WritePlatformLayerData(CompositionLayer compositionLayer, PlatformLayerData platformLayerData)
        {
            if (compositionLayer == null || platformLayerData == null)
                return;

            compositionLayer.SerializePlatformLayerData(platformLayerData);
            // Feedback to current platform layer data cache (m_PlatformLayerData) immediately.
            if (compositionLayer.m_PlatformLayerData != null && compositionLayer.m_PlatformLayerData.GetType() == platformLayerData.GetType() && compositionLayer.m_PlatformLayerData != platformLayerData)
                compositionLayer.DeserializePlatformLayerData(ref compositionLayer.m_PlatformLayerData);
        }

        /// <summary>
        /// This function is similar with GetActivePlatformLayerDataList().
        /// However this function works only target PlatformLayerData.
        /// If target PlatformLayerData is available on CompositionLayer, it's deselialized. If not, it's created.
        /// </summary>
        /// <param name="compositionLayer">Target CompositionLayer.</param>
        /// <param name="platformLayerData">Target PlatformLayerData. All properties will be overwritten.</param>
        public static void ReadPlatformLayerData(CompositionLayer compositionLayer, PlatformLayerData platformLayerData)
        {
            if (compositionLayer == null || platformLayerData == null)
                return;

            compositionLayer.DeserializePlatformLayerData(ref platformLayerData);
            // Feedback to current platform layer data cache (m_PlatformLayerData) immediately.
            if (compositionLayer.m_PlatformLayerData != null && compositionLayer.m_PlatformLayerData.GetType() == platformLayerData.GetType() && compositionLayer.m_PlatformLayerData != platformLayerData)
                compositionLayer.DeserializePlatformLayerData(ref compositionLayer.m_PlatformLayerData);
        }

        // Private functions
        //--------------------------------------------------------------------------------------------------------------------------------------------

        static List<PlatformLayerData> GetPlatformLayerDataList(CompositionLayer compositionLayer)
        {
            var platformLayerDataList = new List<PlatformLayerData>();
            var activePlatformLayerDataTypes = EditorPlatformManager.ActivePlatformLayerDataTypes;

            // Remove unsupported PlatformLayerData from CompositionLayer.m_PlatformLayerDataKeys / m_PlatformLayerDataValues
            CompactPlatformLayerDataArray(compositionLayer, activePlatformLayerDataTypes);

            var keys = compositionLayer.m_PlatformLayerDataKeys;
            var keyLength = keys != null ? keys.Length : 0;

            // Deserialize PlatformLayerData from CompositionLayer.m_PlatformLayerDataKeys / m_PlatformLayerDataValues
            for (int i = 0; i < keyLength; ++i)
            {
                var platformLayerDataType = GetPlatformLayerDataTypeFromKey(activePlatformLayerDataTypes, keys[i]);
                if (platformLayerDataType != null)
                {
                    var platformLayerData = compositionLayer.DeserializePlatformLayerData(platformLayerDataType);
                    if (platformLayerData == null)
                        platformLayerData = Activator.CreateInstance(platformLayerDataType) as PlatformLayerData;

                    platformLayerDataList.Add(platformLayerData);
                }
            }

            return platformLayerDataList;
        }

        static void CompactPlatformLayerDataArray(CompositionLayer compositionLayer, IReadOnlyList<Type> platformLayerDataTypes)
        {
            var keys = compositionLayer.m_PlatformLayerDataKeys;
            var texts = compositionLayer.m_PlatformLayerDataTexts;
            var binaries = CompositionLayer.ToBinaryDataList(compositionLayer.m_PlatformLayerDataBinary);
            var keyLength = keys != null ? keys.Length : 0;
            var textLength = texts != null ? texts.Length : 0;
            var binaryLength = binaries != null ? binaries.Length : 0;

            bool isUpdated = (keyLength < textLength) || (keyLength < binaryLength);

            var newKeys = new List<string>(keyLength);
            var newTexts = new List<string>(keyLength);
            var newBinaries = new List<int[]>(keyLength);

            for (int i = 0; i < keyLength; ++i)
            {
                if (GetPlatformLayerDataTypeFromKey(platformLayerDataTypes, keys[i]) != null)
                {
                    newKeys.Add(keys[i]);
                    newTexts.Add(i < textLength ? texts[i] : null);
                    newBinaries.Add(i < binaryLength ? binaries[i] : null);
                }
                else
                {
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
                var newTextsArray = newTexts.ToArray();
                var newBinariesArray = newBinaries.ToArray();
                CompositionLayer.CompactArray(ref newTextsArray);
                CompositionLayer.CompactArray(ref newBinariesArray);

                compositionLayer.m_PlatformLayerDataKeys = newKeys.ToArray();
                compositionLayer.m_PlatformLayerDataTexts = newTextsArray;
                compositionLayer.m_PlatformLayerDataBinary = CompositionLayer.FromBinaryDataList(newBinariesArray);
            }
        }

        static Type GetPlatformLayerDataTypeFromKey(IReadOnlyList<Type> platformLayerDataTypes, string key)
        {
            if (platformLayerDataTypes == null || key == null)
                return null;

            return platformLayerDataTypes.FirstOrDefault(x => x.FullName == key);
        }

        static List<Type> GetActivePlatformLayerDataTypes()
        {
            var activeProviders = EditorPlatformManager.ActivePlatformProviders;
            if (activeProviders == null)
            {
                return null;
            }

            var activePlatformLayerDataTypes = new List<Type>();
            foreach (var activeProvider in activeProviders)
            {
                if (activeProvider.PlatformLayerDataType != null)
                    activePlatformLayerDataTypes.Add(activeProvider.PlatformLayerDataType);
            }

            return activePlatformLayerDataTypes;
        }
    }
}
