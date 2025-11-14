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
    static class BatchEraseAnchors
    {
        struct EraseRequest
        {
            public AwaitableCompletionSource<NativeArray<XREraseAnchorResult>> completionSource;
            public Allocator allocator;
        }

        static readonly Dictionary<SerializableGuid, EraseRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XREraseAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XREraseAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchEraseAsyncDelegate(
            SerializableGuid requestId, void* saveAnchorResultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_BatchEraseAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((BatchEraseAsyncDelegate)OnBatchEraseAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var eraseRequest in s_PendingRequestsByRequestId.Values)
            {
                var completionSource = eraseRequest.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingRequestsByRequestId.Clear();
        }

        internal static Awaitable<NativeArray<XREraseAnchorResult>> TryEraseAnchorsAsync(
            NativeArray<SerializableGuid> savedAnchorGuids,
            Allocator allocator)
        {
            if (!savedAnchorGuids.IsCreated)
                throw new ArgumentException(nameof(savedAnchorGuids));

            if (savedAnchorGuids.Length == 0)
                return default;

            var completionSource = s_CompletionSourcePool.Get();
            var eraseRequest = new EraseRequest
            {
                completionSource = completionSource,
                allocator = allocator,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, eraseRequest);

            var synchronousResultStatus = new XRResultStatus();
            unsafe
            {
                NativeApi.TryEraseAnchorsAsync(
                    requestId,
                    savedAnchorGuids.GetUnsafePtr(),
                    (uint)savedAnchorGuids.Length,
                    s_BatchEraseAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                var failedResults = new NativeArray<XREraseAnchorResult>(savedAnchorGuids.Length, allocator);

                for (var i = 0; i < savedAnchorGuids.Length; i += 1)
                {
                    var failedEraseResult = new XREraseAnchorResult
                    {
                        resultStatus = synchronousResultStatus,
                        savedAnchorGuid = savedAnchorGuids[i],
                    };

                    failedResults[i] = failedEraseResult;
                }

                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<NativeArray<XREraseAnchorResult>>.FromResult(completionSource, failedResults);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchEraseAsyncDelegate))]
        static unsafe void OnBatchEraseAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var eraseRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryEraseAnchorsAsync)}.");
                return;
            }

            var eraseResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XREraseAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, eraseRequest.allocator);

            var completionSource = eraseRequest.completionSource;
            completionSource.SetResult(eraseResults);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TryEraseAnchorsAsync shared with SingleEraseAnchor.cs
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryEraseAnchorsAsync")]
            public static extern void TryEraseAnchorsAsync(
                SerializableGuid requestId,
                void* anchorIdsToErase,
                uint anchorIdsToEraseCount,
                IntPtr tryEraseAnchorAsyncCallback,
                ref XRResultStatus synchronousResultStatus);
        }
    }
}
