---
uid: androidxr-openxr-scene-setup
---
# Set up your scene

To set up your scene for Android XR, follow the standard AR Foundation [Scene setup](xref:arfoundation-scene-setup) process to add an **AR Session** component to your scene.

## Add an XR Origin GameObject

To use Android XR, you must add an [XR Origin](xref:xr-core-utils-xr-origin) to your scene. If you created a project from a template, the scene will contain an **XR Origin** that you can access in the **Hierarchy** window.

If you're starting a project from scratch, you need to create an **XR Origin** GameObject yourself.

To create an **XR Origin** in a new project, do one of the following:
  * Menu: **GameObject** > **XR** > **Convert Main Camera to XR Rig**.
  * Menu: **GameObject** > **XR** > **XR Origin (VR)**.

Either of these two options creates an **XR Origin** for Android XR.

> [!NOTE]
> Ensure that the **Tracked Pose Driver** component on your XR Origin's camera has `centerEyePosition [XR HMD]` included in the **Position** and **Rotation** input actions.

<a id = "camera-background-passthrough"/>

## Configure camera background for passthrough

If your scene uses the Android XR device's passthrough camera, you need to configure the camera background. Android XR passthrough requires that your Camera's **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) are set to **Solid Color**, with the **Background** color alpha channel value set to zero.

> [!NOTE]
> The passthrough video is layered behind the image rendered by the scene camera. If you configure the camera's background color (or clear flags) to use a skybox or an opaque solid color, then the passthrough video is covered up by the camera background.

If you have completed the AR Foundation scene setup steps, follow these instructions to configure your scene to render with a transparent camera background:

1. Locate the **XR Origin** GameObject in your GameObject hierarchy.
2. Expand the hierarchy to reveal the **Camera Offset** and **Main Camera** GameObjects.
3. Inspect the **Main Camera** GameObject.
4. Select from the following options. The options differ based on the render pipeline you're using:
    * URP: In the **Environment** section, set the **Background Type** to **Solid Color**.
    * Built-In Render Pipeline: Set **Clear Flags** to **Solid Color**.
5. Select the **Background** color to open the color picker.
6. Set the color's **A** value to `0`.

Your scene is now configured to support Android XR passthrough.

## Additional resources

* [Graphics settings](xref:androidxr-openxr-graphics-settings)
* [Scene setup](xref:arfoundation-scene-setup) (AR Foundation)
