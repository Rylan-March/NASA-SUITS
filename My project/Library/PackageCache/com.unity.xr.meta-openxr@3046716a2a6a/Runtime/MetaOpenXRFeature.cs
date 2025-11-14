namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Base class for public OpenXR features in this package.
    /// </summary>
    public class MetaOpenXRFeature : OpenXRFeature
    {
        /// <summary>
        /// Called when the enabled state of a feature changes.
        /// </summary>
        protected override void OnEnabledChange()
        {
#if UNITY_EDITOR
            // Always succeeds. If the user is interacting with Editor UI, XRM is initialized.
            OpenXRLifeCycleFeature.TryRefreshEnabledState();
#endif // UNITY_EDITOR
        }
    }
}
