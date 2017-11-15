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
            Left = System.Windows.SystemParameters.WorkArea.Right - Width;
            Top = System.Windows.SystemParameters.WorkArea.Bottom - Height;
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
            }
            else
            {
                settings.Online = true;
                bottoneOnline.Content = "Online";
                NeighborProtocol.senderEvent.Set();
            }
            Settings.writeSettings(settings);
        }

        private Settings settings = Settings.getInstance;
        public delegate void myDelegate();
        public static event myDelegate openTabSettings;
    }
}
