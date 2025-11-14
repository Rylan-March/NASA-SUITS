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
    static class SingleShareAnchor
    {
        static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<XRResultStatus>>
            s_PendingCompletionSourcesByRequestId = new();

        static readonly ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_CompletionSourcePool =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<XRResultStatus>>();

        unsafe delegate void SingleShareCompletedDelegate(
            SerializableGuid requestId, XRShareAnchorResult* resultPtr, int sizeOfResult, int numResults);

        static readonly unsafe IntPtr s_SingleShareCompletedCallback =
            Marshal.GetFunctionPointerForDelegate((SingleShareCompletedDelegate)OnSingleShareAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var completionSource in s_PendingCompletionSourcesByRequestId.Values)
            {
                completionSource.SetCanceled();
                completionSource.Reset();
                s_CompletionSourcePool.Release(completionSource);
            }
            s_PendingCompletionSourcesByRequestId.Clear();
        }

        internal static unsafe Awaitable<XRResultStatus> TryShareAnchorAsync(TrackableId anchorId, SerializableGuid groupId)
        {
            var completionSource = s_CompletionSourcePool.Get();
            var awaitable = completionSource.Awaitable;

            var anchorIds = new NativeArray<TrackableId>(1, Allocator.Temp)
            {
                [0] = anchorId
            };

            var requestId = new SerializableGuid(Guid.NewGuid());
            s_PendingCompletionSourcesByRequestId.Add(requestId, completionSource);

            bool doesProviderExist = NativeApi.TryShareAnchorsAsync(
                requestId,
                (TrackableId*)anchorIds.GetUnsafePtr(),
                (uint)anchorIds.Length,
                groupId,
                s_SingleShareCompletedCallback);

            if (!doesProviderExist)
                return AwaitableUtils<XRResultStatus>.FromResult(
                    completionSource, new XRResultStatus(StatusCode.ProviderUninitialized));

            return awaitable;
        }

        [MonoPInvokeCallback(typeof(SingleShareCompletedDelegate))]
        static unsafe void OnSingleShareAsyncComplete(
            SerializableGuid requestId, XRShareAnchorResult* resultPtr, int sizeOfResult, int numResults)
        {
            Assert.IsTrue(s_PendingCompletionSourcesByRequestId.ContainsKey(requestId));
            s_PendingCompletionSourcesByRequestId.Remove(requestId, out var completionSource);

            Assert.IsFalse(resultPtr == null);
            Assert.IsTrue(sizeOfResult > 0);
            Assert.IsTrue(numResults == 1);

            var shareResult = XRShareAnchorResult.defaultValue;
            UnsafeUtility.MemCpyStride(
                &shareResult, sizeof(XRShareAnchorResult), resultPtr, sizeOfResult, sizeOfResult, numResults);

            var result = shareResult.resultStatus;
            completionSource.SetResult(result);
            completionSource.Reset();

            s_CompletionSourcePool.Release(completionSource);
        }
    }
}
