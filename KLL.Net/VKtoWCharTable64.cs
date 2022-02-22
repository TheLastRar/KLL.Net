using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    *
    * VK_TO_WCHAR_TABLE64 - Describe a table of VK_TO_WCHARS641
    *
    * pVkToWchars     - points to the table.
    * nModifications  - the number of shift-states supported by this table.
    *                   (this is the number of elements in pVkToWchars[*].wch[])
    *
    * A keyboard may have several such tables: all keys with the same number of
    *    shift-states are grouped together in one table.
    *
    * Special values for pVktoWchars:
    *     NULL     - Terminates a VK_TO_WCHAR_TABLE64[] list.
    *
    \***************************************************************************/
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct VKtoWCharTable64
    {
        public VKtoWChar1* pVkToWchars;
        public byte nModifications;
        public byte cbSize;
    }
}
