using UnityEngine;
using UnityEngine.XR.Hands;

static class TestCommonGestureData
{
    public class PerHandCommonGestureData
    {
        public Pose aimPose { get; set; }
        public float aimActivateValue { get; set; }
        public Pose gripPose { get; set; }
        public float graspValue { get; set; }
        public Pose pinchPose { get; set; }
        public float pinchValue { get; set; }
        public Pose pokePose { get; set; }
    }

    public static PerHandCommonGestureData leftHand { get; }
    public static PerHandCommonGestureData rightHand { get; }

    public static PerHandCommonGestureData GetCommonGestureData(Handedness handedness)
    {
        if (handedness == Handedness.Right)
            return rightHand;
        else if (handedness == Handedness.Left)
            return leftHand;
        else
            throw new System.ArgumentException("Invalid handedness");
    }

    static TestCommonGestureData()
    {
        leftHand = new PerHandCommonGestureData
        {
            aimPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            aimActivateValue = 0.5f,

            gripPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            graspValue = 0.5f,

            pinchPose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
            pinchValue = 0.5f,

            pokePose = new Pose(new Vector3(1.0f, 2.0f, 3.0f), new Quaternion(0.1f, 0.2f, 0.3f, 0.4f)),
        };

        rightHand = new PerHandCommonGestureData
        {
            aimPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            aimActivateValue = 0.75f,

            gripPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            graspValue = 0.75f,

            pinchPose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
            pinchValue = 0.75f,

            pokePose = new Pose(new Vector3(4.0f, 5.0f, 6.0f), new Quaternion(0.5f, 0.6f, 0.7f, 0.8f)),
        };
    }
}
