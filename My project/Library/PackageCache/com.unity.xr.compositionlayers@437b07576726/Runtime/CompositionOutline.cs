using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers
{
    /// <summary>
    /// Component for a Quad or Cylinder layer that generates an outline with the Layer's bounds
    /// Useful for UI that has a different aspect ratio then the child canvas, leaving empty space
    /// </summary>
    [ExecuteInEditMode]
    public class CompositionOutline : MonoBehaviour
    {
        private Mesh outlineMesh;
        private CompositionLayer m_CompositionLayer;
        private LayerData previousLayerData;

#if UNITY_EDITOR
        void Awake()
        {
            TryGetComponent<CompositionLayer>(out m_CompositionLayer);
        }

        void Start()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        void OnDrawGizmos()
        {
            if (m_CompositionLayer == null)
            {
                Debug.LogWarning("A Composition Layer is required to generate an outline.");
                DestroyImmediate(this);
                return;
            }

            // Check if the layer type has changed
            // If changed, delete the old outline mesh
            LayerData layerData = m_CompositionLayer.LayerData;
            if (previousLayerData != layerData)
            {
                previousLayerData = layerData;
                outlineMesh = null;
            }

            if (layerData is QuadLayerData quad)
            {
                GenerateQuadOutline(quad);
            }
            else if (layerData is CylinderLayerData cylinder)
            {
                GenerateCylinderOutline(cylinder);
            }
        }
#endif
        /// <summary>
        /// Generates an outline for a Quad Composition Layer
        /// Draws in order by the left, top, right, then finishes with the bottom
        /// </summary>
        private void GenerateQuadOutline(QuadLayerData data)
        {
            // Set the scalar to the inverse of the composition layer UI's scale
            // to allow us to change the scale independently (if ApplyTransformScale is false)
            var adjustmentScale = !data.ApplyTransformScale ? new Vector2(1 / transform.localScale.x, 1 / transform.lossyScale.y) : Vector2.one;

            // Calculate the scalar value to size the outline
            var outlineScale = new Vector2(data.Size.x / 2 * adjustmentScale.x, data.Size.y / 2 * adjustmentScale.y);

            // Generate a quad mesh so we can draw lines between the verticies
            GeneratePlaneMesh(ref outlineMesh, outlineScale);

            var bottomRightVertex = outlineMesh.vertices[outlineMesh.vertices.Length - 1];
            var bottomLeftVertex = outlineMesh.vertices[0];

            var transformMatrix = GetTransformMatrix();

            // Loop through each vertex and connect it to the next one
            for (int i = 0; i < outlineMesh.vertices.Length - 1; i++)
            {
                var currentVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[i]);
                var nextVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[i + 1]);

                Gizmos.DrawLine(currentVertex, nextVertex);
            }

            // Connect the bottom right vertex to the bottom left to finish the outline
            Gizmos.DrawLine(transformMatrix.MultiplyPoint(bottomRightVertex), transformMatrix.MultiplyPoint(bottomLeftVertex));
        }

        /// <summary>
        /// Generates an outline for a Cylinder Composition Layer
        /// Starts by generating the top and bottom curve, then ends by drawing the left and right side
        /// </summary>
        private void GenerateCylinderOutline(CylinderLayerData data)
        {
            // Set the scalar to the inverse of the composition layer UI's scale
            // to allow us to change the scale independently (if ApplyTransformScale is true)
            var inverseScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
            var adjustmentScale = data.ApplyTransformScale ? transform.lossyScale : Vector3.one;

            // Calculate the scalar value to size the outline
            var outlineScale = new Vector3(adjustmentScale.z, adjustmentScale.y, adjustmentScale.x / adjustmentScale.z);

            // Generate a cylinder mesh so we can draw lines between the verticies
            GenerateCylinderMesh(ref outlineMesh, data.Radius, data.GetHeight(), data.CentralAngle, inverseScale, outlineScale);

            var transformMatrix = GetTransformMatrix();

            var bottomRightVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[outlineMesh.vertices.Length - 1]);
            var topRightVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[outlineMesh.vertices.Length - 2]);
            var topLeftVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[0]);
            var bottomLeftVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[1]);

            // Loop through each vertex and connect it to the next one
            for (int i = 0; i < outlineMesh.vertices.Length - 2; i++)
            {
                var currentVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[i]);
                var nextVertex = transformMatrix.MultiplyPoint(outlineMesh.vertices[i + 2]);

                Gizmos.DrawLine(currentVertex, nextVertex);
            }

            // Connect the top left and bottom left to finish out the left side
            Gizmos.DrawLine(topLeftVertex, bottomLeftVertex);

            // Connect the top right and bottom right to finish out the bottom right side
            Gizmos.DrawLine(topRightVertex, bottomRightVertex);
        }

        /// <summary>
        /// Generates a Cylinder mesh based on radius, height, central angle, as well as scale and adjustment.
        /// </summary>
        /// <param name="mesh">Supplied mesh</param>
        /// <param name="radius">Desired Radius</param>
        /// <param name="height">Desired Height</param>
        /// <param name="centralAngle">Desired Central Angle</param>
        /// <param name="scale">Desired Scale</param>
        /// <param name="adjustment">Adjustments to be applied to Radius, Height, and Central Angle respectively</param>
        private void GenerateCylinderMesh(ref Mesh mesh, float radius, float height, float centralAngle, Vector3 scale, Vector3 adjustment)
        {
            /// Adjustment is used because scaling a cylinder layer does not stretch the game object,
            /// but instead should adjust the radius, height, and central angle.
            radius *= adjustment.x;
            height *= adjustment.y;
            centralAngle *= adjustment.z;

            var stepCount = 32;

            var halfAngle = centralAngle / 2.0f;
            var stepAngleDelta = centralAngle / stepCount;
            var curAngle = -halfAngle;
            var uvStep = 1.0f / stepCount;

            var x = (float)Math.Sin(curAngle) * radius;
            var z = (float)Math.Cos(curAngle) * radius;

            var yTop = height * 0.5f;
            var yBottom = height * -0.5f;

            // Calculate vertex positions with supplied scale
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

                // Calculate vertex positions with supplied scale
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
        }

        /// <summary>
        /// Get transform matrix for scene preview.
        /// </summary>
        private Matrix4x4 GetTransformMatrix()
        {
            var provider = PlatformManager.ActivePlatformProvider;
            if (provider != null && provider.GetSelectedCoordinateSystem(m_CompositionLayer) != PlatformProvider.DefaultCoordinateSystem)
            {
                CustomTransformCameraData cameraData = new CustomTransformCameraData();
                cameraData.MainCamera = Services.CompositionLayerUtils.GetStereoMainCamera();
                cameraData.IsSceneView = true;
                CustomTransformData transformData = provider.GetCustomTransformData(m_CompositionLayer, cameraData);
                return transformData.Matrix;
            }
            else
            {
                return transform.localToWorldMatrix;
            }
        }

    }
}
