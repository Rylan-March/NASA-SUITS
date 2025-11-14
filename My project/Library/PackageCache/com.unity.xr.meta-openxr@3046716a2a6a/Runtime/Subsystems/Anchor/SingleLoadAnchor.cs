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
    static class SingleLoadAnchor
    {
        struct LoadRequest
        {
            public AwaitableCompletionSource<Result<XRAnchor>> completionSource;
            public NativeArray<SerializableGuid> anchorIdsToLoad;
        }

        static readonly Dictionary<SerializableGuid, LoadRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>();

        unsafe delegate void SingleLoadAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleLoadAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((SingleLoadAsyncDelegate)OnSingleLoadAsyncComplete);

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

        internal static Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var requestId = new SerializableGuid(Guid.NewGuid());
            var loadRequest = new LoadRequest
            {
                completionSource = completionSource,
                anchorIdsToLoad = new NativeArray<SerializableGuid>(1, Allocator.Persistent)
                {
                    [0] = savedAnchorGuid
                }
            };

            s_PendingRequestsByRequestId.Add(requestId, loadRequest);
            var synchronousResultStatus = new XRResultStatus();

            unsafe
            {
                NativeApi.TryLoadAnchorsAsync(
                    requestId,
                    loadRequest.anchorIdsToLoad.GetUnsafePtr(),
                    (uint)loadRequest.anchorIdsToLoad.Length,
                    IntPtr.Zero,
                    s_SingleLoadAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                loadRequest.anchorIdsToLoad.Dispose();
                var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleLoadAsyncDelegate))]
        static unsafe void OnSingleLoadAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var loadRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                return;
            }

            var loadResult = XRLoadAnchorResult.defaultValue;
            if (resultsPtr != null)
                UnsafeUtility.MemCpyStride(
                    &loadResult, sizeof(XRLoadAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);
            else
                Debug.LogError(
                    $"An unknown error occurred when retrieving data for anchor {loadRequest.anchorIdsToLoad[0].ToString()}.");

            loadRequest.anchorIdsToLoad.Dispose();

            var completionSource = loadRequest.completionSource;
            completionSource.SetResult(new Result<XRAnchor>(loadResult.resultStatus, loadResult.xrAnchor));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TryLoadAnchorsAsync is also shared with BatchLoadAnchors.cs
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
