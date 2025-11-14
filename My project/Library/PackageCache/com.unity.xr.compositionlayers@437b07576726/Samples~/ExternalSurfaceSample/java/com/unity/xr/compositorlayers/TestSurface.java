package com.unity.xr.compositorlayers;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.view.Surface;

/**
 * Provides functionality to render a Bitmap onto a Surface
 * received from Unity. Primarily used for handling and displaying
 * texture data passed from Unity to Android native code.
 */
public class TestSurface {

    /**
    * Initializes and renders a Bitmap onto a specified Surface.
    *
    * @param surfaceObject An object that should be an instance of android.view.Surface.
    * @param bitmap The Bitmap image to be drawn on the surface.
    * @throws RuntimeException if surfaceObject is not an instance of Surface or if bitmap is null.
    */
    public static void InitTestSurface(Object surfaceObject, Bitmap bitmap) {
        if (!(surfaceObject instanceof Surface)) {
            throw new RuntimeException("TestSurface.ctor: supplied object is not an android.view.Surface!");
        }

        Surface surface = (Surface) surfaceObject;
        Canvas canvas = surface.lockCanvas(null);

        if(bitmap != null)
        {
            canvas.drawBitmap(bitmap, new Matrix(), new Paint());
        }
        else
        {
            throw new RuntimeException("Surface or Image is not initialized or is null.");
        }

        surface.unlockCanvasAndPost(canvas);
    }
}
