using System.Runtime.InteropServices;

namespace KLLNet
{
    /*
     * Table element types (for various numbers of shift states), used
     * to facilitate static initializations of tables.
     * VK_TO_WCHARS641 and PVK_TO_WCHARS641 may be used as the generic type
     */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe struct VKtoWChar
    {
        public byte VirtualKey;
        public byte Attributes;
        //char is 16bit type, matching wchar
        public fixed char wch[1];
    }
}
