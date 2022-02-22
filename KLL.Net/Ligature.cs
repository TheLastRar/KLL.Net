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
    unsafe struct Ligature
    {
        public byte VirtualKey;
        public ushort ModificationNumber;
        public fixed char wch[1];
    }
}
