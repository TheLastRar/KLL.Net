using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    unsafe struct VKtoWCharTableWoW64
    {
        [FieldOffset(0)]
        public VKtoWChar* pVkToWchars;
        [FieldOffset(8)]
        public byte nModifications;
        [FieldOffset(9)]
        public byte cbSize;
    }
}
