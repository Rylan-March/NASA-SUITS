using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace UnityEditor.XR.Hands.Capture
{
    class HandGameObjects
    {
        GameObject m_JointPrefab;
        GameObject m_HandPrefab;
        GameObject m_HandRoot;
        GameObject m_DrawJointsParent;
        GameObject[] m_DrawJoints = new GameObject[XRHandJointID.EndMarker.ToIndex()];
        GameObject[] m_VelocityParents = new GameObject[XRHandJointID.EndMarker.ToIndex()];
        LineRenderer[] m_Lines = new LineRenderer[XRHandJointID.EndMarker.ToIndex()];
        JointVisualizer[] m_JointVisualizers = new JointVisualizer[XRHandJointID.EndMarker.ToIndex()];
        XRHandMeshController m_MeshController;
        Dictionary<int, Transform> m_JointTransformsLookup = new Dictionary<int, Transform>();
        static Vector3[] s_LinePointsReuse = new Vector3[2];
        const float k_LineWidth = 0.005f;
        const HideFlags k_CaptureHideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild |
            HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        internal HandGameObjects(
            Handedness handedness,
            Transform parent,
            GameObject handMeshPrefab,
            Material meshMaterial,
            GameObject jointPrefab)
        {
            m_JointPrefab = jointPrefab;
            m_HandPrefab = handMeshPrefab;

            if (m_HandPrefab == null)
                return;

            var isSceneObject = m_HandPrefab.scene.IsValid();
            m_HandRoot = isSceneObject ? m_HandPrefab : Object.Instantiate(m_HandPrefab);
            m_HandRoot.name = $"{handedness} Hand Capture Playback";
            m_HandRoot.SetHideFlagsRecursively(k_CaptureHideFlags);

            m_HandRoot.transform.localPosition = Vector3.zero;
            m_HandRoot.transform.localRotation = Quaternion.identity;

            var handEvents = m_HandRoot.GetComponent<XRHandTrackingEvents>();
            if (handEvents == null)
            {
                handEvents = m_HandRoot.AddComponent<XRHandTrackingEvents>();
                handEvents.updateType = XRHandTrackingEvents.UpdateTypes.Dynamic;
                handEvents.handedness = handedness;
            }

            m_MeshController = m_HandRoot.GetComponent<XRHandMeshController>();
            if (m_MeshController == null)
            {
                m_MeshController = m_HandRoot.AddComponent<XRHandMeshController>();
                m_MeshController.handMeshRenderer = m_HandRoot.GetComponentInChildren<SkinnedMeshRenderer>();
                m_MeshController.handTrackingEvents = handEvents;
            }

            if (meshMaterial != null)
            {
                m_MeshController.handMeshRenderer.sharedMaterial = meshMaterial;
            }

            var skeletonDriver = m_HandRoot.GetComponent<XRHandSkeletonDriver>();
            if (skeletonDriver == null)
            {
                skeletonDriver = m_HandRoot.AddComponent<XRHandSkeletonDriver>();
                skeletonDriver.jointTransformReferences = new List<JointToTransformReference>();
                Transform root = null;
                for (var childIndex = 0; childIndex < m_HandRoot.transform.childCount; ++childIndex)
                {
                    var child = m_HandRoot.transform.GetChild(childIndex);
                    if (child.gameObject.name.EndsWith(XRHandJointID.Wrist.ToString()))
                        root = child;
                }

                skeletonDriver.rootTransform = root;
                XRHandSkeletonDriverUtility.FindJointsFromRoot(skeletonDriver);
                skeletonDriver.InitializeFromSerializedReferences();
                skeletonDriver.handTrackingEvents = handEvents;
            }

            foreach (var joint in skeletonDriver.jointTransformReferences)
            {
                var jointIndex = joint.xrHandJointID.ToIndex();
                m_JointTransformsLookup[jointIndex] = joint.jointTransform;
            }

            var drawJointsParentName = $"{handedness} Hand Capture Joints";
            m_DrawJointsParent = new GameObject(drawJointsParentName);
            m_DrawJointsParent.transform.parent = parent;
            m_DrawJointsParent.transform.localPosition = Vector3.zero;
            m_DrawJointsParent.transform.localRotation = Quaternion.identity;

            for (var i = 0; i < skeletonDriver.jointTransformReferences.Count; i++)
            {
                var jointTransformReference = skeletonDriver.jointTransformReferences[i];
                var jointTransform = jointTransformReference.jointTransform;
                var jointID = jointTransformReference.xrHandJointID;
                AssignJoint(jointID, jointTransform, m_DrawJointsParent.transform);
            }
            m_DrawJointsParent.SetHideFlagsRecursively(k_CaptureHideFlags);

            m_HandRoot.SetActive(true);
        }

        void AssignJoint(
            XRHandJointID jointId,
            Transform jointDrivenTransform,
            Transform drawJointsParent)
        {
            if (m_JointPrefab == null)
                return;

            var jointIndex = jointId.ToIndex();
            m_DrawJoints[jointIndex] = Object.Instantiate(m_JointPrefab);
            m_DrawJoints[jointIndex].transform.parent = drawJointsParent;
            m_DrawJoints[jointIndex].name = jointId.ToString();

            m_Lines[jointIndex] = m_DrawJoints[jointIndex].GetComponent<LineRenderer>();
            m_Lines[jointIndex].startWidth = m_Lines[jointIndex].endWidth = k_LineWidth;
            s_LinePointsReuse[0] = s_LinePointsReuse[1] = jointDrivenTransform.position;
            m_Lines[jointIndex].SetPositions(s_LinePointsReuse);

            m_JointVisualizers[jointIndex] = new JointVisualizer();
            m_JointVisualizers[jointIndex].Initialize(m_DrawJoints[jointIndex]);
        }

        internal void OnDestroy()
        {
            Object.DestroyImmediate(m_HandRoot);
            m_HandRoot = null;

            for (var jointIndex = 0; jointIndex < m_DrawJoints.Length; ++jointIndex)
            {
                Object.DestroyImmediate(m_DrawJoints[jointIndex]);
                m_DrawJoints[jointIndex] = null;
            }

            for (var jointIndex = 0; jointIndex < m_VelocityParents.Length; ++jointIndex)
            {
                Object.DestroyImmediate(m_VelocityParents[jointIndex]);
                m_VelocityParents[jointIndex] = null;
            }

            Object.DestroyImmediate(m_DrawJointsParent);
            m_DrawJointsParent = null;
        }

        internal void ToggleDrawMesh(bool drawMesh)
        {
            if (m_HandPrefab == null)
                return;

            m_MeshController.enabled = drawMesh;
            m_MeshController.handMeshRenderer.enabled = drawMesh;
        }

        internal void ToggleDebugDrawJoints(bool debugDrawJoints)
        {
            if (m_JointPrefab == null)
                return;

            for (int jointIndex = 0; jointIndex < m_DrawJoints.Length; ++jointIndex)
            {
                ToggleRenderers<MeshRenderer>(debugDrawJoints, m_DrawJoints[jointIndex].transform);
                m_Lines[jointIndex].enabled = debugDrawJoints;
            }

            m_Lines[0].enabled = false;
        }

        internal void UpdateJoints(
            XRHand hand,
            bool areJointsTracked)
        {
            if (!areJointsTracked || m_JointPrefab == null)
                return;

            var wristPose = Pose.identity;
            var parentIndex = XRHandJointID.Wrist.ToIndex();
            UpdateJoint(hand.GetJoint(XRHandJointID.Wrist), ref wristPose, ref parentIndex);
            UpdateJoint(hand.GetJoint(XRHandJointID.Palm), ref wristPose, ref parentIndex, false);

            for (var fingerID = XRHandFingerID.Thumb;
                 fingerID <= XRHandFingerID.Little;
                 ++fingerID)
            {
                var parentPose = wristPose;
                parentIndex = XRHandJointID.Wrist.ToIndex();

                for (var jointId = fingerID.GetFrontJointID();
                     jointId <= fingerID.GetBackJointID();
                     ++jointId)
                {
                    UpdateJoint(hand.GetJoint(jointId), ref parentPose, ref parentIndex);
                }
            }
        }

        void UpdateJoint(
            XRHandJoint joint,
            ref Pose parentPose,
            ref int parentIndex,
            bool cacheParentPose = true)
        {
            if (joint.id == XRHandJointID.Invalid)
                return;

            var jointIndex = joint.id.ToIndex();
            m_JointVisualizers[jointIndex].NotifyTrackingState(joint.trackingState);

            if (!joint.TryGetPose(out var pose))
                return;

            m_DrawJoints[jointIndex].transform.localPosition = pose.position;
            m_DrawJoints[jointIndex].transform.localRotation = pose.rotation;

            if (joint.id != XRHandJointID.Wrist)
            {
                s_LinePointsReuse[0] = m_DrawJoints[parentIndex].transform.position;
                s_LinePointsReuse[1] = m_DrawJoints[jointIndex].transform.position;
                m_Lines[jointIndex].SetPositions(s_LinePointsReuse);
            }

            if (m_MeshController.handMeshRenderer.TryGetComponent<SkinnedMeshRenderer>(out var meshRenderer))
            {
                Pose jointLocalPose = new Pose();

                if (jointIndex == XRHandJointID.Wrist.ToIndex())
                    jointLocalPose = pose;
                else
                    XRHandSkeletonDriver.CalculateLocalTransformPose(parentPose, pose, out jointLocalPose);

                m_JointTransformsLookup[jointIndex].SetLocalPose(jointLocalPose);
            }

            if (cacheParentPose)
            {
                parentPose = pose;
                parentIndex = jointIndex;
            }
        }

        static void ToggleRenderers<TRenderer>(bool toggle, Transform rendererTransform)
            where TRenderer : Renderer
        {
            if (rendererTransform.TryGetComponent<TRenderer>(out var renderer))
                renderer.enabled = toggle;

            for (var childIndex = 0; childIndex < rendererTransform.childCount; ++childIndex)
                ToggleRenderers<TRenderer>(toggle, rendererTransform.GetChild(childIndex));
        }
    }

    class JointVisualizer
    {
        GameObject m_JointVisual;

        Material m_HighFidelityJointMaterial;

        Material m_LowFidelityJointMaterial;

        bool m_HighFidelityJoint;

        Renderer m_JointRenderer;

        internal void NotifyTrackingState(XRHandJointTrackingState jointTrackingState)
        {
            bool highFidelityJoint = (jointTrackingState & XRHandJointTrackingState.HighFidelityPose) == XRHandJointTrackingState.HighFidelityPose;
            if (m_HighFidelityJoint == highFidelityJoint)
                return;

            m_JointRenderer.material = highFidelityJoint ? m_HighFidelityJointMaterial : m_LowFidelityJointMaterial;

            m_HighFidelityJoint = highFidelityJoint;
        }

        internal void Initialize(GameObject jointVisual)
        {
            m_JointVisual = jointVisual;

            if (m_JointVisual.TryGetComponent<Renderer>(out var jointRenderer))
                m_JointRenderer = jointRenderer;
        }
    }
}
