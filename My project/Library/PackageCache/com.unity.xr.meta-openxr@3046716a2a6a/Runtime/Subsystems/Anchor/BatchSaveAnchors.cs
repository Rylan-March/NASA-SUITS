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
    static class BatchSaveAnchors
    {
        struct SaveRequest
        {
            public AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>> completionSource;
            public Allocator allocator;
        }

        static readonly Dictionary<SerializableGuid, SaveRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRSaveAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchSaveAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_BatchSaveAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((BatchSaveAsyncDelegate)OnBatchSaveAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var saveRequest in s_PendingRequestsByRequestId.Values)
            {
                var completionSource = saveRequest.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingRequestsByRequestId.Clear();
        }

        internal static Awaitable<NativeArray<XRSaveAnchorResult>> TrySaveAnchorsAsync(
            NativeArray<TrackableId> anchorIds, Allocator allocator)
        {
            if (!anchorIds.IsCreated)
                throw new ArgumentException(nameof(anchorIds));

            if (anchorIds.Length == 0)
                return default;

            var completionSource = s_CompletionSourcePool.Get();
            var saveRequest = new SaveRequest
            {
                completionSource = completionSource,
                allocator = allocator,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, saveRequest);

            var synchronousResultStatus = new XRResultStatus();
            unsafe
            {
                NativeApi.TrySaveAnchorsAsync(
                    requestId,
                    anchorIds.GetUnsafePtr(),
                    (uint)anchorIds.Length,
                    s_BatchSaveAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                var failedSaveResults = new NativeArray<XRSaveAnchorResult>(anchorIds.Length, allocator);
                for (var i = 0; i < anchorIds.Length; i += 1)
                {
                    var failedSaveResult = new XRSaveAnchorResult
                    {
                        resultStatus = synchronousResultStatus,
                        trackableId = anchorIds[i],
                        savedAnchorGuid = default,
                    };

                    failedSaveResults[i] = failedSaveResult;
                }

                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<NativeArray<XRSaveAnchorResult>>.FromResult(completionSource, failedSaveResults);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchSaveAsyncDelegate))]
        static unsafe void OnBatchSaveAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var saveRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorsAsync)}.");
                return;
            }

            var saveResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRSaveAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, saveRequest.allocator);

            var completionSource = saveRequest.completionSource;
            completionSource.SetResult(saveResults);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TrySaveAnchorsAsync shared with SingleSaveAnchor.cs
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
