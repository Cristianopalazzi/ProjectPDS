using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    /// 

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


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mw = new MainWindow();
            ns = new NeighborSelection();
            queue = new MyQueue();
            r = new Receiver();
            n = NeighborProtocol.getInstance;
            s = Settings.getInstance;
            us = new UserSettings();
            MyQueue.openNeighbors += neighbor_selection;
            ProjectPDSWPF.MainWindow.triggerBalloon += createBalloons;
            initializeNotifyIcon();
        }

        private void createBalloons(string fileName, string userName, int type)
        {
            switch (type)
            {
                case 0:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipText = "ricevuto correttamente da " + userName;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }

                case 1:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipText = "inviato correttamente a " + userName;
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 1; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    };

                case 2:
                    {
                        nIcon.BalloonTipTitle = fileName;
                        nIcon.BalloonTipText = userName + " ha annullato l'invio";
                        nIcon.BalloonTipClicked += delegate { mw.tabControl.SelectedIndex = 0; mw.Show(); mw.Activate(); mw.WindowState = WindowState.Normal; };
                        nIcon.ShowBalloonTip(3000);
                        break;
                    }
            }
        }

        private void initializeNotifyIcon()
        {
            //TODO click sinistro sull'icona
            nIcon.Icon = new System.Drawing.Icon(Directory.GetCurrentDirectory() + "/check.ico");
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
            item3.Text = "Tizi online";
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
        }



        private void neighbor_selection(string file)
        {
            Current.Dispatcher.Invoke(new Action(() =>
            {
                ns.sendingFile.Text = file;
                //TODO vedere perchè non va in activate
                ns.Show();
                ns.Activate();
                ns.WindowState = WindowState.Normal;
                ns.Topmost = true;
                ns.Topmost = false;
                ns.Focus();
            }));
        }
    }
}