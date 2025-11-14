using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class OpenXRUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsOpenXRFeatureEnabled<T>() where T : OpenXRFeature
        {
            var feature = OpenXRSettings.Instance.GetFeature<T>();
            return feature != null && feature.enabled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsExtensionEnabled(string extension, string featureName = null, Type subsystemType = null)
        {
            bool isExtensionEnabled = OpenXRRuntime.IsExtensionEnabled(extension);
            if (isExtensionEnabled)
                return true;

            if (featureName != null && subsystemType != null)
                Debug.Log($"This OpenXR runtime failed to enable {extension}. The {subsystemType.Name} won't be created, and <b>{featureName}</b> will be disabled.");
            else if (featureName != null)
                Debug.Log($"This OpenXR runtime failed to enable {extension}. <b>{featureName}</b> will be disabled.");

            return false;
        }
    }
}
