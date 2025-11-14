using System.Collections.Generic;

namespace UnityEditor.XR.Hands.Capture
{
    interface IDeviceFileService
    {
        /// <summary>
        /// Retrieves a list of recording file names from the device that match the specified pattern.
        /// </summary>
        /// <param name="devicePath">The directory path on the device to search.</param>
        /// <param name="filePattern">The search pattern (e.g., "*.bin") to filter files.</param>
        /// <param name="filePaths">A list to be populated with the full paths of found files on device.</param>
        /// <returns>
        /// True if the search completed successfully (regardless of whether any files were found);
        /// false if the search could not be performed.
        /// </returns>
        bool TryFindFiles(string devicePath, string filePattern, List<string> filePaths);

        /// <summary>
        /// Pulls a file from the device to the local machine, if it exists.
        /// </summary>
        /// <param name="sourcePath">The full path of the file on the device.</param>
        /// <param name="destPath">The full path on the local machine where the file should be saved.</param>
        /// <returns>True if the file was successfully pulled; false otherwise.</returns>
        bool TryPullFile(string sourcePath, string destPath);

        /// <summary>
        /// Gets the persistent data path on the device where recording files are stored.
        /// </summary>
        string GetDevicePersistentDataPath();
    }
}
