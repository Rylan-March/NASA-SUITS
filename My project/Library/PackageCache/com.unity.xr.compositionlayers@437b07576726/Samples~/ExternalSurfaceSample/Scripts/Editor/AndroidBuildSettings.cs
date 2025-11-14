#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// A class that ensures that the Android minimum SDK version meets certain requirements.
/// </summary>
[InitializeOnLoad]
public class AndroidBuildSettings : IPreprocessBuildWithReport
{
    /// Specifies the order in which this pre-build process should be executed.
    public int callbackOrder { get { return 0; } }

    /// Static constructor called on load to initially set Android build settings.
    static AndroidBuildSettings()
    {
        SetAndroidBuildSettings();
    }

    /// <summary>
    /// Called before the build process begins. Ensures Android build settings are correctly set.
    /// </summary>
    /// <param name="report">Contains information about the build, such as its target platform and output path.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        SetAndroidBuildSettings();
    }

    /// <summary>
    /// Sets the minimum SDK version for Android builds to a specified level if it is currently set lower.
    /// This ensures compatibility with features provided in composition layers.
    /// </summary>
    private static void SetAndroidBuildSettings()
    {
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            Debug.Log("Android minimum SDK version has been updated to Level 24. The lowest level supported by the composition layers package.");
        }
    }
}
#endif
