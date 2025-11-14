namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal struct XrBool32
    {
        public static implicit operator bool(XrBool32 boolean) => boolean.m_InternalValue != 0;
        public static implicit operator XrBool32(bool boolean) => new (boolean);

        public XrBool32(bool boolean) => m_InternalValue = boolean ? 1U : 0U;
        public XrBool32(XrBool32 boolean) => m_InternalValue = boolean.m_InternalValue;

        uint m_InternalValue;
    }
}
