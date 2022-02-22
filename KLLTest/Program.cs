using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KLLTest
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        static void Main(string[] args)
        {
            uint KeyboardLayout = (UInt32)GetKeyboardLayout(0).ToInt32();
            KeyboardLayout = (KeyboardLayout >> 16) & 0xFFFF;
            string id = KeyboardLayout.ToString("X8");

            RegistryKey layoutReg = Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Control").OpenSubKey("Keyboard Layouts").OpenSubKey(id);

            string dll = (string)layoutReg.GetValue("Layout File");
            string name = (string)layoutReg.GetValue("Layout Text");

            KLLNet.KLL kLL = new KLLNet.KLL();
            //kLL.LoadDLL(@"C:\Windows\System32\" + "KBDGR.DLL");
            //kLL.LoadDLL(@"C:\Windows\System32\" + "KBDSG.DLL");
            kLL.LoadDLL(@"C:\Windows\System32\" + "KBDINTAM.DLL");
            //kLL.LoadDLL(@"C:\Windows\System32\" + dll);

        }
    }
}
