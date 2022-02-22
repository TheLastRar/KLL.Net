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
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);

        static void Main(string[] args)
        {
            StringBuilder locale = new StringBuilder(new string(' ', 256));
            GetKeyboardLayoutName(locale);
            string id = locale.ToString();

            RegistryKey layoutReg = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{id}");


            string dll = (string)layoutReg.GetValue("Layout File");
            string name = (string)layoutReg.GetValue("Layout Text");

            KLLNet.KLL kLL = new KLLNet.KLL();
            //kLL.LoadLayout(@"C:\Windows\System32\" + "KBDGR.DLL");
            //kLL.LoadLayout(@"C:\Windows\System32\" + "KBDSG.DLL");
            //kLL.LoadLayout(@"C:\Windows\System32\" + "KBDINTAM.DLL");
            kLL.LoadLayout(@"C:\Windows\System32\" + dll);

        }
    }
}
