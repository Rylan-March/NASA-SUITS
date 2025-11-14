namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Represents the visibility of the OpenXR Meta runtime's boundary.
    /// </summary>
    /// <remarks>
    /// Blittable to the native type `XrBoundaryVisibilityMETA`.
    /// </remarks>
    public enum XrBoundaryVisibility
    {
        /// <summary>
        /// Boundary visibility is not suppressed.
        /// </summary>
        VisibilityNotSuppressed = 1,

        /// <summary>
        /// Boundary visibility is suppressed.
        /// </summary>
        VisibilitySuppressed = 2,
    }
}
