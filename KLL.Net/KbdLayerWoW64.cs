using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KLLNet
{
    [StructLayout(LayoutKind.Explicit)]
    unsafe struct KbdLayerWoW64
    {
        [FieldOffset(0)]
        public ModifiersWoW64* pCharModifiers;
        [FieldOffset(8)]
        public VKtoWCharTableWoW64* pVkToWcharTable;
        [FieldOffset(16)]
        public DeadKey* pDeadKey;
        [FieldOffset(24)]
        public VscStrWoW64* pKeyNames;
        [FieldOffset(32)]
        public VscStrWoW64* pKeyNamesExt;

        //array of ptrs, also needs conversion
        //TODO
        [FieldOffset(40)]
        IntPtr* pKeyNamesDead;

        [FieldOffset(48)]
        public ushort* pVSCtoVK;
        [FieldOffset(56)]
        public byte bMaxVSCtoVK;
        [FieldOffset(64)]
        public VscVk* pVSCtoVK_E0;
        [FieldOffset(72)]
        public VscVk* pVSCtoVK_E1;
        [FieldOffset(80)]
        public uint fLocaleFlags;
        [FieldOffset(84)]
        public byte nLgMax;
        [FieldOffset(85)]
        public byte cbLgEntry;
        [FieldOffset(88)]
        public Ligature* pLigature;
        [FieldOffset(96)]
        public uint dwType;
        [FieldOffset(100)]
        public uint dwSubType;

        public PinnedData ConvertToStdStruct(KbdLayer* dest)
        {
            PinnedData pin = new PinnedData();

            #region Modifiers
            //ModNumber is byte array
            //wMaxModBits is the highest valid index
            int ModifersSize = sizeof(IntPtr) + sizeof(ushort) + pCharModifiers->wMaxModBits + 1;

            pin.pModifers = Marshal.AllocHGlobal(ModifersSize);
            dest->pCharModifiers = (Modifiers*)pin.pModifers.ToPointer();

            dest->pCharModifiers->pVkToBitmask = pCharModifiers->pVkToBitmask;
            dest->pCharModifiers->wMaxModBits = pCharModifiers->wMaxModBits;

            Buffer.MemoryCopy(pCharModifiers->ModNumber, dest->pCharModifiers->ModNumber, pCharModifiers->wMaxModBits + 1, pCharModifiers->wMaxModBits + 1);
            #endregion

            #region VkToWcharTable
            List<VKtoWCharTable> vKtoWCharTables = new List<VKtoWCharTable>();
            VKtoWCharTableWoW64* pVkToWchTbl = pVkToWcharTable;

            while (pVkToWchTbl->pVkToWchars != null)
            {
                VKtoWCharTable newTbl = new VKtoWCharTable
                {
                    pVkToWchars = pVkToWchTbl->pVkToWchars,
                    nModifications = pVkToWchTbl->nModifications,
                    cbSize = pVkToWchTbl->cbSize
                };
                vKtoWCharTables.Add(newTbl);
                ++pVkToWchTbl;
            }
            vKtoWCharTables.Add(new VKtoWCharTable());

            VKtoWCharTable[] vKtoWCharTablesArray = vKtoWCharTables.ToArray();
            pin.gcVktoWChar = GCHandle.Alloc(vKtoWCharTablesArray, GCHandleType.Pinned);
            dest->pVkToWcharTable = (VKtoWCharTable*)pin.gcVktoWChar.AddrOfPinnedObject().ToPointer();
            #endregion

            dest->pDeadKey = pDeadKey;

            #region pKeyNames
            List<VscStr> keyNames = new List<VscStr>();

            VscStrWoW64* kNames = pKeyNames;
            while (kNames->Vsc != 0)
            {
                VscStr newVStr = new VscStr
                {
                    Vsc = kNames->Vsc,
                    pwsz = kNames->pwsz
                };
                keyNames.Add(newVStr);

                kNames++;
            }

            VscStr[] keyNamesArray = keyNames.ToArray();
            pin.gcKeyName = GCHandle.Alloc(keyNamesArray, GCHandleType.Pinned);
            dest->pKeyNames = (VscStr*)pin.gcKeyName.AddrOfPinnedObject().ToPointer();
            #endregion

            #region pKeyNamesExt
            keyNames.Clear();

            VscStrWoW64* kNamesExt = pKeyNamesExt;
            while (kNamesExt->Vsc != 0)
            {
                VscStr newVStr = new VscStr
                {
                    Vsc = kNamesExt->Vsc,
                    pwsz = kNamesExt->pwsz
                };
                keyNames.Add(newVStr);

                kNamesExt++;
            }

            VscStr[] keyNamesExtArray = keyNames.ToArray();
            pin.gcKeyNameExt = GCHandle.Alloc(keyNamesExtArray, GCHandleType.Pinned);
            dest->pKeyNamesExt = (VscStr*)pin.gcKeyNameExt.AddrOfPinnedObject().ToPointer();
            #endregion

            dest->pKeyNamesDead = null; //TODO
            dest->pVSCtoVK = pVSCtoVK;
            dest->bMaxVSCtoVK = bMaxVSCtoVK;
            dest->pVSCtoVK_E0 = pVSCtoVK_E0;
            dest->pVSCtoVK_E1 = pVSCtoVK_E1;
            dest->fLocaleFlags = fLocaleFlags;
            dest->nLgMax = nLgMax;
            dest->cbLgEntry = cbLgEntry;
            dest->pLigature = pLigature;
            dest->dwType = dwType;
            dest->dwSubType = dwSubType;

            return pin;
        }
    }

    class PinnedData : IDisposable
    {
        public IntPtr pModifers = IntPtr.Zero;
        public GCHandle gcVktoWChar;
        public GCHandle gcKeyName;
        public GCHandle gcKeyNameExt;

        protected virtual void Dispose(bool disposing)
        {

            if (gcVktoWChar.IsAllocated)
                gcVktoWChar.Free();

            if (gcKeyName.IsAllocated)
                gcKeyName.Free();

            if (gcKeyNameExt.IsAllocated)
                gcKeyNameExt.Free();

            if (pModifers != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pModifers);
                pModifers = IntPtr.Zero;
            }
        }

        ~PinnedData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
