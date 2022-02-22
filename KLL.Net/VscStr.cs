using System;
using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    * VSC_LPWSTR64 - associate a Virtual Scancode with a Text string
    *
    * Uses:
    *   GetKeyNameText(), aKeyNames[]  Map virtual scancode to name of key
    *
    \***************************************************************************/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct VscStr
    {
        public byte Vsc;
        public IntPtr pwsz;
    }
}
