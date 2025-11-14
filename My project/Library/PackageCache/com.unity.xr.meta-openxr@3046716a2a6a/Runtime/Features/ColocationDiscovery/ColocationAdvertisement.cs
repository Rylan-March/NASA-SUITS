using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Assertions;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class ColocationAdvertisement
    {
        internal static event EventHandler<Result<ColocationState>> stateChanged;

        internal static ColocationState advertisementState { get; private set; }

        internal static SerializableGuid advertisementId { get; private set; }

        static AwaitableCompletionSource<Result<SerializableGuid>> s_StartCompletionSource = new();
        static AwaitableCompletionSource<XRResultStatus> s_StopCompletionSource = new();

        delegate void StartAsyncDelegate(XRResultStatus resultStatus, SerializableGuid advertisingId);
        delegate void StopDelegate(XRResultStatus resultStatus);

        static readonly IntPtr s_StartAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((StartAsyncDelegate)OnStartAsyncComplete);

        static readonly IntPtr s_StopAsyncCallback =
            Marshal.GetFunctionPointerForDelegate((StopDelegate)OnStopAsyncComplete);

        static readonly IntPtr s_AdvertisementStoppedCallback =
            Marshal.GetFunctionPointerForDelegate((StopDelegate)OnAdvertisementStopped);

        internal static void CreateNative()
        {
            NativeApi.Create(s_StartAsyncCallback, s_StopAsyncCallback, s_AdvertisementStoppedCallback);
        }

        internal static void CancelRequestsAndDestroyNative()
        {
            s_StartCompletionSource.SetCanceled();
            s_StartCompletionSource.Reset();

            s_StopCompletionSource.SetCanceled();
            s_StopCompletionSource.Reset();

            NativeApi.Destroy();
        }

        internal static unsafe Awaitable<Result<SerializableGuid>> TryStartAsync(Span<byte> message)
        {
            if (advertisementState != ColocationState.Inactive)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                var result = new Result<SerializableGuid>(validationFailure, SerializableGuid.empty);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(s_StartCompletionSource, result);
            }

            XRResultStatus requestResultStatus;

            // This is safe because OpenXR makes a deep copy of this buffer so there is
            // no requirement for this buffer to persist after the call.
            fixed (byte* ptr = message)
            {
                requestResultStatus = NativeApi.TryRequestStart(ptr, (uint)message.Length);
            }

            if (requestResultStatus.IsError())
            {
                var result = new Result<SerializableGuid>(requestResultStatus, advertisementId);
                return AwaitableUtils<Result<SerializableGuid>>.FromResult(s_StartCompletionSource, result);
            }

            advertisementState = ColocationState.Starting;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StartCompletionSource.Awaitable;
        }

        internal static Awaitable<XRResultStatus> TryStopAsync()
        {
            if (advertisementState != ColocationState.Active)
            {
                var validationFailure = new XRResultStatus(StatusCode.ValidationFailure);
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, validationFailure);
            }

            var requestResultStatus = NativeApi.TryRequestStop();

            if (requestResultStatus.IsError())
                return AwaitableUtils<XRResultStatus>.FromResult(s_StopCompletionSource, requestResultStatus);

            advertisementState = ColocationState.Stopping;
            var stateChangedResult = new Result<ColocationState>(requestResultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            return s_StopCompletionSource.Awaitable;
        }

        [MonoPInvokeCallback(typeof(StartAsyncDelegate))]
        static void OnStartAsyncComplete(XRResultStatus resultStatus, SerializableGuid nativeAdvertisementId)
        {
            Assert.IsTrue(advertisementState == ColocationState.Starting);

            advertisementId = nativeAdvertisementId;
            advertisementState = resultStatus.IsSuccess() ? ColocationState.Active : ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);

            var result = new Result<SerializableGuid>(resultStatus, advertisementId);
            s_StartCompletionSource.SetResult(result);
            s_StartCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnStopAsyncComplete(XRResultStatus resultStatus)
        {
            s_StopCompletionSource.SetResult(resultStatus);
            s_StopCompletionSource.Reset();
        }

        [MonoPInvokeCallback(typeof(StopDelegate))]
        static void OnAdvertisementStopped(XRResultStatus resultStatus)
        {
            if (advertisementState == ColocationState.Inactive)
                return;

            advertisementId = SerializableGuid.empty;
            advertisementState = ColocationState.Inactive;
            var stateChangedResult = new Result<ColocationState>(resultStatus, advertisementState);
            stateChanged?.Invoke(typeof(ColocationAdvertisement), stateChangedResult);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_Create")]
            internal static extern void Create(
                IntPtr startCompleteCallback, IntPtr stopCompleteCallback, IntPtr advertisementStoppedCallback);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_Destroy")]
            internal static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_TryRequestStart")]
            internal static extern unsafe XRResultStatus TryRequestStart(void* data, uint dataSize);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityMetaOpenXR_ColocationAdvertisement_TryRequestStop")]
            internal static extern XRResultStatus TryRequestStop();
        }
    }
}
