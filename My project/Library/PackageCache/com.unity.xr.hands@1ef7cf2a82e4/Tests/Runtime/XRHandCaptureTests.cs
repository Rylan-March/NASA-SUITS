using NUnit.Framework;
using UnityEngine.XR.Hands.Capture;
using UnityEngine.XR.Hands.Capture.Recording;

class XRHandCaptureTests
{
    [Test]
    public void TestXRHandRecordingStatusChangedEventArgsEquality()
    {
        TestHandUtils.CreateTwoTestSubsystems(out var testSubsystem1, out var testSubsystem2);

        var recordingName1 = "TestRecording";
        var recordingName2 = "DifferentRecording";

        var errorMessage1 = "Test error message";
        var errorMessage2 = "Different error message";

        var elapsedTime1 = 1.5f;
        var elapsedTime2 = 2.0f;

        var args1 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };

        var args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };

        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.StoppedManually,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime2,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName2,
            subsystem = testSubsystem1,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = elapsedTime1,
            recordingName = recordingName1,
            subsystem = testSubsystem2,
            errorMessage = errorMessage1,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingStatusChangedEventArgs
        {
            status = XRHandRecordingStatus.Recording,
            elapsedTime = 1.5f,
            recordingName = recordingName1,
            subsystem = testSubsystem1,
            errorMessage = errorMessage2,
        };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }

    [Test]
    public void TestXRHandCapturedFrameEquality()
    {
        XRHandCaptureFrame frame1 = new XRHandCaptureFrame();
        XRHandCaptureFrame frame2 = new XRHandCaptureFrame();
        Assert.IsTrue(frame1.Equals(frame2));
        Assert.IsTrue(frame1 == frame2);
        Assert.IsFalse(frame1 != frame2);

        frame1.timestamp = 1.0f;
        frame2.timestamp = 2.0f;
        Assert.IsFalse(frame1.Equals(frame2));
        Assert.IsFalse(frame1 == frame2);
        Assert.IsTrue(frame1 != frame2);
    }

    [Test]
    public void TestXRHandRecordingInitializeArgsEquality()
    {
        XRHandRecordingInitializeArgs args1 = new XRHandRecordingInitializeArgs();
        XRHandRecordingInitializeArgs args2 = new XRHandRecordingInitializeArgs();
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        TestHandUtils.CreateTwoTestSubsystems(out var testSubsystem1, out var testSubsystem2);
        args1 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem1 };
        args2 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem1 };
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingInitializeArgs { subsystem = testSubsystem2 };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingInitializeArgs { subsystem = null };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }

    [Test]
    public void TestXRHandRecordingSaveArgsEquality()
    {
        XRHandRecordingSaveArgs args1 = new XRHandRecordingSaveArgs();
        XRHandRecordingSaveArgs args2 = new XRHandRecordingSaveArgs();
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        var recordingName1 = "TestRecording";
        var recordingName2 = "DifferentRecording";

        args1 = new XRHandRecordingSaveArgs { recordingName = recordingName1 };
        args2 = new XRHandRecordingSaveArgs { recordingName = recordingName1 };
        Assert.IsTrue(args1.Equals(args2));
        Assert.IsTrue(args1 == args2);
        Assert.IsFalse(args1 != args2);

        args2 = new XRHandRecordingSaveArgs { recordingName = recordingName2 };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);

        args2 = new XRHandRecordingSaveArgs { recordingName = null };
        Assert.IsFalse(args1.Equals(args2));
        Assert.IsFalse(args1 == args2);
        Assert.IsTrue(args1 != args2);
    }
}
