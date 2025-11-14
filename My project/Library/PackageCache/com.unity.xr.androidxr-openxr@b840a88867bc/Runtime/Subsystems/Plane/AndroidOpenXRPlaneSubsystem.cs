using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Android;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR implementation of the <see cref="XRPlaneSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXRPlaneSubsystem : XRPlaneSubsystem
    {
        internal const string k_SubsystemId = "Android-Plane";

        class AndroidOpenXRPlaneProvider : Provider
        {
            protected override bool TryInitialize()
            {
                if (OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_ANDROID_trackables))
                {
                    NativeApi.Create();
                    return true;
                }

                return false;
            }

            public override void Start()
            {
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingFinePermission)
                    && !Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingCoarsePermission))
                {
                    Debug.LogWarning($"Plane detection requires system permission {Constants.Permissions.k_SceneUnderstandingCoarsePermission}, but permission was not granted.");
                }
#endif
            }

            public override void Stop() {}

            public override void Destroy() => NativeApi.Destroy();

            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => NativeApi.GetPlaneDetectionMode();
                set => NativeApi.SetPlaneDetectionMode(value);
            }

            public override PlaneDetectionMode currentPlaneDetectionMode => requestedPlaneDetectionMode;

            public override unsafe TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
            {
                NativeApi.GetChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize);
                try
                {
                    return new TrackableChanges<BoundedPlane>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultPlane, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ClearChanges();
                }
            }

            public override unsafe void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                uint vertexCount = NativeApi.GetBoundaryVertexCount(in trackableId);
                int vertexCountAsInt = (int)vertexCount;

                if (vertexCountAsInt < 0)
                {
                    throw new OverflowException("Exceeded the maximum number of boundary vertices.");
                }

                CreateOrResizeNativeArrayIfNecessary(vertexCountAsInt, allocator, ref boundary);
                NativeApi.GetBoundaryVertexData(in trackableId, boundary.GetUnsafePtr());

                // Flip winding order
                boundary.AsSpan().Reverse();

                // Flip handedness
                for (int b = 0; b < boundary.Length; b++)
                {
                    boundary[b] = new Vector2(boundary[b].x, -boundary[b].y);
                }
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                XRPlaneSubsystemDescriptor.Register(new XRPlaneSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(AndroidOpenXRPlaneProvider),
                    subsystemTypeOverride = typeof(AndroidOpenXRPlaneSubsystem),
                    supportsHorizontalPlaneDetection = true,
                    supportsVerticalPlaneDetection = true,
                    supportsArbitraryPlaneDetection = true,
                    supportsBoundaryVertices = true,
                    supportsClassification = true
                });
            }

            static class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_Create")]
                public static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_GetChanges")]
                public static extern unsafe void GetChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_SetPlaneDetectionMode")]
                public static extern void SetPlaneDetectionMode(PlaneDetectionMode planeDetectionMode);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_GetPlaneDetectionMode")]
                public static extern PlaneDetectionMode GetPlaneDetectionMode();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_GetBoundaryVertexCount")]
                public static extern uint GetBoundaryVertexCount(in TrackableId trackableId);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_GetBoundaryVertexData")]
                public static extern unsafe void GetBoundaryVertexData(in TrackableId trackableId, void* boundary);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Plane_ClearChanges")]
                public static extern void ClearChanges();
            }
        }
    }
}
