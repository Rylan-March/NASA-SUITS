#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace UnityEditor.XR.Hands.Capture
{
    class AndroidDeviceFileService : IDeviceFileService
    {
        static string s_AdbPath;

        public AndroidDeviceFileService()
        {
            s_AdbPath = GetAdbPath();
            if (string.IsNullOrEmpty(s_AdbPath))
            {
                UnityEngine.Debug.LogError("ADB executable not found. Please ensure Android SDK path is configured.");
            }
        }
        static string GetAdbPath()
        {
            // Retrieve Android SDK path
            var sdkPath = AndroidExternalToolsSettings.sdkRootPath;
            if (string.IsNullOrEmpty(sdkPath))
                return null;

            var adbPath = Path.Combine(sdkPath, "platform-tools", Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb");
            if (!File.Exists(adbPath))
                return null;

            return adbPath;
        }

        static bool TryExecuteAdbCommand(string adbPath, string arguments, out string commandOutput)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process();
                process.StartInfo = startInfo;

                string errorOut = null;
                process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                    { errorOut += e.Data; });

                process.Start();

                // To avoid deadlocks, use an asynchronous read operation on the error stream.
                process.BeginErrorReadLine();
                commandOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(errorOut))
                {
                    UnityEngine.Debug.LogError(
                        $"ADB command failed. Error: \"{errorOut}\". ADB command arguments: {startInfo.Arguments}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Exception while executing ADB command: {e.Message}");
                commandOutput = string.Empty;
                return false;
            }
        }

        public bool TryFindFiles(string devicePath, string filePattern, List<string> filePaths)
        {
            bool res = TryExecuteAdbCommand(
                s_AdbPath,
                $"shell find \"{devicePath}\" -type f -name \"{filePattern}\"",
                out var commandOutput);

            if (res)
            {
                var recordingFullPaths = new List<string>(
                    commandOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                filePaths.Clear();
                filePaths.AddRange(recordingFullPaths);
            }

            return res;
        }

        public bool TryPullFile(string sourcePath, string destPath)
        {
            bool res = TryExecuteAdbCommand(
                s_AdbPath,
                $"pull \"{sourcePath}\" \"{destPath}\"",
                out var _);

            return res && File.Exists(destPath) && new FileInfo(destPath).Length > 0;
        }

        public string GetDevicePersistentDataPath()
        {
            // This is the path where the recordings are stored on Android devices (Quest and AndroidXR)
            return $"/storage/emulated/0/Android/data/{PlayerSettings.applicationIdentifier}/files/RecordedHandData";
        }
    }
}
#endif
