---
uid: meta-openxr-device-setup
---
# Set up device

To build and run your AR app on a Meta Quest device, some device setup steps are required. The following sections explain the necessary steps to prepare your Meta Quest device for AR app development.

## Get started

If you are setting up a Meta Quest device for the first time, refer to Meta's [Set up Development Environment and Headset](https://developer.oculus.com/documentation/unity/unity-env-device-setup/) article to learn how to set your headset into Developer Mode and deploy your app via Android Debug Bridge (ADB).

## Meta software update

For the best experience on your Meta Quest device, update to the latest version of the Meta Quest software. This will ensure that you have access to Meta's latest enhancements and bug fixes. You can update your Meta Quest software by going to **Settings** > **General** > **Software Update** in the universal menu on your device.

<a id="space-setup"/>

## Space Setup

Some AR Foundation features require information from Meta's Space Setup before they can report any information. For example, AR Foundation can't track any planes, bounding boxes, or meshes until your user completes Space Setup.

Refer to the following instructions to run Space Setup on your device:

1. Press ![the universal menu button](../images/universal-menu.png) /![the universal menu button](../images/universal-menu-2.png) on your right Touch controller to access the universal menu.
2. Go to **Settings** > **Environment Setup** > **Space Setup**, then select the **Set up** button in the **Space setup** pane.
3. Follow the prompts in your headset to complete Space Setup.

> [!NOTE]
> If **Space Setup** isn't an available option in the **Settings** > **Physical Space** menu on your device, you must update to a more recent version of the Meta Quest software for this option to appear.

### Permissions

Your app's user must grant an Android system permission before you can access Space Setup data. Refer to [Permissions](xref:meta-openxr-scene-setup#permissions) for more information.
