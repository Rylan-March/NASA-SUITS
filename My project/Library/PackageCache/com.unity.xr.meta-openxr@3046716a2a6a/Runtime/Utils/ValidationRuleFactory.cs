#if UNITY_EDITOR
using System;
using UnityEditor;
using ValidationRule = UnityEngine.XR.OpenXR.Features.OpenXRFeature.ValidationRule;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Validation rule generator for shared validation rules between multiple features.
    /// </summary>
    static class ValidationRuleFactory
    {
        internal static ValidationRule CreateARSessionValidationRule(OpenXRFeature feature, BuildTargetGroup buildTarget)
        {
            return new(feature)
            {
                message = $"<b>{ARSessionFeature.displayName}</b> is a required dependency of this feature.",
                checkPredicate = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    var targetFeature = settings.GetFeature(feature.GetType());
                    var sessionFeature = settings.GetFeature<ARSessionFeature>();
                    return targetFeature.enabled == false || (targetFeature.enabled && sessionFeature.enabled);
                },
                fixItAutomatic = true,
                fixItMessage = $"Go to <b>Project Settings</b> > <b>XR Plug-in Management</b> > <b>OpenXR</b> > <b>{buildTarget}</b> tab. In the list of OpenXR Features, enable <b>{ARSessionFeature.displayName}</b>.",
                fixIt = () =>
                {
                    var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
                    var sessionFeature = settings.GetFeature<ARSessionFeature>();
                    sessionFeature.enabled = true;
                },
                error = true
            };
        }
    }
}
#endif
