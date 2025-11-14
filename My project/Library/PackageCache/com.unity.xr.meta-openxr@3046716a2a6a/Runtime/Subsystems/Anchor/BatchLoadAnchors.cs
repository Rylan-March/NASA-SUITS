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
    static class BatchLoadAnchors
    {
        struct LoadRequest
        {
            public AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>> completionSource;
            public Allocator allocator;
            public Action<NativeArray<XRLoadAnchorResult>> incrementalResultsCallback;
        }

        static readonly Dictionary<SerializableGuid, LoadRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRLoadAnchorResult>>>(defaultCapacity: 2);

        unsafe delegate void BatchLoadAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_BatchLoadAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((BatchLoadAsyncDelegate)OnBatchLoadAsyncComplete);

        unsafe delegate void IncrementalResultsDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_IncrementalLoadResultsCallback =
            Marshal.GetFunctionPointerForDelegate((IncrementalResultsDelegate)OnIncrementalLoadResultsAvailable);

        internal static void CancelAllRequests()
        {
            foreach (var loadRequest in s_PendingRequestsByRequestId.Values)
            {
                var completionSource = loadRequest.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingRequestsByRequestId.Clear();
        }

        internal static Awaitable<NativeArray<XRLoadAnchorResult>> TryLoadAnchorsAsync(
            NativeArray<SerializableGuid> savedAnchorGuids,
            Allocator allocator,
            Action<NativeArray<XRLoadAnchorResult>> incrementalResultsCallback)
        {
            if (!savedAnchorGuids.IsCreated)
                throw new ArgumentException(nameof(savedAnchorGuids));

            if (savedAnchorGuids.Length == 0)
                return default;

            var completionSource = s_CompletionSourcePool.Get();
            var loadRequest = new LoadRequest
            {
                completionSource = completionSource,
                allocator = allocator,
                incrementalResultsCallback = incrementalResultsCallback,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, loadRequest);

            var synchronousResultStatus = new XRResultStatus();
            unsafe
            {
                NativeApi.TryLoadAnchorsAsync(
                    requestId,
                    savedAnchorGuids.GetUnsafePtr(),
                    (uint)savedAnchorGuids.Length,
                    s_IncrementalLoadResultsCallback,
                    s_BatchLoadAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                var failedLoadResults = new NativeArray<XRLoadAnchorResult>(
                    savedAnchorGuids.Length,
                    allocator);

                for (var i = 0; i < savedAnchorGuids.Length; i += 1)
                {
                    var failedLoadResult = new XRLoadAnchorResult
                    {
                        resultStatus = synchronousResultStatus,
                        savedAnchorGuid = savedAnchorGuids[i],
                        xrAnchor = default,
                    };

                    failedLoadResults[i] = failedLoadResult;
                }

                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<NativeArray<XRLoadAnchorResult>>.FromResult(
                    completionSource,
                    failedLoadResults);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(IncrementalResultsDelegate))]
        static unsafe void OnIncrementalLoadResultsAvailable(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.TryGetValue(requestId, out var loadRequest))
            {
                Debug.LogError("An unknown error occurred during a system callback for incremental load results.");
                return;
            }

            if (loadRequest.incrementalResultsCallback == null)
                return;

            var loadResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRLoadAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, Allocator.Temp);

            loadRequest.incrementalResultsCallback.Invoke(loadResults);
        }

        [MonoPInvokeCallback(typeof(BatchLoadAsyncDelegate))]
        static unsafe void OnBatchLoadAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var loadRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorsAsync)}.");
                return;
            }

            var loadResults = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRLoadAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, loadRequest.allocator);

            var completionSource = loadRequest.completionSource;
            completionSource.SetResult(loadResults);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TryLoadAnchorsAsync is also shared with SingleLoadAnchor.cs
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAnchorsAsync")]
            public static extern void TryLoadAnchorsAsync(
                SerializableGuid requestId,
                void* anchorIdsToLoad,
                uint anchorIdsToLoadCount,
                IntPtr incrementalResultsCallback,
                IntPtr tryLoadAnchorAsyncCallback,
                ref XRResultStatus synchronousResultStatus);
        }
    }
}
