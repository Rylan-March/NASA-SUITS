using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Android;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR implementation of the <see cref="XRAnchorSubsystem"/>.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXRAnchorSubsystem : XRAnchorSubsystem
    {
        internal const string k_SubsystemId = "Android-Anchor";

        class AndroidOpenXRAnchorProvider : Provider
        {

            // Info to retry saving anchors
            List<SerializableGuid> m_AnchorsBeingSaved = new();
            List<long> m_SaveAnchorStartTimes = new();
            List<AwaitableCompletionSource<Result<SerializableGuid>>> m_SaveCompletionSources = new();
            List<bool> m_SavingAnchorSuccessfullyStarted = new();
            List<TrackableId> m_SavingAnchorIds = new();

            // Info to retry loading stored anchors
            List<SerializableGuid> m_AnchorsToLoad = new();
            List<long> m_LoadAnchorStartTimes = new();
            List<AwaitableCompletionSource<Result<XRAnchor>>> m_LoadCompletionSources = new();

            // Info for stored IDs
            bool m_IsCurrentlyLoadingIds = false;
            long m_LoadIdsStartTime = 0;
            Allocator m_AllocatorForIds;
            static AwaitableCompletionSource<Result<NativeArray<SerializableGuid>>> s_LoadAllIdsCompletionSource = new();

            // Info to retry erasing a persistent anchor
            List<SerializableGuid> m_AnchorsToErase = new();
            List<long> m_EraseAnchorStartTimes = new();
            List<AwaitableCompletionSource<XRResultStatus>> m_EraseAnchorCompletionSources = new();

            // This is currently being used as a time out so if our async-under-the-covers
            // functions never complete, we stop trying after ten seconds
            const long k_TenSeconds = 10000000000;
            const int k_DataNotReady = -1000457003;

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
                    Debug.LogWarning($"Placing anchors requires system permission {Constants.Permissions.k_SceneUnderstandingCoarsePermission}, but permission was not granted.");
                }
#endif
                NativeApi.Start();
            }

            public override void Stop() {}

            public override void Destroy() => NativeApi.Destroy();

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                RetrySaveAnchors();
                RetryLoadAnchorIds();
                RetryLoadAnchors();
                RetryEraseAnchors();

                NativeApi.GetChanges(
                    out var addedPtr, out var addedCount,
                    out var updatedPtr, out var updatedCount,
                    out var removedPtr, out var removedCount,
                    out var elementSize);
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
                    NativeApi.ClearChanges();
                }
            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                return NativeApi.TryAdd(pose, out anchor);
            }

            public override bool TryAttachAnchor(
                TrackableId attachedToId,
                Pose pose,
                out XRAnchor anchor)
            {
                return NativeApi.TryAttach(attachedToId, pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return NativeApi.TryRemove(anchorId);
            }

            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken cancellationToken = default)
            {
                var resultStatus = new XRResultStatus();
                var guid = new SerializableGuid();
                NativeApi.TrySaveAnchor(anchorId, ref guid, ref resultStatus);
                AwaitableCompletionSource<Result<SerializableGuid>> saveAnchorCompletionSource = new();

                // If we don't get a guid back or get a failure from the platform, don't add it to the list
                if (resultStatus.IsError() || guid == SerializableGuid.empty)
                {
                    // If we don't have an error from the platform, we have a 0 guid so we should return an error
                    if (resultStatus.IsSuccess())
                    {
                        resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError, resultStatus.nativeStatusCode);
                    }
                    // Case where persistence handle isn't created yet
                    else if (resultStatus.nativeStatusCode == k_DataNotReady)
                    {
                        m_SaveCompletionSources.Add(saveAnchorCompletionSource);
                        m_SaveAnchorStartTimes.Add(NativeApi.GetCurrentTime());
                        m_AnchorsBeingSaved.Add(guid); // if this fails the guid is bad but we need it because of the bad parallel list thing
                        m_SavingAnchorSuccessfullyStarted.Add(false);
                        m_SavingAnchorIds.Add(anchorId);
                        return saveAnchorCompletionSource.Awaitable;

                    }
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(
                        saveAnchorCompletionSource,
                        new Result<SerializableGuid>(resultStatus, guid));
                }

                // If the save completed on the first try
                bool dataNotReady = false;
                if (IsPersistedAnchor(guid, ref dataNotReady))
                {
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(
                        saveAnchorCompletionSource,
                        new Result<SerializableGuid>(resultStatus, guid));
                }

                // If we get a non-zero guid, we'll check back to see its persistent state
                m_SaveCompletionSources.Add(saveAnchorCompletionSource);
                m_SaveAnchorStartTimes.Add(NativeApi.GetCurrentTime());
                m_AnchorsBeingSaved.Add(guid);
                m_SavingAnchorSuccessfullyStarted.Add(true);
                m_SavingAnchorIds.Add(anchorId);
                return saveAnchorCompletionSource.Awaitable;
            }

            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var anchor = new XRAnchor();
                AwaitableCompletionSource<Result<XRAnchor>> loadAnchorCompletionSource = new();

                bool dataNotReady = false;
                bool persistedAnchor = IsPersistedAnchor(savedAnchorGuid, ref dataNotReady);

                // If we're getting data not ready, we may still be waiting for the persistence handle
                if (!persistedAnchor && !dataNotReady)
                {
                    Debug.LogError($"Could not load anchor because no persisted anchor with guid {savedAnchorGuid} was found.");
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(
                        loadAnchorCompletionSource,
                        new Result<XRAnchor>(new XRResultStatus(persistedAnchor), anchor));
                }

                var resultStatus = new XRResultStatus();
                NativeApi.TryLoadAnchor(savedAnchorGuid, ref anchor, ref resultStatus);

                if (resultStatus.IsSuccess())
                {
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(
                        loadAnchorCompletionSource,
                        new Result<XRAnchor>(resultStatus, anchor));
                }

                m_LoadCompletionSources.Add(loadAnchorCompletionSource);
                m_LoadAnchorStartTimes.Add(NativeApi.GetCurrentTime());
                m_AnchorsToLoad.Add(savedAnchorGuid);
                return loadAnchorCompletionSource.Awaitable;
            }

            public override Awaitable<Result<NativeArray<SerializableGuid>>> TryGetSavedAnchorIdsAsync(
                Allocator allocator, CancellationToken cancellationToken = default)
            {
                // Kick off the load
                if (m_IsCurrentlyLoadingIds == false)
                {
                    m_IsCurrentlyLoadingIds = true;
                    m_AllocatorForIds = allocator;
                    var resultStatus = new XRResultStatus();
                    NativeApi.GetSavedIdCount(ref resultStatus);
                    m_LoadIdsStartTime = NativeApi.GetCurrentTime();
                }
                return s_LoadAllIdsCompletionSource.Awaitable;
            }

            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(
                SerializableGuid anchorId, CancellationToken cancellationToken = default)
            {
                var resultStatus = new XRResultStatus();
                NativeApi.TryEraseAnchor(anchorId, ref resultStatus);
                AwaitableCompletionSource<XRResultStatus> eraseAnchorCompletionSource = new();

                // If we get a success, the anchor has already been erased
                if (resultStatus.IsSuccess())
                {
                    return AwaitableUtils<XRResultStatus>.FromResult(
                        eraseAnchorCompletionSource, resultStatus);
                }

                // If we didn't get success, try again next later
                m_EraseAnchorCompletionSources.Add(eraseAnchorCompletionSource);
                m_EraseAnchorStartTimes.Add(NativeApi.GetCurrentTime());
                m_AnchorsToErase.Add(anchorId);
                return eraseAnchorCompletionSource.Awaitable;
            }

            public bool IsPersistedAnchor(TrackableId anchorId, ref bool dataNotReady)
            {
                return NativeApi.IsPersistedAnchor(anchorId, ref dataNotReady);
            }

            private void RetrySaveAnchors()
            {
                bool needToRemove = false;
                for (int a = m_AnchorsBeingSaved.Count - 1; a >= 0; a--)
                {
                    if (m_SavingAnchorSuccessfullyStarted[a] == false)
                    {
                        var resultStatus = new XRResultStatus();
                        var guid = new SerializableGuid();
                        NativeApi.TrySaveAnchor(m_SavingAnchorIds[a], ref guid, ref resultStatus);
                        if (resultStatus.IsSuccess())
                        {
                            m_SavingAnchorSuccessfullyStarted[a] = true;
                            m_AnchorsBeingSaved[a] = guid;
                        }
                        // If we fail for a reason other than data not ready
                        else if (resultStatus.nativeStatusCode != k_DataNotReady)
                        {
                            m_SaveCompletionSources[a].SetResult(new Result<SerializableGuid>(resultStatus, m_AnchorsBeingSaved[a]));
                            needToRemove = true;
                        }
                        // If we time out when the save has never successfully started
                        else if (NativeApi.GetCurrentTime() - m_SaveAnchorStartTimes[a] > k_TenSeconds)
                        {
                            m_SaveCompletionSources[a].SetResult(new Result<SerializableGuid>(resultStatus, m_AnchorsBeingSaved[a]));
                            needToRemove = true;
                        }
                    }
                    if (m_SavingAnchorSuccessfullyStarted[a] == true)
                    {
                        bool dataNotReady = false;
                        bool saveCompleted = IsPersistedAnchor(m_AnchorsBeingSaved[a], ref dataNotReady);
                        // End if we've timed out (after ten secods) or if the save has completed
                        if ((NativeApi.GetCurrentTime() - m_SaveAnchorStartTimes[a] > k_TenSeconds) ||
                            saveCompleted)
                        {
                            m_SaveCompletionSources[a].SetResult(new Result<SerializableGuid>(new XRResultStatus(saveCompleted), m_AnchorsBeingSaved[a]));
                            needToRemove = true;
                        }
                    }
                    if (needToRemove)
                    {
                        needToRemove = false;
                        m_AnchorsBeingSaved.RemoveAt(a);
                        m_SaveCompletionSources.RemoveAt(a);
                        m_SaveAnchorStartTimes.RemoveAt(a);
                        m_SavingAnchorSuccessfullyStarted.RemoveAt(a);
                        m_SavingAnchorIds.RemoveAt(a);
                    }
                }
            }

            private void RetryLoadAnchors()
            {
                for (int a = m_AnchorsToLoad.Count - 1; a >= 0; a--)
                {
                    var anchor = new XRAnchor();
                    bool dataNotReady = false;
                    bool persistedAnchor = IsPersistedAnchor(m_AnchorsToLoad[a], ref dataNotReady);

                    // If we're getting data not ready, we may still be waiting for the persistence handle
                    if (!persistedAnchor && !dataNotReady)
                    {
                        Debug.LogError($"Could not load anchor because no persisted anchor with guid {m_AnchorsToLoad[a]} was found.");
                        m_LoadCompletionSources[a].SetResult(new Result<XRAnchor>(new XRResultStatus(persistedAnchor), anchor));
                        return;
                    }

                    var resultStatus = new XRResultStatus();
                    NativeApi.TryLoadAnchor(m_AnchorsToLoad[a], ref anchor, ref resultStatus);

                    // End if we've timed out (after ten secods) or if we have a success result
                    if ((NativeApi.GetCurrentTime() - m_LoadAnchorStartTimes[a] > k_TenSeconds) ||
                        resultStatus.IsSuccess())
                    {
                        // If we timed out, we need to remove an anchor if we've created the space and are waiting on load
                        if(resultStatus.IsError())
                        {
                            NativeApi.StopLoadingAnchor(m_AnchorsToLoad[a]);
                        }

                        m_LoadCompletionSources[a].SetResult(new Result<XRAnchor>(resultStatus, anchor));
                        m_AnchorsToLoad.RemoveAt(a);
                        m_LoadCompletionSources.RemoveAt(a);
                        m_LoadAnchorStartTimes.RemoveAt(a);
                    }
                }
            }

            private unsafe void RetryLoadAnchorIds()
            {
                if (m_IsCurrentlyLoadingIds)
                {
                    var resultStatus = new XRResultStatus();
                    uint idCount = NativeApi.GetSavedIdCount(ref resultStatus);
                    if (resultStatus.nativeStatusCode >= 0)
                    {
                        int idCountAsInt = (int)idCount;
                        NativeArray<SerializableGuid> savedIds = new NativeArray<SerializableGuid>(idCountAsInt, m_AllocatorForIds);
                        resultStatus = new XRResultStatus();
                        NativeApi.GetSavedIds(idCount, savedIds.GetUnsafePtr(), ref resultStatus);
                        if ((NativeApi.GetCurrentTime() - m_LoadIdsStartTime > k_TenSeconds) ||
                            resultStatus.IsSuccess())
                        {
                            s_LoadAllIdsCompletionSource.SetResult(new Result<NativeArray<SerializableGuid>>(resultStatus, savedIds));
                            m_IsCurrentlyLoadingIds = false;
                            m_LoadIdsStartTime = 0;
                        }
                    }
                    else if (NativeApi.GetCurrentTime() - m_LoadIdsStartTime > k_TenSeconds)
                    {
                        s_LoadAllIdsCompletionSource.SetResult(new Result<NativeArray<SerializableGuid>>(resultStatus, new NativeArray<SerializableGuid>()));
                        m_IsCurrentlyLoadingIds = false;
                        m_LoadIdsStartTime = 0;
                    }
                }
            }

            void RetryEraseAnchors()
            {
                for (int a = m_AnchorsToErase.Count - 1; a >= 0; a--)
                {
                    var resultStatus = new XRResultStatus();
                    NativeApi.TryEraseAnchor(m_AnchorsToErase[a], ref resultStatus);
                    // End if we've timed out (after ten secods) or if we have a success result
                    if ((NativeApi.GetCurrentTime() - m_EraseAnchorStartTimes[a] > k_TenSeconds) ||
                        resultStatus.IsSuccess())
                    {
                        m_EraseAnchorCompletionSources[a].SetResult(resultStatus);
                        m_AnchorsToErase.RemoveAt(a);
                        m_EraseAnchorCompletionSources.RemoveAt(a);
                        m_EraseAnchorStartTimes.RemoveAt(a);
                    }
                }
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void RegisterDescriptor()
            {
                XRAnchorSubsystemDescriptor.Register(new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = k_SubsystemId,
                    providerType = typeof(AndroidOpenXRAnchorProvider),
                    subsystemTypeOverride = typeof(AndroidOpenXRAnchorSubsystem),
                    supportsTrackableAttachments = true,
                    supportsSynchronousAdd = true,
                    supportsSaveAnchor = true,
                    supportsLoadAnchor = true,
                    supportsEraseAnchor = true,
                    supportsGetSavedAnchorIds = true,
                    supportsAsyncCancellation = false,
                });
            }

            static class NativeApi
            {
                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_Create")]
                public static extern void Create();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_Start")]
                public static extern void Start();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_Destroy")]
                public static extern void Destroy();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_GetChanges")]
                public static extern unsafe void GetChanges(
                    out void* addedPtr, out int addedCount,
                    out void* updatedPtr, out int updatedCount,
                    out void* removedPtr, out int removedCount,
                    out int elementSize);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TryAdd")]
                public static extern bool TryAdd(
                    Pose pose,
                    out XRAnchor anchor);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TryAttach")]
                public static extern bool TryAttach(
                    TrackableId trackableToAffix,
                    Pose pose,
                    out XRAnchor anchor);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TryRemove")]
                public static extern bool TryRemove(TrackableId anchorId);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_ClearChanges")]
                public static extern void ClearChanges();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TrySaveAnchor")]
                public static extern void TrySaveAnchor(TrackableId anchorId, ref SerializableGuid anchorGuid, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TryLoadAnchor")]
                public static extern void TryLoadAnchor(SerializableGuid anchorId, ref XRAnchor anchor, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_FrameManager_GetCurrentTime")]
                public static extern long GetCurrentTime();

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_GetSavedIdCount")]
                public static extern uint GetSavedIdCount(ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_GetSavedIds")]
                public static extern unsafe void GetSavedIds(uint idCount, void* savedIds, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_TryEraseAnchor")]
                public static extern bool TryEraseAnchor(TrackableId anchorId, ref XRResultStatus synchronousResultStatus);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_IsPersistedAnchor")]
                public static extern bool IsPersistedAnchor(TrackableId anchorId, ref bool dataNotReady);

                [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Anchor_StopLoadingAnchor")]
                public static extern void StopLoadingAnchor(SerializableGuid anchorId);
            }
        }
    }
}
