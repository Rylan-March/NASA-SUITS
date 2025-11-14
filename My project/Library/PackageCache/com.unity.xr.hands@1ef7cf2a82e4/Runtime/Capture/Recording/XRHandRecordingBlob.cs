using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Manages the state and data for a single XR hand recording session.
    /// This class manages the lifecycle of a recording,
    /// including capturing data from the <see cref="XRHandSubsystem"/>, saving and deleting the data.
    /// </summary>
    public class XRHandRecordingBlob : XRHandRecordingBase, IDisposable
    {
        XRHandSubsystem m_Subsystem;
        bool m_IsRecording;
        float m_StartTime;
        XRHandRecordingStatusChangedEventArgs m_CurrentStatusChangedEventArgs;
        List<XRHandRecordingRawFrame> m_Frames;

        /// <summary>
        /// Called when the status of the recording changes.
        /// </summary>
        public event Action<XRHandRecordingStatusChangedEventArgs> statusChanged;

        /// <summary>
        /// Called when a new frame is captured during a recording.
        /// </summary>
        public event Action<XRHandRecordingFrameCapturedEventArgs> frameCaptured;

        /// <summary>
        /// Creates a new instance of the <see cref="XRHandRecordingBlob"/> class.
        /// </summary>
        public XRHandRecordingBlob()
        {
            m_Frames = new List<XRHandRecordingRawFrame>();
            m_CurrentStatusChangedEventArgs = new XRHandRecordingStatusChangedEventArgs();
        }

        /// <summary>
        /// Disposes any captured in-memory data. All allocated resources will be freed.
        /// </summary>
        /// <remarks>
        /// Any unsaved data is lost.
        /// </remarks>
        public void Dispose()
        {
            if (m_IsRecording)
                Stop();

            foreach (var frame in m_Frames)
            {
                frame.Dispose();
            }

            m_Frames.Clear();
        }

        /// <summary>
        /// Resets the recording to its initial state and dispose any allocated resources.
        /// </summary>
        /// <remarks>
        /// Resetting this recording allows it to be reused for a new recording session.
        /// </remarks>
        public void Reset()
        {
            Clear();
            m_DurationInSeconds = 0f;
            m_StartTime = 0f;
            m_AssetName = null;
            m_UniqueID = null;
            m_IsRecording = false;
            m_CurrentStatusChangedEventArgs = new XRHandRecordingStatusChangedEventArgs();
        }

        /// <summary>
        /// Clears the recording blob and release the memory used by the captured data.
        /// </summary>
        /// <remarks>
        /// This method clears all captured data and unsubscribes from hand tracking updates,
        /// while preserving metadata like the asset name and elapsed time.
        /// To make the object ready for a completely new recording, call <see cref="Reset()"/> instead.
        /// </remarks>
        public void Clear()
        {
            if (m_IsRecording)
                Stop();

            Dispose();
            CleanupSubscription();
        }

        /// <summary>
        /// Initializes a new recording and prepares for capturing hand data, if possible.
        /// </summary>
        /// <param name="args">The initialization arguments <see cref="XRHandRecordingInitArgs"/>.</param>
        /// <returns><c>true</c> if the recording was successfully initialized; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// If successful, the recording begins on the subsequent frame update.
        /// Initializing a new recording can fail if a recording is already in progress or the
        /// <c>XRHandSubsystem</c> is null.
        /// It can also fail for other reasons. Refer to the player log
        /// for more detailed information about the cause of a failure.
        /// </remarks>
        public bool TryInitialize(XRHandRecordingInitializeArgs args)
        {
            if (args.subsystem == null)
            {
                Debug.LogError("Cannot start recording: XRHandSubsystem is null.");
                return false;
            }

            if (m_IsRecording)
            {
                Debug.LogWarning("Cannot start recording: a recording session is already in progress.");
                return false;
            }

            try
            {
                CleanupSubscription();
                m_Subsystem = args.subsystem;
                m_Subsystem.updatedHands += OnUpdatedHands;

                m_IsRecording = true;
                m_DurationInSeconds = 0f;
                m_StartTime = 0f;
                m_UniqueID = GenerateUniqueID();

                NotifyStatusChanged(XRHandRecordingStatus.Ready);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to start recording: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops the current recording session. The captured data remains in memory
        /// until it is saved, cleared or disposed.
        /// </summary>
        public void Stop()
        {
            StopRecordingInternal(XRHandRecordingStatus.StoppedManually);
        }

        /// <summary>
        /// Attempts to save the captured hand data to disk.
        /// </summary>
        /// <param name="args">The save arguments <see cref="XRHandRecordingSaveArgs"/>.</param>
        /// <returns>Return <c>true</c> if the data was saved successfully, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// If successful, the in-memory data is disposed to free up resources.
        /// Otherwise, the data remains in memory and should be managed accordingly.
        /// </remarks>
        public bool TrySave(XRHandRecordingSaveArgs args)
        {
            // Validate recording name
            if (string.IsNullOrEmpty(args.recordingName))
            {
                Debug.LogError("Recording name cannot be null or empty");
                return false;
            }

            m_AssetName = args.recordingName;

            try
            {
                var success = TryWriteRecordedDataToDisk();

                if (success)
                {
                    NotifyStatusChanged(XRHandRecordingStatus.Saved);

                    // Only dispose the data if saving was successful
                    Dispose();
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save recording: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the saved file of this recording from the device's storage.
        /// </summary>
        /// <remarks>
        /// Only call this method after the recording has been successfully saved.
        /// To dispose an unsaved recording, use <see cref="Dispose"/> instead.
        /// </remarks>
        public override void Delete()
        {
            if (m_IsRecording)
                Stop();

            Dispose();
            DeleteFileFromDisk(internalBinaryFileName);
        }

        void OnUpdatedHands(
            XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (updateType == XRHandSubsystem.UpdateType.Dynamic || !m_IsRecording)
                return;

            if (m_StartTime == 0)
                m_StartTime = Time.timeSinceLevelLoad;

            m_DurationInSeconds = Time.timeSinceLevelLoad - m_StartTime;

            if (m_DurationInSeconds >= XRHandRecordingSettings.timeLimitInSeconds)
            {
                StopRecordingInternal(XRHandRecordingStatus.StoppedAtTimeLimit);
                return;
            }

            try
            {
                CaptureFrame(subsystem, updateSuccessFlags);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to record frame: " + ex.Message);
                StopRecordingInternal(XRHandRecordingStatus.StoppedWithError, ex.Message);
            }
        }

        void CaptureFrame(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags)
        {
            var frameBuffer = new XRHandRecordingFrameBuffer(
                m_DurationInSeconds, subsystem.leftHand.isTracked, subsystem.rightHand.isTracked);

            try
            {
                frameBuffer.areAllLeftJointsValid = frameBuffer.TryCaptureHandJoints(subsystem.leftHand);
                frameBuffer.areAllRightJointsValid = frameBuffer.TryCaptureHandJoints(subsystem.rightHand);
                var frameData = frameBuffer.WriteFrameHandData();

                m_Frames.Add(frameData);

                // Notify that recording has started on the first captured frame
                if (m_Frames.Count == 1)
                {
                    NotifyStatusChanged(XRHandRecordingStatus.Recording);
                }

                NotifyFrameCaptured(updateSuccessFlags);
            }
            finally
            {
                frameBuffer.Dispose();
            }
        }

        void StopRecordingInternal(XRHandRecordingStatus status, string errorMessage = null)
        {
            if (!m_IsRecording)
                return;

            m_IsRecording = false;
            CleanupSubscription();
            NotifyStatusChanged(status, errorMessage);
        }

        void NotifyStatusChanged(XRHandRecordingStatus status, string errorMessage = null)
        {
            var args = new XRHandRecordingStatusChangedEventArgs
            {
                status = status,
                elapsedTime = m_DurationInSeconds,
                recordingName = m_AssetName,
                subsystem = m_Subsystem,
                errorMessage = errorMessage
            };

            if (args == m_CurrentStatusChangedEventArgs)
                return;

            m_CurrentStatusChangedEventArgs = args;
            statusChanged?.Invoke(args);
        }

        void NotifyFrameCaptured(XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags)
        {
            var args = new XRHandRecordingFrameCapturedEventArgs
            {
                elapsedTime = m_DurationInSeconds,
                subsystem = m_Subsystem,
                updateSuccessFlags = updateSuccessFlags,
                frameIndex = m_Frames.Count - 1
            };

            frameCaptured?.Invoke(args);
        }

        void CleanupSubscription()
        {
            if (m_Subsystem != null)
                m_Subsystem.updatedHands -= OnUpdatedHands;
        }

        bool TryWriteRecordedDataToDisk()
        {
            try
            {
                var savePath = GetDeviceStoragePath();
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                var fullPath = Path.Combine(savePath, internalBinaryFileName);

                using (var stream = File.Open(fullPath, FileMode.Create))
                {
                    stream.Position = 0;
                    using (var writer = new BinaryWriter(stream))
                    {
                        // Write the version number
                        writer.Write(XRHandRecordingBinaryFileFormatConfigs.k_Version);

                        // Write the recording name string
                        writer.Write(m_AssetName);

                        // Write the total time of the recording
                        writer.Write(m_DurationInSeconds);

                        // Write the frame count
                        writer.Write(m_Frames.Count);

                        foreach (var frame in m_Frames)
                        {
                            writer.Write(frame.blob.ToArray());
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {internalBinaryFileName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to read an <see cref="XRHandCaptureSequence"/> from disk at the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the file containing the captured sequence data.</param>
        /// <param name="capturedData">
        /// When this method returns, contains the loaded <see cref="XRHandCaptureSequence"/> if successful;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the sequence was successfully read and loaded; <c>false</c> otherwise.
        /// </returns>
        internal static bool TryReadCaptureSequenceFromDisk(string filePath, out XRHandCaptureSequence captureData)
        {
            captureData = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"File does not exist: {filePath}");
                    return false;
                }

                using (var stream = File.Open(filePath, FileMode.Open))
                {
                    stream.Position = 0;
                    using (var reader = new BinaryReader(stream))
                    {
                        captureData = ScriptableObject.CreateInstance<XRHandCaptureSequence>();

                        // Read the version number
                        int version = reader.ReadInt32();
                        if (version != XRHandRecordingBinaryFileFormatConfigs.k_Version)
                        {
                            Debug.LogError($"XR Hand Capture data format version mismatch. File uses v{version}, " +
                                $"but this application requires v{XRHandRecordingBinaryFileFormatConfigs.k_Version}." +
                                $" Please use a compatible recording file or update the application.");
                            Object.DestroyImmediate(captureData);
                            captureData = null;
                            return false;
                        }

                        // Read the recording name
                        string recordingName = reader.ReadString();

                        captureData.name = recordingName;

                        // Read the total time of the recording
                        captureData.durationInSeconds = reader.ReadSingle();

                        // Read the frame count
                        int frameCount = reader.ReadInt32();

                        for (var i = 0; i < frameCount; ++i)
                        {
                            XRHandCaptureFrame frame = new XRHandCaptureFrame();

                            // Timestamp
                            frame.timestamp = reader.ReadSingle();

                            // Read whether hands are tracked
                            frame.isLeftHandTracked = reader.ReadBoolean();
                            frame.isRightHandTracked = reader.ReadBoolean();

                            // Read whether all joints are valid
                            frame.areAllLeftJointsValid = reader.ReadBoolean();
                            frame.areAllRightJointsValid = reader.ReadBoolean();

                            // Left hand joints
                            if (frame.areAllLeftJointsValid)
                            {
                                frame.leftHandJoints = new XRHandJoint[XRHandJointID.EndMarker.ToIndex()];
                                for (var id = XRHandJointID.BeginMarker; id < XRHandJointID.EndMarker; ++id)
                                {
                                    Pose pose = ReadPose(reader);
                                    XRHandJoint joint = XRHandProviderUtility.CreateJoint(
                                        Handedness.Left, XRHandJointTrackingState.Pose, id, pose);
                                    frame.leftHandJoints[id.ToIndex()] = joint;
                                }
                            }

                            // Right hand joints
                            if (frame.areAllRightJointsValid)
                            {
                                frame.rightHandJoints = new XRHandJoint[XRHandJointID.EndMarker.ToIndex()];
                                for (var id = XRHandJointID.BeginMarker; id < XRHandJointID.EndMarker; ++id)
                                {
                                    Pose pose = ReadPose(reader);
                                    XRHandJoint joint = XRHandProviderUtility.CreateJoint(
                                        Handedness.Right, XRHandJointTrackingState.Pose, id, pose);
                                    frame.rightHandJoints[id.ToIndex()] = joint;
                                }
                            }

                            // Add the frame to the captured data
                            captureData.AddFrame(frame);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to retrieve captured data from file {filePath}: {e.Message}");
                if (captureData != null)
                {
                    if (Application.isEditor)
                        Object.DestroyImmediate(captureData);
                    else
                        Object.Destroy(captureData);
                    captureData = null;
                }
                return false;
            }
        }

        static Pose ReadPose(BinaryReader reader)
        {
            // Read position components
            Vector3 position = new Vector3(
                reader.ReadSingle(), // x
                reader.ReadSingle(), // y
                reader.ReadSingle()  // z
            );

            // Read rotation components
            Quaternion rotation = new Quaternion(
                reader.ReadSingle(), // x
                reader.ReadSingle(), // y
                reader.ReadSingle(), // z
                reader.ReadSingle()  // w
            );

            return new Pose(position, rotation);
        }
    }
}
