using System.Runtime.CompilerServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class AwaitableUtils<T>
    {
        /// <summary>
        /// An `Awaitable` equivalent to C#'s `Task.FromResult`.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Awaitable<T> FromResult(
            AwaitableCompletionSource<T> completionSource, T result)
        {
            var awaitable = completionSource.Awaitable;
            completionSource.SetResult(result);
            completionSource.Reset();
            return awaitable;
        }
    }

    static class AwaitableUtils
    {
        static AwaitableCompletionSource<XRResultStatus> s_CompletionSource = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Awaitable<XRResultStatus> CompletedAwaitable(XRResultStatus resultStatus)
        {
            var awaitable = s_CompletionSource.Awaitable;
            s_CompletionSource.SetResult(resultStatus);
            s_CompletionSource.Reset();
            return awaitable;
        }
    }
}
