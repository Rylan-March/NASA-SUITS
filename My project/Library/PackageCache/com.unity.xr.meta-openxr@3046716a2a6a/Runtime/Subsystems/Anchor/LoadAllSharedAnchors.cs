using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class LoadAllSharedAnchors
    {
        struct LoadRequest
        {
            public AwaitableCompletionSource<Result<NativeArray<XRAnchor>>> completionSource;
            public Allocator allocator;
            public Action<NativeArray<XRAnchor>> incrementalResultsCallback;
        }

        static readonly Dictionary<SerializableGuid, LoadRequest> s_RequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<NativeArray<XRAnchor>>>>(defaultCapacity: 2);

        unsafe delegate void LoadAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_BatchLoadAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnBatchLoadAsyncComplete);

        unsafe delegate void IncrementalResultsDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int resultsCount);

        static readonly unsafe IntPtr s_IncrementalLoadResultsCallback =
            Marshal.GetFunctionPointerForDelegate((IncrementalResultsDelegate)OnIncrementalLoadResultsAvailable);

        internal static void CancelAllRequests()
        {
            foreach (var loadRequest in s_RequestsByRequestId.Values)
            {
                var completionSource = loadRequest.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_RequestsByRequestId.Clear();
        }

        internal static Awaitable<Result<NativeArray<XRAnchor>>> TryLoadAllSharedAnchorsAsync(
            SerializableGuid groupId, Allocator allocator, Action<NativeArray<XRAnchor>> incrementalResultsCallback)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var loadRequest = new LoadRequest
            {
                completionSource = completionSource,
                allocator = allocator,
                incrementalResultsCallback = incrementalResultsCallback,
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_RequestsByRequestId.Add(requestId, loadRequest);

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TryLoadAllSharedAnchorsAsync(
                requestId,
                groupId,
                s_IncrementalLoadResultsCallback,
                s_BatchLoadAsyncCallback,
                ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
            {
                var failedLoadResults = new NativeArray<XRAnchor>(0, allocator);

                s_RequestsByRequestId.Remove(requestId);
                var result = new Result<NativeArray<XRAnchor>>(
                    new XRResultStatus(synchronousResultStatus), failedLoadResults);

                return AwaitableUtils<Result<NativeArray<XRAnchor>>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(IncrementalResultsDelegate))]
        static unsafe void OnIncrementalLoadResultsAvailable(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_RequestsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(resultsPtr != null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(numResults > 0);

            var request = s_RequestsByRequestId[requestId];

            if (request.incrementalResultsCallback == null)
                return;

            var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRAnchor.defaultValue, resultsPtr, sizeOfResult, numResults, Allocator.Temp);

            request.incrementalResultsCallback.Invoke(results);
        }

        [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
        static unsafe void OnBatchLoadAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_RequestsByRequestId.ContainsKey(requestId));
            Assert.IsTrue(sizeOfResult > 0);

            s_RequestsByRequestId.Remove(requestId, out var request);

            Result<NativeArray<XRAnchor>> result = default;
            if (resultsPtr == null)
            {
                Assert.IsTrue(numResults == 0);
            }
            else
            {
                Assert.IsTrue(numResults > 0);
                var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                    XRAnchor.defaultValue, resultsPtr, sizeOfResult, numResults, request.allocator);

                result = new Result<NativeArray<XRAnchor>>(
                    new XRResultStatus(XRResultStatus.StatusCode.UnqualifiedSuccess), results);
            }

            var completionSource = request.completionSource;
            completionSource.SetResult(result);
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryLoadAllSharedAnchorsAsync")]
            public static extern void TryLoadAllSharedAnchorsAsync(
                SerializableGuid requestId,
                SerializableGuid groupId,
                IntPtr incrementalResultsCallback,
                IntPtr tryLoadSharedAnchorAsyncCallback,
                ref XRResultStatus synchronousResultStatus);
        }
    }
}
