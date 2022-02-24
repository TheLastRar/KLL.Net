using System;
using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 16)]
    struct VscStrWoW64
    {
        [FieldOffset(0)]
        public byte Vsc;
        [FieldOffset(8)]
        public IntPtr pwsz;
    }
}
