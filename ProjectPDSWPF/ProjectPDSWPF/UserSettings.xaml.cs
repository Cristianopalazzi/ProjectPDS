using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : MetroWindow
    {
        public UserSettings()
        {
            InitializeComponent();
            settings = Settings.getInstance;
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;
            if (settings.Online)
                bottoneOnline.Content = "Pubblico";
            else
                bottoneOnline.Content = "Privato";
            Closing += UserSettings_Closing;
            Deactivated += UserSettings_Deactivated;
        }

        private void UserSettings_Deactivated(object sender, EventArgs e)
        {
            
        }

        private void UserSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.writeSettings(settings);
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }

        public void openSettings(object sender, EventArgs e)
        {
            openTabSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Online)
            {
                settings.Online = false;
                bottoneOnline.Content = "Privato";
                NeighborProtocol.getInstance.quitMe();
                NeighborProtocol.senderEvent.Reset();
            }
            else
            {
                settings.Online = true;
                bottoneOnline.Content = "Pubblico";
                NeighborProtocol.senderEvent.Set();
            }
            Settings.writeSettings(settings);
        }
        private Settings settings;
        public delegate void myDelegate();
        public static event myDelegate openTabSettings;
    }
}