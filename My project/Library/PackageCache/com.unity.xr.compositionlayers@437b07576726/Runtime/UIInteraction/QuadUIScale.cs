using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Handles quad collider generation and maintaining aspect ratio with Canvas changes
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(CompositionLayer), typeof(MeshCollider))]
    public class QuadUIScale : LayerUIScale
    {
        private CompositionLayer quadLayer;
        private QuadLayerData quad = null;

        private float quadSizeX = -1;
        private float quadSizeY = -1;
        private Vector3 lossyScale;
        private bool applyTransformScale;

        private MeshCollider meshCollider;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            if(!TryGetComponent<CompositionLayer>(out quadLayer))
                throw new InvalidOperationException("Quad UI Scale is missing a Composition Layer!");
            if(!TryGetComponent<MeshCollider>(out meshCollider))
                throw new InvalidOperationException("Quad UI Scale is missing a Mesh Collider!");

        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            quad = quadLayer.LayerData as QuadLayerData;
            if (quad == null) return;

            // Check if the quad size, scale, or applyTransform has changed
            // If changed, redraw the mesh
            if (quadSizeX != quad.Size.x || quadSizeY != quad.Size.y || lossyScale != transform.lossyScale
                || applyTransformScale != quad.ApplyTransformScale || CanvasAdjusted())
            {
                quadSizeX = quad.Size.x;
                quadSizeY = quad.Size.y;
                lossyScale = transform.lossyScale;
                applyTransformScale = quad.ApplyTransformScale;

                RedrawMesh();
            }
        }

        /// <summary>
        /// Called whenever a mesh changing adjustment to the layer is done (i.e. scale change)
        /// Recalculates Cylinder collider mesh and applies it to the Mesh Collider
        /// </summary>
        private void RedrawMesh()
        {
            Mesh colliderMesh = meshCollider.sharedMesh;

            // Calculate HeightScale and WidthScale based on the quad's aspect ratio
            UpdateDestinationRectScale(quad.Size.x / quad.Size.y);

            // Set the collider scale to the inverse of the composition layer UI's scale
            // to allow us to change the scale independently
            Vector2 colliderAdjustmentScale = !quad.ApplyTransformScale ? new Vector2(1 / lossyScale.x, 1 / lossyScale.y) : Vector2.one;

            // Calculate the size of the collider based on the calculated scalars (HeightScale and WidthScale)
            var colliderScale = new Vector2(quadSizeX / 2 * WidthScale * colliderAdjustmentScale.x, quadSizeY / 2 * HeightScale * colliderAdjustmentScale.y);

            // Generate the mesh
            GeneratePlaneMesh(ref colliderMesh, colliderScale);
            meshCollider.sharedMesh = colliderMesh;
        }

        /// <summary>
        /// Calculates a Vector3 scalar to help convert a point from world space to the local space
        /// Mainly used for resizing
        /// </summary>
        /// <returns>Returns a Vector3 scalar to transform a point on a Canvas to a point on the Layer</returns>
        public override Vector3 GetUIScale()
        {
            return new Vector3(1f / canvasSizeX * WidthScale * quadSizeX,
                                1f / canvasSizeY * HeightScale * quadSizeY,
                                0);
        }

        /// <summary>
        /// Generates a plane mesh based on scale.
        /// </summary>
        /// <param name="mesh">Supplied mesh</param>
        /// <param name="scale">Desired Scale</param>
        private void GeneratePlaneMesh(ref Mesh mesh, Vector2 scale)
        {
            bool isNew = (mesh == null);
            if (isNew)
            {
                mesh = new Mesh();
                mesh.name = "Composition Layer Plane";
            }

            mesh.vertices = new Vector3[]
            {
                new(-scale.x, -scale.y, 0f),
                new(-scale.x, scale.y, 0f),
                new(scale.x, scale.y, 0f),
                new(scale.x, -scale.y, 0f),
            };

            // Note: These mesh setters (mesh.trialges,...) will cause an error without mesh.vertices.
            if (isNew)
            {
                mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
                mesh.normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                mesh.uv = new Vector2[] { new(1f, 0f), new(1f, 1f), new(0f, 1f), new(0f, 0f) };
            }

            mesh.UploadMeshData(false);
        }

        private void OnDestroy()
        {
            if (meshCollider != null)
                meshCollider.sharedMesh = null;
        }
    }
}
