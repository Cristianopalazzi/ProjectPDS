using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            settings.Online = !settings.Online;
            if (settings.Online)
                bottoneOnline.Content = "Online";
            else
                bottoneOnline.Content = "Offline";
            Settings.writeSettings(settings);
        }

        Settings settings = Settings.getInstance;

        public delegate void myDelegate();
        public static event myDelegate openTabSettings;
    }
}
