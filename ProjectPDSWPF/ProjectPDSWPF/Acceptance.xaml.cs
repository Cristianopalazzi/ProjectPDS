using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per Acceptance.xaml
    /// </summary>
    /// //TODO modificare layout
    /// //TODO creare installer
    public partial class Acceptance : MetroWindow
    {

        public ObservableCollection<fileToAccept> AcceptingFiles { get => acceptingFiles; set => acceptingFiles = value; }
        private ObservableCollection<fileToAccept> acceptingFiles = new ObservableCollection<fileToAccept>();

        public Acceptance()
        {
            InitializeComponent();
            DataContext = this;
            filesToAccept.ItemsSource = AcceptingFiles;
        }


        private void acceptOrReject(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            fileToAccept sf = b.DataContext as fileToAccept;
            if (String.Compare(b.Name, "accept") == 0)
                Receiver.accepted = true;
            else if (String.Compare(b.Name, "reject") == 0)
                Receiver.accepted = false;
            Receiver.idFileToAccept = sf.Id;
            Receiver.mre.Set();
            AcceptingFiles.Remove(sf);
            if (acceptingFiles.Count == 0)
                Close();
        }

        private void acceptOrRejectAll(object sender, RoutedEventArgs e)
        {
            List<fileToAccept> toRemove = new List<fileToAccept>();
            Button b = sender as Button;
            if (String.Compare(b.Name, "acceptAll") == 0)
                Receiver.accepted = true;
            else if (String.Compare(b.Name, "rejectAll") == 0)
                Receiver.accepted = false;
            foreach (fileToAccept f in AcceptingFiles)
            {
                Receiver.idFileToAccept = f.Id;
                Receiver.mre.Set();
                Thread.Sleep(50);
                toRemove.Add(f);
            }
            foreach (fileToAccept f in toRemove)
                AcceptingFiles.Remove(f);
            Close();
        }

        public class fileToAccept
        {
            public string FileName { get => fileName; set => fileName = value; }
            public string UserName { get => userName; set => userName = value; }
            public string FileSize { get => fileSize; set => fileSize = value; }
            public string Id { get => id; set => id = value; }

            public fileToAccept(string fileName, string userName, string fileSize, string id)
            {
                FileName = fileName;
                UserName = userName;
                FileSize = fileSize;
                Id = id;
            }
            private string fileName, userName, fileSize, id;
        }
    }
}
