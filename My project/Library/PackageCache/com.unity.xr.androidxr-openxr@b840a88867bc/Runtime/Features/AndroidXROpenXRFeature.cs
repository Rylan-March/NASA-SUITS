namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Base class for public OpenXR features in this package.
    /// </summary>
    public class AndroidXROpenXRFeature : OpenXRFeature
    {
        /// <summary>
        /// Called when the enabled state of a feature changes.
        /// </summary>
        protected override void OnEnabledChange()
        {
#if UNITY_EDITOR
            OpenXRLifeCycleFeature.RefreshEnabledState();
#endif // UNITY_EDITOR
        }
    }
}
