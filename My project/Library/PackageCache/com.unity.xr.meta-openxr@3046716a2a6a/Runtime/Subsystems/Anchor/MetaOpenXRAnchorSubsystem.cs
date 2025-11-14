using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using SerializableGuid = UnityEngine.XR.ARSubsystems.SerializableGuid;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRAnchorSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    [Preserve]
    public sealed class MetaOpenXRAnchorSubsystem : XRAnchorSubsystem
    {
        internal const string k_SubsystemId = "Meta-Anchor";

        /// <summary>
        /// Get whether shared anchors is supported by the OpenXR runtime.
        /// </summary>
        /// <value>
        /// * `Unknown` if the OpenXR instance has not yet been created.
        /// * `Unsupported` if shared anchors is not supported.
        /// * `Supported` if shared anchors is supported.
        /// </value>
        public Supported isSharedAnchorsSupported =>
            GetCachedSystemCapabilities()[SystemCapability.SharedAnchors].isSupported;

        /// <summary>
        /// The group ID that anchors are shared with and loaded from.
        /// </summary>
        public SerializableGuid sharedAnchorsGroupId
        {
            get => ((MetaOpenXRAnchorProvider)provider).sharedAnchorsGroupId;
            set => ((MetaOpenXRAnchorProvider)provider).sharedAnchorsGroupId = value;
        }

        /// <summary>
        /// Attempts to share an anchor so that it can be loaded by colocated users of your app. Notify colocated
        /// users of your app to load shared anchors from the <see cref="sharedAnchorsGroupId"/>. Once shared, anchors
        /// cannot be unshared. Shared anchors remain shared for 30 days until they expire.
        /// </summary>
        /// <param name="anchorId">The `TrackableId` of the anchor you wish to share.</param>
        /// <returns>The result of the async operation. You are responsible to <see langword="await"/> this result.</returns>
        public Awaitable<XRResultStatus> TryShareAnchorAsync(TrackableId anchorId)
        {
            return ((MetaOpenXRAnchorProvider)provider).TryShareAnchorAsync(anchorId);
        }

        /// <summary>
        /// Attempts to share a batch of anchors so that they can be loaded by colocated users of your app. Notify
        /// colocated users of your app to load shared anchors from the <see cref="sharedAnchorsGroupId"/>. Once shared,
        /// anchors cannot be unshared. Shared anchors remain shared for 30 days until they expire.
        /// </summary>
        /// <param name="anchorIds">The `TrackableId`s of the anchors you wish to share.</param>
        /// <param name="allocator">The allocation strategy to use for the resulting `NativeArray`.</param>
        /// <returns>The result of the async operation, containing a `NativeArray` of `XRShareAnchorResult`s
        /// allocated with the given <paramref name="allocator"/> if the operation succeeded.
        /// You are responsible to <see langword="await"/> this result.</returns>
        public Awaitable<NativeArray<XRShareAnchorResult>> TryShareAnchorsAsync(
            NativeArray<TrackableId> anchorIds, Allocator allocator)
        {
            return ((MetaOpenXRAnchorProvider)provider).TryShareAnchorsAsync(anchorIds, allocator);
        }

        /// <summary>
        /// Attempts to load a batch of anchors that were shared with <see cref="sharedAnchorsGroupId"/>.
        /// </summary>
        /// <param name="allocator">The allocation strategy to use for the resulting `NativeArray`.</param>
        /// <param name="incrementalResultsCallback">A callback method that will be called when any anchors are loaded.
        /// This callback is invoked at least once if any anchors are successfully
        /// loaded, and possibly multiple times before the async operation completes. Pass a `null` argument for this
        /// parameter to ignore it.</param>
        /// <returns>A `NativeArray` containing the complete set of <see cref="XRAnchor"/>s that were loaded with
        /// <see cref="sharedAnchorsGroupId"/>. You are responsible to <see langword="await"/> this result.</returns>
        /// <remarks>
        /// If no anchors were shared to the <see cref="sharedAnchorsGroupId"/> then this method will return 0 anchors and
        /// <see cref="XRResultStatus.IsSuccess">status.IsSuccess()</see> will be true.
        /// </remarks>
        public Awaitable<Result<NativeArray<XRAnchor>>> TryLoadAllSharedAnchorsAsync(
            Allocator allocator, Action<NativeArray<XRAnchor>> incrementalResultsCallback)
            => LoadAllSharedAnchors.TryLoadAllSharedAnchorsAsync(
                sharedAnchorsGroupId, allocator, incrementalResultsCallback);

        class MetaOpenXRAnchorProvider : Provider
        {
            internal SerializableGuid sharedAnchorsGroupId { get; set; }

            protected override bool TryInitialize()
            {
                NativeApi.Create(AddAnchor.addAsyncCallback);
                return true;
            }

            public override void Start() { }

            public override void Stop() { }

            public override void Destroy()
            {
                AddAnchor.CancelAllRequests();
                SingleSaveAnchor.CancelAllRequests();
                BatchSaveAnchors.CancelAllRequests();
                SingleLoadAnchor.CancelAllRequests();
                BatchLoadAnchors.CancelAllRequests();
                SingleEraseAnchor.CancelAllRequests();
                BatchEraseAnchors.CancelAllRequests();
                SingleShareAnchor.CancelAllRequests();
                BatchShareAnchors.CancelAllRequests();
                LoadAllSharedAnchors.CancelAllRequests();
                NativeApi.Destroy();
            }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                int addedCount = 0, updatedCount = 0, removedCount = 0, elementSize = 0;
                void* addedPtr = null, updatedPtr = null, removedPtr = null;
                NativeApi.AcquireChanges(
                    ref addedPtr, ref addedCount,
                    ref updatedPtr, ref updatedCount,
                    ref removedPtr, ref removedCount,
                    ref elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    NativeApi.ReleaseChanges();
                }
            }

            public override Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
                => AddAnchor.TryAddAnchorAsync(pose);

            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken _ = default)
                => SingleSaveAnchor.TrySaveAnchorAsync(anchorId);

            public override Awaitable<NativeArray<XRSaveAnchorResult>> TrySaveAnchorsAsync(
                NativeArray<TrackableId> anchorIds,
                Allocator allocator,
                CancellationToken cancellationToken = default)
                => BatchSaveAnchors.TrySaveAnchorsAsync(anchorIds, allocator);

            public override bool TryRemoveAnchor(TrackableId anchorId)
                => NativeApi.TryRemoveAnchor(anchorId);

            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
                => SingleLoadAnchor.TryLoadAnchorAsync(savedAnchorGuid);

            public override Awaitable<NativeArray<XRLoadAnchorResult>> TryLoadAnchorsAsync(
                NativeArray<SerializableGuid> savedAnchorGuids,
                Allocator allocator,
                Action<NativeArray<XRLoadAnchorResult>> incrementalResultsCallback,
                CancellationToken cancellationToken = default)
                => BatchLoadAnchors.TryLoadAnchorsAsync(savedAnchorGuids, allocator, incrementalResultsCallback);

            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
                => SingleEraseAnchor.TryEraseAnchorAsync(savedAnchorGuid);

            public override Awaitable<NativeArray<XREraseAnchorResult>> TryEraseAnchorsAsync(
                NativeArray<SerializableGuid> savedAnchorGuids,
                Allocator allocator,
                CancellationToken cancellationToken = default)
                => BatchEraseAnchors.TryEraseAnchorsAsync(savedAnchorGuids, allocator);

            public Awaitable<XRResultStatus> TryShareAnchorAsync(TrackableId anchorId)
                => SingleShareAnchor.TryShareAnchorAsync(anchorId, sharedAnchorsGroupId);

            public Awaitable<NativeArray<XRShareAnchorResult>> TryShareAnchorsAsync(
                NativeArray<TrackableId> anchorIds, Allocator allocator)
                => BatchShareAnchors.TryShareAnchorsAsync(anchorIds, sharedAnchorsGroupId, allocator);

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                var anchorSubsystemCinfo = new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(MetaOpenXRAnchorProvider),
                    subsystemTypeOverride = typeof(MetaOpenXRAnchorSubsystem),
                    supportsTrackableAttachments = false,
                    supportsSynchronousAdd = false,
                    supportsSaveAnchor = true,
                    supportsLoadAnchor = true,
                    supportsEraseAnchor = true,
                    supportsGetSavedAnchorIds = false,
                    supportsAsyncCancellation = false,
                };

                XRAnchorSubsystemDescriptor.Register(anchorSubsystemCinfo);
            }
        }

        internal static unsafe class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Create")]
            internal static extern void Create(IntPtr tryAddAnchorAsyncCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_Destroy")]
            internal static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchors_AcquireChanges")]
            internal static extern void AcquireChanges(
                ref void* addedPtr, ref int addedCount,
                ref void* updatedPtr, ref int updatedCount,
                ref void* removedPtr, ref int removedCount,
                ref int elementSize);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_ReleaseChanges")]
            internal static extern void ReleaseChanges();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryRemoveAnchor")]
            internal static extern bool TryRemoveAnchor(TrackableId anchorId);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryShareAnchorsAsync")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool TryShareAnchorsAsync(
                SerializableGuid requestId,
                TrackableId* anchorsToShare,
                uint numAnchorsToShare,
                SerializableGuid groupId,
                IntPtr shareCompletedCallback);
        }
    }
}
