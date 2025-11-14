using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SingleSaveAnchor
    {
        struct SaveRequest
        {
            public AwaitableCompletionSource<Result<SerializableGuid>> completionSource;
            public NativeArray<TrackableId> anchorsToSave;
        }

        static readonly Dictionary<SerializableGuid, SaveRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<SerializableGuid>>>();

        unsafe delegate void SingleSaveAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleSaveAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((SingleSaveAsyncDelegate)OnSingleSaveAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var saveCompletionData in s_PendingRequestsByRequestId.Values)
            {
                var completionSource = saveCompletionData.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingRequestsByRequestId.Clear();
        }

        internal static Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(TrackableId anchorId)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var saveRequest = new SaveRequest
            {
                completionSource = completionSource,
                anchorsToSave = new NativeArray<TrackableId>(1, Allocator.Persistent)
                {
                    [0] = anchorId
                }
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, saveRequest);

            var synchronousResultStatus = new XRResultStatus();
            unsafe
            {
                NativeApi.TrySaveAnchorsAsync(
                    requestId,
                    saveRequest.anchorsToSave.GetUnsafePtr(),
                    (uint)saveRequest.anchorsToSave.Length,
                    s_SingleSaveAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                saveRequest.anchorsToSave.Dispose();
                var result = new Result<SerializableGuid>(synchronousResultStatus, default);
                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleSaveAsyncDelegate))]
        static unsafe void OnSingleSaveAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var saveRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                return;
            }

            var saveResult = XRSaveAnchorResult.defaultValue;
            if (resultsPtr != null)
                UnsafeUtility.MemCpyStride(
                    &saveResult, sizeof(XRSaveAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);
            else
                Debug.LogError(
                    $"An unknown error occurred when retrieving saved data for anchor {saveRequest.anchorsToSave[0].ToString()}.");

            saveRequest.anchorsToSave.Dispose();

            var completionSource = saveRequest.completionSource;
            completionSource.SetResult(new Result<SerializableGuid>(
                saveResult.resultStatus,
                saveResult.savedAnchorGuid));

            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TrySaveAnchorsAsync shared with BatchSaveAnchors.cs
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TrySaveAnchorsAsync")]
            public static extern void TrySaveAnchorsAsync(
                SerializableGuid requestId,
                void* anchorIdsToSave,
                uint anchorIdsToSaveCount,
                IntPtr trySaveAnchorAsyncCallback,
                ref XRResultStatus synchronousResultStatus);
        }
    }
}
