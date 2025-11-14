using UnityEngine.XR.OpenXR.NativeTypes;
using XrSession = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe delegate XrResult XrLocateViews_Delegate(
        XrSession session,
        XrViewLocateInfo* viewLocateInfo,
        XrViewState* viewState,
        uint viewCapacityInput,
        uint* viewCountOutput,
        XrView* views);
}
