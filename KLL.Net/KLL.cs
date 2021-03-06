using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KLLNet
{
    public class KLL
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr hLibModule);

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        unsafe delegate void* KbdLayerDescriptorDelegate();

        public KeyboardLayout LoadLayout(string keyboardDll)
        {
            IntPtr hHandle = IntPtr.Zero;
            try
            {

                //Load the dll
                hHandle = LoadLibrary(keyboardDll);
                if (hHandle == IntPtr.Zero)
                {
                    Console.Error.WriteLine("Failed to load dll");
                    return null;
                }

                //Get the Keyboard import function
                IntPtr fpKbdLayerDescriptor = GetProcAddress(hHandle, "KbdLayerDescriptor");

                //Return if error
                if (fpKbdLayerDescriptor == IntPtr.Zero)
                {
                    Console.Error.WriteLine("Could not load kbdLayerDescriptor, is it a real keyboard layout file?");
                    FreeLibrary(hHandle);
                    return null;
                }

                KbdLayerDescriptorDelegate KbdLayerDescriptor = Marshal.GetDelegateForFunctionPointer<KbdLayerDescriptorDelegate>(fpKbdLayerDescriptor);

                KeyboardLayout layout = null;

                unsafe
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        void* pKbdTables = KbdLayerDescriptor();

                        if (pKbdTables == null)
                        {
                            FreeLibrary(hHandle);
                            return null;
                        }

                        if (!Environment.Is64BitProcess)
                        {
                            KbdLayer kbdLayerConverted;
                            using (PinnedData pin = ((KbdLayerWoW64*)pKbdTables)->ConvertToStdStruct(&kbdLayerConverted))
                                layout = Fill(&kbdLayerConverted);
                        }
                        else
                            layout = Fill((KbdLayer*)pKbdTables);
                    }
                    else
                    {
                        Console.Error.WriteLine("32bit OS not supported");
                        FreeLibrary(hHandle);
                        return null;
                    }
                }
                return layout;
            }
            finally
            {
                FreeLibrary(hHandle);
            }
        }

        unsafe static KeyboardLayout Fill(KbdLayer* KbdTables)
        {
            List<PhysicalKey> vkArrayNoScanCode = new List<PhysicalKey>();

            if (KbdTables == null)
                return null;

            KeyboardLayout layout = new KeyboardLayout { lFlags = (KeyboardLayout.LocalFlags)(KbdTables->fLocaleFlags & 0xFFFF) };

            #region Modifiers
            Modifiers* pCharModifiers = KbdTables->pCharModifiers;
            VKtoBitmask* pVkToBit = pCharModifiers->pVkToBitmask;

            //VK given is for Unsided Keys (i.e. not LShift but Shift)
            layout.modifierKeys = new List<ModifierEntry>();
            while (pVkToBit->Vk != 0)
            {
                ModifierEntry entry = new ModifierEntry
                {
                    virtualKey = pVkToBit->Vk,
                    modiferKey = ModiferKey.FromVirtualKey(pVkToBit->Vk)
                };
                layout.modifierKeys.Add(entry);
                ++pVkToBit;
            }

            //Modifier bits/combinations
            //the bitmask of each pressed modifer key are combined to produce an index into pCharModifiers->ModNumber
            //the value stored at that point in the ModNumber array is the index of the appriopiate key

            //A Dictionary indexed by the Char Index
            //A Dictionary indexed by modifier key presses
            layout.charIndexToModifierKeySet = new Dictionary<int, ModiferKeys>();
            layout.modifierKeySetToCharIndex = new Dictionary<ModiferKeys, int>();
            for (int modBitsIndex = 0; modBitsIndex <= pCharModifiers->wMaxModBits; modBitsIndex++)
            {
                //A value of 0x0F is SHIFT_INVALID, no character is produced
                if (pCharModifiers->ModNumber[modBitsIndex] != 0x0F)
                {
                    ModiferKeys keysForCurrentCombination = ModiferKeys.None;
                    //Loop though all modifier key bitmasks to find keys required for this character index
                    for (int keyIndex = 0; pCharModifiers->pVkToBitmask[keyIndex].Vk != 0; keyIndex++)
                    {
                        if ((modBitsIndex & pCharModifiers->pVkToBitmask[keyIndex].ModBits) != 0)
                            keysForCurrentCombination |= ModiferKey.FromVirtualKey(pCharModifiers->pVkToBitmask[keyIndex].Vk);
                    }
                    //Record
                    int modBits = pCharModifiers->ModNumber[modBitsIndex];
                    layout.charIndexToModifierKeySet.Add(pCharModifiers->ModNumber[modBitsIndex], keysForCurrentCombination);
                    layout.modifierKeySetToCharIndex.Add(keysForCurrentCombination, pCharModifiers->ModNumber[modBitsIndex]);
                }
            }
            #endregion

            //Fill all the SC into VKs array
            #region ScanCode
            layout.keys = new Dictionary<ScanCode, PhysicalKey>();
            //Some virtual keys are assigned to multiple Scancodes (TAB gets assigned to 0x7C)
            Dictionary<ushort, PhysicalKey> vkToKey = new Dictionary<ushort, PhysicalKey>();
            for (int i = 0; i < KbdTables->bMaxVSCtoVK; i++)
            {
                if (KbdTables->pVSCtoVK[i] == 0xFF)
                    continue;
                ScanCode sc = new ScanCode { Code = (byte)i };

                PhysicalKey pk = new PhysicalKey(layout);

                if (vkToKey.ContainsKey(KbdTables->pVSCtoVK[i]))
                {
                    ScanCode oldsc = layout.keys.FirstOrDefault(x => x.Value == vkToKey[KbdTables->pVSCtoVK[i]]).Key;
                    Console.WriteLine($"VK 0x{KbdTables->pVSCtoVK[i]:X} has multiple SC (All Non-Extended), new SC:0x{sc.Code:X}, old SC:0x{oldsc.Code:X}");
                }
                else
                    vkToKey.Add(KbdTables->pVSCtoVK[i], pk);

                pk.virtualKeyCode = KbdTables->pVSCtoVK[i];
                layout.keys.Add(sc, pk);
            }

            VscVk* e0ScanCodes = KbdTables->pVSCtoVK_E0;
            while (e0ScanCodes->Vsc > 0)
            {
                ScanCode sc = new ScanCode
                {
                    Code = e0ScanCodes->Vsc,
                    E0Set = true
                };

                PhysicalKey pk = new PhysicalKey(layout);

                if (vkToKey.ContainsKey(e0ScanCodes->Vk))
                {
                    ScanCode oldsc = layout.keys.FirstOrDefault(x => x.Value == vkToKey[e0ScanCodes->Vk]).Key;
                    Console.WriteLine($"VK 0x{e0ScanCodes->Vk:X} has multiple SC, new SC:0x{sc.Code:X}, E0:true, old SC:0x{oldsc.Code:X}, E0:{oldsc.E0Set}");
                }
                else
                    vkToKey.Add(e0ScanCodes->Vk, pk);

                pk.virtualKeyCode = e0ScanCodes->Vk;
                layout.keys.Add(sc, pk);

                e0ScanCodes++;
            }

            VscVk* e1ScanCodes = KbdTables->pVSCtoVK_E1;
            while (e1ScanCodes->Vsc > 0)
            {
                ScanCode sc = new ScanCode
                {
                    Code = e1ScanCodes->Vsc,
                    E1Set = true
                };

                PhysicalKey pk = new PhysicalKey(layout);

                if (vkToKey.ContainsKey(e1ScanCodes->Vk))
                {
                    ScanCode oldsc = layout.keys.FirstOrDefault(x => x.Value == vkToKey[e1ScanCodes->Vk]).Key;
                    Console.WriteLine($"VK 0x{e1ScanCodes->Vk:X} has multiple SC");
                }
                else
                    vkToKey.Add(e1ScanCodes->Vk, pk);

                pk.virtualKeyCode = e1ScanCodes->Vk;
                layout.keys.Add(sc, pk);

                e1ScanCodes++;
            }
            #endregion

            //Handle all the chars with modifieres
            VKtoWCharTable* pVkToWchTbl = KbdTables->pVkToWcharTable;

            while (pVkToWchTbl->pVkToWchars != null)
            {
                //pVkToWchars is an array of VKtoWChar1
                VKtoWChar* pVkToWch = pVkToWchTbl->pVkToWchars;
                while (pVkToWch->VirtualKey != 0)
                {
                    if (pVkToWch->VirtualKey != 0xFF)
                    {
                        //Handle VK assigned to multiple SC
                        PhysicalKey[] pkinfos = layout.keys.Values.Where(pk => pk.virtualKeyCode == pVkToWch->VirtualKey).ToArray();

                        //Keep track of VKs we don't hava a scan code for
                        if (pkinfos.Count() == 0)
                        {
                            pkinfos = vkArrayNoScanCode.Where(vk => vk.virtualKeyCode == pVkToWch->VirtualKey).ToArray();

                            if (pkinfos.Count() == 0)
                            {
                                PhysicalKey orphanKey = new PhysicalKey(layout)
                                {
                                    virtualKeyCode = pVkToWch->VirtualKey
                                };
                                vkArrayNoScanCode.Add(orphanKey);
                                pkinfos = new PhysicalKey[] { orphanKey };
                            }
                        }

                        List<KeyCharacter> charList = new List<KeyCharacter>();
                        for (int i = 0; i < pVkToWchTbl->nModifications; ++i)
                        {
                            KeyCharacter character = new KeyCharacter();
                            //WCH_NONE - No character is generated by pressing this key with the current shift state.
                            if (pVkToWch->wch[i] == 0xF000)
                                character.Character = null;
                            //WCH_DEAD - The character is a dead-key: the next VK_TO_WCHARS64[] entry will contain the values of the dead characters (diaresis) that can be produced by the Virtual Key.
                            else if (pVkToWch->wch[i] == 0xF001)
                            {
                                Console.WriteLine($"Found Dead Key {pVkToWch->VirtualKey:X}");

                                VKtoWChar* pVkToWchMext = (VKtoWChar*)(((byte*)pVkToWch) + pVkToWchTbl->cbSize);
                                character.Character = pVkToWchMext->wch[i].ToString();
                                character.IsDeadKey = true;
                            }
                            //WCH_LGTR - The character is a LIGATURE64. The characters generated by this keystroke are found in the LIGATURE64 table.
                            else if (pVkToWch->wch[i] == 0xF002)
                            {
                                character.Character = "";
                                character.isLigature = true;
                                Ligature* current = KbdTables->pLigature;
                                while (current != null)
                                {
                                    if (current->VirtualKey == pVkToWch->VirtualKey && layout.modifierKeySetToCharIndex[(ModiferKeys)current->ModificationNumber] == i)
                                    {
                                        char* ligChar = current->wch;
                                        for (int j = 0; j < KbdTables->nLgMax; j++)
                                        {
                                            if (current->wch[j] == 0xF000)
                                                break;

                                            character.Character += current->wch[j];
                                        }
                                        break;
                                    }
                                    current = (Ligature*)(((byte*)current) + KbdTables->cbLgEntry);
                                }
                            }
                            //Regular character
                            else
                                character.Character = pVkToWch->wch[i].ToString();

                            charList.Add(character);
                        }

                        foreach (PhysicalKey pkinfo in pkinfos)
                        {
                            //Check SGCaps
                            if (pkinfo.attribute == PhysicalKey.KeyFlags.CapslockSwitchesCharacters)
                            {
                                //SGCaps uses multiple pVkToWch to describe
                                //characters with & without caps lock
                                pkinfo.sgCharacters = charList;
                            }
                            else
                            {
                                pkinfo.attribute = (PhysicalKey.KeyFlags)pVkToWch->Attributes;
                                pkinfo.characters = charList;
                            }
                        }
                    }
                    pVkToWch = (VKtoWChar*)(((byte*)pVkToWch) + pVkToWchTbl->cbSize);
                }
                ++pVkToWchTbl;
            }

            //The keys PrtSc/SysRq and Pause/Break are special.
            //The former produces scancode E0 2A E0 37 when no modifier key is pressed simultaneously,
            //E0 37 together with Shift or Ctrl, but 54 together with(left or right) Alt. (And one gets the expected sequences upon release)
            //The latter produces scancode sequence E1 1D 45 E1 9D C5 when pressed(without modifier) and nothing at all upon release.
            //However, together with(left or right) Ctrl, one gets E0 46 E0 C6, and again nothing at release. It does not repeat. 

            //VK Text
            VscStr* keyNames = KbdTables->pKeyNames;
            while (keyNames->Vsc != 0)
            {
                ScanCode sc = new ScanCode { Code = keyNames->Vsc };

                //Pause appears in reqular key names, but needs to be assigned to the E1 key
                if (sc.Code == 0x45)
                {
                    sc.Code = 0x1D;
                    sc.E1Set = true;
                }

                //Printscreen's SysReq mode logged correctly for some layouts

                PhysicalKey pkinfo = null;
                if (layout.keys.ContainsKey(sc))
                {
                    pkinfo = layout.keys[sc];
                }

                if (pkinfo != null)
                    pkinfo.Name = Marshal.PtrToStringUni(keyNames->pwsz);

                keyNames++;
            }
            VscStr* keyNamesExt = KbdTables->pKeyNamesExt;
            while (keyNamesExt->Vsc != 0)
            {
                ScanCode sc = new ScanCode { Code = keyNamesExt->Vsc };

                //Pause and Break are the same button mapped to multiple Scan codes
                if (sc.Code == 0x46)
                {
                    ScanCode pause = new ScanCode
                    {
                        Code = 0x1D,
                        E1Set = true
                    };
                    PhysicalKey pauseKeyInfo = layout.keys[pause];

                    pauseKeyInfo.Name += "\r" + Marshal.PtrToStringUni(keyNamesExt->pwsz);

                    keyNamesExt++;
                    continue;
                }
                //Printscreen appears in extended key names, but needs to be assigned to the non-extended key
                else if (sc.Code == 0x37)
                {
                    ScanCode print = new ScanCode { Code = 0x54 };
                    PhysicalKey printKeyInfo = layout.keys[print];

                    printKeyInfo.Name = Marshal.PtrToStringUni(keyNamesExt->pwsz) + (printKeyInfo.Name != null ? "\r" + printKeyInfo.Name : "");

                    keyNamesExt++;
                    continue;
                }
                else
                    //Numlock appears in the extended names, but needs to be assigned to the non-extended key
                    //otherwise mapping is correct
                    sc.E0Set = sc.Code != 0x45;

                //Only one E1 Key, and we already handle the name
                //So no need to check for E1 keys
                PhysicalKey pkinfo = null;
                if (layout.keys.ContainsKey(sc))
                    pkinfo = layout.keys[sc];

                if (pkinfo != null)
                    pkinfo.Name = Marshal.PtrToStringUni(keyNamesExt->pwsz);
                else
                    Console.WriteLine("NameExt with no key:" + Marshal.PtrToStringUni(keyNamesExt->pwsz));

                keyNamesExt++;
            }

            ////Don't know where to store
            //if (KbdTables64->pKeyNamesDead != null)
            //{
            //    IntPtr* keyNamesDead = KbdTables64->pKeyNamesDead;
            //    while (*keyNamesDead != IntPtr.Zero)
            //    {
            //        //1st Char is the Dead Key Char
            //        Console.WriteLine(Marshal.PtrToStringUni(*keyNamesDead));
            //        keyNamesDead++;
            //    }

            //    //Don't know what to do with these
            //    DeadKey* pDeadKey = KbdTables64->pDEADKEY64;
            //    while (pDeadKey->dsboth != 0)
            //    {
            //        Console.WriteLine(pDeadKey->wchComposed);
            //        Console.WriteLine(pDeadKey->wchCharacter);
            //        Console.WriteLine(pDeadKey->wchDiacritic);
            //        ++pDeadKey;
            //    }
            //}

            return layout;
        }
    }
}
