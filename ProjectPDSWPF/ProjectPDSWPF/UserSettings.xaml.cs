using System;
using System.Windows;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : Window
    {
        public UserSettings()
        {
            InitializeComponent();
            settings = Settings.getInstance;
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;
            if (settings.Online)
                bottoneOnline.Content = "Online";
            else
                bottoneOnline.Content = "Offline";
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
                bottoneOnline.Content = "Offline";
                NeighborProtocol.senderEvent.Reset();
                NeighborProtocol.getInstance.quitMe();
            }
            else
            {
                settings.Online = true;
                bottoneOnline.Content = "Online";
                NeighborProtocol.senderEvent.Set();
            }
            Settings.writeSettings(settings);
        }

        private void deactivated_settings(object sender, EventArgs e)
        {
            Settings.writeSettings(settings);
            WindowState = WindowState.Minimized;
            Hide();
        }

        private Settings settings;
        public delegate void myDelegate();
        public static event myDelegate openTabSettings;
    }
}