using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Editor;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Services;
using UnityEditor;
using UnityEngine;
#if UNITY_XR_INTERACTION_TOOLKIT
using UnityEngine.UI;
#endif
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using Unity.XR.CompositionLayers.Services.Editor;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// Utility class for for interacting with Composition Layers.
    /// </summary>
    [InitializeOnLoad]
    static class CompositionLayerEditorUtils
    {
        const string k_EmptyLayerName = "[Empty]";
        const string k_UnknownLayerName = "(UnKnown) - ";

        internal const string k_UnsupportedUnderlayMessage = "Underlay isn't supported on {0}.";
        internal const string k_UnsupportedLayerTypeMessage = "This layer type isn't supported on {0}.";

        internal static List<string> UILayerNames = new();

        internal static List<string> LayerNames = new();

        internal static List<CompositionLayer> SortedLayers = new List<CompositionLayer>();

        static CompositionLayerEditorUtils()
        {
            foreach (var descriptor in CompositionLayerUtils.GetAllLayerDescriptors())
            {
                // Hides these layers from the list of options in the Layer Type dropdown
                if (descriptor.DataType == typeof(ProjectionLayerRigData) || descriptor.DataType == typeof(DefaultLayerData))
                    continue;

                if (descriptor.DataType == typeof(QuadLayerData) || descriptor.DataType == typeof(CylinderLayerData))
                    UILayerNames.Add(descriptor.TypeFullName);

                LayerNames.Add(descriptor.TypeFullName);
            }

            Undo.undoRedoPerformed += UndoRedoPerformed;
            CompositionLayerUtils.SetOrderValueInEditor = SetOrderValueInEditor;
            AssemblyReloadEvents.afterAssemblyReload += CheckCompositionLayerManagerRunningInEditor;
        }

        /// <summary>
        /// Creates a GameObject with a <see cref="CompositionLayer"/> component set with the <see cref="LayerData"/>
        /// referenced in the <param name="dataType"></param> and any Suggested Extension Types created for that
        /// <see cref="LayerData"/> type.
        /// </summary>
        /// <param name="dataType">The <see cref="LayerData"/> type used to create the GameObject.</param>
        /// <param name="rotateFacing">If <c>true</c> the new GameObject's positive z-axis will face the camera.</param>
        public static void CreateLayerGameObjectMenuItem(Type dataType, bool rotateFacing = false)
        {
            var lastSceneView = SceneView.lastActiveSceneView;
            var position = lastSceneView == null ? Vector3.zero : lastSceneView.pivot;

            var go = new GameObject();
            go.transform.position = position;

            if (rotateFacing)
                go.transform.rotation = Quaternion.LookRotation(Vector3.back);

            CreateCompositionLayerComponents(go, dataType, false);
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(dataType);
            var name = descriptor.Name;
            if (!name.Contains("Layer", StringComparison.CurrentCultureIgnoreCase))
                name = $"{name} Layer";

            go.name = name;

            Undo.RegisterCreatedObjectUndo(go, $"Created {name}");
            Selection.activeGameObject = go;
        }

        public static void AddCanvasToActiveGameObject()
        {
            // Create a Canvas GameObject
            GameObject canvasGameObject = new GameObject("Canvas");
            Canvas canvas = canvasGameObject.AddComponent<Canvas>();
            canvasGameObject.transform.SetParent(Selection.activeGameObject.transform, false);
            canvasGameObject.transform.localPosition = Vector3.zero;
            canvasGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 500f);
            Undo.RegisterCreatedObjectUndo(canvasGameObject, "Created canvas");
        }

        static CompositionLayer CreateCompositionLayerComponents(GameObject gameObject, Type dataType, bool recordUndo)
        {
            if (gameObject == null)
                return null;

            gameObject.SetActive(false);
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(dataType);

            CompositionLayer layer;
            if (recordUndo)
                layer = Undo.AddComponent<CompositionLayer>(gameObject);
            else
                layer = gameObject.AddComponent<CompositionLayer>();

            if (layer == null)
                return null;

            var layerData = CompositionLayerUtils.CreateLayerData(dataType);
            layer.ChangeLayerDataType(layerData);
            foreach (var extension in descriptor.SuggestedExtensions)
            {
                if (extension.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    if (recordUndo)
                        Undo.AddComponent(gameObject, extension);
                    else
                        gameObject.AddComponent(extension);
                }
            }

            if (dataType == typeof(ProjectionLayerRigData))
            {
                CreateProjectionLayerRig(gameObject);
            }

            gameObject.SetActive(true);
            return layer;
        }

        public static void CreateProjectionLayerRig(GameObject gameObj)
        {
            var leftCamGo = ObjectFactory.CreateGameObject("Left Camera", typeof(Camera));
            var rightCamGo = ObjectFactory.CreateGameObject("Right Camera", typeof(Camera));
            leftCamGo.transform.parent = gameObj.transform;
            rightCamGo.transform.parent = gameObj.transform;
            leftCamGo.transform.SetLocalPose(Pose.identity);
            rightCamGo.transform.SetLocalPose(Pose.identity);

            leftCamGo.AddComponent<TrackedPoseDriver>();
            rightCamGo.AddComponent<TrackedPoseDriver>();

            //Set Left eye and rigt eye target for trackedPoseDriver
            var leftPoseDriver = leftCamGo.GetComponent<TrackedPoseDriver>();
            InputAction leftEyePos = new InputAction("LeftEyePosition");
            leftEyePos.AddBinding("<XRHMD>/leftEyePosition");
            leftPoseDriver.positionAction = leftEyePos;
            InputAction leftEyeRot = new InputAction("LeftEyeRotation");
            leftEyeRot.AddBinding("<XRHMD>/leftEyeRotation");
            leftPoseDriver.rotationAction = leftEyeRot;

            var rightPoseDriver = rightCamGo.GetComponent<TrackedPoseDriver>();
            InputAction rightEyePos = new InputAction("RightEyePosition");
            rightEyePos.AddBinding("<XRHMD>/rightEyePosition");
            rightPoseDriver.positionAction = rightEyePos;
            InputAction rightEyeRot = new InputAction("RightEyeRotation");
            rightEyeRot.AddBinding("<XRHMD>/rightEyeRotation");
            rightPoseDriver.rotationAction = rightEyeRot;

            //hide Source Texture component
            TexturesExtension texExt = gameObj.GetComponent<TexturesExtension>();
            if (texExt == null) texExt = gameObj.AddComponent<TexturesExtension>();
            texExt.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;

            ProjectionEyeRigUtil.ShowWindow(gameObj);
        }

        public static void CreateLayerComponentMenuItem(Type dataType)
        {
            var gameObject = Selection.activeGameObject;
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(dataType);
            var name = descriptor.Name;

            Undo.RecordObject(gameObject, $"Add {name} Composition Layer Component");
            CreateCompositionLayerComponents(gameObject, dataType, true);
        }

        public static bool ValidateCreateLayerComponentMenuItem()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject == null)
                return false;

            var layer = gameObject.GetComponent(typeof(CompositionLayer));
            if (layer != null)
                return false;

            return true;
        }

        static void UndoRedoPerformed()
        {
            if (!CompositionLayerManager.ManagerActive)
                return;

            CompositionLayerManager.Instance.FindAllLayersInScene();
        }

        /// <summary>
        /// Gets a formatted name for the <see cref="LayerData"/> type based on the <see cref="CompositionLayerDataAttribute"/>
        /// </summary>
        /// <param name="typeFullName">The <see cref="LayerData"/> type name defined in the
        /// <see cref="CompositionLayerDataAttribute"/></param>
        /// <returns>Display name in th format of "<see cref="CompositionLayerDataAttribute.Provider"/> - <see cref="CompositionLayerDataAttribute.Name"/></returns>
        public static string GetFormattedLayerName(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
                return k_EmptyLayerName;

            var layerDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeFullName);

            if (!layerDescriptor.Equals(default) && !layerDescriptor.Equals(LayerDataDescriptor.Empty))
            {
                var providerName = layerDescriptor.Provider != "Unity" ?  $"{layerDescriptor.Provider} - " : "";
                var layerName = $"{providerName}{layerDescriptor.Name}";
                if (EditorPlatformManager.IsSupportedLayerDataAllPlatforms(typeFullName))
                {
                    return layerName;
                }
                else
                {
                    return $"! {layerName}";
                }
            }
            else
            {
                return $"{k_UnknownLayerName}{typeFullName}";
            }
        }

        internal static void GetKnownOccupiedLayersFromManager()
        {
            if (CompositionLayerManager.ManagerActive == false)
            {
                SortedLayers.Clear();
                return;
            }

            CompositionLayerManager.GetOccupiedLayers(SortedLayers);
            SortedLayers.Sort(LayerSorter);
        }

        static int LayerSorter(CompositionLayer lhs, CompositionLayer rhs)
        {
            if (lhs.Order == rhs.Order) return 1;
            if (lhs.Order < rhs.Order) return -1;
            return 1;
        }

        internal static void SetOrderInEditor(CompositionLayer layer, int oldOrder, int newOrder)
        {
            if (layer.TryChangeLayerOrder(oldOrder, newOrder))
                return;

            if (layer.TryChangeLayerOrder(oldOrder, oldOrder))
                return;

            var orderValue = CompositionLayerManager.GetNextUnusedLayer(newOrder);
            layer.TryChangeLayerOrder(oldOrder, orderValue);
        }

        static void SetOrderValueInEditor(this CompositionLayer layer, int order)
        {
            var serializedObject = new SerializedObject(layer);
            var orderProp = serializedObject.FindProperty("m_Order");
            orderProp.intValue = order;

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var targetObject in serializedObject.targetObjects)
                {
                    if (targetObject is CompositionLayer compositionLayer)
                        compositionLayer.ReportStateChange();
                }
            }
        }

        internal static Texture2D GetInspectorIcon(string typeFullName)
        {
            var layerDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeFullName);

            if (string.IsNullOrEmpty(layerDescriptor.IconPath) || string.IsNullOrEmpty(layerDescriptor.InspectorIcon))
                return GUIHelpers.LoadIcon(CompositionLayerConstants.IconPath, "LayerGeneric");

            return GUIHelpers.LoadIcon(layerDescriptor.IconPath, layerDescriptor.InspectorIcon);
        }

        internal static Texture2D GetMaxSizeInspectorIcon(string typeFullName)
        {
            var layerDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeFullName);

            if (string.IsNullOrEmpty(layerDescriptor.IconPath) || string.IsNullOrEmpty(layerDescriptor.InspectorIcon))
                return GUIHelpers.LoadIconMaxSize(CompositionLayerConstants.IconPath, "LayerGenericColor");

            return GUIHelpers.LoadIconMaxSize(layerDescriptor.IconPath, layerDescriptor.InspectorIcon);
        }

        internal static Texture2D GetListViewIcon(string typeFullName)
        {
            var layerDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeFullName);

            // Try to fall back to the inspector icon
            if (string.IsNullOrEmpty(layerDescriptor.IconPath) || string.IsNullOrEmpty(layerDescriptor.ListViewIcon))
                return GetInspectorIcon(typeFullName);

            return GUIHelpers.LoadIcon(layerDescriptor.IconPath, layerDescriptor.ListViewIcon);
        }

        static void CheckCompositionLayerManagerRunningInEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            var compositionLayers = UnityEngine.Object.FindObjectsByType<CompositionLayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var compositionLayer in compositionLayers)
            {
                if (CompositionLayerManager.IsLayerSceneValid(compositionLayer))
                {
                    CompositionLayerManager.StartCompositionLayerManager();
                    compositionLayer.InitializeLayerOrder();
                }
            }
        }

        /// <summary>
        /// Clamp the input value between [0,1].
        /// </summary>
        /// <param name="v">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        internal static float Clamp01(float v)
        {
            return Math.Max(0, Math.Min(1.0f, v));
        }

        /// <summary>
        /// Clamp all fields on the input vector to between [0, 1].
        /// </summary>
        /// <param name="v">The vector to clamp.</param>
        /// <returns>A new vector with clamped fields.</returns>
        internal static Vector3 Clamp01(Vector3 v)
        {
            v.x = Clamp01(v.x);
            v.y = Clamp01(v.y);
            v.z = Clamp01(v.z);
            return v;
        }

        /// <summary>
        /// Clamp all fields on the input vector to between [0, 1].
        /// </summary>
        /// <param name="v">The vector to clamp.</param>
        /// <returns>A new vector with clamped fields.</returns>
        internal static Vector4 Clamp01(Vector4 v)
        {
            v.x = Clamp01(v.x);
            v.y = Clamp01(v.y);
            v.z = Clamp01(v.z);
            v.w = Clamp01(v.w);
            return v;
        }

        /// <summary>
        /// Clamp all fields on the input color to between [0, 1].
        /// </summary>
        /// <param name="c">The color to clamp.</param>
        /// <returns>A new color with clamped fields.</returns>
        internal static Color Clamp01(Color c)
        {
            c.r = Clamp01(c.r);
            c.g = Clamp01(c.g);
            c.b = Clamp01(c.b);
            c.a = Clamp01(c.a);
            return c;
        }

        /// <summary>
        /// Clamp all fields on the input rect to between [0, 1]. Will adjust
        /// the (x,y) of the rectangle to make sure that the rect bounds
        /// are within [0,1].
        /// </summary>
        /// <param name="r">The rect to clamp.</param>
        /// <returns>A new rect with clamped fields.</returns>
        internal static Rect Clamp01(Rect r)
        {
            r.x = Clamp01(r.x);
            r.y = Clamp01(r.y);
            r.width = Clamp01(r.width);
            r.height = Clamp01(r.height);

            if (r.x + r.width > 1.0f)
                r.x = 1.0f - r.width;

            if (r.y + r.height > 1.0f)
                r.y = 1.0f - r.height;
            return r;
        }
    }
}
