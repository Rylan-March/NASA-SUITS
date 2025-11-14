When used with certain OpenXR features from other XR packages, Google's Android XR runtime exhibits unique behaviors, as explained in the following table:

| Feature | Description | Unique behavior |
| :------ | :---------- | :-------------- |
| [Hand tracking](xref:xrhands-openxr-hands-feature) (XR Hands) | Detect and track human hands. | [Hand Tracking](xref:androidxr-openxr-hand-tracking) (Android XR) |
| Hand mesh data | Use hand data for occlusion | [Hand mesh data](xref:androidxr-openxr-hand-mesh-data) |
| [Foveated rendering](xref:openxr-foveated-rendering) (OpenXR) | Optimize rendering on XR devices. | [Eye tracking and foveated rendering](xref:androidxr-openxr-foveated-rendering) (Android XR) |
| [Multiview Render Regions](xref:openxr-multiview-render-regions) (OpenXR) | Optimize rendering to prevent processing on areas of the screen not visible. | [Multiview Render Regions](xref:androidxr-openxr-multiview-render-regions) (Android XR) |
| Display Utilities | Get the supported display refresh rates for the device and request a display refresh rate. | [Display Utilities](xref:androidxr-openxr-display-utilities) (Android XR) |
| [Spacewarp](https://docs.unity3d.com/6000.1/Documentation/Manual/xr-graphics-spacewarp.html) | Use spacewarp to help maintain high frame rates. | [Spacewarp](xref:androidxr-openxr-spacewarp) (Android XR) |