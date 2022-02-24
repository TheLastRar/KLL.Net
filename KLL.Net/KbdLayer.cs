using System;
using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct KbdLayer
    {
        /*
         * Modifier keys
         */
        public Modifiers* pCharModifiers;

        /*
         * Characters
         */
        public VKtoWCharTable* pVkToWcharTable; //ptr to tbl of ptrs to tbl

        /*
         * Diacritics
         */
        public DeadKey* pDeadKey;

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
        public Ligature* pLigature;

        /*
         * Type and subtype. These are optional.
         */
        public uint dwType; // Keyboard Type
        public uint dwSubType; // Keyboard SubType: may contain OemId
    }
}
