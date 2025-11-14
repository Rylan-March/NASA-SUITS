using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// A static class that initializes a test surface on Android
/// using Java Native Interface (JNI) calls. It facilitates interaction between Unity and
/// Android native code, particularly for handling texture and surface operations.
/// </summary>
public static class AndroidTestSurface
{
    private static System.IntPtr? _TestSurfaceClass;
    private static System.IntPtr initTestSurfaceMethodId;

    /// <summary>
    /// Gets the reference to the Java class 'TestSurface'. It uses JNI to find and hold a
    /// global reference to the class for future use.
    /// </summary>
    private static System.IntPtr TestSurfaceClass
    {
        get
        {
            if (!_TestSurfaceClass.HasValue)
            {
                try
                {
                    // Find the Java class and create a global reference
                    System.IntPtr testSurfaceClassLocal = AndroidJNI.FindClass("com/unity/xr/compositorlayers/TestSurface");

                    if (testSurfaceClassLocal != System.IntPtr.Zero)
                    {
                        _TestSurfaceClass = AndroidJNI.NewGlobalRef(testSurfaceClassLocal);
                        AndroidJNI.DeleteLocalRef(testSurfaceClassLocal);
                    }
                    else
                    {
                        Debug.LogError("Failed to find Java class `TestSurface`");
                        _TestSurfaceClass = System.IntPtr.Zero;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Exception occurred while finding Java class `TestSurface`");
                    Debug.LogException(ex);
                    _TestSurfaceClass = System.IntPtr.Zero;
                }
            }
            return _TestSurfaceClass.GetValueOrDefault();
        }
    }

    /// <summary>
    /// Initializes a test surface using a JNI call to the 'InitTestSurface' method of the
    /// 'TestSurface' Java class. It passes a native Android object and a texture converted to
    /// a Bitmap to the Java method.
    /// </summary>
    /// <param name="jobject">The native Android object to which the surface is attached.</param>
    /// <param name="texture">The Unity Texture2D to be converted and passed as a Bitmap.</param>
    public static void InitTestSurface(System.IntPtr jobject, Texture2D texture)
    {
        if (initTestSurfaceMethodId == System.IntPtr.Zero)
        {
            // Retrieve the method ID for 'InitTestSurface'
            initTestSurfaceMethodId = AndroidJNI.GetStaticMethodID(TestSurfaceClass, "InitTestSurface", "(Ljava/lang/Object;Landroid/graphics/Bitmap;)V");
        }

        // Convert the texture to a Bitmap and call the Java method
        using (AndroidJavaObject bitmap = Texture2DToBitmap(texture))
        {
            System.IntPtr bitmapPtr = bitmap.GetRawObject();
            jvalue[] args = new jvalue[2];
            args[0] = new jvalue { l = jobject };
            args[1] = new jvalue { l = bitmapPtr };

            AndroidJNI.CallStaticVoidMethod(TestSurfaceClass, initTestSurfaceMethodId, args);
        }
    }

    /// <summary>
    /// Converts a Unity Texture2D to an Android Bitmap object. This is used to pass textures
    /// from Unity to Android native code.
    /// </summary>
    /// <param name="texture">The Unity Texture2D to be converted.</param>
    /// <returns>An AndroidJavaObject representing the Bitmap.</returns>
    private static AndroidJavaObject Texture2DToBitmap(Texture2D texture)
    {
        byte[] imageBytes = texture.EncodeToPNG();
        sbyte[] signedImageBytes = (sbyte[])(System.Array)imageBytes;

        using (AndroidJavaClass byteBufferClass = new AndroidJavaClass("java.nio.ByteBuffer"))
        {
            AndroidJavaObject byteBuffer = byteBufferClass.CallStatic<AndroidJavaObject>("wrap", signedImageBytes);
            using (AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory"))
            {
                AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeByteArray", byteBuffer.Call<sbyte[]>("array"), 0, signedImageBytes.Length);

                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                {
                    using (var matrix = new AndroidJavaObject("android.graphics.Matrix"))
                    {
                        matrix.Call<bool>("preScale", 1.0f, -1.0f);

                        using (var bitmapClass = new AndroidJavaClass("android.graphics.Bitmap"))
                        {
                            AndroidJavaObject flippedBitmap = bitmapClass.CallStatic<AndroidJavaObject>("createBitmap",
                                bitmap, 0, 0, bitmap.Call<int>("getWidth"), bitmap.Call<int>("getHeight"), matrix, false);
                            return flippedBitmap;
                        }
                    }
                }

                return bitmap;
            }
        }
    }
}
