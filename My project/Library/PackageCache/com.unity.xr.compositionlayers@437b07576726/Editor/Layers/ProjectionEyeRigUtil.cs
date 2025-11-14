using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// Supports workflow for creating projection eye rig.
    /// </summary>
    public class ProjectionEyeRigUtil : EditorWindow
    {
        private string newLayerName = ""; // Stores the new layer name
        private static GameObject projectionObj;

        /// <summary>
        /// Show pop-up window for creating new projection eye rig.
        /// </summary>
        /// <param name="obj">Current gameobject</param>
        public static void ShowWindow(GameObject obj)
        {
            projectionObj = obj;
            ProjectionEyeRigUtil window = (ProjectionEyeRigUtil)EditorWindow.GetWindow(typeof(ProjectionEyeRigUtil));
            window.titleContent = new GUIContent("New Projection Eye Rig");
            window.minSize = new Vector2(320f, 120f);
            window.maxSize = new Vector2(320f, 120f);
            window.Show();
        }

        /// <summary>
        /// Close window for creating projection eye rig.
        /// </summary>
        public static void CloseWindow()
        {
            projectionObj = null;
            ProjectionEyeRigUtil window = (ProjectionEyeRigUtil)EditorWindow.GetWindow(typeof(ProjectionEyeRigUtil));
            window.Close();
        }

        private void OnGUI()
        {
            var iconSize = EditorGUIUtility.GetIconSize();

            int padding = 8;
            Rect area = new Rect(padding, padding, position.width - (padding * 2), position.height - (padding * 2));

            GUILayout.BeginArea(area);

            EditorGUILayout.HelpBox("Projection Layer Eye Rig will automatically create cameras for each eye, and setup appropriate culling masks based on your Layer Name.", MessageType.Info);

            GUILayout.Label("Layer name:");
            newLayerName = EditorGUILayout.TextField(newLayerName);

            if (GUILayout.Button("Create"))
            {
                bool result = SetUp();
                //Close window after renaming.
                if (result)
                {
                    ProjectionEyeRigUtil window = (ProjectionEyeRigUtil)EditorWindow.GetWindow(typeof(ProjectionEyeRigUtil));
                    window.Close();
                }
            }

            GUILayout.EndArea();

        }

        private bool SetUp()
        {
            if (string.IsNullOrEmpty(newLayerName))
            {
                Debug.LogWarning("Failed to create layer for Projection Layer Eye Rig: layer name can't be empty.");
                return false;
            }
            if (projectionObj == null)
                return false;
            //Create layer name
            int layerIndex = AddLayerIDForProjectionEyeRig(newLayerName);
            Tools.visibleLayers |= 1 << LayerMask.NameToLayer(newLayerName);
            projectionObj.layer = layerIndex;
            projectionObj.name = newLayerName;
            //Set all the child objects to layerIndex
            foreach (Transform child in projectionObj.transform)
            {
                child.gameObject.layer = layerIndex;
            }
            // if found main camera, then eliminate layer from cullingMask
            var mainCamera = CompositionLayerManager.mainCameraCache;
            float cameraDepth = 0;
            if (mainCamera != null)
            {
                mainCamera.cullingMask &= ~(1 << layerIndex);
                Debug.LogFormat("Remove {0} from Main Camera cullingmask.", newLayerName);
                cameraDepth = mainCamera.depth - 1;
            }

            CompositionLayer Layer = projectionObj.GetComponent<CompositionLayer>();
            if (Layer != null)
            {
                //Set render textures to cameras render targets
                Camera leftCam = projectionObj.transform.GetChild(0).GetComponent<Camera>();
                Camera rightCam = projectionObj.transform.GetChild(1).GetComponent<Camera>();
                leftCam.cullingMask = 1 << layerIndex;
                rightCam.cullingMask = 1 << layerIndex;
                leftCam.clearFlags = CameraClearFlags.SolidColor;
                rightCam.clearFlags = CameraClearFlags.SolidColor;
                leftCam.backgroundColor = Color.clear;
                rightCam.backgroundColor = Color.clear;
                leftCam.depth = cameraDepth;
                rightCam.depth = cameraDepth;
            }

            return true;
        }

        private int AddLayerIDForProjectionEyeRig(string layerName)
        {
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            // First check if it is not already present
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                SerializedProperty l = layersProp.GetArrayElementAtIndex(i);
                if (l.stringValue.Equals(layerName))
                {
                    Debug.LogFormat("Layer : {0} already existed in the Layers setting.", layerName);
                    return i;
                }
            }

            // if not found, add it
            int index = 0;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layer = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    index = i;
                    break;
                }
            }

            SerializedProperty newLayerProperty = layersProp.GetArrayElementAtIndex(index);
            newLayerProperty.stringValue = layerName;

            tagManager.ApplyModifiedProperties();
            return index;
        }
    }
}
