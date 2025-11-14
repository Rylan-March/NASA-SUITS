using System;
using Unity.XR.CoreUtils.Editor;

namespace UnityEditor.XR.CompositionLayers.Editor.ProjectValidation
{
    /// <summary>
    /// Project validation rule class for composition layer package.
    /// </summary>
    public class CompositionLayerBuildValidationRule : BuildValidationRule
    {

        /// <summary>
        /// Used to Determine which Build Target to display the Validation Rules for.
        /// Unknown is used to display the rules for all Build Targets.
        /// </summary>
        internal BuildTargetGroup buildTargetGroup = BuildTargetGroup.Unknown;
    }
}
