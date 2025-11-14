using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Handles cylinder collider generation and maintaining aspect ratio with Canvas changes
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(CompositionLayer), typeof(MeshCollider))]
    public class CylinderUIScale : LayerUIScale
    {
        private CompositionLayer cylinderLayer;
        private CylinderLayerData cylinder;

        private float cylinderAspect = -1;
        private float radius = -1;
        private float height = -1;
        private Vector3 lossyScale;
        private bool applyTransformScale;

        private MeshCollider meshCollider;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            if(!TryGetComponent<CompositionLayer>(out cylinderLayer))
                throw new InvalidOperationException("Cylinder UI Scale is missing a Composition Layer!");
            if(!TryGetComponent<MeshCollider>(out meshCollider))
                throw new InvalidOperationException("Cylinder UI Scale is missing a Mesh Collider!");

        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            cylinder = cylinderLayer.LayerData as CylinderLayerData;
            if (cylinder == null) return;

            // Check if the cylinder aspect, radius, height, scale, or applyTransform has changed
            // If changed, redraw the mesh
            if (cylinderAspect != cylinder.AspectRatio || radius != cylinder.Radius || height != cylinder.GetHeight() ||
                lossyScale != transform.lossyScale || applyTransformScale != cylinder.ApplyTransformScale || CanvasAdjusted())
            {
                cylinderAspect = cylinder.AspectRatio;
                radius = cylinder.Radius;
                height = cylinder.GetHeight();
                lossyScale = transform.lossyScale;
                applyTransformScale = cylinder.ApplyTransformScale;

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

            // Calculate HeightScale and WidthScale based on the cylinder's aspect ratio
            UpdateDestinationRectScale(cylinderAspect);

            // Set the collider scale to the inverse of the composition layer UI's scale
            // to allow us to change the scale independently
            var colliderScale = new Vector3(1 / lossyScale.x, 1 / lossyScale.y, 1 / lossyScale.z);

            // Only apply the scale if ApplyTransformScale is true
            var colliderAdjustmentScale = cylinder.ApplyTransformScale ? lossyScale : Vector3.one;

            // Calculate the size of the collider based on the calculated scalars (HeightScale and WidthScale)
            var colliderAdjustment = new Vector3(1 * colliderAdjustmentScale.z,
                                                    HeightScale * colliderAdjustmentScale.y,
                                                    WidthScale * colliderAdjustmentScale.x / colliderAdjustmentScale.z);

            // Generate the mesh
            GenerateCylinderMesh(ref colliderMesh, colliderScale, colliderAdjustment);
            meshCollider.sharedMesh = colliderMesh;
        }

        /// <summary>
        /// Calculates a Vector3 scalar to help convert a point from world space to the local space
        /// Mainly used for resizing
        /// </summary>
        /// <returns>Returns a Vector3 scalar to transform a point on a Canvas to a point on the Layer</returns>
        public override Vector3 GetUIScale()
        {
            return new Vector3(1f / canvasSizeX * height * (WidthScale * cylinderAspect),
                                1f / canvasSizeY * height * HeightScale,
                                0);
        }

        private void GenerateCylinderMesh(ref Mesh mesh, Vector3 scale, Vector3 adjustment)
        {
            float radius = cylinder.Radius * adjustment.x;
            float height = cylinder.GetHeight() * adjustment.y;
            float centralAngle = cylinder.CentralAngle * adjustment.z;

            var stepCount = 32;

            var halfAngle = centralAngle / 2.0f;
            var stepAngleDelta = centralAngle / stepCount;
            var curAngle = -halfAngle;
            var uvStep = 1.0f / stepCount;

            var x = (float)Math.Sin(curAngle) * radius;
            var z = (float)Math.Cos(curAngle) * radius;

            var yTop = height * 0.5f;
            var yBottom = height * -0.5f;

            var leftTop = new Vector3(x * scale.x, yTop * scale.y, z * scale.z);
            var leftBottom = new Vector3(x * scale.x, yBottom * scale.y, z * scale.z);

            var quadIndices = new[] { 0, 2, 1, 3, 1, 2 };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var indices = new List<int>();

            vertices.Add(leftTop);
            vertices.Add(leftBottom);

            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0f));

            for (var i = 0; i < stepCount; i++)
            {
                curAngle += stepAngleDelta;

                var nextX = (float)Math.Sin(curAngle) * radius;
                var nextZ = (float)Math.Cos(curAngle) * radius;

                var rightTop = new Vector3(nextX * scale.x, yTop * scale.y, nextZ * scale.z);
                var rightBottom = new Vector3(nextX * scale.x, yBottom * scale.y, nextZ * scale.z);

                vertices.Add(rightTop);
                vertices.Add(rightBottom);

                uvs.Add(new Vector2(1f - uvStep * (i + 1), 1f));
                uvs.Add(new Vector2(1f - uvStep * (i + 1), 0f));

                foreach (var index in quadIndices)
                {
                    indices.Add(index + (2 * i));
                }
            }

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "Composition Layer Cylinder";
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.UploadMeshData(false);
        }

        private void OnDestroy()
        {
            if (meshCollider != null)
                meshCollider.sharedMesh = null;
        }
    }
}
