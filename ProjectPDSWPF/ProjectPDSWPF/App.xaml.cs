using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            initializeNotifyIcon();
        }

        private void initializeNotifyIcon()
        {
            nIcon.Icon = new System.Drawing.Icon(@"C:\Users\Gianmaria\Desktop\check.ico");
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
            item4.Text = "impostazioni";
            item5.Text = "Esci";
            item1.Click += delegate { mw.Show(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 0; };
            item2.Click += delegate { mw.Show(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 1; };
            item3.Click += delegate { mw.Show(); mw.WindowState = WindowState.Normal; mw.tabControl.SelectedIndex = 2; };
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
            item5.Click += delegate { nIcon.Dispose(); App.Current.Shutdown(); };
            nIcon.Visible = true;
        }



        private void neighbor_selection(string file)
        {
            Current.Dispatcher.Invoke(new Action(() =>
            {
                ns.sendingFile.Text = file;
                //TODO sistemare la scritta del file che stiamo mandando
                ns.Show();
            }));
        }
    }

}
