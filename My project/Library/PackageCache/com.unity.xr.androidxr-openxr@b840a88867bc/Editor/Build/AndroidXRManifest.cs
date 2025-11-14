using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Android;
using UnityEngine.XR.OpenXR.Features.Interactions;

#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands.OpenXR;
#endif // XR_HANDS_1_1_OR_NEWER

namespace UnityEditor.XR.OpenXR.Features.Android
{
    class AndroidXRManifest : IPostGenerateGradleAndroidProject
    {
        static readonly string k_RelativeManifestPath = Path.Join(
            "xrmanifest.androidlib",
            "AndroidManifest.xml"
        );

        static readonly string k_OpenXRPermission = "org.khronos.openxr.permission.OPENXR";
        static readonly string k_SceneUnderstandingCoarsePermission = "android.permission.SCENE_UNDERSTANDING_COARSE";
        static readonly string k_SceneUnderstandingFinePermission = "android.permission.SCENE_UNDERSTANDING_FINE";
        static readonly string k_EyeTrackingCoarsePermission = "android.permission.EYE_TRACKING_COARSE";
        static readonly string k_EyeTrackingFinePermission = "android.permission.EYE_TRACKING_FINE";
        static readonly string k_UsesPermission = "uses-permission";
        static readonly string k_UsesFeature = "uses-feature";
        static readonly string k_UsesNativeLibrary = "uses-native-library";
        static readonly string k_ControllerHardwareFeature = "android.hardware.xr.input.controller";
        static readonly string k_HandTrackingHardwareFeature = "android.hardware.xr.input.hand_tracking";
        static readonly string k_EyeTrackingHardwareFeature = "android.hardware.xr.input.eye_tracking";
        static readonly string k_OpenXRSoftwareAPIFeature = "android.software.xr.api.openxr";
        static readonly string k_OpenXRSoftwareAPIFeatureVersion = "0x00010001";
        static readonly XNamespace k_Android = "http://schemas.android.com/apk/res/android";
        static readonly XName k_AndroidName = k_Android + "name";
        static readonly XName k_AndroidValue = k_Android + "value";
        static readonly XName k_AndroidRequired = k_Android + "required";
        static readonly XName k_AndroidVersion = k_Android + "version";
        static readonly XName k_AndroidAuthorities = k_Android + "authorities";
        static readonly string k_Property = "property";
        static readonly string k_Queries = "queries";
        static readonly string k_Provider = "provider";
        static readonly string k_False = "false";
        static readonly string k_StringArraySeparator = ";";
        static readonly string k_ActivityStartsFullSpaceUnmanaged = "XR_ACTIVITY_START_MODE_FULL_SPACE_UNMANAGED";

        static readonly string[] k_OpenXRQueriesProviders = new[]
        {
            "org.khronos.openxr.runtime_broker",
            "org.khronos.openxr.system_runtime_broker"
        };

        static readonly string k_XRActivityStartMode =
            "android.window.PROPERTY_XR_ACTIVITY_START_MODE";

        static readonly string k_OpenXRAndroidLibrary = "libopenxr.google.so";

#if XR_HANDS_1_1_OR_NEWER
        static readonly string k_HandTrackingPermission = "android.permission.HAND_TRACKING";
#endif // XR_HANDS_1_1_OR_NEWER

        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string gradleProjectPath)
        {
            if (!XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget())
                return;

            var fullManifestPath = Path.Join(gradleProjectPath, k_RelativeManifestPath);

            var xml = XDocument.Load(fullManifestPath);
            var manifest = xml.Root;
            Debug.Assert(manifest.Name == "manifest");

            var manifestApplication = GetManifestApplication(manifest);
            if (manifestApplication != null)
                AddElement(manifestApplication, k_UsesNativeLibrary, k_OpenXRAndroidLibrary, false);

            AddPermission(manifest, k_OpenXRPermission);

            var androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var arPlaneFeature = androidOpenXRSettings.GetFeature<ARPlaneFeature>();
            var arAnchorFeature = androidOpenXRSettings.GetFeature<ARAnchorFeature>();
            var arRaycastFeature = androidOpenXRSettings.GetFeature<ARRaycastFeature>();
            var arCameraFeature = androidOpenXRSettings.GetFeature<ARCameraFeature>();
            var arOcclusionFeature = androidOpenXRSettings.GetFeature<AROcclusionFeature>();
            if ((arPlaneFeature != null && arPlaneFeature.enabled) ||
                (arAnchorFeature != null && arAnchorFeature.enabled) ||
                (arRaycastFeature != null && arRaycastFeature.enabled) ||
                (arCameraFeature != null && arCameraFeature.enabled) ||
                (arOcclusionFeature != null && arOcclusionFeature.enabled))
            {
                AddPermission(manifest, k_SceneUnderstandingCoarsePermission);
            }

            if ((arOcclusionFeature != null && arOcclusionFeature.enabled) ||
                (arRaycastFeature != null && arRaycastFeature.enabled))
            {
                AddPermission(manifest, k_SceneUnderstandingFinePermission);
            }

#if XR_HANDS_1_1_OR_NEWER
            var handTrackingFeature = androidOpenXRSettings.GetFeature<HandTracking>();
            if (handTrackingFeature != null && handTrackingFeature.enabled)
            {
                AddPermission(manifest, k_HandTrackingPermission);
            }

#endif // XR_HANDS_1_1_OR_NEWER

            var foveatedRenderingFeature = androidOpenXRSettings.GetFeature<FoveatedRenderingFeature>();

            var faceFeature = androidOpenXRSettings.GetFeature<ARFaceFeature>();
            if ((faceFeature != null && faceFeature.enabled) || (foveatedRenderingFeature != null && foveatedRenderingFeature.enabled))
                AddPermission(manifest, k_EyeTrackingCoarsePermission);

            var gazeFeature = androidOpenXRSettings.GetFeature<EyeGazeInteraction>();
            if (gazeFeature != null && gazeFeature.enabled)
            {
                AddPermission(manifest, k_EyeTrackingFinePermission);
                AddFeature(manifest, k_EyeTrackingHardwareFeature);
            }
            else if (foveatedRenderingFeature != null && foveatedRenderingFeature.enabled)
            {
                AddPermission(manifest, k_EyeTrackingFinePermission);
            }

            var handInteractionFeature = androidOpenXRSettings.GetFeature<HandInteractionProfile>();
            if (handInteractionFeature != null && handInteractionFeature.enabled)
            {
                AddFeature(manifest, k_HandTrackingHardwareFeature);
            }

            var oculusTouchControllerFeature = androidOpenXRSettings.GetFeature<OculusTouchControllerProfile>();
            var khrSimpleControllerFeature = androidOpenXRSettings.GetFeature<KHRSimpleControllerProfile>();
            if ((oculusTouchControllerFeature != null && oculusTouchControllerFeature.enabled) || (khrSimpleControllerFeature != null && khrSimpleControllerFeature.enabled))
            {
                AddFeature(manifest, k_ControllerHardwareFeature);
            }

            AddFeature(manifest, k_OpenXRSoftwareAPIFeature, true, k_OpenXRSoftwareAPIFeatureVersion);
            AddQueries(manifest);
            AddProperties(manifest);

            xml.Save(fullManifestPath);
        }

        void AddElement(XElement container, string elementName, string value, bool required = true, string version = null)
        {
            var existingElement = container
                .Elements()
                .Any(x => x.Name == elementName
                    && x.Attribute(k_AndroidName).Value == value);

            if (existingElement)
                return;

            var elementContent = new List<object> { new XAttribute(k_AndroidName, value) };
            if (version != null)
                elementContent.Add(new XAttribute(k_AndroidVersion, version));
            if (!required)
                elementContent.Add(new XAttribute(k_AndroidRequired, k_False));

            container.Add(new XElement(elementName, elementContent.ToArray()));
        }

        void AddPermission(XElement manifest, string permission)
        {
            AddElement(manifest, k_UsesPermission, permission);
        }

        void AddFeature(XElement manifest, string feature, bool required = true, string version = null)
        {
            AddElement(manifest, k_UsesFeature, feature, required, version);
        }

        void AddQueries(XElement manifest)
        {
            if (manifest.Elements(k_Queries).Count() == 0)
                manifest.Add(new XElement(k_Queries));

            var queriesProviders = manifest
                .Elements(k_Queries)
                .SelectMany(x => x.Elements(k_Provider));
            if (
                !queriesProviders
                    .Select(
                        x => x.Attribute(k_AndroidAuthorities).Value.Split(k_StringArraySeparator[0])
                    )
                    .Where(
                        x =>
                            x.Intersect(k_OpenXRQueriesProviders).Count()
                            == k_OpenXRQueriesProviders.Count()
                    )
                    .Any()
            )
            {
                manifest
                    .Elements(k_Queries)
                    .First()
                    .Add(
                        new XElement(
                            k_Provider,
                            new XAttribute(
                                k_AndroidAuthorities,
                                string.Join(k_StringArraySeparator, k_OpenXRQueriesProviders)
                            )
                        )
                    );
            }
        }

        void AddProperties(XElement manifest)
        {
            var (onlyOne, launcherActivity) = GetLauncherActivity(manifest);
            if (!onlyOne)
                return;
            if (
                launcherActivity
                    .Elements(k_Property)
                    .Select(x => x.Attribute(k_AndroidName).Value == k_XRActivityStartMode)
                    .Any()
            )
                return;

            launcherActivity.Add(
                new XElement(
                    k_Property,
                    new XAttribute(k_AndroidName, k_XRActivityStartMode),
                    new XAttribute(k_AndroidValue, k_ActivityStartsFullSpaceUnmanaged)
                )
            );
        }

        XElement GetManifestApplication(XElement manifest)
        {
            var manifestApplication = manifest.Descendants("application");

            if (!manifestApplication.Any())
            {
                Debug.LogWarning("no application found in manifest");
                return null;
            }

            return manifestApplication.First();
        }

        (bool, XElement) GetLauncherActivity(XElement manifest)
        {
            var launcherActivities = manifest.Descendants("activity");
            if (!launcherActivities.Any())
            {
                Debug.LogWarning("no launcher activity found in manifest");
                return (false, null);
            }

            if (launcherActivities.Count() > 1)
            {
                Debug.LogWarning(
                    "more than one launcher activity found in manifest; this configuration may require custom handling"
                );
                return (false, null);
            }

            return (true, launcherActivities.First());
        }
    }
}
