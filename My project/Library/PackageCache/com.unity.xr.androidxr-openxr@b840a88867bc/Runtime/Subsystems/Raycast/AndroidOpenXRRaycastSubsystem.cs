using System.Runtime.InteropServices;
using UnityEngine.Android;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Android XR implementation of the [XRRaycastSubsystem](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem).
    /// Do not create this directly. Use the [SubsystemManager](xref:UnityEngine.SubsystemManager) instead.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXRRaycastSubsystem : XRRaycastSubsystem
    {
        internal const string k_SubsystemId = "Android-Raycast";

        class AndroidOpenXRRaycastPovider : Provider
        {

            protected override bool TryInitialize()
            {
                if (OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_ANDROID_trackables) &&
                    OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_ANDROID_raycast))
                {
                    NativeApi.Create();
                    return true;
                }
                return false;
            }

            public override void Start()
            {
#if UNITY_ANDROID
                // According to Google's spec, SCENE_UDERSTANDING_FINE implies SCENE_UDERSTANDING_COARSE as well
                if (!Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingFinePermission))
                {
                    Debug.LogWarning($"Raycasting against depth requires system permission {Constants.Permissions.k_SceneUnderstandingFinePermission}, but permission was not granted.");

                    if (!Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingCoarsePermission))
                    {
                        Debug.LogWarning($"Raycasting against planes requires system permission {Constants.Permissions.k_SceneUnderstandingCoarsePermission}, but permission was not granted.");
                    }
                }
#endif // UNITY_ANDROID
            }

            public override void Destroy() => NativeApi.Destroy();

            public override unsafe NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Ray ray,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                void* hitBuffer;
                int hitCount = 0;
                int elementSize = 0;

                NativeApi.AcquireHitResults(
                    ray.origin,
                    ray.direction,
                    trackableTypeMask,
                    out hitBuffer,
                    out hitCount,
                    out elementSize);
                try
                {
                    return NativeCopyUtility.PtrToNativeArrayWithDefault<XRRaycastHit>(
                        defaultRaycastHit,
                        hitBuffer, elementSize,
                        hitCount, allocator);
                }
                finally
                {
                    NativeApi.ReleaseHitResults(hitBuffer);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRRaycastSubsystemDescriptor.Register(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(AndroidOpenXRRaycastPovider),
                subsystemTypeOverride = typeof(AndroidOpenXRRaycastSubsystem),
                supportsViewportBasedRaycast = false,
                supportsWorldBasedRaycast = true,
                supportedTrackableTypes =
                    TrackableType.Planes |
                    TrackableType.Depth,
                supportsTrackedRaycasts = false,
            });
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Raycast_Create")]
            public static extern void Create();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Raycast_Destroy")]
            public static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Raycast_AcquireHitResults")]
            public static unsafe extern void AcquireHitResults(
                Vector3 rayOrigin,
                Vector3 rayDirection,
                TrackableType filter,
                out void* hitBuffer,
                out int hitCount,
                out int elementSize);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Raycast_ReleaseHitResults")]
            public static unsafe extern void ReleaseHitResults(void* buffer);
        }
    }
}
