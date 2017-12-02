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
                bottoneOnline.Content = "Pubblico";
            else
                bottoneOnline.Content = "Privato";
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