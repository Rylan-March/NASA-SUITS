namespace UnityEngine.XR.Hands.Configuration
{
    /// <summary>
    /// Configuration settings for the XR Hand Subsystem.
    /// </summary>
    public struct XRHandSubsystemConfiguration
    {
        /// <summary>
        /// The pose source to be used by <see cref="XRHandDevice"/>. This will take effect the next time
        /// XRHandSbusystem invokes <see cref="XRHandSubsystem.updatedHands"/>.
        /// </summary>
        /// <remarks>
        /// The transition to the new pose source will be immediate. The device will not attempt to transition or cancel
        /// in-flight data streams.
        /// </remarks>
        public XRHandDevicePoseSource xrHandDevicePoseSource { get; set; }
    }

    /// <summary>
    /// Enumeration for controlling how <see cref="XRHandDevice"/> gets data for its public InputControls.
    /// </summary>
    public enum XRHandDevicePoseSource
    {
        /// <summary>
        /// This is the default and existing behavior for XRHandDevice.
        ///
        /// <see cref="XRHandDevice"/> will continue to report non-OpenXR compliant position and rotation data using
        /// bone joints for poke, pinch, and grip. XRHandDevice's position and rotation will continue to be in the
        /// wrist.
        /// </summary>
        LegacyJointRecognition,

        /// <summary>
        /// <see cref="XRHandDevice"/> will report OpenXR compliant hand interaction poses using <see cref="XRCommonHandGestures"/>
        /// as a data source for each hand. XRHandDevice's <see cref="XRHandDevice.devicePosition"/> and
        /// <see cref="XRHandDevice.deviceRotation"/> will match grip pose to align with other Unity XR input
        /// devices which report 'device' poses where the grip is.
        /// </summary>
        CommonGestures
    }

    /// <summary>
    /// Payload for when the subsystem updates its configuration.
    /// </summary>
    readonly struct XRHandSubsystemConfigurationUpdatedEventArgs
    {
        /// <summary>
        /// The hands subsystem that is sending the event.
        /// </summary>
        public XRHandSubsystem subsystem { get; }

        /// <summary>
        /// The new configuration that was set and processed by the subsystem.
        /// </summary>
        public XRHandSubsystemConfiguration newConfiguration { get; }

        internal XRHandSubsystemConfigurationUpdatedEventArgs(XRHandSubsystem subsystem,
            XRHandSubsystemConfiguration newConfiguration)
        {
            this.subsystem = subsystem;
            this.newConfiguration = newConfiguration;
        }
    }
}
