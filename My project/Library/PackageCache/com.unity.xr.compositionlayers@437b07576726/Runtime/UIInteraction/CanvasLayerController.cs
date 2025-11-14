#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Helper for creating and deleting Composition Layer Canvas layers
    /// </summary>
    internal class CanvasLayerController : IDisposable, IPreprocessBuildWithReport
    {
        // Prefix to add before the created layer
        private const string CanvasLayerTagPrefix = "Canvas_";

        // Tag Manager for tag layer creation
        private TagManagerController tagManager;

        // current tag
        private string canvasLayerTag = null;

        public int callbackOrder => Int32.MaxValue;

        /// <summary>
        /// Initialization
        /// Subscribes to quitting event to remove layers on exit
        /// </summary>
        public CanvasLayerController()
        {
            tagManager = new TagManagerController();
            EditorApplication.quitting += OnEditorQuit;
        }

        /// <summary>
        /// Unsubscribes from quitting event on dispose
        /// </summary>
        public void Dispose()
        {
            EditorApplication.quitting -= OnEditorQuit;
        }

        /// <summary>
        /// Creates a layer for supplied Canvas
        /// </summary>
        /// <param name="canvas">Canvas to create layer for</param>
        public void CreateAndSetCanvasLayer(Canvas canvas)
        {
            canvasLayerTag = CanvasLayerTagPrefix + canvas.GetInstanceID().ToString();
            if (!tagManager.TryAddLayer(canvasLayerTag))
            {
                Debug.LogError("Unable to add new canvas layer, try removing some unused layers in Project Settings to make space.");
                return;
            }
            int canvasLayerBit = LayerMask.NameToLayer(canvasLayerTag);
            ChangeLayerOfAllChildren(canvas.gameObject, canvasLayerBit);

            // Remove canvas layer from all cameras
            Tools.visibleLayers &= ~(1 << canvasLayerBit);
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                if (camera.gameObject.layer == canvasLayerBit)
                    continue;

                camera.cullingMask &= ~(1 << canvasLayerBit);
            }
        }

        /// <summary>
        /// Sets Canvas back to default layer before CreateAndSetCanvasLayer
        /// </summary>
        /// <param name="canvas">Canvas to set layer back to default</param>
        /// <seealso cref="CreateAndSetCanvasLayer"/>
        public void SetCanvasLayerToDefault(Canvas canvas)
        {
            tagManager.RemoveLayer(canvasLayerTag);

            // Remove layer and set canvas to default layer
            if (canvas != null)
            {
                var defaultLayerBit = LayerMask.NameToLayer("Default");
                ChangeLayerOfAllChildren(canvas.gameObject, defaultLayerBit);
            }
        }

        /// <summary>
        /// Changes layer of all children to specified layer
        /// </summary>
        /// <param name="gameObj">GameObject to change all children of</param>
        /// <param name="layerBit">Layer to change children to</param>
        private void ChangeLayerOfAllChildren(GameObject gameObj, int layerBit)
        {
            gameObj.layer = layerBit;
            foreach (Transform t in gameObj.transform)
                ChangeLayerOfAllChildren(t.gameObject, layerBit);
        }

        /// <summary>
        /// Removes layers on quit
        /// </summary>
        private void OnEditorQuit()
        {
            tagManager.RemoveLayer(canvasLayerTag);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            tagManager = new TagManagerController();
            tagManager.RemoveAllLayersContaining(CanvasLayerTagPrefix);
        }
    }
}
#endif
