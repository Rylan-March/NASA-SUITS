using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CoreUtils;

namespace Unity.XR.CompositionLayers.Emulation
{
    static class EmulatedCompositionLayerUtils
    {
        static readonly Dictionary<Type, Type> k_EmulatedLayerDataTypes = new();

        internal static Func<bool> GetEmulationInScene;
        internal static Func<bool> GetEmulationInPlayMode;
        internal static Func<bool> GetEmulationInStandalone;
        internal static bool EmulationInScene => GetEmulationInScene != null && GetEmulationInScene();
        internal static bool EmulationInPlayMode => GetEmulationInPlayMode != null && GetEmulationInPlayMode();
        internal static bool EmulationInStandalone => GetEmulationInStandalone != null && GetEmulationInStandalone();

        static EmulatedCompositionLayerUtils()
        {
            GetEmulatedLayerDataTypes();
        }

        static void GetEmulatedLayerDataTypes()
        {
            var emulatedLayerDataTypes = new List<Type>();
            typeof(EmulatedLayerData).GetExtensionsOfClass(emulatedLayerDataTypes);
            foreach (var emulatedLayerDataType in emulatedLayerDataTypes)
            {
                if (emulatedLayerDataType.IsAbstract)
                    continue;

                var attributes = emulatedLayerDataType.GetCustomAttributes(typeof(EmulatedLayerDataTypeAttribute), false) as EmulatedLayerDataTypeAttribute[];
                if (attributes == null || attributes.Length == 0)
                {
                    throw new Exception($"{emulatedLayerDataType.FullName} requires an `EmulatedLayerDataTypeAttribute` to be used!");
                }

                foreach (var attribute in attributes)
                {
                    if (!k_EmulatedLayerDataTypes.TryAdd(attribute.LayerDataType, emulatedLayerDataType))
                    {
                        k_EmulatedLayerDataTypes.TryGetValue(attribute.LayerDataType, out var associatedType);
                        throw new Exception($"Cannot use {attribute.LayerDataType} with {emulatedLayerDataType.FullName}. The type is already using ${associatedType}!");
                    }
                }
            }
        }

        internal static Type GetEmulatedLayerDataType<T>() where T : LayerData
        {
            return GetEmulatedLayerDataType(typeof(T));
        }

        internal static Type GetEmulatedLayerDataType(Type type)
        {
            if (type == null)
                return null;

            k_EmulatedLayerDataTypes.TryGetValue(type, out var emulatedLayerDataType);
            return emulatedLayerDataType;
        }
    }
}
