using System.Runtime.CompilerServices;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    static class OpenXRUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsOpenXRFeatureEnabled<T>() where T : OpenXRFeature
        {
            var feature = OpenXRSettings.Instance.GetFeature<T>();
            return feature != null && feature.enabled;
        }
    }
}
