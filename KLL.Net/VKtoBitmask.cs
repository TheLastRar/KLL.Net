using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    * VK_TO_BIT64 - associate a Virtual Key with a Modifier bitmask.
    *
    * Vk        - the Virtual key (eg: VK_SHIFT, VK_RMENU, VK_CONTROL etc.)
    *             Special Values:
    *                0        null terminator
    * ModBits   - a combination of KBDALT, KBDCTRL, KBDSHIFT and kbd-specific bits
    *             Any kbd-specific shift bits must be the lowest-order bits other
    *             than KBDSHIFT, KBDCTRL and KBDALT (0, 1 & 2)
    *
    * Those languages that use AltGr (VK_RMENU) to shift keys convert it to
    * CTRL+ALT with the KBDSPECIAL bit in the ausVK[] entry for VK_RMENU
    * and by having an entry in aVkToPfnOem[] to simulate the right Vk sequence.
    *
    \***************************************************************************/
    [StructLayout(LayoutKind.Sequential)]
    internal struct VKtoBitmask
    {
        public byte Vk;
        public byte ModBits;
    }
}
