using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Extensions;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Base class for handling quad / cylinder collider generation and maintaining aspect ratio with Canvas changes
    /// </summary>
    [ExecuteInEditMode]
    public class LayerUIScale : MonoBehaviour
    {
        /// <summary>
        /// Reference to TexturesExtension for cropping and stretching
        /// </summary>
        protected TexturesExtension texturesExtension;

        /// <summary>
        /// Reference to RectTransform to check for size or scale changes
        /// </summary>
        protected RectTransform canvasRect;

        /// <summary>
        /// Cached RectTransform's sizeDelta.x
        /// </summary>
        protected float canvasSizeX = -1;

        /// <summary>
        /// Cached RectTransform's sizeDelta.y
        /// </summary>
        protected float canvasSizeY = -1;

        /// <summary>
        /// Cached RectTransform's localScale.x
        /// </summary>
        protected float canvasScaleX = -1;

        /// <summary>
        /// Cached RectTransform's localScale.y
        /// </summary>
        protected float canvasScaleY = -1;

        /// <summary>
        /// Scalar of height for transform recalculations
        /// </summary>
        public float HeightScale;

        /// <summary>
        /// Scalar of width  for transform recalculations
        /// </summary>
        public float WidthScale;

        /// <summary>
        /// Calculates the HeightScale and WidthScale based on the composition layer's aspect, the canvas size, and canvas scale.
        /// </summary>
        /// <param name="aspect">Aspect ratio of Layer</param>
        public void UpdateDestinationRectScale(float aspect)
        {
            if (texturesExtension == null && !TryGetComponent<TexturesExtension>(out texturesExtension)) return;

            texturesExtension.CustomRects = true;

            // Canvas Aspect Ratio
            float canvasScaleAspect = canvasScaleX / canvasScaleY;
            float canvasAspect = (canvasSizeX / canvasSizeY) * canvasScaleAspect;

            // Set destination rect values based on the canvas vs cylinder aspect ratio
            bool canvasAspectLarger = canvasAspect > aspect;

            // Calculate destination rect H and Y value to maintain aspect ratio
            var H = canvasAspectLarger ? aspect / canvasAspect : 1.0f;
            float Y = canvasAspectLarger ? (1.0f - H) / 2.0f : 0.0f;

            // Calculate destination rect W and X value to maintain aspect ratio
            var W = !canvasAspectLarger ? canvasAspect / aspect : 1.0f;
            float X = !canvasAspectLarger ? (1.0f - W) / 2.0f : 0.0f;

            // Apply Width, Height, X, and Y to source and destination rects
            texturesExtension.LeftEyeDestinationRect = new Rect(X, Y, W, H);

            HeightScale = H;
            WidthScale = W;
        }

        /// <summary>
        /// Check whether or not the canvas has changed in size or scale.
        /// </summary>
        /// <returns>Whether or not the canvas rect has been changed (size or scale)</returns>
        protected bool CanvasAdjusted()
        {
            if (canvasRect == null && !transform.GetChild(0).TryGetComponent<RectTransform>(out canvasRect)) return false;

            if (canvasSizeX != canvasRect.sizeDelta.x || canvasSizeY != canvasRect.sizeDelta.y
                || canvasScaleX != canvasRect.localScale.x || canvasScaleY != canvasRect.localScale.y)
            {
                canvasSizeX = canvasRect.sizeDelta.x;
                canvasSizeY = canvasRect.sizeDelta.y;
                canvasScaleX = canvasRect.localScale.x;
                canvasScaleY = canvasRect.localScale.y;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates a Vector3 scalar to help convert a point from world space to the local space.
        /// Mainly used for the UIHandle positioning.
        /// </summary>
        /// <returns>Returns a Vector3 scalar to transform a point on a Canvas to a point on the Layer</returns>
        public virtual Vector3 GetUIScale() => Vector3.one;

        /// <summary>
        /// Inverts the UI Scale from GetUIScale for inverse scaling
        /// Mainly used for the UIHandle positioning
        /// </summary>
        /// <returns>Returns a Vector3 scalar to transform a point on a Canvas to a point on the Layer</returns>
        /// <seealso cref="GetUIScale"/>
        public Vector3 GetInverseUIScale() => new Vector3(1f / GetUIScale().x, 1f / GetUIScale().y, 1f / GetUIScale().z);
    }
}
