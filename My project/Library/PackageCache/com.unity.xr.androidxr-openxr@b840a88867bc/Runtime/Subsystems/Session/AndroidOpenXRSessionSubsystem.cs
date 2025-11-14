using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR implementation of the <see cref="XRSessionSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    public sealed class AndroidOpenXRSessionSubsystem : XRSessionSubsystem
    {
        internal const string k_SubsystemId = "Android-Session";

        internal static AndroidOpenXRSessionSubsystem instance { get; private set; }

        /// <summary>
        /// Do not call this directly. Call create on a valid <see cref="XRSessionSubsystemDescriptor"/> instead.
        /// </summary>
        public AndroidOpenXRSessionSubsystem()
        {
            instance = this;
        }

        internal void OnSessionStateChange(int oldState, int newState) => ((AndroidOpenXRProvider)provider).OnSessionStateChange(oldState, newState);

        internal void RefreshSessionId() => ((AndroidOpenXRProvider)provider).RefreshSessionId();

        class AndroidOpenXRProvider : Provider
        {
            XrSessionState m_SessionState;
            Guid m_SessionId;

            internal void RefreshSessionId() => m_SessionId = Guid.NewGuid();

            public override Guid sessionId => m_SessionId;

            public override TrackingState trackingState
            {
                get
                {
                    switch (m_SessionState)
                    {
                        case XrSessionState.Idle:
                        case XrSessionState.Ready:
                        case XrSessionState.Synchronized:
                            return TrackingState.Limited;

                        case XrSessionState.Visible:
                        case XrSessionState.Focused:
                            return TrackingState.Tracking;

                        case XrSessionState.Unknown:
                        case XrSessionState.Stopping:
                        case XrSessionState.LossPending:
                        case XrSessionState.Exiting:
                        default:
                            return TrackingState.None;
                    }
                }
            }

            public override NotTrackingReason notTrackingReason
            {
                get
                {
                    switch (m_SessionState)
                    {
                        case XrSessionState.Idle:
                        case XrSessionState.Ready:
                        case XrSessionState.Synchronized:
                            return NotTrackingReason.Initializing;

                        case XrSessionState.Visible:
                        case XrSessionState.Focused:
                            return NotTrackingReason.None;

                        case XrSessionState.Unknown:
                        case XrSessionState.Stopping:
                        case XrSessionState.LossPending:
                        case XrSessionState.Exiting:
                        default:
                            return NotTrackingReason.Unsupported;
                    }
                }
            }

            public AndroidOpenXRProvider()
            {}

            public override void Start()
            {}

            public override void Stop()
            {}

            public override void Destroy()
            {}

            public override Promise<SessionAvailability> GetAvailabilityAsync()
                => Promise<SessionAvailability>.CreateResolvedPromise(
                    NativeApi.UnityOpenXRAndroid_Session_IsSupported()
                        ? SessionAvailability.Supported | SessionAvailability.Installed
                        : SessionAvailability.None);

            public void OnSessionStateChange(int oldState, int newState)
            {
                m_SessionState = (XrSessionState)newState;
            }
        }

        enum XrSessionState
        {
            Unknown = 0,
            Idle = 1,
            Ready = 2,
            Synchronized = 3,
            Visible = 4,
            Focused = 5,
            Stopping = 6,
            LossPending = 7,
            Exiting = 8,
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRSessionSubsystemDescriptor.Register(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(AndroidOpenXRProvider),
                subsystemTypeOverride = typeof(AndroidOpenXRSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityOpenXRAndroid_Session_IsSupported();
        }
    }
}
