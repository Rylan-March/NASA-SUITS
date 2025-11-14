using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;
using static UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class BatchShareAnchors
    {
        class BatchShareOperation : IReleasable
        {
            internal AwaitableCompletionSource<NativeArray<XRShareAnchorResult>> completionSource;
            internal Allocator allocator;
            internal int numExpectedResults;

            public BatchShareOperation()
            {
                Release();
            }

            public void Release()
            {
                completionSource = null;
                allocator = Allocator.None;
                numExpectedResults = 0;
            }
        }

        static readonly ObjectPool<BatchShareOperation> s_OperationPool =
            ObjectPoolCreateUtil.CreateWithReleaseTrigger<BatchShareOperation>(defaultCapacity: 2);

        static readonly Dictionary<SerializableGuid, BatchShareOperation> s_PendingOpsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<NativeArray<XRShareAnchorResult>>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<NativeArray<XRShareAnchorResult>>>();

        unsafe delegate void BatchShareCompletedDelegate(
            SerializableGuid requestId, XRShareAnchorResult* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_BatchShareCompletedCallback =
            Marshal.GetFunctionPointerForDelegate((BatchShareCompletedDelegate)OnBatchShareAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var op in s_PendingOpsByRequestId.Values)
            {
                var completionSource = op.completionSource;
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
                s_OperationPool.Release(op);
            }
            s_PendingOpsByRequestId.Clear();
        }

        internal static unsafe Awaitable<NativeArray<XRShareAnchorResult>> TryShareAnchorsAsync(
            NativeArray<TrackableId> anchorIds, SerializableGuid groupId, Allocator allocator)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            if (anchorIds.Length == 0)
            {
                awaitable = AwaitableUtils<NativeArray<XRShareAnchorResult>>.FromResult(
                    completionSource, new NativeArray<XRShareAnchorResult>(0, allocator));

                s_CompletionSourcePool.Release(completionSource);
                return awaitable;
            }

            var operation = s_OperationPool.Get();
            operation.completionSource = completionSource;

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingOpsByRequestId.Add(requestId, operation);

            bool doesProviderExist = NativeApi.TryShareAnchorsAsync(
                requestId,
                (TrackableId*)anchorIds.GetUnsafePtr(),
                (uint)anchorIds.Length,
                groupId,
                s_BatchShareCompletedCallback);

            if (!doesProviderExist)
            {
                s_PendingOpsByRequestId.Remove(requestId);
                return FailWithProviderUninitialized(operation, anchorIds, allocator);
            }

            return awaitable;
        }

        static Awaitable<NativeArray<XRShareAnchorResult>> FailWithProviderUninitialized(
            BatchShareOperation operation, NativeArray<TrackableId> anchorIds, Allocator allocator)
        {
            var completionSource = operation.completionSource;
            var results = new NativeArray<XRShareAnchorResult>(anchorIds.Length, allocator);
            for (var i = 0; i < anchorIds.Length; ++i)
            {
                results[i] = new XRShareAnchorResult(new XRResultStatus(StatusCode.ProviderUninitialized), anchorIds[i]);
            }

            var awaitable = AwaitableUtils<NativeArray<XRShareAnchorResult>>.FromResult(completionSource, results);
            s_CompletionSourcePool.Release(completionSource);
            s_OperationPool.Release(operation);
            return awaitable;
        }

        [MonoPInvokeCallback(typeof(BatchShareCompletedDelegate))]
        static unsafe void OnBatchShareAsyncComplete(
            SerializableGuid requestId, XRShareAnchorResult* resultsPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingOpsByRequestId.ContainsKey(requestId));
            s_PendingOpsByRequestId.Remove(requestId, out var operation);

            Assert.IsFalse(resultsPtr == null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(operation.numExpectedResults == numResults);

            var results = NativeCopyUtility.PtrToNativeArrayWithDefault(
                XRShareAnchorResult.defaultValue, resultsPtr, sizeOfResult, numResults, operation.allocator);

            operation.completionSource.SetResult(results);
            operation.completionSource.Reset();
            s_CompletionSourcePool.Release(operation.completionSource);
            s_OperationPool.Release(operation);
        }
    }
}
