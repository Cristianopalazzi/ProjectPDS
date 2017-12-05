
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Setup
{
    class Program
    {
        public delegate void myDelegate(bool fine);
        public static event myDelegate FineInstallazione;
        [STAThread]
        static void Main(string[] args)
        {
            SetupForm.installation += installation;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }

        private static void installation(string installationPath)
        {
            if (String.IsNullOrEmpty(installationPath))
            {
                Console.WriteLine("Non c'è una directory selezionata");
                return;
            }

            Console.WriteLine("PATH DI INSTALLAZIONE " + installationPath);
            try
            {
                string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NonnaPapera";
                DirectoryInfo dirInfo;
                if (!Directory.Exists(dirPath))
                {
                    dirInfo = Directory.CreateDirectory(dirPath);
                    creaRisorse("Setup.check.ico", dirPath);
                    creaRisorse("Setup.cross.ico", dirPath);
                    creaRisorse("Setup.share.ico", dirPath);
                    creaRisorse("Setup.warning.ico", dirPath);
                    creaRisorse("Setup.guest.png", dirPath);
                    //creaRisorse("Setup.FileNameSender.exe", installationPath);
                    //creaRisorse("Setup.MyZip.zip", installationPath);
                    Console.WriteLine("Icons created succesfully");
                }
                Console.WriteLine("no need to create icons");

                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey sk = rk.OpenSubKey("\\*\\shell\\Condividi con");
                if (sk != null)
                {
                    Console.WriteLine("no need for file keys");
                }
                else
                {
                    sk = rk.CreateSubKey("\\*\\shell\\Condividi con");
                    sk.SetValue("icon", dirPath + "\\share.ico");
                    RegistryKey skCommand = sk.OpenSubKey("\\command");
                    if (skCommand != null)
                        Console.WriteLine("no need for file command keys");
                    else
                    {
                        skCommand = sk.CreateSubKey("\\command");
                        //skCommand.SetValue("@", "C:\\Users\\Gianmaria\\source\\repos\\ProjectPDS\\FileNameSender\\FileNameSender\\bin\\Debug\\FileNameSender.exe\"%1\"");
                        skCommand.SetValue("@", installationPath + "\\FileNameSender.exe\"%1\"");
                    }
                }

                RegistryKey skDir = rk.OpenSubKey("\\Directory\\shell\\Condividi con");
                if (skDir != null)
                {
                    Console.WriteLine("no need for directory keys");
                }
                else
                {
                    skDir = rk.CreateSubKey("\\Directory\\shell\\Condividi con");
                    skDir.SetValue("icon", dirPath + "\\share.ico");
                    RegistryKey skDirCommand = skDir.OpenSubKey("\\command");
                    if (skDirCommand != null)
                        Console.WriteLine("no need for Directory command");
                    else
                    {
                        skDirCommand = skDir.CreateSubKey("\\command");
                        //skDirCommand.SetValue("@", "C:\\Users\\Gianmaria\\source\\repos\\ProjectPDS\\FileNameSender\\FileNameSender\\bin\\Debug\\FileNameSender.exe\"%1\"");
                        skDirCommand.SetValue("@", installationPath + "\\FileNameSender.exe\"%1\"");

                    }

                }
                FineInstallazione(true);

            }
            catch
            {
                Console.WriteLine("Error finding resources");
            }



        }

        public static bool creaRisorse(string name, string path)
        {
            Stream stream = null;
            MemoryStream ms = null;
            FileStream fs = null;
            try
            {
                Assembly ass = Assembly.GetExecutingAssembly();
                stream = ass.GetManifestResourceStream(name);
                ms = new MemoryStream();
                stream.CopyTo(ms);
                int index = name.IndexOf(".");
                string substring = name.Substring(index + 1);
                fs = File.Create(path + "\\" + substring);
                fs.Write(ms.ToArray(), 0, ms.ToArray().Length);
                fs.Flush();
                return true;

            }
            catch
            {
                Console.WriteLine("Error while creating {0}", name);
                return false;
            }
            finally
            {

                if (fs != null)
                    fs.Close();
                if (ms != null)
                    ms.Close();
                if (stream != null)
                    stream.Close();
            }

        }
    }
}
