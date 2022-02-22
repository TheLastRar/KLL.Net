using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    * VSC_VK     - Associate a Virtual Scancode with a Virtual Key
    *  Vsc - Virtual Scancode
    *  Vk  - Virtual Key | flags
    * Used by VKFromVSC() for scancodes prefixed 0xE0 or 0xE1
    \***************************************************************************/
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    struct VscVk
    {
        [FieldOffset(0)]
        public byte Vsc;
        [FieldOffset(2)]
        public byte Vk;
        [FieldOffset(3)]
        public byte Flags;
    }
}
