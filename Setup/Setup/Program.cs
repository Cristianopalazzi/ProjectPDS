
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Setup
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                Stream stream = ass.GetManifestResourceStream("Setup.check.ico");
                Icon ico = new Icon(stream);
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey sk = rk.OpenSubKey("\\*\\shell\\Condividi con", true);
                //  sk.SetValue("icon", "C:\\Users\\Gianmaria\\source\\repos\\projectPds\\share.ico");
                RegistryKey skCommand = sk.OpenSubKey("\\command", true);
                //sk.SetValue("@", "C:\\Users\\Gianmaria\\source\\repos\\ProjectPDS\\FileNameSender\\FileNameSender\\bin\\Debug\\FileNameSender.exe\"%1\"");
                RegistryKey skDir = rk.OpenSubKey("\\Directory\\shell\\Condividi con", true);
                //skDir.SetValue("icon", "C:\\Users\\Gianmaria\\source\\repos\\projectPds\\share.ico");
                RegistryKey skDirCommand = sk.OpenSubKey("\\command");
                // skDirCommand.SetValue("@", "C:\\Users\\Gianmaria\\source\\repos\\ProjectPDS\\FileNameSender\\FileNameSender\\bin\\Debug\\FileNameSender.exe\"%1\"");
            }
            catch
            {
                Console.WriteLine("Error finding resources");
            }


        }
    }
}
