using System.Runtime.InteropServices;

namespace KLLNet
{
    /*
     * Table element types (for various numbers of ligatures), used
     * to facilitate static initializations of tables.
     *
     * LIGATURE1 and PLIGATURE1 are used as the generic type
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct Ligature1
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[1];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct Ligature2
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[2];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct Ligature3
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[3];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct Ligature4
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[4];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct Ligature5
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[5];
    }
}
