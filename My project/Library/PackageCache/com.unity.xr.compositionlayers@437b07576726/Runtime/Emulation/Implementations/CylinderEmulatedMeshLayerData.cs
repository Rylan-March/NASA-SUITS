using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;
using UnityEngine.XR;

namespace Unity.XR.CompositionLayers.Emulation.Implementations
{
    [EmulatedLayerDataType(typeof(CylinderLayerData))]
    internal class CylinderEmulatedMeshLayerData : EmulatedMeshLayerData
    {
        float m_Radius = 1f;
        float m_Height = 1f;
        float m_Size = 5f;

        float m_CentralAngle = (float)Math.PI * 0.5f;
        float m_AspectRatio = 1f;

        Vector3 m_AdjustmentScale = Vector3.one;

        bool m_ApplyTransformScale = false;
        bool m_ValuesChanged = true;

        public override bool IsSupported(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView)
                return true;

            var isSupported = !Application.isPlaying;
#if ENABLE_UNITY_VR
            isSupported = isSupported || !XRSettings.isDeviceActive;
#endif
            return isSupported;
        }


        protected internal override void UpdateEmulatedLayerData()
        {
            base.UpdateEmulatedLayerData();

            if (LayerData is not CylinderLayerData cylinderLayer)
            {
                Debug.LogError("LayerData isn't CylinderLayerData. Please check EmulatedLayerDataTypeAttribute.");
                return;
            }

            m_ValuesChanged = UpdateValue(ref m_ApplyTransformScale, cylinderLayer.ApplyTransformScale);
            m_ValuesChanged |= UpdateValue(ref m_AdjustmentScale, (m_ApplyTransformScale ? Transform.lossyScale : Vector3.one));
            m_ValuesChanged |= UpdateValue(ref m_Radius, cylinderLayer.Radius * m_AdjustmentScale.z);
            m_ValuesChanged |= UpdateValue(ref m_CentralAngle, cylinderLayer.CentralAngle * m_AdjustmentScale.x / m_AdjustmentScale.z);
            m_ValuesChanged |= UpdateValue(ref m_Height, cylinderLayer.GetHeight() * m_AdjustmentScale.y);
            m_ValuesChanged |= UpdateValue(ref m_AspectRatio, (m_Radius * m_CentralAngle) / m_Height);
            m_ValuesChanged |= UpdateValue(ref m_Size, Mathf.Max(m_Radius, m_Height * 0.5f));
        }

        bool UpdateValue(ref float currentValue, float newValue)
        {
            if (currentValue == newValue)
                return false;

            currentValue = newValue;
            return true;
        }

        bool UpdateValue(ref bool currentValue, bool newValue)
        {
            if (currentValue == newValue)
                return false;

            currentValue = newValue;
            return true;
        }

        bool UpdateValue(ref Vector3 currentValue, Vector3 newValue)
        {
            if (currentValue == newValue)
                return false;

            currentValue = newValue;
            return true;
        }

        protected override void UpdateMesh(ref Mesh mesh)
        {
            if (mesh != null && !m_ValuesChanged)
                return;

            // TODO dynamic step count was causing an issue when scrubbing the fov slider.
            // var stepCount = (int)m_FOV / 10;
            // Cropping to maintain aspect ratio if enabled.
            FixAspectRatio();
            var stepCount = 32;

            var halfAngle = m_CentralAngle / 2.0f;
            var stepAngleDelta = m_CentralAngle / stepCount;
            var curAngle = -halfAngle;
            var uvStep = 1.0f / stepCount;

            var x = (float)Math.Sin(curAngle) * m_Radius;
            var z = (float)Math.Cos(curAngle) * m_Radius;

            var yTop = m_Height * 0.5f;
            var yBottom = m_Height * -0.5f;

            var leftTop = new Vector3(x, yTop, z);
            var leftBottom = new Vector3(x, yBottom, z);

            var quadIndices = new[] { 0, 2, 1, 3, 1, 2 };
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var indices = new List<int>();

            vertices.Add(leftTop);
            vertices.Add(leftBottom);

            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(0f, 0f));

            for (var i = 0; i < stepCount; i++)
            {
                curAngle += stepAngleDelta;

                var nextX = (float)Math.Sin(curAngle) * m_Radius;
                var nextZ = (float)Math.Cos(curAngle) * m_Radius;

                var rightTop = new Vector3(nextX, yTop, nextZ);
                var rightBottom = new Vector3(nextX, yBottom, nextZ);

                vertices.Add(rightTop);
                vertices.Add(rightBottom);

                uvs.Add(new Vector2(uvStep * (i + 1), 1f));
                uvs.Add(new Vector2(uvStep * (i + 1), 0f));

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
            mesh.RecalculateNormals();
            mesh.UploadMeshData(false);
        }

        void FixAspectRatio()
        {
            if (LayerData is CylinderLayerData cylinderLayer)
            {
                var scale = cylinderLayer.GetScaledSize(Transform.lossyScale);
                foreach (var extension in CompositionLayer.Extensions)
                {
                    if (extension is TexturesExtension textureExt)
                    {
                        if (textureExt.CropToAspect)
                        {
                            float texRatio = 1f;


                            if (textureExt.sourceTexture == TexturesExtension.SourceTextureEnum.LocalTexture)
                            {
                                if (textureExt.LeftTexture == null)
                                    break;
                                texRatio = (float)textureExt.LeftTexture.width / (float)textureExt.LeftTexture.height;
                            }
                            else if (textureExt.sourceTexture == TexturesExtension.SourceTextureEnum.AndroidSurface)
                            {
                                texRatio = (float)textureExt.Resolution.x / (float)textureExt.Resolution.y;
                            }

                            if (scale.z > texRatio)
                            {
                                // too wide
                                float width = scale.x * scale.y;
                                float height = width / scale.z;
                                m_CentralAngle = height * texRatio / scale.x;
                                m_AspectRatio = texRatio;
                            }
                            else if (scale.z < texRatio)
                            {
                                // too narrow
                                m_Height = scale.x * scale.y / texRatio;
                                m_AspectRatio = texRatio;
                            }
                        }
                    }
                }
            }
        }
    }
}
