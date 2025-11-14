#if UNITY_EDITOR && UNITY_ANDROID
using UnityEditor;
using UnityEditor.Android;
using System.IO;
using UnityEngine;

/// <summary>
/// This class primarily focuses on copying specific files
/// from a source folder to the Android project's assets directory after the Gradle project
/// has been generated.
/// </summary>
public class UploadAndroidFiles : IPostGenerateGradleAndroidProject
{
    /// <summary>
    /// Specifies the order in which this post-processing task should be executed.
    /// </summary>
    public int callbackOrder => 1;

    /// <summary>
    /// Called after the Gradle Android project is generated. Copies files from a specified
    /// source folder to the project's assets directory.
    /// </summary>
    /// <param name="path">The path to the generated Gradle project.</param>
    public void OnPostGenerateGradleAndroidProject(string path)
    {

        string sourceFolder = Path.Combine(Application.dataPath, "Samples/XR Composition Layers/2.1.1/Sample External Android Surface Project/StreamingAssets");
        string destinationFolder = Path.Combine(path, "src/main/assets");

        // Create the destination folder if it does not exist
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        CopyFilesRecursively(sourceFolder, destinationFolder);
    }

    /// <summary>
    /// Copies all files and subdirectories from a source path to a target path recursively.
    /// </summary>
    /// <param name="sourcePath">The source directory path.</param>
    /// <param name="targetPath">The target directory path where files should be copied.</param>
    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        // Copy each file in the source directory to the target directory
        foreach (string filePath in Directory.GetFiles(sourcePath))
        {
            string fileName = Path.GetFileName(filePath);
            string destFile = Path.Combine(targetPath, fileName);
            File.Copy(filePath, destFile, true);
        }

        // Recursively copy each subdirectory
        foreach (string subdirectoryPath in Directory.GetDirectories(sourcePath))
        {
            string subdirectoryName = Path.GetFileName(subdirectoryPath);
            string destSubdirectory = Path.Combine(targetPath, subdirectoryName);
            if (!Directory.Exists(destSubdirectory))
            {
                Directory.CreateDirectory(destSubdirectory);
            }
            CopyFilesRecursively(subdirectoryPath, destSubdirectory);
        }
    }
}

#endif
