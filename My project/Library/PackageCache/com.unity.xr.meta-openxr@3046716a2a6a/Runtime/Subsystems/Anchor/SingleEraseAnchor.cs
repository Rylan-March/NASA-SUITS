using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SingleEraseAnchor
    {
        struct EraseRequest
        {
            public AwaitableCompletionSource<XRResultStatus> completionSource;
            public NativeArray<SerializableGuid> anchorIdsToErase;
        }

        static readonly Dictionary<SerializableGuid, EraseRequest> s_PendingRequestsByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<XRResultStatus>>();

        unsafe delegate void SingleEraseAsyncDelegate(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleEraseAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((SingleEraseAsyncDelegate)OnSingleEraseAsyncComplete);

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

        internal static Awaitable<XRResultStatus> TryEraseAnchorAsync(
            SerializableGuid savedAnchorGuid)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var eraseRequest = new EraseRequest
            {
                completionSource = completionSource,
                anchorIdsToErase = new NativeArray<SerializableGuid>(1, Allocator.Persistent)
                {
                    [0] = savedAnchorGuid
                }
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingRequestsByRequestId.Add(requestId, eraseRequest);

            var synchronousResultStatus = new XRResultStatus();
            unsafe
            {
                NativeApi.TryEraseAnchorsAsync(
                    requestId,
                    eraseRequest.anchorIdsToErase.GetUnsafePtr(),
                    (uint)eraseRequest.anchorIdsToErase.Length,
                    s_SingleEraseAsyncCallback,
                    ref synchronousResultStatus);
            }

            if (synchronousResultStatus.IsError())
            {
                eraseRequest.anchorIdsToErase.Dispose();
                s_PendingRequestsByRequestId.Remove(requestId);
                return AwaitableUtils<XRResultStatus>.FromResult(completionSource, synchronousResultStatus);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleEraseAsyncDelegate))]
        static unsafe void OnSingleEraseAsyncComplete(
            SerializableGuid requestId, void* resultsPtr, int sizeOfResult, int numResults)
        {
            if (!s_PendingRequestsByRequestId.Remove(requestId, out var eraseRequest))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryEraseAnchorAsync)}.");
                return;
            }

            var eraseResult = XREraseAnchorResult.defaultValue;
            if (resultsPtr != null)
                UnsafeUtility.MemCpyStride(
                    &eraseResult, sizeof(XREraseAnchorResult), resultsPtr, sizeOfResult, sizeOfResult, numResults);
            else
                Debug.LogError(
                    $"An unknown error occurred when retrieving data for anchor {eraseRequest.anchorIdsToErase[0].ToString()}.");

            eraseRequest.anchorIdsToErase.Dispose();

            var completionSource = eraseRequest.completionSource;
            completionSource.SetResult(new XRResultStatus(eraseResult.resultStatus));
            completionSource.Reset();
            s_CompletionSourcePool.Release(completionSource);
        }

        static unsafe class NativeApi
        {
            // TryEraseAnchorsAsync shared with BatchEraseAnchors.cs
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
