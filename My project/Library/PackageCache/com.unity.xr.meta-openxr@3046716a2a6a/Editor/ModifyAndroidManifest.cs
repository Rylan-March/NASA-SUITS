using System;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;
using Unity.XR.Management.AndroidManifest.Editor;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    class ModifyAndroidManifestMeta : OpenXRFeatureBuildHooks
    {
        public override int callbackOrder => 1;
        public override Type featureType => typeof(ARSessionFeature);

        protected override void OnPreprocessBuildExt(BuildReport report) { }
        protected override void OnPostGenerateGradleAndroidProjectExt(string path) { }
        protected override void OnPostprocessBuildExt(BuildReport report) { }

        protected override ManifestRequirement ProvideManifestRequirementExt()
        {
            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var elementsToAdd = new List<ManifestElement>();

            var arCameraFeature = androidOpenXRSettings.GetFeature<ARCameraFeature>();
            if (arCameraFeature != null && arCameraFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-feature" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.feature.PASSTHROUGH" },
                            { "required", "true" },
                        }
                    }
                );
            }

            var arAnchorFeature = androidOpenXRSettings.GetFeature<ARAnchorFeature>();
            if (arAnchorFeature != null && arAnchorFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.IMPORT_EXPORT_IOT_MAP_DATA" },
                        }
                    }
                );
            }

            var arPlaneFeature = androidOpenXRSettings.GetFeature<ARPlaneFeature>();
            var arBoundingBoxFeature = androidOpenXRSettings.GetFeature<ARBoundingBoxFeature>();
            var arMeshFeature = androidOpenXRSettings.GetFeature<ARMeshFeature>();
            if ((arPlaneFeature != null && arPlaneFeature.enabled)
                || (arBoundingBoxFeature != null && arBoundingBoxFeature.enabled)
                || (arMeshFeature != null && arMeshFeature.enabled))
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.USE_SCENE" },
                        }
                    }
                );
            }

            // USE_ANCHOR_API is required for any feature that uses XR_FB_spatial_entity
            if ((arAnchorFeature != null && arAnchorFeature.enabled)
                || (arPlaneFeature != null && arPlaneFeature.enabled)
                || (arBoundingBoxFeature != null && arBoundingBoxFeature.enabled)
                || (arMeshFeature != null && arMeshFeature.enabled))
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.USE_ANCHOR_API" },
                        }
                    }
                );
            }

            var boundaryVisibilityFeature = androidOpenXRSettings.GetFeature<BoundaryVisibilityFeature>();
            if (boundaryVisibilityFeature != null && boundaryVisibilityFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.BOUNDARY_VISIBILITY" },
                        }
                    }
                );
            }

            var colocationDiscoveryFeature = androidOpenXRSettings.GetFeature<ColocationDiscoveryFeature>();
            if (colocationDiscoveryFeature != null && colocationDiscoveryFeature.enabled)
            {
                elementsToAdd.Add(
                    new ManifestElement
                    {
                        ElementPath = new List<string> { "manifest", "uses-permission" },
                        Attributes = new Dictionary<string, string>
                        {
                            { "name", "com.oculus.permission.USE_COLOCATION_DISCOVERY_API" },
                        }
                    }
                );
            }

            return new ManifestRequirement
            {
                SupportedXRLoaders = new HashSet<Type>()
                {
                    typeof(OpenXRLoader)
                },
                NewElements = elementsToAdd
            };
        }
    }
}
