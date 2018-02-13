using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;

namespace EasyShare
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        private NeighborSelection ns;
        private Queue queue;
        private Receiver r;
        private NeighborProtocol n;
        private Settings s;
        private System.Windows.Forms.NotifyIcon nIcon;
        private MainWindow mw;
        private UserSettings us;
        public static string defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Constants.projectName;
        public static string defaultResourcesFolder = defaultFolder + "\\Resources";
        public static string currentDirectoryResources = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Resources";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            String appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            Process[] p = Process.GetProcessesByName("EasyShare");

            if (p.Length > 1)
                Environment.Exit(0);

            Queue.OpenNeighbors += Neighbor_selection;
            Queue.QueueBalloon += CreateBalloons;
            Sender.FileRejected += CreateBalloons;
            EasyShare.MainWindow.TriggerBalloon += CreateBalloons;
            SystemEvents.SessionEnded += SystemEvents_SessionEnded;
            Receiver.ReceivingFailure += CreateBalloons;
            NeighborSelection.ClosingSelection += NeighborSelection_closingSelection;

            if (Directory.Exists(defaultFolder))
                foreach (FileInfo f in new DirectoryInfo(defaultFolder).GetFiles("*.zip"))
                    f.Delete();
            else Directory.CreateDirectory(defaultFolder);


            s = Settings.GetInstance;
            queue = new Queue();
            n = NeighborProtocol.GetInstance;
            r = new Receiver();
            mw = new MainWindow();
            ns = new NeighborSelection();
            us = new UserSettings();

            nIcon = new System.Windows.Forms.NotifyIcon();
            InitializeNotifyIcon();
        }

        private void NeighborSelection_closingSelection()
        {
            ns.Acceso = false;
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            n.QuitMe(); nIcon.Dispose(); Settings.WriteSettings(Settings.GetInstance);
            NeighborProtocol.ShutDown = true;
            NeighborProtocol.senderEvent.Set();
            App.Current.Shutdown();
        }

        private void CreateBalloons(string fileName, string userName, Constants.NOTIFICATION_STATE state)
        {
            if (!Settings.GetInstance.EnableNotification) return;
            switch (state)
            {
                case Constants.NOTIFICATION_STATE.RECEIVED:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipText = "ricevuto correttamente da " + userName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.None;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }

                case Constants.NOTIFICATION_STATE.SENT:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.None;
                        nIcon.BalloonTipText = "inviato correttamente a " + userName;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }

                case Constants.NOTIFICATION_STATE.CANCELED:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipText = userName + " ha annullato l'invio";
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.REFUSED:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipText = userName + " ha rifiutato il tuo file";
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.NET_ERROR:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        string user = NeighborProtocol.GetInstance.GetUserFromIp(userName);
                        string text = "Errore nella connessione con l'host";
                        if (!String.IsNullOrEmpty(user))
                            text = String.Concat(text, ": " + user);
                        nIcon.BalloonTipText = text;
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.SEND_ERROR:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        string user = NeighborProtocol.GetInstance.GetUserFromIp(userName);
                        string text = "Errore durante l'invio ";
                        if (!String.IsNullOrEmpty(user))
                            text = String.Concat(text, "a: " + user);
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.REC_ERROR:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = "Errore durante la ricezione da: " + userName;
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.FILE_ERROR_REC:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = "Errore durante la preparazione del file in ricezione";
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.FILE_ERROR_SEND:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = "Errore durante la preparazione del file in invio";
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.EXISTS:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = userName + " sta ancora elaborando questa versione del file";
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
            }
        }

        private void InitializeNotifyIcon()
        {
            nIcon.Icon = new System.Drawing.Icon(currentDirectoryResources + "/share.ico");
            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item3 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item4 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item5 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add(item1);
            cMenu.MenuItems.Add(item2);
            cMenu.MenuItems.Add(item3);
            cMenu.MenuItems.Add(item4);
            cMenu.MenuItems.Add(item5);
            nIcon.ContextMenu = cMenu;
            item1.Text = "File in ricezione";
            item2.Text = "File in invio";
            item3.Text = "Contatti online";
            item4.Text = "Impostazioni";
            item5.Text = "Esci";
            item1.Click += delegate { mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 0; };
            item2.Click += delegate { mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 1; };
            item3.Click += delegate { mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 2; };
            item4.Click += delegate { mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 3; };

            item5.Click += delegate
            {
                bool filesInProgress = mw.CheckForFilesInProgress();
                if (filesInProgress)
                    if (AskForExit != null)
                        if (!AskForExit())
                            return;

                n.QuitMe(); nIcon.Dispose(); Settings.WriteSettings(Settings.GetInstance);
                NeighborProtocol.ShutDown = true;
                NeighborProtocol.senderEvent.Set();
                App.Current.Shutdown();
            };
            nIcon.Visible = true;
            nIcon.MouseClick += NIcon_MouseClick;
        }

        private void NIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (us.WindowState == WindowState.Normal)
                {
                    us.WindowState = WindowState.Minimized;
                    us.Hide();
                }
                else
                {
                    us.Show();
                    us.WindowState = WindowState.Normal;
                    us.Activate();
                }
            }
        }

        private void Neighbor_selection(string file)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (ns.Acceso)
                {
                    if (!ns.FileList.Contains(file))
                        ns.FileList.Add(file);
                }
                else
                {
                    NeighborProtocol.GetInstance.Clean();
                    ns.listNeighborSelection.UnselectAll();
                    ns.FileList.Clear();
                    ns.Acceso = true;
                    ns.FileList.Add(file);
                }
                ns.WindowState = WindowState.Normal;
                ns.Show();
                ns.Activate();
                ns.Topmost = true;
                ns.Topmost = false;
                ns.Focus();
            }));
        }

        public delegate bool myDelegate();
        public static event myDelegate AskForExit;
    }
}