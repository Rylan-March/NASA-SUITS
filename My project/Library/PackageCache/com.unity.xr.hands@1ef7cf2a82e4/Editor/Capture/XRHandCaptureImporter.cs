using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Recording;

namespace UnityEditor.XR.Hands.Capture
{
    class XRHandCaptureImporter
    {
        readonly IDeviceFileService m_FileService;
        readonly List<string> m_DeviceRecordingPathsReuse;

        const string k_ImportTempFolder = "RecordingImport_Temp";

        internal XRHandCaptureImporter(IDeviceFileService fileService)
        {
            m_FileService = fileService;
            m_DeviceRecordingPathsReuse = new List<string>();
        }

        internal bool TryGetAllCaptureSequences(List<XRHandCaptureSequence> recordings)
        {
            string tmpSaveDir = Path.Combine(Application.dataPath, k_ImportTempFolder);

            try
            {
                if (m_FileService == null)
                {
                    Debug.LogError("File service is not initialized.");
                    return false;
                }

                // Find all raw recording files on device
                var deviceStoragePath = m_FileService.GetDevicePersistentDataPath();
                var filePattern = $"*{XRHandRecordingBinaryFileFormatConfigs.k_FileExtension}";

                m_DeviceRecordingPathsReuse.Clear();
                if (!m_FileService.TryFindFiles(deviceStoragePath, filePattern, m_DeviceRecordingPathsReuse))
                    return false;

                if (Directory.Exists(tmpSaveDir))
                    Directory.Delete(tmpSaveDir, true);
                Directory.CreateDirectory(tmpSaveDir);

                // Transfer and convert each recording file to XRHandCaptureSequence
                foreach (var deviceFilePath in m_DeviceRecordingPathsReuse)
                {
                    if (TryTransferAndConvertRecording(deviceFilePath, tmpSaveDir, out var captureSequence))
                    {
                        recordings.Add(captureSequence);
                    }
                }
            }
            finally
            {
                if (Directory.Exists(tmpSaveDir))
                    Directory.Delete(tmpSaveDir, true);
            }

            return true;
        }

        bool TryTransferAndConvertRecording(string deviceFilePath, string savePath, out XRHandCaptureSequence recording)
        {
            recording = null;

            // Transfer raw recording files from device to local storage
            var fileName = Path.GetFileName(deviceFilePath);
            var destPath = Path.Combine(savePath, fileName);

            if (!m_FileService.TryPullFile(deviceFilePath, destPath))
            {
                Debug.LogError($"Failed to transfer file: {fileName}");
                return false;
            }

            // Convert the raw recording to XRHandCaptureSequence
            if (XRHandRecordingBlob.TryReadCaptureSequenceFromDisk(destPath, out recording))
            {
                return true;
            }

            // Clean up if reading fails
            if (recording != null)
            {
                ScriptableObject.DestroyImmediate(recording);
            }
            Debug.LogError($"Failed to read recording data from file: {fileName}");
            return false;
        }
    }
}
