using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Hands;
using Is = Unity.XR.Hands.Tests.NUnitExtensions.Is;

static class TestHandUtils
{
    static readonly float k_EpsilonSqrt = Mathf.Sqrt(Mathf.Epsilon);

    internal delegate bool FloatTryGetFunc(out float param);
    internal delegate bool PoseTryGetFunc(out Pose param);

    public static void AssertSubsystemGestureSupport(XRHandSubsystemDescriptor.Cinfo cinfo, XRHandSubsystemDescriptor descriptor)
    {
        string errorMessage = $"cinfo:{cinfo} descriptor:{descriptor}";
        Assert.IsTrue(cinfo.id == descriptor.id &&
            cinfo.supportsAimPose == descriptor.supportsAimPose &&
            cinfo.supportsAimActivateValue == descriptor.supportsAimActivateValue &&
            cinfo.supportsGripPose == descriptor.supportsGripPose &&
            cinfo.supportsGraspValue == descriptor.supportsGraspValue &&
            cinfo.supportsPinchPose == descriptor.supportsPinchPose &&
            cinfo.supportsPinchValue == descriptor.supportsPinchValue &&
            cinfo.supportsPokePose == descriptor.supportsPokePose, errorMessage);
    }

    public class MockHandAction<TParam>
    {
        public delegate void MockFunction(TParam param);

        public delegate bool ArgumentValidator(TParam param);

        public int callCount => m_CallCount;

        ArgumentValidator m_ArgumentValidator;
        bool m_ChecksEnabled;
        int m_CallCount;
        int m_ExpectedCallCount;
        MockFunction m_MockFunctionDelegate;

        public void WillNotBeCalled()
        {
            m_ExpectedCallCount = 0;
            m_ChecksEnabled = true;
        }

        public MockHandAction<TParam> WillBeCalled(int count)
        {
            m_ExpectedCallCount = count;
            m_ChecksEnabled = true;
            return this;
        }

        public MockHandAction<TParam> WithArguments(TParam param)
        {
            m_ArgumentValidator = args => { return args.Equals(param); };
            return this;
        }

        public MockHandAction<TParam> WithArgumentValidator(ArgumentValidator validator)
        {
            m_ArgumentValidator = validator;
            return this;
        }

        public void Calls(MockFunction mockFunction)
        {
            m_MockFunctionDelegate = mockFunction;
        }

        public void InvokeMock(TParam param)
        {
            m_CallCount++;

            if (m_ChecksEnabled && m_ArgumentValidator != null)
            {
                Assert.IsTrue(m_ArgumentValidator(param), $"Mock invoked with incorrect parameter: ${param.ToString()}");
            }

            if (m_MockFunctionDelegate != null)
            {
                m_MockFunctionDelegate(param);
            }
        }

        public void AssertMockSatisfied()
        {
            Assert.AreEqual(m_ExpectedCallCount, m_CallCount);
        }
    }

    internal static (bool, float) InvokeToTuple(FloatTryGetFunc func)
    {
        bool result = func(out float param);
        return (result, param);
    }

    internal static (bool, Pose) InvokeToTuple(PoseTryGetFunc func)
    {
        bool result = func(out Pose param);
        return (result, param);
    }

    public static void AssertAimPoseUpdated(Handedness handedness, XRCommonHandGestures.AimPoseUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsAimPoseTracked);
        Assert.That(InvokeToTuple(args.TryGetAimPose), Is.EqualTo((true, expectedData.aimPose)));
    }

    public static void AssertAimActivateUpdated(Handedness handedness, XRCommonHandGestures.AimActivateValueUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsAimActivateValueReady);
        Assert.That(InvokeToTuple(args.TryGetAimActivateValue), Is.EqualTo((true, expectedData.aimActivateValue)));
    }

    public static void AssertGripPoseUpdated(Handedness handedness, XRCommonHandGestures.GripPoseUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsGripPoseTracked);
        Assert.That(InvokeToTuple(args.TryGetGripPose), Is.EqualTo((true, expectedData.gripPose)));
    }

    public static void AssertGraspValueUpdated(Handedness handedness, XRCommonHandGestures.GraspValueUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsGraspValueReady);
        Assert.That(InvokeToTuple(args.TryGetGraspValue), Is.EqualTo((true, expectedData.graspValue)));
    }

    public static void AssertPinchPoseUpdated(Handedness handedness, XRCommonHandGestures.PinchPoseUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsPinchPoseTracked);
        Assert.That(InvokeToTuple(args.TryGetPinchPose), Is.EqualTo((true, expectedData.pinchPose)));
    }

    public static void AssertPinchValueUpdated(Handedness handedness, XRCommonHandGestures.PinchValueUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsPinchValueReady);
        Assert.That(InvokeToTuple(args.TryGetPinchValue), Is.EqualTo((true, expectedData.pinchValue)));
    }

    public static void AssertPokePoseUpdated(Handedness handedness, XRCommonHandGestures.PokePoseUpdatedEventArgs args)
    {
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        Assert.IsTrue(args.m_IsPokePoseTracked);
        Assert.That(InvokeToTuple(args.TryGetPokePose), Is.EqualTo((true, expectedData.pokePose)));
    }

    public static void AssertAreApproximatelyEqual(float expected, float actual, string message = null)
    {
        Assert.IsTrue(Mathf.Abs(actual - expected) <= k_EpsilonSqrt, message);
    }

    public static void AssertAreApproximatelyEqual(Vector3 expected, Vector3 actual, string message = null)
    {
        AssertAreApproximatelyEqual(expected.x, actual.x, message);
        AssertAreApproximatelyEqual(expected.y, actual.y, message);
        AssertAreApproximatelyEqual(expected.z, actual.z, message);
    }

    public static void AssertAreApproximatelyEqual(Quaternion expected, Quaternion actual, string message = null)
    {
        AssertAreApproximatelyEqual(expected.x, actual.x, message);
        AssertAreApproximatelyEqual(expected.y, actual.y, message);
        AssertAreApproximatelyEqual(expected.z, actual.z, message);
        AssertAreApproximatelyEqual(expected.w, actual.w, message);
    }

    public static void AssertAreApproximatelyEqual(Pose expected, Pose actual, string message = null)
    {
        AssertAreApproximatelyEqual(expected.position, actual.position, message);
        AssertAreApproximatelyEqual(expected.rotation, actual.rotation, message);
    }

    public static XRHandSubsystem CreateTestSubsystem()
    {
        var handsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = TestHandProvider.descriptorId,
            providerType = typeof(TestHandProvider)
        };
        return CreateTestSubsystem(handsSubsystemCinfo);
    }

    static void EnsureTestSubsystemDescriptorRegistered(XRHandSubsystemDescriptor.Cinfo handsSubsystemCinfo)
    {
        var descriptors = new List<XRHandSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(descriptors);

        foreach (var descriptor in descriptors)
        {
            if (descriptor.id == handsSubsystemCinfo.id)
                return;
        }

        XRHandSubsystemDescriptor.Register(handsSubsystemCinfo);
    }

    public static XRHandSubsystem CreateTestSubsystem(XRHandSubsystemDescriptor.Cinfo handsSubsystemDescriptor)
    {
        EnsureTestSubsystemDescriptorRegistered(handsSubsystemDescriptor);

        var descriptors = new List<XRHandSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(descriptors);

        foreach (var descriptor in descriptors)
        {
            if (descriptor.id == handsSubsystemDescriptor.id)
                return descriptor.Create();
        }

        return null;
    }

    public static void CreateTwoTestSubsystems(out XRHandSubsystem subsystem1, out XRHandSubsystem subsystem2)
    {
        var subsystemCinfo1 = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "TestHandProvider1",
            providerType = typeof(TestHandProvider)
        };
        subsystem1 = TestHandUtils.CreateTestSubsystem(subsystemCinfo1);

        var subsystemCinfo2 = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "TestHandProvider2",
            providerType = typeof(TestHandProvider)
        };
        subsystem2 = TestHandUtils.CreateTestSubsystem(subsystemCinfo2);
    }
}
