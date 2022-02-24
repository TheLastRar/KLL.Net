using System.Runtime.InteropServices;

namespace KLLNet
{
    /***************************************************************************\
    * pModNumber  - a table to map shift bits to enumerated shift states
    *
    * Table attributes: Ordered table
    *
    * Maps all possible shifter key combinations to an enumerated shift state.
    * The size of the table depends on the value of the highest order bit used
    * in aCharMODIFIERS64[*].ModBits
    *
    * Special values for aModification[*]
    *   SHFT_INVALID - no characters produced with this shift state.
    LATER: (ianja) no SHFT_CTRL - control characters encoded in tables like others
    *   SHFT_CTRL    - standard control character production (all keyboards must
    *                  be able to produce CTRL-C == 0x0003 etc.)
    *   Other        - enumerated shift state (not less than 0)
    *
    * This table is indexed by the Modifier Bits to obtain an Modification Number.
    *
    *                        CONTROL MENU SHIFT
    *
    *    aModification[] = {
    *        0,            //   0     0     0     = 000  <none>
    *        1,            //   0     0     1     = 001  SHIFT
    *        SHFT_INVALID, //   0     1     0     = 010  ALT
    *        2,            //   0     1     1     = 011  SHIFT ALT
    *        3,            //   1     0     0     = 100  CTRL
    *        4,            //   1     0     1     = 101  SHIFT CTRL
    *        5,            //   1     1     0     = 110  CTRL ALT
    *        SHFT_INVALID  //   1     1     1     = 111  SHIFT CTRL ALT
    *    };
    *
    \***************************************************************************/
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct Modifiers
    {
        //64bit Pointers, regardless of process bitness
        public VKtoBitmask* pVkToBitmask;
        public ushort wMaxModBits;
        public fixed byte ModNumber[1]; //Fixed array
    }
}
