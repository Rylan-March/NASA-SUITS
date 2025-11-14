using UnityEngine;
using System;
using System.Collections;
using System.IO;
using Unity.XR.CompositionLayers;
#if UNITY_XR_OPENXR_COMPLAYER
using UnityEngine.XR.OpenXR.CompositionLayers;
#endif
using UnityEngine.Networking;

/// <summary>
/// Class for loading and displaying an image on Android devices.
/// </summary>
public class TestAndroidImage : MonoBehaviour
{
    public string ImageName;

    /// <summary>
    /// Coroutine started upon script activation to initiate image loading.
    /// </summary>
    /// <returns>IEnumerator for coroutine management.</returns>
    private IEnumerator Start()
    {
        yield return StartCoroutine(LoadImage());
    }

    /// <summary>
    /// Main coroutine handling the image loading process based on the platform.
    /// Loads an image using specified ImageName and displays it if found.
    /// </summary>
    /// <returns>IEnumerator for coroutine management.</returns>
    private IEnumerator LoadImage()
    {
        if (!string.IsNullOrEmpty(ImageName))
        {
            Texture2D image = null;

#if UNITY_EDITOR
            // Load image for Unity Editor
            image = LoadImageForEditor();
#elif UNITY_ANDROID
            // Load image for Android
            yield return StartCoroutine(LoadImageForAndroid(loadedImage => image = loadedImage));
#endif
            if (image != null)
            {
                // Display the loaded image
                yield return DisplayImage(image);
            }
            else
            {
                Debug.LogError("Image not found at path: " + ImageName);
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Loads an image from the project's assets when running in the Unity Editor.
    /// </summary>
    /// <returns>Texture2D - The loaded image texture.</returns>
    private Texture2D LoadImageForEditor()
    {
        var guids = UnityEditor.AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(ImageName));
        if (guids.Length > 0)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
        return null;
    }
#elif UNITY_ANDROID
    /// <summary>
    /// Coroutine for loading an image in an Android build. Uses UnityWebRequest to fetch the image.
    /// </summary>
    /// <param name="onLoaded">Callback action to handle the loaded Texture2D.</param>
    /// <returns>IEnumerator for coroutine management.</returns>
    private IEnumerator LoadImageForAndroid(Action<Texture2D> onLoaded)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, ImageName);
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error while loading image: " + uwr.error);
                onLoaded(null);
            }
            else
            {
                onLoaded(DownloadHandlerTexture.GetContent(uwr));
            }
        }
    }
#endif

    /// <summary>
    /// Coroutine handling the display of a loaded image using XR composition layers.
    /// Waits for the surface from OpenXRLayerUtility before displaying the image.
    /// </summary>
    /// <param name="image">The Texture2D image to be displayed.</param>
    /// <returns>IEnumerator for coroutine management.</returns>
    private IEnumerator DisplayImage(Texture2D image)
    {
        if (image != null)
        {
            CompositionLayer layer = gameObject.GetComponent<CompositionLayer>();
            IntPtr surface = IntPtr.Zero;
            yield return new WaitUntil(() =>
            {
#if UNITY_XR_OPENXR_COMPLAYER
                surface = OpenXRLayerUtility.GetLayerAndroidSurfaceObject(layer.GetInstanceID());
#endif
                return (surface != IntPtr.Zero);
            });

            AndroidTestSurface.InitTestSurface(surface, image);
        }
        else
        {
            Debug.LogError("No image provided.");
        }
    }

    /// <summary>
    /// Event handler triggered when the application gains or loses focus.
    /// Restarts image loading process when the application gains focus.
    /// </summary>
    /// <param name="hasFocus">Boolean indicating if the application has focus.</param>
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            StartCoroutine(LoadImage());
        }
    }
}
