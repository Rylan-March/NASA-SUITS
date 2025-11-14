using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The OpenXR Meta implementation of <see cref="XRBoundingBoxSubsystem"/>, built with the OpenXR Meta Mobile SDK.
    /// Bounding boxes are provided based on bounded3d components present in your
    /// [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model).
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXRBoundingBoxSubsystem : XRBoundingBoxSubsystem
    {
        internal const string k_SubsystemId = "MetaOpenXR-BoundingBox";

        const string k_AndroidScenePermission = "com.oculus.permission.USE_SCENE";

        class MetaOpenXRBoundingBoxProvider : Provider
        {
            protected override bool TryInitialize()
            {
                NativeApi.Create();
                return true;
            }

            public override void Start()
            {
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(k_AndroidScenePermission))
                    Debug.LogWarning($"Bounding boxes requires system permission {k_AndroidScenePermission}, but permission was not granted.");
#endif

                NativeApi.Start();
            }

            public override void Stop() { }

            public override void Destroy() => NativeApi.Destroy();

            public override unsafe TrackableChanges<XRBoundingBox> GetChanges(XRBoundingBox defaultXRBoundingBox, Allocator allocator)
            {
                NativeApi.GetChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize);

                try
                {
                    return new TrackableChanges<XRBoundingBox>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultXRBoundingBox, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ReleaseChanges();
                }
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                XRBoundingBoxSubsystemDescriptor.Register(new XRBoundingBoxSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXRBoundingBoxProvider),
                    subsystemTypeOverride = typeof(MetaOpenXRBoundingBoxSubsystem),
                    supportsClassification = true,
                });
            }

            static class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Create")]
                public static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Start")]
                public static extern void Start();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_GetChanges")]
                public static extern unsafe void GetChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_BoundingBox_ReleaseChanges")]
                public static extern void ReleaseChanges();
            }
        }
    }
}
