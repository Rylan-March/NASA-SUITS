using System;
using System.Runtime.InteropServices;
using AOT;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class ColocationDiscovery
    {
        internal static event EventHandler<Result<ColocationState>> stateChanged;

        internal static event EventHandler<ColocationDiscoveryMessage> messageDiscovered;

        internal static ColocationState discoveryState;

        static AwaitableCompletionSource<XRResultStatus> s_StartCompletionSource = new();
        static AwaitableCompletionSource<XRResultStatus> s_StopCompletionSource = new();

        delegate void StartAsyncDelegate(XRResultStatus resultStatus);
        delegate void StopDelegate(XRResultStatus resultStatus);
        unsafe delegate void MessageDiscoveredDelegate(
            void* messageBytesPtr,
            int sizeOfMessage,
            int bytesCount,
            SerializableGuid advertisementId);

        static readonly IntPtr s_StartAsyncCallback = Marshal.GetFunctionPointerForDelegate((StartAsyncDelegate)OnStartAsyncComplete);
        static readonly IntPtr s_StopAsyncCallback = Marshal.GetFunctionPointerForDelegate((StopDelegate)OnStopAsyncComplete);
        static readonly IntPtr s_DiscoveryStoppedCallback = Marshal.GetFunctionPointerForDelegate((StopDelegate)OnDiscoveryStopped);
        static readonly unsafe IntPtr s_MessageDiscoveredCallback = Marshal.GetFunctionPointerForDelegate((MessageDiscoveredDelegate)OnMessageDiscovered);

        internal static void Create()
        {
            NativeApi.Create(
                s_StartAsyncCallback, s_StopAsyncCallback, s_DiscoveryStoppedCallback, s_MessageDiscoveredCallback);
        }

        internal static void CancelRequestsAndDestroyNative()
        {
            s_StartCompletionSource.SetCanceled();
            s_StartCompletionSource.Reset();

            s_StopCompletionSource.SetCanceled();
            s_StopCompletionSource.Reset();

            NativeApi.Destroy();
        }

        internal static Awaitable<XRResultStatus> TryStartAsync()
        {
            if (discoveryState != ColocationState.Inactive)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StartCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStart();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StartCompletionSource, requestResultStatus);

            discoveryState = ColocationState.Starting;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);

            return s_StartCompletionSource.Awaitable;
        }

        internal static Awaitable<XRResultStatus> TryStopAsync()
        {
            if (discoveryState != ColocationState.Active)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStop();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, requestResultStatus);

            discoveryState = ColocationState.Stopping;
            var stateChangedResult = new Result<ColocationState>(
                new XRResultStatus(StatusCode.UnqualifiedSuccess), discoveryState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StopCompletionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(StartAsyncDelegate))]
        static void OnStartAsyncComplete(XRResultStatus resultStatus)
        {
            Assert.IsTrue(discoveryState == ColocationState.Starting);

            discoveryState = resultStatus.IsSuccess() ? ColocationState.Active : ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);

            s_StartCompletionSource.SetResult(resultStatus);
            s_StartCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnStopAsyncComplete(XRResultStatus resultStatus)
        {
            s_StopCompletionSource.SetResult(resultStatus);
            s_StopCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnDiscoveryStopped(XRResultStatus resultStatus)
        {
            if (discoveryState == ColocationState.Inactive)
                return;

            discoveryState = ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, discoveryState);
            stateChanged?.Invoke(typeof(ColocationDiscovery), stateChangedResult);
        }

        [MonoPInvokeCallback(typeof(MessageDiscoveredDelegate))]
        static unsafe void OnMessageDiscovered(
            void* messageBytesPtr,
            int sizeOfMessage,
            int bytesCount,
            SerializableGuid advertisementId)
        {
            var data = NativeCopyUtility.PtrToNativeArrayWithDefault(
                (byte)0, messageBytesPtr, sizeOfMessage, bytesCount, Allocator.Temp);

            var result = new ColocationDiscoveryMessage(advertisementId, data);
            messageDiscovered?.Invoke(typeof(ColocationDiscovery), result);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_Create")]
            internal static extern void Create(
                IntPtr startCallback, IntPtr stopCallback, IntPtr discoveryStoppedCallback, IntPtr messageDiscoveredCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_Destroy")]
            internal static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_TryRequestStart")]
            internal static extern XRResultStatus TryRequestStart();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationDiscovery_TryRequestStop")]
            internal static extern XRResultStatus TryRequestStop();
        }
    }
}
