using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Pool;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class AddAnchor
    {
        static readonly Dictionary<Guid, AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncPendingRequests = new();

        static readonly ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncCompletionSources =
            ObjectPoolCreateUtil.Create<AwaitableCompletionSource<Result<XRAnchor>>>(defaultCapacity: 4);

        delegate void AddAsyncDelegate(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor);
        internal static readonly IntPtr addAsyncCallback = Marshal.GetFunctionPointerForDelegate((AddAsyncDelegate)OnAddAsyncComplete);

        internal static void CancelAllRequests()
        {
            foreach (var completionSource in s_AddAsyncPendingRequests.Values)
            {
                completionSource.SetCanceled();
                completionSource.Reset();
                s_AddAsyncCompletionSources.Release(completionSource);
            }
            s_AddAsyncPendingRequests.Clear();
        }

        internal static Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
        {
            var requestId = Guid.NewGuid();
            var completionSource = s_AddAsyncCompletionSources.Get();
            s_AddAsyncPendingRequests.Add(requestId, completionSource);

            var synchronousResultStatus = new XRResultStatus();
            NativeApi.TryAddAnchorAsync(requestId, pose, ref synchronousResultStatus);

            if (synchronousResultStatus.IsError())
            {
                var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
            }

            return completionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(AddAsyncDelegate))]
        static void OnAddAsyncComplete(Guid requestId, XRResultStatus resultStatus, XRAnchor anchor)
        {
            if (!s_AddAsyncPendingRequests.Remove(requestId, out var completionSource))
            {
                Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryAddAnchorAsync)}.");
                return;
            }

            completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
            completionSource.Reset();
            s_AddAsyncCompletionSources.Release(completionSource);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaQuest_Anchor_TryAddAnchorAsync")]
            public static extern void TryAddAnchorAsync(Guid requestId, Pose pose, ref XRResultStatus synchronousResultStatus);
        }
    }
}
