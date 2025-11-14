---
uid: xrhands-gesture-debugger
---

# Gesture Debugger

The **HandGestures** sample scene includes gesture debugging tools to help you design and test both [XRHandShape](xref:xrhands-hand-shapes) and [XRHandPose](xref:xrhands-hand-poses) assets.

> [!NOTE]
>  `XRHandPose` is `XRHandShape` with orientation.

The debugger provides the following information:

* **Current [finger shape values](xref:xrhands-finger-shapes)** for all fingers.
* **Target finger shape values** for one configured `XRHandShape` asset.
* **Tolerance range** for the configured `XRHandShape` asset.
* **Hand pose detection** for six preconfigured `XRHandPose`s.
* **Hand shape completeness** progress bars to indicate the accuracy of the current hand shape match.

> [!TIP]
> You can [edit the HandGestures sample scene](xref:xrhands-customize-gesture-debugger) to change the hand shape used for the target values and tolerances shown on the finger shape graphs. You can also replace the preconfigured hand poses with your own or add additional hand poses.

### **Related Topics**

Refer to the following topics for more information:

| Topic | Description |
| :---- | :---------- |
| [Install the gesture debugger](xref:xrhands-install-gesture-debugger) | How to import the sample HandGestures scene into your project. |
| [Understand the gesture debugger](xref:xrhands-understand-gesture-debugger) | Describes the information shown by the debugger. |
| [Customize the gesture debugger](xref:xrhands-customize-gesture-debugger) | How to add your own hand shapes and hand poses to the debugger. |
| [Understand the hand shape completeness calculator](xref:xrhands-handshape-completeness-calculator) | Describes how hand shape completeness score is calculated and how to implement a custom completeness calculation method. |
