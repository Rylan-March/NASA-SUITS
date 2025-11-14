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
    /// The Android-OpenXR implementation of the <see cref="XRFaceSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXRFaceSubsystem : XRFaceSubsystem
    {
        /// <summary>
        /// Do not call this directly. Call <c>Create</c> on a relevant <see cref="XRFaceSubsystemDescriptor"/> instead.
        /// </summary>
        public AndroidOpenXRFaceSubsystem() => instance = this;

        /// <summary>
        /// <see cref="TrackableId"/> for the face of the person wearing the headset.
        /// </summary>
        public TrackableId inwardID
        {
            get
            {
                var derivedProvider = provider as AndroidOpenXRFaceProvider;
                if (derivedProvider == null)
                    return TrackableId.invalidId;

                return derivedProvider.inwardID;
            }
        }

        internal bool TryGetExtraFaceTrackingStates(out AndroidOpenXRFaceTrackingStates states, TrackableId id)
        {
            var derivedProvider = provider as AndroidOpenXRFaceProvider;
            if (derivedProvider == null || id != derivedProvider.inwardID)
            {
                states = AndroidOpenXRFaceTrackingStates.None;
                return false;
            }

            states = derivedProvider.inwardExtraTrackingStates;
            return true;
        }

        internal const string k_SubsystemId = "Android-Face";


        internal static AndroidOpenXRFaceSubsystem instance { get; private set; }

        class AndroidOpenXRFaceProvider : Provider
        {
            protected override bool TryInitialize()
            {
                if (!OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_ANDROID_faces))
                    return false;

                NativeApi.Create();
                return true;
            }

            public override void Start()
            {
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Constants.Permissions.k_EyeTrackingFinePermission)
                    && !Permission.HasUserAuthorizedPermission(Constants.Permissions.k_EyeTrackingCoarsePermission))
                {
                    Debug.LogWarning($"Face detection requires system permission {Constants.Permissions.k_EyeTrackingCoarsePermission} (or {Constants.Permissions.k_EyeTrackingFinePermission}), but no permission was granted.");
                    return;
                }
#endif // UNITY_ANDROID

                m_InwardID = NativeApi.StartAndGetInwardID();
            }

            public override void Stop() {}

            public override void Destroy() => NativeApi.Destroy();

            public override unsafe TrackableChanges<XRFace> GetChanges(XRFace defaultFace, Allocator allocator)
            {
                NativeApi.AcquireChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize,
                    ref m_InwardExtraTrackingStates);

                try
                {
                    return new TrackableChanges<XRFace>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultFace, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ReleaseChanges();
                }
            }

            internal TrackableId inwardID => m_InwardID;

            TrackableId m_InwardID;
            AndroidOpenXRFaceTrackingStates m_InwardExtraTrackingStates;

            internal AndroidOpenXRFaceTrackingStates inwardExtraTrackingStates => m_InwardExtraTrackingStates;
            internal static AndroidOpenXRFaceSubsystem instance { get; private set; }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                XRFaceSubsystemDescriptor.Register(new XRFaceSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(AndroidOpenXRFaceProvider),
                    subsystemTypeOverride = typeof(AndroidOpenXRFaceSubsystem),
                    supportsEyeTracking  = true,
                });
            }

            static class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_Create")]
                public static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_StartAndGetInwardID")]
                public static extern TrackableId StartAndGetInwardID();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_AcquireChanges")]
                public static extern unsafe void AcquireChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize,
                    ref AndroidOpenXRFaceTrackingStates inwardExtraTrackingStates);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Face_ReleaseChanges")]
                public static extern void ReleaseChanges();
            }
        }
    }
}
