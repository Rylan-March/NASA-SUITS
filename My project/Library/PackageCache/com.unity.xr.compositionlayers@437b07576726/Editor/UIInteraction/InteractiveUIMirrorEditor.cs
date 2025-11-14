#if !UNITY_XR_INTERACTION_TOOLKIT && UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Unity.XR.CompositionLayers.UIInteraction.Editor
{
    /// <summary>
    /// Flags a Warningn in the Inspector if the XR Interaction Toolkit is not installed.
    /// Directs Users to install the XR Interaction Toolkit.
    /// </summary>
    [CustomEditor(typeof(InteractableUIMirror))]
    public class InteractiveUIMirrorEditor : UnityEditor.Editor
    {
        private static AddRequest addPackageRequest;
        private static bool isInstalling = false;

        void Awake()
        {
            if (!isInstalling)
            {
                var allInteractiveUI = Resources.FindObjectsOfTypeAll<InteractableUIMirror>();
                if (allInteractiveUI.Length > 0)
                {
                    InstallXRIPackage();
                }
            }

        }

        /// <summary>
        /// Prompts users to installs the Unity XRI Package.
        /// </summary>
        public static void InstallXRIPackage()
        {
            isInstalling = true;
            bool userWantsToInstall = EditorUtility.DisplayDialog(
                "Install XR Interaction Toolkit",
                "The XR Interaction Toolkit is required for Composition Layers Interactive UI. Would you like to install it now?",
                "Yes", "No");

            if (userWantsToInstall)
            {
                addPackageRequest = Client.Add("com.unity.xr.interaction.toolkit");
                EditorApplication.update += Progress;
            }
            else
                isInstalling = false;
        }

        /// <summary>
        /// Tracks the progress of a Package installation.
        /// </summary>
        private static void Progress()
        {
            if (addPackageRequest.IsCompleted)
            {
                if (addPackageRequest.Status == StatusCode.Success)
                    Debug.Log("XRI Package installed successfully");
                else if (addPackageRequest.Status >= StatusCode.Failure)
                    Debug.LogError("Failed to install XRI Package: " + addPackageRequest.Error.message);

                EditorApplication.update -= Progress;
                isInstalling = false;
            }
        }

        /// <summary>
        /// Add a Warning to the Inspector if the XR Interaction Toolkit is not installed.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }

            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Installing the XR Interaction Toolkit is required for this component.", MessageType.Warning);
            if (GUILayout.Button("Install XRI Package"))
            {
               InstallXRIPackage();
            }
        }
    }
}
#endif
