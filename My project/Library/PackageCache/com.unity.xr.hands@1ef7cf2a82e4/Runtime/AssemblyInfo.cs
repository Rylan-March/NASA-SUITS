using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Unity.XR.Hands.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Analytics.Editor")]
[assembly: InternalsVisibleTo("Unity.XR.Hands.Analytics.Hooks.Editor")]

// Enable C# 9.0 init-only properties
namespace System.Runtime.CompilerServices
{
    static class IsExternalInit { }
}
