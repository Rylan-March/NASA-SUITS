using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils.Editor;
#if URP_7_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif // URP_7_OR_NEWER

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    static class SoftShadowsMetaOpenXRValidationRules
    {
        const string k_Category = "Meta Quest Support";
        const string k_ValidationMessage = "[Optional] Soft shadows can negatively impact performance on Meta Quest, disabling soft shadows is recommended";
        const string k_FixItMessage =
@"When using the Built-In Render Pipeline, enable hard shadows only.

When using the Universal Render Pipeline, open the Render Pipeline Asset in Editor for modification.";

        [InitializeOnLoadMethod]
        static void AddSoftShadowsMetaOpenXRValidationRules()
        {
            var softShadowsMetaOpenXRRules = new[]
            {
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = k_ValidationMessage,
                    IsRuleEnabled = () =>
                    {
                        return XRManagerEditorUtility.IsMetaOpenXRTheActiveBuildTarget() && SoftShadowsEnabled();
                    },
                    CheckPredicate = () => { return false; },
                    FixItMessage = k_FixItMessage,
                    FixIt = SoftShadowFixItButtonPress,
                    Error = false
                }
            };
            BuildValidator.AddRules(BuildTargetGroup.Android, softShadowsMetaOpenXRRules);
        }

        private static bool SoftShadowsEnabled()
        {
            RenderPipelineAsset currentRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;

            // If current render pipeline is Built-In Render Pipeline
            if (currentRenderPipelineAsset == null)
            {
                return QualitySettings.shadows == UnityEngine.ShadowQuality.All;
            }
#if URP_7_OR_NEWER
            UniversalRenderPipelineAsset urpAsset = currentRenderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                return urpAsset.supportsSoftShadows;
            }
#endif // URP_7_OR_NEWER

            Debug.LogWarning("Unable to determine if soft shadows are enabled.");
            return false;
        }

        private static void SoftShadowFixItButtonPress()
        {
            RenderPipelineAsset currentRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;

            // If current render pipeline is Built-In Render Pipeline
            if (currentRenderPipelineAsset == null)
            {
                QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
                return;
            }
#if URP_7_OR_NEWER
            UniversalRenderPipelineAsset urpAsset = currentRenderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                var urpAssetID = urpAsset.GetInstanceID();
                if (AssetDatabase.CanOpenAssetInEditor(urpAssetID))
                    AssetDatabase.OpenAsset(urpAssetID);
                else
                    Debug.LogWarning("Unable to open URP asset in Editor.");
                return;
            }
#endif // URP_7_OR_NEWER

            Debug.LogWarning("Unable to disable soft shadows.");
        }
    }
}
