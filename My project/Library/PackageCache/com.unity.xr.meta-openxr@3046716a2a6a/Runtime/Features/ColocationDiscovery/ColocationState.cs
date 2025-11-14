namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Represents the possible states for colocation advertisement or discovery.
    /// </summary>
    public enum ColocationState
    {
        /// <summary>
        /// The state is inactive.
        /// </summary>
        Inactive,

        /// <summary>
        /// The state is transitioning from `Inactive` to `Active`.
        /// </summary>
        Starting,

        /// <summary>
        /// The state is active.
        /// </summary>
        Active,

        /// <summary>
        /// The state is transitioning from `Active` to `Inactive`.
        /// </summary>
        Stopping,
    }
}
