using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Provides functionality to determine focus and bounds for UI elements within a composition layer.
    /// </summary>
    [ExecuteInEditMode]
    public class UIFocus : MonoBehaviour
    {
        /// <summary>
        /// Reference to the parent Composition Layer
        /// </summary>
        public CompositionLayer CompositionLayer { get => m_CompositionLayer; }
        private CompositionLayer m_CompositionLayer;

        /// <summary>
        /// Reference to the MeshCollider of the parent Composition Layer
        /// </summary>
        public MeshCollider MeshCollider { get => m_MeshCollider; }
        private MeshCollider m_MeshCollider;

        /// <summary>
        /// Reference to the Canvas this element is attached to
        /// </summary>
        public Canvas Canvas { get => m_Canvas; }
        private Canvas m_Canvas;
        private RectTransform m_CanvasRectTransform;

        /// <summary>
        /// Reference to this element's RectTransform
        /// </summary>
        public RectTransform RectTransform { get => m_RectTransform; }
        private RectTransform m_RectTransform;

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnEnable()
        {
            m_CompositionLayer = GetComponentInParent<CompositionLayer>();
            m_MeshCollider = GetComponentInParent<MeshCollider>();
            m_Canvas = GetComponentInParent<Canvas>();
            m_CanvasRectTransform = m_Canvas?.GetComponent<RectTransform>();

            m_RectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Determines the bounds of the element based on the type of element it is
        /// </summary>
        /// <returns>The bounds of the type</returns>
        public Bounds GetBounds()
        {
            Bounds bounds = new Bounds();

            if (m_Canvas)
                bounds = GetCanvasBounds();
            else if (MeshCollider)
                bounds = MeshCollider.bounds;

            return bounds;
        }

        /// <summary>
        /// Gets the bounds of the RectTransform in the layer's local space and scales it to the root meshCollider
        /// </summary>
        /// <returns>The bounds of the RectTransform in the layer's local space</returns>
        public Bounds GetCanvasBounds()
        {
            float centerScale = 1.0f / GetMaxCanvasSize();
            Vector3 offsetRelativeToCanvas = m_RectTransform.position - m_CanvasRectTransform.position;

            Bounds bounds = new Bounds
            {
                extents = m_RectTransform.sizeDelta * centerScale,
                center = CompositionLayer.transform.position + offsetRelativeToCanvas * centerScale
            };

            return bounds;
        }


        /// <summary>
        /// Gets the maximum size of the canvas on the X or Y axis
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns>Returns the maximum size on the X or Y axis of the canvas</returns>
        private float GetMaxCanvasSize() => Mathf.Max(m_CanvasRectTransform.sizeDelta.x, m_CanvasRectTransform.sizeDelta.y);
    }
}
