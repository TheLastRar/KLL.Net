using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLLNet
{
    /// <summary>
    /// An enum of Keys that can be used as modifers
    /// </summary>
    [Flags]
    public enum ModiferKeys
    {
        None = 0,
        Shift = 0x1,
        Control = 0x2,
        Alt = 0x4,
        Kanna = 0x8,
        Oem8 = 0x10
    }

    public static class ModiferKey
    {
        internal static ModiferKeys FromVirtualKey(ushort virtualKey)
        {
            switch (virtualKey)
            {
                case KeyboardLayout.VK_Shift:
                    return ModiferKeys.Shift;
                case KeyboardLayout.VK_Control:
                    return ModiferKeys.Control;
                case KeyboardLayout.VK_Alt:
                    return ModiferKeys.Alt;
                case KeyboardLayout.VK_Kanna:
                    return ModiferKeys.Kanna;
                case KeyboardLayout.VK_Oem8:
                    return ModiferKeys.Oem8;
                default:
                    return ModiferKeys.None;
            }
        }
    }
    public class KeyboardLayout
    {
        [Flags]
        internal enum LocalFlags
        {
            RightAltIsAltGr = 0x001,
            ShiftCancelsCapsLock = 0x002,
            ChangesDirectionality = 0x004
        }

        internal LocalFlags lFlags;

        //Keys that can be used as shifts
        public const ushort VK_Shift = 0x10;
        public const ushort VK_Control = 0x11;
        public const ushort VK_Alt = 0x12;
        public const ushort VK_Kanna = 0x15;
        public const ushort VK_Oem8 = 0xDF;

        //Capital
        public const ushort VK_Capital = 0x14;

        public bool RightAltIsAltGr { get => lFlags.HasFlag(LocalFlags.RightAltIsAltGr); }
        public bool ShiftCancelsCapsLock { get => lFlags.HasFlag(LocalFlags.ShiftCancelsCapsLock); }
        public bool ChangesDirectionality { get => lFlags.HasFlag(LocalFlags.ChangesDirectionality); }

        //List of modifiers
        internal List<ModifierEntry> modifierKeys;
        internal Dictionary<ModiferKeys, int> modifierKeySetToCharIndex;
        internal Dictionary<int, ModiferKeys> charIndexToModifierKeySet;

        internal Dictionary<ScanCode, PhysicalKey> keys;

        public IReadOnlyList<ModifierEntry> ModifierKeys { get => modifierKeys; }
        public IReadOnlyDictionary<ModiferKeys, int> ModifierKeySetToCharIndex { get => modifierKeySetToCharIndex; }
        public IReadOnlyDictionary<int, ModiferKeys> CharIndexToModifierKeySet { get => charIndexToModifierKeySet; }

        public IReadOnlyDictionary<ScanCode, PhysicalKey> Keys { get => keys; }
    }

    public struct ModifierEntry
    {
        public ushort virtualKey;
        public ModiferKeys modiferKey;
    }

    public struct ScanCode
    {
        public byte Code;
        public bool E0Set;
        public bool E1Set;

        public override bool Equals(object obj) => obj is ScanCode code && this == code;
        public override int GetHashCode() => (Code + (E0Set ? 0xE0 << 8 : 0) + (E1Set ? 0xE1 << 8 : 0)).GetHashCode();
        public static bool operator ==(ScanCode x, ScanCode y)
        {
            return
                x.Code == y.Code &&
                x.E0Set == y.E0Set &&
                x.E1Set == y.E1Set;
        }
        public static bool operator !=(ScanCode x, ScanCode y) => !(x == y);
    }

    public class PhysicalKey
    {
        KeyboardLayout parent;

        string name;

        /// <summary>
        /// Name for the key (e.g. Backspace), can be null
        /// </summary>
        public string Name
        {
            get => name;
            internal set { name = value; }
        }


        internal enum KeyFlags
        {
            CapslockShiftsBase = 0x001,
            CapslockSwitchesCharacters = 0x002,
            CapslockShfitsAltGr = 0x004
        }
        internal KeyFlags attribute;
        public bool CapslockShiftsBase { get => attribute.HasFlag(KeyFlags.CapslockShiftsBase); }
        public bool CapslockSwitchesCharacters { get => attribute.HasFlag(KeyFlags.CapslockSwitchesCharacters); }
        public bool CapslockShfitsAltGr { get => attribute.HasFlag(KeyFlags.CapslockShfitsAltGr); }

        internal ushort virtualKeyCode;

        internal List<KeyCharacter> characters = new List<KeyCharacter>();
        internal List<KeyCharacter> sgCharacters = new List<KeyCharacter>();

        /// <summary>
        /// A list of characters accesable when pressing the key
        /// Note that keys that switch character sets (SGCAPS) have those charecters stored in SecondaryCharacters
        /// </summary>
        public IReadOnlyList<KeyCharacter> Characters { get => characters; }

        /// <summary>
        /// A list of secondary characters accesable when pressing the key while capslock is active
        /// Only valid when CapslockSwitchesCharacters is true
        /// </summary>
        public IReadOnlyList<KeyCharacter> SecondaryCharacters { get => sgCharacters; }


        internal PhysicalKey(KeyboardLayout parentLayout)
        {
            parent = parentLayout;
        }

        /// <summary>
        /// Gets the Character outputted when the specified modifer keys are pressed
        /// </summary>
        /// <param name="capsLock">Wheter Capslock is active</param>
        /// <param name="modiferKeyStates">A flag with all pressed ModiferKeys ORed together</param>
        /// <returns>The resulting character</returns>
        public KeyCharacter GetCharacter(bool capsLock, ModiferKeys modiferKeyStates)
        {
            if (!parent.ModifierKeySetToCharIndex.ContainsKey(modiferKeyStates))
                return new KeyCharacter();

            IReadOnlyList<KeyCharacter> charSet = Characters;
            if (capsLock)
            {
                //Only Shift & base
                if (CapslockShiftsBase & (modiferKeyStates == ModiferKeys.Shift | modiferKeyStates == ModiferKeys.None))
                {
                    //XOR to flip bit
                    modiferKeyStates ^= ModiferKeys.Shift;
                }
                //Any combo using AltGr
                if (CapslockSwitchesCharacters & modiferKeyStates.HasFlag(ModiferKeys.Alt | ModiferKeys.Control))
                    //XOR to flip bit
                    modiferKeyStates ^= ModiferKeys.Shift;

                if (CapslockSwitchesCharacters)
                    charSet = SecondaryCharacters;
            }

            int index = parent.ModifierKeySetToCharIndex[modiferKeyStates];
            if (index >= Characters.Count)
                return new KeyCharacter();

            return Characters[index];
        }
    }

    public struct KeyCharacter
    {
        public bool IsDeadKey;
        public bool isLigature;
        public string Character;
    }
}
