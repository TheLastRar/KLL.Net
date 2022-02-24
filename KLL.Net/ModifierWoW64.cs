using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct ModifiersWoW64
    {
        //64bit Pointers, regardless of process bitness
        [FieldOffset(0)]
        public VKtoBitmask* pVkToBitmask;
        [FieldOffset(8)]
        public ushort wMaxModBits;
        [FieldOffset(10)]
        public fixed byte ModNumber[1]; //Fixed array
    }
}
