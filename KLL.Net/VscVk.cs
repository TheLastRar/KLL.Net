using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    * VSC_VK     - Associate a Virtual Scancode with a Virtual Key
    *  Vsc - Virtual Scancode
    *  Vk  - Virtual Key | flags
    * Used by VKFromVSC() for scancodes prefixed 0xE0 or 0xE1
    \***************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct VscVk
    {
        public byte Vsc;
        public ushort Vk;
    }
}
