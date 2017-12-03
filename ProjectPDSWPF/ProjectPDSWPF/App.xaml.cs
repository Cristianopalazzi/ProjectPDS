using System;
using System.Windows;
using System.IO;
using Microsoft.Win32;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    /// 

        //TODO cambiare tempo di attesa per la accetazione lato sender: lasciamo infinito o mettiamo tempo altissimo?
    public partial class App : Application
    {
        private NeighborSelection ns;
        private MyQueue queue;
        private Receiver r;
        private NeighborProtocol n;
        private Settings s;
        private System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private MainWindow mw;
        private UserSettings us;
        public static string defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + Constants.projectName;
        public static string defaultResourcesFolder = defaultFolder + "\\Resources";

        //TODO file < 260 directory <248

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            s = Settings.getInstance;
            queue = new MyQueue();
            n = NeighborProtocol.getInstance;
            r = new Receiver();

            if (Directory.Exists(defaultFolder))
                foreach (FileInfo f in new DirectoryInfo(defaultFolder).GetFiles("*.zip"))
                    f.Delete();
            else Directory.CreateDirectory(defaultFolder);

            mw = new MainWindow();
            ns = new NeighborSelection();
            us = new UserSettings();

            MyQueue.openNeighbors += neighbor_selection;
            Sender.fileRejected += createBalloons;
            ProjectPDSWPF.MainWindow.triggerBalloon += createBalloons;
            SystemEvents.SessionEnded += SystemEvents_SessionEnded;
            Receiver.receivingFailure += createBalloons;
            NeighborSelection.closingSelection += NeighborSelection_closingSelection;
            us.Deactivated += Us_Deactivated;

            initializeNotifyIcon();
        }

        private void Us_Deactivated(object sender, EventArgs e)
        {
            Settings.writeSettings(Settings.getInstance);
            us.WindowState = WindowState.Minimized;
            us.Hide();
        }

        private void NeighborSelection_closingSelection()
        {
            ns.Acceso = false;
        }

        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            Settings.writeSettings(Settings.getInstance);
            n.quitMe();
            nIcon.Dispose();
        }

        private void createBalloons(string fileName, string userName, Constants.NOTIFICATION_STATE state)
        {
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
                        string user = NeighborProtocol.getInstance.getUserFromIp(userName);
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
                        string user = NeighborProtocol.getInstance.getUserFromIp(userName);
                        string text = "Errore durante l'invio ";
                        if (!String.IsNullOrEmpty(user))
                            text = String.Concat(text, "a: " + user);
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.FILE_ERROR:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = "Errore durante la preparazione del file";
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
                case Constants.NOTIFICATION_STATE.REC_ERROR:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        string text = "Errore durante la ricezione da" + userName;
                        nIcon.BalloonTipText = text;
                        nIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
            }
        }

        private void initializeNotifyIcon()
        {
            //TODO provare time estimation con lo stesso evento e non 2 separati
            nIcon.Icon = new System.Drawing.Icon(defaultResourcesFolder + "/check.ico");
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
            item4.Click += delegate
            {
                if (us.WindowState == WindowState.Normal)
                {
                    us.Hide();
                    us.WindowState = WindowState.Minimized;
                }
                else
                {
                    us.Show();
                    us.WindowState = WindowState.Normal;
                }
            };
            item5.Click += delegate { n.quitMe(); nIcon.Dispose(); Settings.writeSettings(Settings.getInstance); App.Current.Shutdown(); };
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

        private void neighbor_selection(string file)
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
                    NeighborProtocol.getInstance.clean();
                    ns.listNeighborSelection.UnselectAll();
                    ns.FileList.Clear();
                    ns.Acceso = true;
                    ns.FileList.Add(file);
                }
                ns.Show();
                ns.Activate();
                ns.Topmost = true;
                ns.Topmost = false;
                ns.Focus();
            }));
        }
    }
}