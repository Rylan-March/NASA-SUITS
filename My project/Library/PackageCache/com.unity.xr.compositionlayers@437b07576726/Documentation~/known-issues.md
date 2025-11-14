---
uid: xr-layer-known-issues
---

# Known Issues

* Equirect layer type - When running your application on an Android head-mounted display (HMD) with the Equirect layer, you may encounter clipping at the top and bottom edges of the displayed content. This can result in parts of the shape being cut off.
* Equirect layer type - When deploying your application to an Android-based head-mounted display (HMD) with the Equirect layer, when both the Upper and Lower Vertical Angles fall into negative values, unexpected behaviors may manifest. These behaviors include image flipping, opacity anomalies affecting surrounding objects, layer displaying incorrectly in size or the layer failing to display altogether.
* Equirect layer type - When running your application on an Android head-mounted display (HMD) with the Equirect layer set to a 360 Central Angle, you may see a vertical 1px seam in the sphere.
* Equirect layer type - Upon entering play mode while the Equirect layer is present in the scene, the Equirect layer will extend to occupy the entire field of view within the HMD, completely filling the visual space.
* Equirect layer type - Upon entering play mode while connected to Android head-mounted display (HMD), the Equirect layer will not properly display any transparency when using a transparent materials.
* Cube layer type - Textures with Mipmaps aren't supported.
* Projection layer type - Single Pass Instanced rendering is not currently supported, which likely affect performance. Future releases will add Single Pass Instance rendering support.
* Projection Eye Rig type - When using Projection Eye Rig in HDR, emulation in the scene view for this layer does not display.
* Layer textures have "fade in" effects from a certain distance. To work around it, try to set the texture format to RGBA 32bit.
