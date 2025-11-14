namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class BoundaryVisibilitySample
    {
        public void ExampleSetBoundaryVisibility()
        {
            #region boundary_visibility
            var feature = OpenXRSettings.Instance.GetFeature<BoundaryVisibilityFeature>();
            var result = feature.TryRequestBoundaryVisibility(
                XrBoundaryVisibility.VisibilitySuppressed);

            if ((int)result ==
                BoundaryVisibilityFeature.XR_BOUNDARY_VISIBILITY_SUPPRESSION_NOT_ALLOWED_META)
            {
                // The runtime did not accept the request to suppress the boundary.
                // Your app must render passthrough to suppress boundary visibility.
            }

            if (result < 0)
            {
                // XrResult values less than zero are errors.
                // Handle error here.
            }
            #endregion
        }
    }
}
