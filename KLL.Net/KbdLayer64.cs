using System;
using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct KbdLayer64
    {
        /*
         * Modifier keys
         */
        public Modifiers64* pCharModifiers64;

        /*
         * Characters
         */
        public VKtoWCharTable64* pVkToWcharTable; //ptr to tbl of ptrs to tbl

        /*
         * Diacritics
         */
        public DeadKey* pDEADKEY64;

        /*
         * Names of Keys
         */
        public VscStr* pKeyNames;
        public VscStr* pKeyNamesExt;
        public IntPtr* pKeyNamesDead;

        /*
         * Scan codes to Virtual Keys
         */
        public ushort* pVSCtoVK;
        public byte bMaxVSCtoVK;
        public VscVk* pVSCtoVK_E0; // Scancode has E0 prefix
        public VscVk* pVSCtoVK_E1; // Scancode has E1 prefix

        /*
         * Locale-specific special processing
         */
        public uint fLocaleFlags;

        /*
         * Ligatures
         */
        public byte nLgMax;
        public byte cbLgEntry;
        public Ligature1* pLigature;

        /*
         * Type and subtype. These are optional.
         */
        public uint dwType; // Keyboard Type
        public uint dwSubType; // Keyboard SubType: may contain OemId
    }
}
