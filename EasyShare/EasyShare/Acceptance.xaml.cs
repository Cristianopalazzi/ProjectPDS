using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace EasyShare
{
    /// <summary>
    /// Logica di interazione per Acceptance.xaml
    /// </summary>
    public partial class Acceptance : MetroWindow
    {

        public ObservableCollection<FileToAccept> AcceptingFiles { get => acceptingFiles; set => acceptingFiles = value; }
        private ObservableCollection<FileToAccept> acceptingFiles = new ObservableCollection<FileToAccept>();

        public Acceptance()
        {
            InitializeComponent();
            DataContext = this;
            filesToAccept.ItemsSource = AcceptingFiles;
        }


        private void AcceptOrReject(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            FileToAccept sf = b.DataContext as FileToAccept;
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

        private void AcceptOrRejectAll(object sender, RoutedEventArgs e)
        {
            List<FileToAccept> toRemove = new List<FileToAccept>();
            Button b = sender as Button;
            if (String.Compare(b.Name, "acceptAll") == 0)
                Receiver.accepted = true;
            else if (String.Compare(b.Name, "rejectAll") == 0)
                Receiver.accepted = false;
            foreach (FileToAccept f in AcceptingFiles)
            {
                Receiver.idFileToAccept = f.Id;
                Receiver.mre.Set();
                Thread.Sleep(50);
                toRemove.Add(f);
            }
            foreach (FileToAccept f in toRemove)
                AcceptingFiles.Remove(f);
            Close();
        }

        public class FileToAccept
        {
            public string FileName { get => fileName; set => fileName = value; }
            public string UserName { get => userName; set => userName = value; }
            public string FileSize { get => fileSize; set => fileSize = value; }
            public string Id { get => id; set => id = value; }

            public FileToAccept(string fileName, string userName, string fileSize, string id)
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
