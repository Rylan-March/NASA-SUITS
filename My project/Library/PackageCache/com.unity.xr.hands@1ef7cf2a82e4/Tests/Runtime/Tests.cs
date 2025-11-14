using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.TestTools;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Configuration;
using UnityEngine.XR.Hands.ProviderImplementation;
using static UnityEngine.XR.Hands.Configuration.XRHandSubsystemConfiguration;
using Is = Unity.XR.Hands.Tests.NUnitExtensions.Is;

class Tests
{
    static readonly Handedness[] s_HandednessOptions =
    {
        Handedness.Left,
        Handedness.Right
    };

    [Test]
    public void TestFingerIDsToJointIDs()
    {
        Assert.AreEqual((int)XRHandFingerID.Thumb.GetFrontJointID() + 3, (int)XRHandFingerID.Thumb.GetBackJointID());
        Assert.AreEqual((int)XRHandFingerID.Index.GetFrontJointID() + 4, (int)XRHandFingerID.Index.GetBackJointID());
        Assert.AreEqual((int)XRHandFingerID.Middle.GetFrontJointID() + 4, (int)XRHandFingerID.Middle.GetBackJointID());
        Assert.AreEqual((int)XRHandFingerID.Ring.GetFrontJointID() + 4, (int)XRHandFingerID.Ring.GetBackJointID());
        Assert.AreEqual((int)XRHandFingerID.Little.GetFrontJointID() + 4, (int)XRHandFingerID.Little.GetBackJointID());

        Assert.IsTrue(XRHandFingerID.Thumb.GetBackJointID() < XRHandFingerID.Index.GetFrontJointID());
        Assert.IsTrue(XRHandFingerID.Index.GetBackJointID() < XRHandFingerID.Middle.GetFrontJointID());
        Assert.IsTrue(XRHandFingerID.Middle.GetBackJointID() < XRHandFingerID.Ring.GetFrontJointID());
        Assert.IsTrue(XRHandFingerID.Ring.GetBackJointID() < XRHandFingerID.Little.GetFrontJointID());
    }

    [Test]
    public void CanCreateTestSubsystem()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        Assert.AreNotEqual(subsystem, null);
        subsystem.Destroy();
    }

    [Test]
    public void SubsystemAsksForHandLayoutDuringCreate()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();

        var testProvider = subsystem.GetProvider() as TestHandProvider;
        Assert.IsNotNull(testProvider);
        Assert.AreEqual(1, testProvider.numGetHandLayoutCalls);

        subsystem.Destroy();
    }

    [Test]
    public void SubsystemWontAskProviderForHandDataWithoutStarting()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        for (var call = 0; call < 10; ++call)
        {
            var flags = subsystem.TryUpdateHands(
                ((call & 1) != 0)
                    ? XRHandSubsystem.UpdateType.Dynamic
                    : XRHandSubsystem.UpdateType.BeforeRender);
            Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.None, flags);
        }

        var testProvider = subsystem.GetProvider() as TestHandProvider;
        Assert.IsNotNull(testProvider);
        Assert.AreEqual(0, testProvider.numTryUpdateHandsCalls);

        subsystem.Destroy();
    }

    [Test]
    public void SubsystemAsksForHandsDataIfRunning()
    {
        const int numUpdates = 10;
        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.Start();

        for (var call = 0; call < numUpdates; ++call)
        {
            var flags = subsystem.TryUpdateHands(
                ((call & 1) != 0)
                ? XRHandSubsystem.UpdateType.Dynamic
                : XRHandSubsystem.UpdateType.BeforeRender);
            Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.All, flags);
        }

        var testProvider = subsystem.GetProvider() as TestHandProvider;
        Assert.IsNotNull(testProvider);
        Assert.AreEqual(numUpdates, testProvider.numTryUpdateHandsCalls);

        subsystem.Destroy();
    }

    [Test]
    public void HandsMarkedWithCorrectHandedness()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();

        Assert.AreEqual(Handedness.Left, subsystem.leftHand.handedness);
        Assert.AreEqual(Handedness.Right, subsystem.rightHand.handedness);

        subsystem.Destroy();
    }

    [Test]
    public void StopIsCalledImplcitlyOnDestroyIfRunning()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.Start();
        subsystem.Destroy();

        var testProvider = subsystem.GetProvider() as TestHandProvider;
        Assert.IsNotNull(testProvider);
        Assert.AreEqual(1, testProvider.numStartCalls);
        Assert.AreEqual(1, testProvider.numStopCalls);
        Assert.AreEqual(1, testProvider.numDestroyCalls);
    }

    [Test]
    public void ProviderGivesValidHandDataWhenRunning()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.Start();

        var updateFlags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.All, updateFlags);

        TestHandUtils.AssertAreApproximatelyEqual(TestHandData.leftRoot, subsystem.leftHand.rootPose);
        TestHandUtils.AssertAreApproximatelyEqual(TestHandData.rightRoot, subsystem.rightHand.rootPose);

        int numValidLeftJoints = 0, numValidRightJoints = 0;
        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
             jointIndex < XRHandJointID.EndMarker.ToIndex();
             ++jointIndex)
        {
            var id = XRHandJointIDUtility.FromIndex(jointIndex);

            var leftJoint = subsystem.leftHand.GetJoint(id);
            if (leftJoint.TryGetPose(out var leftPose))
            {
                ++numValidLeftJoints;
                TestHandUtils.AssertAreApproximatelyEqual(TestHandData.leftHand[jointIndex], leftPose);
            }
            else
            {
                Assert.IsFalse(subsystem.jointsInLayout[jointIndex]);
            }

            var rightJoint = subsystem.rightHand.GetJoint(id);
            if (rightJoint.TryGetPose(out var rightPose))
            {
                ++numValidRightJoints;
                TestHandUtils.AssertAreApproximatelyEqual(TestHandData.rightHand[jointIndex], rightPose);
            }
            else
            {
                Assert.IsFalse(subsystem.jointsInLayout[jointIndex]);
            }
        }

        Assert.AreEqual(26, numValidLeftJoints);
        Assert.AreEqual(26, numValidRightJoints);
        subsystem.Destroy();
    }

    [Test]
    public void MockProviderOnlyGivesHandPosesAndSensibleDefaults()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.Start();

        var updateFlags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.All, updateFlags);

        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
             jointIndex < XRHandJointID.EndMarker.ToIndex();
             ++jointIndex)
        {
            var id = XRHandJointIDUtility.FromIndex(jointIndex);

            var leftJoint = subsystem.leftHand.GetJoint(id);
            Assert.AreEqual(XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose, leftJoint.trackingState);
            Assert.IsFalse(leftJoint.TryGetRadius(out var leftRadius));
            Assert.IsFalse(leftJoint.TryGetLinearVelocity(out var leftLinearVelocity));
            Assert.IsFalse(leftJoint.TryGetAngularVelocity(out var leftAngularVelocity));
            Assert.AreEqual(0f, leftRadius);
            Assert.AreEqual(Vector3.zero, leftLinearVelocity);
            Assert.AreEqual(Vector3.zero, leftAngularVelocity);

            var rightJoint = subsystem.rightHand.GetJoint(id);
            Assert.AreEqual(XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose, rightJoint.trackingState);
            Assert.IsFalse(rightJoint.TryGetRadius(out var rightRadius));
            Assert.IsFalse(rightJoint.TryGetLinearVelocity(out var rightLinearVelocity));
            Assert.IsFalse(rightJoint.TryGetAngularVelocity(out var rightAngularVelocity));
            Assert.AreEqual(0f, rightRadius);
            Assert.AreEqual(Vector3.zero, rightLinearVelocity);
            Assert.AreEqual(Vector3.zero, rightAngularVelocity);
        }

        subsystem.Destroy();
    }

    [Test]
    public void XRHandJointToStringNeverNull()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();

        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
             jointIndex < XRHandJointID.EndMarker.ToIndex();
             ++jointIndex)
        {
            var id = XRHandJointIDUtility.FromIndex(jointIndex);
            var leftJoint = subsystem.leftHand.GetJoint(id);
            var rightJoint = subsystem.rightHand.GetJoint(id);
            Assert.AreNotEqual(null, leftJoint.ToString());
            Assert.AreNotEqual(null, rightJoint.ToString());
        }

        subsystem.Destroy();
    }

    [Test]
    public void ToIndexJustSubtractsOne()
    {
        Assert.AreEqual(XRHandJointID.Invalid.ToIndex(), (int)XRHandJointID.Invalid - 1);
        Assert.AreEqual(XRHandJointID.BeginMarker.ToIndex(), (int)XRHandJointID.BeginMarker - 1);
        Assert.AreEqual(XRHandJointID.Wrist.ToIndex(), (int)XRHandJointID.Wrist - 1);
        Assert.AreEqual(XRHandJointID.Palm.ToIndex(), (int)XRHandJointID.Palm - 1);
        Assert.AreEqual(XRHandJointID.ThumbMetacarpal.ToIndex(), (int)XRHandJointID.ThumbMetacarpal - 1);
        Assert.AreEqual(XRHandJointID.ThumbProximal.ToIndex(), (int)XRHandJointID.ThumbProximal - 1);
        Assert.AreEqual(XRHandJointID.ThumbDistal.ToIndex(), (int)XRHandJointID.ThumbDistal - 1);
        Assert.AreEqual(XRHandJointID.ThumbTip.ToIndex(), (int)XRHandJointID.ThumbTip - 1);
        Assert.AreEqual(XRHandJointID.IndexMetacarpal.ToIndex(), (int)XRHandJointID.IndexMetacarpal - 1);
        Assert.AreEqual(XRHandJointID.IndexProximal.ToIndex(), (int)XRHandJointID.IndexProximal - 1);
        Assert.AreEqual(XRHandJointID.IndexIntermediate.ToIndex(), (int)XRHandJointID.IndexIntermediate - 1);
        Assert.AreEqual(XRHandJointID.IndexDistal.ToIndex(), (int)XRHandJointID.IndexDistal - 1);
        Assert.AreEqual(XRHandJointID.IndexTip.ToIndex(), (int)XRHandJointID.IndexTip - 1);
        Assert.AreEqual(XRHandJointID.MiddleMetacarpal.ToIndex(), (int)XRHandJointID.MiddleMetacarpal - 1);
        Assert.AreEqual(XRHandJointID.MiddleProximal.ToIndex(), (int)XRHandJointID.MiddleProximal - 1);
        Assert.AreEqual(XRHandJointID.MiddleIntermediate.ToIndex(), (int)XRHandJointID.MiddleIntermediate - 1);
        Assert.AreEqual(XRHandJointID.MiddleDistal.ToIndex(), (int)XRHandJointID.MiddleDistal - 1);
        Assert.AreEqual(XRHandJointID.MiddleTip.ToIndex(), (int)XRHandJointID.MiddleTip - 1);
        Assert.AreEqual(XRHandJointID.RingMetacarpal.ToIndex(), (int)XRHandJointID.RingMetacarpal - 1);
        Assert.AreEqual(XRHandJointID.RingProximal.ToIndex(), (int)XRHandJointID.RingProximal - 1);
        Assert.AreEqual(XRHandJointID.RingIntermediate.ToIndex(), (int)XRHandJointID.RingIntermediate - 1);
        Assert.AreEqual(XRHandJointID.RingDistal.ToIndex(), (int)XRHandJointID.RingDistal - 1);
        Assert.AreEqual(XRHandJointID.RingTip.ToIndex(), (int)XRHandJointID.RingTip - 1);
        Assert.AreEqual(XRHandJointID.LittleMetacarpal.ToIndex(), (int)XRHandJointID.LittleMetacarpal - 1);
        Assert.AreEqual(XRHandJointID.LittleProximal.ToIndex(), (int)XRHandJointID.LittleProximal - 1);
        Assert.AreEqual(XRHandJointID.LittleIntermediate.ToIndex(), (int)XRHandJointID.LittleIntermediate - 1);
        Assert.AreEqual(XRHandJointID.LittleDistal.ToIndex(), (int)XRHandJointID.LittleDistal - 1);
        Assert.AreEqual(XRHandJointID.LittleTip.ToIndex(), (int)XRHandJointID.LittleTip - 1);
        Assert.AreEqual(XRHandJointID.EndMarker.ToIndex(), (int)XRHandJointID.EndMarker - 1);
    }

    [Test]
    public void FromIndexJustAddsOne()
    {
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.Invalid - 1), XRHandJointID.Invalid);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.BeginMarker - 1), XRHandJointID.BeginMarker);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.Wrist - 1), XRHandJointID.Wrist);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.Palm - 1), XRHandJointID.Palm);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.ThumbMetacarpal - 1), XRHandJointID.ThumbMetacarpal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.ThumbProximal - 1), XRHandJointID.ThumbProximal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.ThumbDistal - 1), XRHandJointID.ThumbDistal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.ThumbTip - 1), XRHandJointID.ThumbTip);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexMetacarpal - 1), XRHandJointID.IndexMetacarpal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexProximal - 1), XRHandJointID.IndexProximal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexIntermediate - 1), XRHandJointID.IndexIntermediate);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexDistal - 1), XRHandJointID.IndexDistal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.IndexTip - 1), XRHandJointID.IndexTip);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.MiddleMetacarpal - 1), XRHandJointID.MiddleMetacarpal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.MiddleProximal - 1), XRHandJointID.MiddleProximal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.MiddleIntermediate - 1), XRHandJointID.MiddleIntermediate);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.MiddleDistal - 1), XRHandJointID.MiddleDistal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.MiddleTip - 1), XRHandJointID.MiddleTip);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.RingMetacarpal - 1), XRHandJointID.RingMetacarpal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.RingProximal - 1), XRHandJointID.RingProximal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.RingIntermediate - 1), XRHandJointID.RingIntermediate);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.RingDistal - 1), XRHandJointID.RingDistal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.RingTip - 1), XRHandJointID.RingTip);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.LittleMetacarpal - 1), XRHandJointID.LittleMetacarpal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.LittleProximal - 1), XRHandJointID.LittleProximal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.LittleIntermediate - 1), XRHandJointID.LittleIntermediate);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.LittleDistal - 1), XRHandJointID.LittleDistal);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.LittleTip - 1), XRHandJointID.LittleTip);
        Assert.AreEqual(XRHandJointIDUtility.FromIndex((int)XRHandJointID.EndMarker - 1), XRHandJointID.EndMarker);
    }

    [Test]
    public void SubsystemPassesUpdateTypeDirectlyToProvider()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        var testProvider = subsystem.GetProvider() as TestHandProvider;
        subsystem.Start();

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.IsNotNull(testProvider);
        Assert.AreEqual(XRHandSubsystem.UpdateType.Dynamic, testProvider.mostRecentUpdateType);

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.BeforeRender);
        Assert.AreEqual(XRHandSubsystem.UpdateType.BeforeRender, testProvider.mostRecentUpdateType);

        subsystem.Destroy();
    }

    [Test]
    public void HandsUpdatedCallbackWorks()
    {
        var flags = XRHandSubsystem.UpdateSuccessFlags.None;
        void OnUpdatedHands(XRHandSubsystem xrHandSubsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType) => flags = updateSuccessFlags;

        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.updatedHands += OnUpdatedHands;
        subsystem.Start();

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.All, flags);

        subsystem.Destroy();
    }

    [Test]
    public void DescriptorCinfoEqualsReturnsTrue()
    {
        var testCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = TestHandProvider.descriptorId,
            providerType = typeof(TestHandProvider)
        };

        var testCinfo2 = new XRHandSubsystemDescriptor.Cinfo
        {
            id = TestHandProvider.descriptorId,
            providerType = typeof(TestHandProvider)
        };

        Assert.IsTrue(testCinfo.Equals(testCinfo2));
        Assert.AreEqual(testCinfo.GetHashCode(), testCinfo2.GetHashCode());
    }

    [Test]
    public void DescriptorCinfoEqualsReturnsFalse()
    {
        var testCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = TestHandProvider.descriptorId,
            providerType = typeof(TestHandProvider)
        };

        var testCinfo2 = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "Dummy-Hands"
        };

        Assert.IsFalse(testCinfo.Equals(testCinfo2));
        Assert.AreNotEqual(testCinfo.GetHashCode(), testCinfo2.GetHashCode());
    }

    [Test]
    public void JointIDsMatch()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        subsystem.Start();

        var flags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        Assert.AreEqual(XRHandSubsystem.UpdateSuccessFlags.All, flags);

        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
             jointIndex < XRHandJointID.EndMarker.ToIndex();
             ++jointIndex)
        {
            var id = XRHandJointIDUtility.FromIndex(jointIndex);
            var leftJoint = subsystem.leftHand.GetJoint(id);
            var rightJoint = subsystem.rightHand.GetJoint(id);

            Assert.AreEqual(id, leftJoint.id);
            Assert.AreEqual(id, rightJoint.id);
        }

        subsystem.Destroy();
    }

    [Test]
    public void TestJointsAreInSubsystemLayout()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();

        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
             jointIndex < XRHandJointID.EndMarker.ToIndex();
             ++jointIndex)
        {
            Assert.AreEqual(TestHandData.jointsInLayout[jointIndex], subsystem.jointsInLayout[jointIndex]);
        }

        subsystem.Destroy();
    }

    [Test]
    public void TestConfigurationUpdate()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        XRHandSubsystemConfiguration configuration = new()
        {
            xrHandDevicePoseSource = XRHandDevicePoseSource.CommonGestures
        };

        XRHandDevicePoseSource expectedPoseSource = XRHandDevicePoseSource.CommonGestures;
        XRHandSubsystemConfiguration receivedConfiguration = default;
        XRHandSubsystem subsystemSender = null;
        subsystem.configurationUpdated += (args) =>
        {
            receivedConfiguration = args.newConfiguration;
            subsystemSender = args.subsystem;
        };

        subsystem.UpdateHandsConfiguration(configuration);

        Assert.That(receivedConfiguration.xrHandDevicePoseSource, Is.EqualTo(expectedPoseSource));
        Assert.That(subsystemSender, Is.EqualTo(subsystem));
        Assert.That(subsystem.handSubsystemConfiguration.xrHandDevicePoseSource, Is.EqualTo(expectedPoseSource));

        expectedPoseSource = XRHandDevicePoseSource.LegacyJointRecognition;
        subsystemSender = null;
        configuration.xrHandDevicePoseSource = expectedPoseSource;
        subsystem.UpdateHandsConfiguration(configuration);

        Assert.That(receivedConfiguration.xrHandDevicePoseSource, Is.EqualTo(expectedPoseSource));
        Assert.That(subsystemSender, Is.EqualTo(subsystem));
        Assert.That(subsystem.handSubsystemConfiguration.xrHandDevicePoseSource, Is.EqualTo(expectedPoseSource));
    }

    [UnityTest]
    public IEnumerator CommonGesturePoseUpdatesXRHandDevice([ValueSource(nameof(s_HandednessOptions))] Handedness handedness)
    {
#if ENABLE_INPUT_SYSTEM
        var expectedData = TestCommonGestureData.GetCommonGestureData(handedness);
        var subsystem = TestHandUtils.CreateTestSubsystem();
        XRHandSubsystemConfiguration configuration = new()
        {
            xrHandDevicePoseSource = XRHandDevicePoseSource.CommonGestures
        };
        subsystem.UpdateHandsConfiguration(configuration);
        subsystem.Start();

        var updateSuccessFlags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        if (handedness == Handedness.Left && XRHandDevice.leftHand == null)
            XRHandDevice.leftHand = XRHandDevice.Create(subsystem, Handedness.Left, updateSuccessFlags, XRHandSubsystem.UpdateType.Dynamic);
        else if (handedness == Handedness.Right && XRHandDevice.rightHand == null)
            XRHandDevice.rightHand = XRHandDevice.Create(subsystem, Handedness.Right, updateSuccessFlags, XRHandSubsystem.UpdateType.Dynamic);

        XRHandDevice currentHandDevice = handedness == Handedness.Left ? XRHandDevice.leftHand : XRHandDevice.rightHand;

        Assert.That((InputTrackingState)currentHandDevice.pinchTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.pinchPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.pinchRotation.ReadValue(), Is.EqualTo(new Quaternion()));
        Assert.That(currentHandDevice.pinchValue.ReadValue(), Is.EqualTo(0.0f));

        Assert.That((InputTrackingState)currentHandDevice.aimTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.aimPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.aimRotation.ReadValue(), Is.EqualTo(new Quaternion()));
        Assert.That(currentHandDevice.aimActivateValue.ReadValue(), Is.EqualTo(0.0f));

        Assert.That((InputTrackingState)currentHandDevice.gripTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.gripPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.gripRotation.ReadValue(), Is.EqualTo(new Quaternion()));
        Assert.That(currentHandDevice.graspValue.ReadValue(), Is.EqualTo(0.0f));

        Assert.That((InputTrackingState)currentHandDevice.pokeTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.pokePosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.pokeRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        XRCommonHandGestures currentCommonGestures = handedness == Handedness.Left ? subsystem.leftHandCommonGestures : subsystem.rightHandCommonGestures;
        currentCommonGestures.UpdatePinchPose(expectedData.pinchPose);
        currentCommonGestures.UpdateAimPose(expectedData.aimPose);
        currentCommonGestures.UpdateGripPose(expectedData.gripPose);
        currentCommonGestures.UpdatePinchValue(expectedData.pinchValue);
        currentCommonGestures.UpdateAimActivateValue(expectedData.aimActivateValue);
        currentCommonGestures.UpdateGraspValue(expectedData.graspValue);
        currentCommonGestures.UpdatePokePose(expectedData.pokePose);

        subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        InputSystem.Update();
        yield return null;

        Vector3 pinchPosition = currentHandDevice.pinchPosition.ReadValue();
        Quaternion pinchRotation = currentHandDevice.pinchRotation.ReadValue();
        float pinchValue = currentHandDevice.pinchValue.ReadValue();
        var pinchTrackingState = (InputTrackingState)currentHandDevice.pinchTrackingState.ReadValue();

        Vector3 aimPosition = currentHandDevice.aimPosition.ReadValue();
        Quaternion aimRotation = currentHandDevice.aimRotation.ReadValue();
        float aimValue = currentHandDevice.aimActivateValue.ReadValue();
        var aimTrackingState = (InputTrackingState)currentHandDevice.aimTrackingState.ReadValue();

        Vector3 gripPosition = currentHandDevice.gripPosition.ReadValue();
        Quaternion gripRotation = currentHandDevice.gripRotation.ReadValue();
        float graspValue = currentHandDevice.graspValue.ReadValue();
        var gripTrackingState = (InputTrackingState)currentHandDevice.gripTrackingState.ReadValue();

        Vector3 pokePosition = currentHandDevice.pokePosition.ReadValue();
        Quaternion pokeRotation = currentHandDevice.pokeRotation.ReadValue();
        var pokeTrackingState = (InputTrackingState)currentHandDevice.pokeTrackingState.ReadValue();

        Assert.That(pinchPosition, Is.EqualTo(expectedData.pinchPose.position));
        Assert.That(pinchRotation, Is.EqualTo(expectedData.pinchPose.rotation));
        Assert.That(pinchValue, Is.EqualTo(expectedData.pinchValue));
        Assert.That(pinchTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));

        Assert.That(aimPosition, Is.EqualTo(expectedData.aimPose.position));
        Assert.That(aimRotation, Is.EqualTo(expectedData.aimPose.rotation));
        Assert.That(aimValue, Is.EqualTo(expectedData.aimActivateValue));
        Assert.That(aimTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));

        Assert.That(gripPosition, Is.EqualTo(expectedData.gripPose.position));
        Assert.That(gripRotation, Is.EqualTo(expectedData.gripPose.rotation));
        Assert.That(graspValue, Is.EqualTo(expectedData.graspValue));
        Assert.That(gripTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));

        Assert.That(pokePosition, Is.EqualTo(expectedData.pokePose.position));
        Assert.That(pokeRotation, Is.EqualTo(expectedData.pokePose.rotation));
        Assert.That(pokeTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));

        XRHandDevice.leftHand = null;
        XRHandDevice.rightHand = null;
#else
        yield return null;
#endif
    }

    [UnityTest]
    public IEnumerator LegacyJointRecognitionPoseUpdatesXRHandDevice([ValueSource(nameof(s_HandednessOptions))] Handedness handedness)
    {
#if ENABLE_INPUT_SYSTEM
        var subsystem = TestHandUtils.CreateTestSubsystem();
        XRHandSubsystemConfiguration configuration = new()
        {
            xrHandDevicePoseSource = XRHandDevicePoseSource.LegacyJointRecognition
        };

        subsystem.UpdateHandsConfiguration(configuration);
        subsystem.Start();

        var updateSuccessFlags = subsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);

        yield return null;

        if (handedness == Handedness.Left && XRHandDevice.leftHand == null)
            XRHandDevice.leftHand = XRHandDevice.Create(subsystem, Handedness.Left, updateSuccessFlags, XRHandSubsystem.UpdateType.Dynamic);
        else if (handedness == Handedness.Right && XRHandDevice.rightHand == null)
            XRHandDevice.rightHand = XRHandDevice.Create(subsystem, Handedness.Right, updateSuccessFlags, XRHandSubsystem.UpdateType.Dynamic);

        XRHandDevice currentHandDevice = handedness == Handedness.Left ? XRHandDevice.leftHand : XRHandDevice.rightHand;
        XRHand currentHand = handedness == Handedness.Left ? subsystem.leftHand : subsystem.rightHand;

        Assert.That((InputTrackingState)currentHandDevice.trackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.devicePosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.deviceRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        Assert.That((InputTrackingState)currentHandDevice.gripTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.gripPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.gripRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        Assert.That((InputTrackingState)currentHandDevice.pokeTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.pokePosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.pokeRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        Assert.That((InputTrackingState)currentHandDevice.pinchTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.pinchPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.pinchRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        // Assert tracking states are None before update
        Assert.That((InputTrackingState)currentHandDevice.aimTrackingState.ReadValue(), Is.EqualTo(InputTrackingState.None));
        Assert.That(currentHandDevice.aimPosition.ReadValue(), Is.EqualTo(Vector3.zero));
        Assert.That(currentHandDevice.aimRotation.ReadValue(), Is.EqualTo(new Quaternion()));

        InputSystem.Update();
        yield return null;

        var deviceTrackingState = (InputTrackingState)currentHandDevice.trackingState.ReadValue();
        Vector3 devicePosition = currentHandDevice.devicePosition.ReadValue();
        Quaternion deviceRotation = currentHandDevice.deviceRotation.ReadValue();
        currentHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose);

        var aimTrackingState = (InputTrackingState)currentHandDevice.aimTrackingState.ReadValue();
        Vector3 aimPosition = currentHandDevice.aimPosition.ReadValue();
        Quaternion aimRotation = currentHandDevice.aimRotation.ReadValue();

        var gripTrackingState = (InputTrackingState)currentHandDevice.gripTrackingState.ReadValue();
        Vector3 gripPosition = currentHandDevice.gripPosition.ReadValue();
        Quaternion gripRotation = currentHandDevice.gripRotation.ReadValue();
        currentHand.GetJoint(XRHandJointID.Palm).TryGetPose(out var palmPose);

        var pokeTrackingState = (InputTrackingState)currentHandDevice.pokeTrackingState.ReadValue();
        Vector3 pokePosition = currentHandDevice.pokePosition.ReadValue();
        Quaternion pokeRotation = currentHandDevice.pokeRotation.ReadValue();
        currentHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var indexTipPose);

        var pinchTrackingState = (InputTrackingState)currentHandDevice.pinchTrackingState.ReadValue();
        Vector3 pinchPosition = currentHandDevice.pinchPosition.ReadValue();
        Quaternion pinchRotation = currentHandDevice.pinchRotation.ReadValue();
        currentHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var thumbTipPose);

        Assert.That(deviceTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
        Assert.That(devicePosition, Is.EqualTo(wristPose.position));
        Assert.That(deviceRotation, Is.EqualTo(wristPose.rotation));

        Assert.That(gripTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
        Assert.That(gripPosition, Is.EqualTo(palmPose.position));
        Assert.That(gripRotation, Is.EqualTo(palmPose.rotation));

        Assert.That(pokeTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
        Assert.That(pokePosition, Is.EqualTo(indexTipPose.position));
        Assert.That(pokeRotation, Is.EqualTo(indexTipPose.rotation));

        Assert.That(pinchTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
        Assert.That(pinchPosition, Is.EqualTo(thumbTipPose.position));
        Assert.That(pinchRotation, Is.EqualTo(thumbTipPose.rotation));

        // Make sure aim is still reporting nothing
        Assert.That(aimTrackingState, Is.EqualTo(InputTrackingState.None));
        Assert.That(aimPosition, Is.EqualTo(Vector3.zero));
        Assert.That(aimRotation, Is.EqualTo(new Quaternion()));

       XRHandDevice.leftHand = null;
       XRHandDevice.rightHand = null;
#else
        yield return null;
#endif
    }

    [Test]
    public void TestDefaultConfiguration()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        Assert.That(subsystem.handSubsystemConfiguration.xrHandDevicePoseSource, Is.EqualTo(XRHandDevicePoseSource.LegacyJointRecognition));
    }

    [UnityTest]
    public IEnumerator SubsystemUpdaterWorks()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);
        try
        {
            bool updated = false;
            void OnUpdatedHands(XRHandSubsystem xrHandSubsystem, XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateType updateType) => updated = true;
            subsystem.updatedHands += OnUpdatedHands;

            subsystem.Start();
            updater.Start();

            yield return null;
            Assert.IsTrue(updated);
        }
        finally
        {
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();
        }
    }

    [UnityTest]
    public IEnumerator HandTrackingEventCallbacks()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);
        var provider = (TestHandProvider)subsystem.GetProvider();
        var go = new GameObject("TestHandTrackingEvents");

        try
        {
            subsystem.Start();
            updater.Start();
            yield return null;

            var rightHandTrackingEvents = go.AddComponent<XRHandTrackingEvents>();
            rightHandTrackingEvents.handedness = Handedness.Right;
            rightHandTrackingEvents.updateType = XRHandTrackingEvents.UpdateTypes.Dynamic;
            bool jointsUpdated = false, poseUpdated = false, trackingAcquired = false, trackingLost = false, trackingStateChanged = false;
            var jointsUpdateCallbackCount = 0;

            rightHandTrackingEvents.trackingAcquired.AddListener(() => trackingAcquired = true);
            rightHandTrackingEvents.trackingLost.AddListener(() => trackingLost = true);
            rightHandTrackingEvents.trackingChanged.AddListener(_ => trackingStateChanged = true);
            rightHandTrackingEvents.jointsUpdated.AddListener(_ =>
            {
                jointsUpdateCallbackCount++;
                jointsUpdated = true;
            });

            rightHandTrackingEvents.poseUpdated.AddListener(_ =>
            {
                poseUpdated = true;
            });

            rightHandTrackingEvents.SetSubsystem(subsystem);
            yield return null;

            // First acquire and first update
            Assert.AreEqual(1, jointsUpdateCallbackCount);
            Assert.IsTrue(rightHandTrackingEvents.handIsTracked);
            Assert.IsTrue(trackingAcquired);
            Assert.IsFalse(trackingLost);
            Assert.IsTrue(trackingStateChanged);
            Assert.IsTrue(jointsUpdated);
            Assert.IsTrue(poseUpdated);

            trackingStateChanged = false;
            yield return null;

            // Second update, no change to tracking state
            Assert.AreEqual(2, jointsUpdateCallbackCount);
            Assert.IsTrue(rightHandTrackingEvents.handIsTracked);
            Assert.IsFalse(trackingStateChanged);

            jointsUpdated = poseUpdated = trackingAcquired = trackingLost = trackingStateChanged = false;

            provider.rightHandIsTracked = false;

            yield return null;

            // Tracking changed (lost) no joints updated
            Assert.AreEqual(2, jointsUpdateCallbackCount);
            Assert.IsFalse(rightHandTrackingEvents.handIsTracked);
            Assert.IsTrue(trackingLost);
            Assert.IsTrue(trackingStateChanged);
            Assert.IsFalse(trackingAcquired);
            Assert.IsFalse(jointsUpdated);
            Assert.IsFalse(poseUpdated);

            jointsUpdated = poseUpdated = trackingAcquired = trackingLost = trackingStateChanged = false;
            provider.rightHandIsTracked = true;
            yield return null;

            // Tracking changed (acquired) and updated again
            Assert.AreEqual(3, jointsUpdateCallbackCount);
            Assert.IsTrue(rightHandTrackingEvents.handIsTracked);
            Assert.IsTrue(trackingAcquired);
            Assert.IsFalse(trackingLost);
            Assert.IsTrue(trackingStateChanged);
            Assert.IsTrue(jointsUpdated);
            Assert.IsTrue(poseUpdated);

            // Stopping tracking on the left hand does not affect the right hand
            jointsUpdated = poseUpdated = trackingAcquired = trackingLost = trackingStateChanged = false;
            provider.leftHandIsTracked = false;
            yield return null;

            Assert.AreEqual(4, jointsUpdateCallbackCount);
            Assert.IsTrue(rightHandTrackingEvents.handIsTracked);
            Assert.IsFalse(trackingAcquired);
            Assert.IsFalse(trackingLost);
            Assert.IsFalse(trackingStateChanged);
            Assert.IsTrue(jointsUpdated);
            Assert.IsTrue(poseUpdated);
        }
        finally
        {
            Object.DestroyImmediate(go);
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();
        }
    }

    [UnityTest]
    public IEnumerator HandTrackingPoseStatusUpdates()
    {
        var subsystem = TestHandUtils.CreateTestSubsystem();
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);
        var provider = (TestHandProvider)subsystem.GetProvider();

        int[] fingerTipIndices =
        {
            XRHandJointID.ThumbTip.ToIndex(),
            XRHandJointID.IndexTip.ToIndex(),
            XRHandJointID.MiddleTip.ToIndex(),
            XRHandJointID.RingTip.ToIndex(),
            XRHandJointID.LittleTip.ToIndex()
        };

        XRHandJointTrackingState[] expectedLeftHandJointsTrackingStates = new XRHandJointTrackingState[XRHandJointID.EndMarker.ToIndex()];
        XRHandJointTrackingState[] expectedRightHandJointsTrackingStates = new XRHandJointTrackingState[XRHandJointID.EndMarker.ToIndex()];

        System.Array.Fill(expectedLeftHandJointsTrackingStates, XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose);
        System.Array.Fill(expectedRightHandJointsTrackingStates, XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose);

        bool leftHandJointsUpdatedThisFrame = false;
        XRHandJointTrackingState[] actualLeftHandJointsTrackingStates = new XRHandJointTrackingState[XRHandJointID.EndMarker.ToIndex()];

        bool rightHandJointsUpdatedThisFrame = false;
        XRHandJointTrackingState[] actualRightHandJointsTrackingStates = new XRHandJointTrackingState[XRHandJointID.EndMarker.ToIndex()];

        void OnUpdatedHands(
            XRHandSubsystem xrHandSubsystem,
            XRHandSubsystem.UpdateSuccessFlags successFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (updateType != XRHandSubsystem.UpdateType.Dynamic)
                return;

            leftHandJointsUpdatedThisFrame = successFlags.HasFlag(XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose)
                && successFlags.HasFlag(XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints);
            rightHandJointsUpdatedThisFrame = successFlags.HasFlag(XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose)
                && successFlags.HasFlag(XRHandSubsystem.UpdateSuccessFlags.RightHandJoints);

            for (int jointIndex = XRHandJointID.BeginMarker.ToIndex();
                 jointIndex < XRHandJointID.EndMarker.ToIndex();
                 ++jointIndex)
            {
                if (leftHandJointsUpdatedThisFrame)
                {
                    actualLeftHandJointsTrackingStates[jointIndex] =
                        xrHandSubsystem.leftHand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)).trackingState;
                }

                if (rightHandJointsUpdatedThisFrame)
                {
                    actualRightHandJointsTrackingStates[jointIndex] =
                        xrHandSubsystem.rightHand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)).trackingState;
                }
            }
        }

        subsystem.updatedHands += OnUpdatedHands;

        try
        {
            subsystem.Start();
            updater.Start();
            yield return null;

            Assert.IsTrue(leftHandJointsUpdatedThisFrame);
            Assert.IsTrue(rightHandJointsUpdatedThisFrame);
            Assert.IsTrue(expectedLeftHandJointsTrackingStates.SequenceEqual(actualLeftHandJointsTrackingStates));
            Assert.IsTrue(expectedRightHandJointsTrackingStates.SequenceEqual(actualRightHandJointsTrackingStates));


            // Mark some of the joints as having low-fidelity poses, as if they are occluded. Only mark the left hand.
            foreach (var fingerTipIndex in fingerTipIndices)
            {
                expectedLeftHandJointsTrackingStates[fingerTipIndex] = XRHandJointTrackingState.Pose;
            }
            expectedLeftHandJointsTrackingStates.CopyTo(provider.leftHandJointsTrackingStates, 0);

            yield return null;

            Assert.IsTrue(leftHandJointsUpdatedThisFrame);
            Assert.IsTrue(rightHandJointsUpdatedThisFrame);
            Assert.IsTrue(expectedLeftHandJointsTrackingStates.SequenceEqual(actualLeftHandJointsTrackingStates));
            Assert.IsTrue(expectedRightHandJointsTrackingStates.SequenceEqual(actualRightHandJointsTrackingStates));

            // Now mark the finger tips of the right hand as occluded.
            foreach (var fingerTipIndex in fingerTipIndices)
            {
                expectedRightHandJointsTrackingStates[fingerTipIndex] = XRHandJointTrackingState.Pose;
            }
            expectedRightHandJointsTrackingStates.CopyTo(provider.rightHandJointsTrackingStates, 0);

            yield return null;

            Assert.IsTrue(leftHandJointsUpdatedThisFrame);
            Assert.IsTrue(rightHandJointsUpdatedThisFrame);
            Assert.IsTrue(expectedLeftHandJointsTrackingStates.SequenceEqual(actualLeftHandJointsTrackingStates));
            Assert.IsTrue(expectedRightHandJointsTrackingStates.SequenceEqual(actualRightHandJointsTrackingStates));

            // Unocclude the left hand.
            foreach (var fingerTipIndex in fingerTipIndices)
            {
                expectedLeftHandJointsTrackingStates[fingerTipIndex] = XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose;
            }
            expectedLeftHandJointsTrackingStates.CopyTo(provider.leftHandJointsTrackingStates, 0);

            yield return null;

            Assert.IsTrue(leftHandJointsUpdatedThisFrame);
            Assert.IsTrue(rightHandJointsUpdatedThisFrame);
            Assert.IsTrue(expectedLeftHandJointsTrackingStates.SequenceEqual(actualLeftHandJointsTrackingStates));
            Assert.IsTrue(expectedRightHandJointsTrackingStates.SequenceEqual(actualRightHandJointsTrackingStates));

            // ...and now the right hand.
            foreach (var fingerTipIndex in fingerTipIndices)
            {
                expectedRightHandJointsTrackingStates[fingerTipIndex] = XRHandJointTrackingState.Pose | XRHandJointTrackingState.HighFidelityPose;
            }
            expectedRightHandJointsTrackingStates.CopyTo(provider.rightHandJointsTrackingStates, 0);

            yield return null;

            Assert.IsTrue(leftHandJointsUpdatedThisFrame);
            Assert.IsTrue(rightHandJointsUpdatedThisFrame);
            Assert.IsTrue(expectedLeftHandJointsTrackingStates.SequenceEqual(actualLeftHandJointsTrackingStates));
            Assert.IsTrue(expectedRightHandJointsTrackingStates.SequenceEqual(actualRightHandJointsTrackingStates));
        }
        finally
        {
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();
        }
    }

    [UnityTest]
    public IEnumerator CommonGesturesProviderNoSupport()
    {
        var expectedDescriptorCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "Test-Hands-NoGestureSupport",
            providerType = typeof(TestHandProvider),
            supportsAimPose = false,
            supportsAimActivateValue = false,
            supportsGripPose = false,
            supportsGraspValue = false,
            supportsPinchPose = false,
            supportsPinchValue = false,
            supportsPokePose = false
        };

        void CheckUnsupportedCommonGestureGetters(XRHandSubsystem subsystem, Handedness handedness)
        {
            var commonGestures =
                handedness == Handedness.Left ? subsystem.leftHandCommonGestures : subsystem.rightHandCommonGestures;

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimPose), Is.EqualTo((false, Pose.identity)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPokePose), Is.EqualTo((false, Pose.identity)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchPose), Is.EqualTo((false, Pose.identity)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetGripPose), Is.EqualTo((false, Pose.identity)));

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetGraspValue), Is.EqualTo((false, 0.0f)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimActivateValue), Is.EqualTo((false, 0.0f)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchValue), Is.EqualTo((false, 0.0f)));
        }

        void CheckUnsupportedCommonGestures(XRHandSubsystem subsystem, bool checkGestures)
        {
            TestHandUtils.AssertSubsystemGestureSupport(expectedDescriptorCinfo, subsystem.subsystemDescriptor);
            if (checkGestures)
            {
                CheckUnsupportedCommonGestureGetters(subsystem, Handedness.Left);
                CheckUnsupportedCommonGestureGetters(subsystem, Handedness.Right);
            }
        }

        void OnUpdatedHands(
            XRHandSubsystem subsystem,
            XRHandSubsystem.UpdateSuccessFlags successFlags,
            XRHandSubsystem.UpdateType updateType)
        {
            if (updateType != XRHandSubsystem.UpdateType.Dynamic)
                return;
        }

        var subsystem = TestHandUtils.CreateTestSubsystem(expectedDescriptorCinfo);
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);

        subsystem.updatedHands += OnUpdatedHands;

        CheckUnsupportedCommonGestures(subsystem, false);

        try
        {
            subsystem.Start();
            updater.Start();
            yield return null;

            CheckUnsupportedCommonGestures(subsystem, true);
        }
        finally
        {
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();
        }
    }

    [UnityTest]
    public IEnumerator CommonGesturesFullySupported()
    {
        var expectedHandsDescriptorCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "Test-Hands-FullGestureSupport",
            providerType = typeof(TestHandProvider),
            supportsAimPose = true,
            supportsAimActivateValue = true,
            supportsGripPose = true,
            supportsGraspValue = true,
            supportsPinchPose = true,
            supportsPinchValue = true,
            supportsPokePose = true
        };

        void CheckCommonGestureGetters(XRHandSubsystem subsystem, Handedness handedness)
        {
            var commonGestures =
                handedness == Handedness.Left ? subsystem.leftHandCommonGestures : subsystem.rightHandCommonGestures;

            var expectedGestures = TestCommonGestureData.GetCommonGestureData(handedness);

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimPose), Is.EqualTo((true, expectedGestures.aimPose)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPokePose), Is.EqualTo((true, expectedGestures.pokePose)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchPose), Is.EqualTo((true, expectedGestures.pinchPose)));

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetGraspValue), Is.EqualTo((true, expectedGestures.graspValue)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimActivateValue), Is.EqualTo((true, expectedGestures.aimActivateValue)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchValue), Is.EqualTo((true, expectedGestures.pinchValue)));
        }

        void CheckSupportedCommonGestures(XRHandSubsystem subsystem, bool checkGestures)
        {
            TestHandUtils.AssertSubsystemGestureSupport(expectedHandsDescriptorCinfo, subsystem.subsystemDescriptor);

            if (checkGestures)
            {
                CheckCommonGestureGetters(subsystem, Handedness.Left);
                CheckCommonGestureGetters(subsystem, Handedness.Right);
            }
        }

        var subsystem = TestHandUtils.CreateTestSubsystem(expectedHandsDescriptorCinfo);
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);
        var provider = (TestHandProvider)subsystem.GetProvider();

        provider.commonGestureBehavior = TestHandProvider.CommonGestureBehavior.Extended;

        MockCommonGestureListener mockGestureListener = new MockCommonGestureListener();
        mockGestureListener.SubscribeToAllGestureUpdates(subsystem);

        mockGestureListener.leftHandMocks.aimPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertAimPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.aimPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertAimPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.aimActivateValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertAimActivateUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.aimActivateValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertAimActivateUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.gripPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertGripPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.gripPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertGripPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.graspValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertGraspValueUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.graspValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertGraspValueUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.pinchPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertPinchPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.pinchPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertPinchPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.pinchValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertPinchValueUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.pinchValueUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertPinchValueUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.pokePoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertPokePoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.pokePoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertPokePoseUpdated(Handedness.Right, args));

        try
        {
            subsystem.Start();
            updater.Start();
            yield return null;

            CheckSupportedCommonGestures(subsystem, true);
        }
        finally
        {
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();

            mockGestureListener.AssertAllConditions();
        }
    }

    [UnityTest]
    public IEnumerator CommonGesturesPosesOnlySupported()
    {
        var expectedHandsSubsystemCinfo = new XRHandSubsystemDescriptor.Cinfo
        {
            id = "Test-Hands-PoseOnlySupport",
            providerType = typeof(TestHandProvider),
            supportsAimPose = true,
            supportsAimActivateValue = false,
            supportsGripPose = true,
            supportsGraspValue = false,
            supportsPinchPose = true,
            supportsPinchValue = false,
            supportsPokePose = true
        };

        void CheckGestureGetters(XRHandSubsystem subsystem, Handedness handedness)
        {
            var commonGestures =
                handedness == Handedness.Left ? subsystem.leftHandCommonGestures : subsystem.rightHandCommonGestures;

            var expectedGestures = TestCommonGestureData.GetCommonGestureData(handedness);

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimPose), Is.EqualTo((true, expectedGestures.aimPose)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPokePose), Is.EqualTo((true, expectedGestures.pokePose)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchPose), Is.EqualTo((true, expectedGestures.pinchPose)));

            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetGraspValue), Is.EqualTo((false, 0.0f)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetAimActivateValue), Is.EqualTo((false, 0.0f)));
            Assert.That(TestHandUtils.InvokeToTuple(commonGestures.TryGetPinchValue), Is.EqualTo((false, 0.0f)));
        }

        void CheckCommonGestures(XRHandSubsystem subsystem, bool checkGestures)
        {
            TestHandUtils.AssertSubsystemGestureSupport(expectedHandsSubsystemCinfo, subsystem.subsystemDescriptor);

            if (checkGestures)
            {
                CheckGestureGetters(subsystem, Handedness.Left);
                CheckGestureGetters(subsystem, Handedness.Right);
            }
        }

        var subsystem = TestHandUtils.CreateTestSubsystem(expectedHandsSubsystemCinfo);
        var updater = new XRHandProviderUtility.SubsystemUpdater(subsystem);
        var provider = (TestHandProvider)subsystem.GetProvider();

        provider.commonGestureBehavior = TestHandProvider.CommonGestureBehavior.CorePosesOnly;

        MockCommonGestureListener mockGestureListener = new MockCommonGestureListener();
        mockGestureListener.SubscribeToAllGestureUpdates(subsystem);

        mockGestureListener.leftHandMocks.aimPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertAimPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.aimPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertAimPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.gripPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertGripPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.gripPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertGripPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.pinchPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertPinchPoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.pinchPoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertPinchPoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.pokePoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Left)
            .Calls(args => TestHandUtils.AssertPokePoseUpdated(Handedness.Left, args));

        mockGestureListener.rightHandMocks.pokePoseUpdated
            .WillBeCalled(1)
            .WithArgumentValidator(args => args.handedness == Handedness.Right)
            .Calls(args => TestHandUtils.AssertPokePoseUpdated(Handedness.Right, args));

        mockGestureListener.leftHandMocks.aimActivateValueUpdated.WillNotBeCalled();
        mockGestureListener.rightHandMocks.aimActivateValueUpdated.WillNotBeCalled();

        mockGestureListener.leftHandMocks.graspValueUpdated.WillNotBeCalled();
        mockGestureListener.rightHandMocks.graspValueUpdated.WillNotBeCalled();

        mockGestureListener.leftHandMocks.pinchValueUpdated.WillNotBeCalled();
        mockGestureListener.rightHandMocks.pinchValueUpdated.WillNotBeCalled();

        try
        {
            subsystem.Start();
            updater.Start();
            yield return null;

            CheckCommonGestures(subsystem, true);
        }
        finally
        {
            updater.Stop();
            updater.Destroy();
            subsystem.Destroy();

            mockGestureListener.AssertAllConditions();
        }
    }
}
