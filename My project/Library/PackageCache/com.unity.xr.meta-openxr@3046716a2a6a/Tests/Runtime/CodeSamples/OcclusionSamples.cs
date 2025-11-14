using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class OcclusionSamples
    {
        #region SetHandRemovalEnabled
        public void SetHandRemovalEnabled(AROcclusionManager manager)
        {
            var subsystem = manager.subsystem as MetaOpenXROcclusionSubsystem;
            var result = subsystem.TrySetHandRemovalEnabled(true);
            if (result.IsError())
            {
                // Handle error
            }
        }
        #endregion
    }
}
