#if ENABLE_CLOUD_SERVICES_ANALYTICS
#if !UNITY_2023_2_OR_NEWER
using System.Reflection;
#endif // !UNITY_2023_2_OR_NEWER

namespace UnityEditor.XR.Hands.Analytics
{
    /// <summary>
    /// The entry point class to send XR Hands analytics data.
    /// </summary>
    static class XRHandsAnalytics
    {
        internal const string k_VendorKey = "unity.xr.hands";
        internal const string k_PackageName = "com.unity.xr.hands";
#if UNITY_2023_2_OR_NEWER
        internal static readonly string k_PackageVersion = PackageManager.PackageInfo.FindForPackageName(k_PackageName).version;
#else
        internal static readonly string k_PackageVersion = PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).version;
#endif

        internal static XRHandsPlaymodeUsageEvent playmodeUsageEvent { get; } = new();
        internal static XRHandsEditmodeUsageEvent editmodeUsageEvent { get; } = new();
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS
