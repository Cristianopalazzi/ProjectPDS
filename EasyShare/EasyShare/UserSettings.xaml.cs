using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace EasyShare
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : MetroWindow
    {
        public UserSettings()
        {
            InitializeComponent();
            settings = Settings.GetInstance;
            Left = SystemParameters.WorkArea.Right - Width;
            Top = SystemParameters.WorkArea.Bottom - Height;
            if (settings.Online)
                bottoneOnline.Content = "Pubblico";
            else
                bottoneOnline.Content = "Privato";
            Closing += UserSettings_Closing;
        }
        private void UserSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.WriteSettings(settings);
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }

        public void OpenSettings(object sender, EventArgs e)
        {
            OpenTabSettings?.Invoke();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Online)
            {
                settings.Online = false;
                bottoneOnline.Content = "Privato";
                NeighborProtocol.GetInstance.QuitMe();
                NeighborProtocol.senderEvent.Reset();
            }
            else
            {
                settings.Online = true;
                bottoneOnline.Content = "Pubblico";
                NeighborProtocol.senderEvent.Set();
            }
            Settings.WriteSettings(settings);
        }
        private Settings settings;
        public delegate void myDelegate();
        public static event myDelegate OpenTabSettings;
    }
}