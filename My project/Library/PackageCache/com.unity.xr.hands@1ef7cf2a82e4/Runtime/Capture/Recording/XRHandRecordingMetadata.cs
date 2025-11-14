using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Metadata for a XR Hand recording session.
    /// </summary>
    /// <remarks>
    /// This class provides a lightweight way to access recording session metadata, such as
    /// name and duration, without loading all captured frames into memory. Use this method
    /// to get information for such tasks as listing existing recordings or displaying recording summaries.
    /// To load the complete recording data including all frames, use the <see cref="XRHandRecordingBlob"/> class.
    /// </remarks>
    public class XRHandRecordingMetadata : XRHandRecordingBase
    {
        /// <summary>
        /// Internal constructor used by <see cref="GetSavedRecordingMetadata"/> to create a
        /// <see cref="XRHandRecordingMetadata"/> instance containing only metadata from a saved recording file.
        /// </summary>
        XRHandRecordingMetadata(string uniqueID, string assetName, float durationInSeconds)
        {
            m_UniqueID = uniqueID;
            m_AssetName = assetName;
            m_DurationInSeconds = durationInSeconds;
        }

        /// <summary>
        /// Deletes the recording from device storage.
        /// </summary>
        /// <remarks>
        /// This method removes the entire recording file, including both metadata and frame data.
        /// Once deleted, the recording cannot be recovered.
        /// </remarks>
        public override void Delete()
        {
            DeleteFileFromDisk(internalBinaryFileName);
        }

        /// <summary>
        /// Scans the device's persistent storage path for saved recording files and populates a list
        /// with <see cref="XRHandRecordingMetadata"/> instances representing their metadata (name, duration).
        /// </summary>
        /// <param name="existingRecordings">The list to fill with the metadata of any recordings found.</param>
        /// <remarks>
        /// This method loads only essential metadata without loading the entire frame data,
        /// making it efficient for listing recordings or displaying recording summaries.
        /// </remarks>
        public static void GetSavedRecordingMetadata(List<XRHandRecordingMetadata> existingRecordings)
        {
            try
            {
                // Get all recording files from the device storage path
                var deviceStoragePath = GetDeviceStoragePath();
                string[] files = Directory.Exists(deviceStoragePath) ?
                    Directory.GetFiles(deviceStoragePath, $"*{XRHandRecordingBinaryFileFormatConfigs.k_FileExtension}") :
                    Array.Empty<string>();

                foreach (var file in files)
                {
                    if (TryGetSavedRecordingMetadata(file, out XRHandRecordingMetadata recordingMetadata))
                    {
                        existingRecordings.Add(recordingMetadata);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to retrieve existing recordings: " + ex.Message);
            }
        }

        /// <summary>
        /// Attempts to retrieve a saved recording from disk.
        /// This method reads only the metadata of the recording, it does not load the full frame data.
        /// </summary>
        static bool TryGetSavedRecordingMetadata(string filePath, out XRHandRecordingMetadata recordingMetadata)
        {
            recordingMetadata = null;
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"File does not exist: {filePath}");
                    return false;
                }

                var uniqueID = Path.GetFileNameWithoutExtension(filePath);
                string assetName;
                float elapsedTime;

                using (var stream = File.Open(filePath, FileMode.Open))
                {
                    stream.Position = 0;
                    using (var reader = new BinaryReader(stream))
                    {
                        // Read the version number
                        int version = reader.ReadInt32();
                        if (version != XRHandRecordingBinaryFileFormatConfigs.k_Version)
                        {
                            Debug.LogError($"XR Hand Capture data format version mismatch. " +
                                $"Saved recording uses v{version}, but this application now requires " +
                                $"v{XRHandRecordingBinaryFileFormatConfigs.k_Version}. " +
                                $"Please use a compatible recording file or update the application.");
                            recordingMetadata = null;
                            return false;
                        }

                        // Read the recording name
                        assetName = reader.ReadString();

                        // Read the total time of the recording
                        elapsedTime = reader.ReadSingle();
                    }
                }
                recordingMetadata = new XRHandRecordingMetadata(uniqueID, assetName, elapsedTime);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to retrieve captured data from file {filePath}: {e.Message}");
                return false;
            }
        }
    }
}
