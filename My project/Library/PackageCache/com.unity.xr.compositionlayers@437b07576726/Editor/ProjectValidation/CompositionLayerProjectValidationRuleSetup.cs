using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using System;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CoreUtils.Editor;

namespace UnityEditor.XR.CompositionLayers.Editor.ProjectValidation
{
    /// <summary>
    /// Project validation rule class for composition layer package.
    /// </summary>
    public class CompositionLayerProjectValidationRuleSetup
    {
        static BuildTargetGroup[] s_BuildTargetGroups =
            ((BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup))).Distinct().ToArray();

        internal const string CompositionLayersProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";

        internal static string PackageId = "com.unity.xr.compositionlayers";

        [InitializeOnLoadMethod]
        static void CompositionLayersProjectValidationCheck()
        {
            UnityEditor.PackageManager.Events.registeredPackages += (packageRegistrationEventArgs) =>
            {
                // In the Player Settings UI we have to delay the call one frame to let CompositionLayersSettings constructor to get initialized
                EditorApplication.delayCall += () =>
                {
                    if (HasXRPackageVersionChanged(packageRegistrationEventArgs))
                    {
                        ShowWindowIfIssuesExist();
                    }
                };
            };
            AddCompositionLayersValidationRules();
        }

        /// <summary>
        /// Checks if Composition Layers package version has changed or been updated
        /// </summary>
        private static bool HasXRPackageVersionChanged(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            bool packageChanged = packageRegistrationEventArgs.changedTo.Any(p => p.name.Equals(PackageId));
            return packageRegistrationEventArgs.added.Any(p => p.name.Equals(PackageId)) || packageChanged;
        }

        /// <summary>
        /// Opens Project Validation Window to Display only Failed Composition Layers Validation Rules
        /// </summary>
        private static void ShowWindowIfIssuesExist()
        {
            List<CompositionLayerBuildValidationRule> failures = new List<CompositionLayerBuildValidationRule>();
            BuildTargetGroup activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            CompositionLayerProjectValidation.GetCurrentValidationIssues(failures, activeBuildTargetGroup);

            if (failures.Count > 0)
            {
                ShowWindow();
            }
        }

        /// <summary>
        /// Adds Composition Layers validation rules to the Project Validator
        /// </summary>
        static void AddCompositionLayersValidationRules()
        {
            foreach (var buildTargetGroup in s_BuildTargetGroups)
            {
                var issues = new List<CompositionLayerBuildValidationRule>();
                CompositionLayerProjectValidation.GetAllValidationIssues(issues, buildTargetGroup);

                var coreIssues = new List<BuildValidationRule>();
                foreach (var issue in issues)
                {
                    var rule = new BuildValidationRule
                    {
                        // This will hide the rules given a condition so that when you click "Show all" it doesn't show up as passed
                        IsRuleEnabled = () =>
                        {
                            // If Composition Layers isn't enabled, no need to show the rule
                            if (!CompositionLayerManager.ManagerActive)
                                return false;

                            return true;
                        },
                        CheckPredicate = issue.CheckPredicate,
                        Error = issue.Error,
                        FixIt = issue.FixIt,
                        FixItAutomatic = issue.FixItAutomatic,
                        FixItMessage = issue.FixItMessage,
                        HelpLink = issue.HelpLink,
                        HelpText = issue.HelpText,
                        Message = issue.Message,
                        Category = "CompositionLayers",
                        SceneOnlyValidation = false
                    };

                    coreIssues.Add(rule);
                }

                BuildValidator.AddRules(buildTargetGroup, coreIssues);
            }
        }

        [MenuItem("Window/XR/Composition Layers/Project Validation")]
        private static void MenuItem()
        {
            ShowWindow();
        }

        /// <summary>
        /// Opens Project Validation Window
        /// </summary>
        private static void ShowWindow()
        {
            // Delay opening the window since sometimes other settings in the player settings provider redirect to the
            // project validation window causing serialized objects to be nullified
            EditorApplication.delayCall += () =>
            {
                SettingsService.OpenProjectSettings(CompositionLayersProjectValidationSettingsPath);
            };
        }
    }
}
