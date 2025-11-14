using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    internal static class XrFuncTableUtils
    {
        internal static void ClearCachedFunc(string methodName)
        {
            NativeApi.UnityOpenXRMeta_XrFuncTable_ClearCachedFunc(methodName);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            internal static extern void UnityOpenXRMeta_XrFuncTable_ClearCachedFunc(string methodName);
        }
    }
}
