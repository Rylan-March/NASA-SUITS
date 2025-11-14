using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    struct XRHandRecordingFrameBuffer
    {
        float m_Timestamp;
        NativeArray<Pose> m_LeftHandJoints;
        NativeArray<Pose> m_RightHandJoints;
        bool m_AreAllLeftJointsValid;
        bool m_AreAllRightJointsValid;
        bool m_IsLeftHandTracked;
        bool m_IsRightHandTracked;

        internal bool areAllLeftJointsValid
        {
            get => m_AreAllLeftJointsValid;
            set => m_AreAllLeftJointsValid = value;
        }

        internal bool areAllRightJointsValid
        {
            get => m_AreAllRightJointsValid;
            set => m_AreAllRightJointsValid = value;
        }

        internal bool isLeftHandTracked
        {
            get => m_IsLeftHandTracked;
            set => m_IsLeftHandTracked = value;
        }

        internal bool isRightHandTracked
        {
            get => m_IsRightHandTracked;
            set => m_IsRightHandTracked = value;
        }

        internal XRHandRecordingFrameBuffer(float timestamp, bool isLeftHandTracked, bool isRightHandTracked)
        {
            m_Timestamp = timestamp;

            m_IsLeftHandTracked = isLeftHandTracked;
            m_IsRightHandTracked = isRightHandTracked;

            m_LeftHandJoints = isLeftHandTracked ?
                new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Temp) : default;
            m_RightHandJoints = isRightHandTracked ?
                new NativeArray<Pose>(XRHandJointID.EndMarker.ToIndex(), Allocator.Temp) : default;

            m_AreAllLeftJointsValid = false;
            m_AreAllRightJointsValid = false;
        }

        internal void Dispose()
        {
            if (m_LeftHandJoints.IsCreated)
                m_LeftHandJoints.Dispose();

            if (m_RightHandJoints.IsCreated)
                m_RightHandJoints.Dispose();
        }

        internal bool TryCaptureHandJoints(in XRHand hand)
        {
            if (!hand.isTracked)
                return false;

            var handJoints = hand.handedness == Handedness.Left ? m_LeftHandJoints : m_RightHandJoints;

            if (!handJoints.IsCreated || handJoints.Length != XRHandJointID.EndMarker.ToIndex())
                return false;

            for (var jointID = XRHandJointID.BeginMarker; jointID < XRHandJointID.EndMarker; ++jointID)
            {
                var jointData = hand.GetJoint(jointID);
                if (!jointData.TryGetPose(out var pose))
                {
                    return false;
                }
                handJoints[jointID.ToIndex()] = pose;
            }
            return true;
        }

        internal unsafe XRHandRecordingRawFrame WriteFrameHandData()
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Timestamp
                writer.Write(m_Timestamp);

                // Validity flags for left and right hand
                writer.Write(m_IsLeftHandTracked);
                writer.Write(m_IsRightHandTracked);
                writer.Write(m_AreAllLeftJointsValid);
                writer.Write(m_AreAllRightJointsValid);

                // Left hand joints
                if (m_AreAllLeftJointsValid)
                {
                    foreach (var pose in m_LeftHandJoints)
                    {
                        WritePose(writer, pose);
                    }
                }

                // Right hand joints
                if (m_AreAllRightJointsValid)
                {
                    foreach (var pose in m_RightHandJoints)
                    {
                        WritePose(writer, pose);
                    }
                }

                byte[] data = memoryStream.ToArray();
                var blob = new NativeArray<byte>(data.Length, Allocator.Persistent);
                fixed (byte* dataPtr = data)
                {
                    UnsafeUtility.MemCpy((byte*)blob.GetUnsafePtr(), dataPtr, data.Length);
                }

                return new XRHandRecordingRawFrame(blob);
            }
        }

        static void WritePose(BinaryWriter writer, Pose pose)
        {
            // Write position components
            writer.Write(pose.position.x);
            writer.Write(pose.position.y);
            writer.Write(pose.position.z);

            // Write rotation components
            writer.Write(pose.rotation.x);
            writer.Write(pose.rotation.y);
            writer.Write(pose.rotation.z);
            writer.Write(pose.rotation.w);
        }
    }
}
