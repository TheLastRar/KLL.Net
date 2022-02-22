using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    *
    * Dead Key (diaresis) tables
    *
    \***************************************************************************/
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    struct DeadKey
    {
        [FieldOffset(0)]
        public uint dsboth; // diacritic & char
        [FieldOffset(0)]
        public char wchCharacter;
        [FieldOffset(2)]
        public char wchDiacritic;
        [FieldOffset(4)]
        public char wchComposed;
        [FieldOffset(6)]
        public ushort uflags;
    }
}
