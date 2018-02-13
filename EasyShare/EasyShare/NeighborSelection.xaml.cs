using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;
using System.IO;

namespace EasyShare
{
    /// <summary>
    /// Logica di interazione per NeighborSelection.xaml
    /// </summary>
    public partial class NeighborSelection : MetroWindow
    {
        public delegate void del(List<SendingFile> sf);
        public static event del SendSelectedNeighbors;

        public delegate void myDelegate();
        public static event myDelegate ClosingSelection;

        private Boolean acceso;

        public bool Acceso { get => acceso; set => acceso = value; }
        public ObservableCollection<string> FileList { get => fileList; set => fileList = value; }

        private ObservableCollection<String> fileList;

        public NeighborSelection()
        {
            InitializeComponent();
            DataContext = this;
            NeighborProtocol n = NeighborProtocol.GetInstance;
            FileList = new ObservableCollection<String>();
            listNeighborSelection.ItemsSource = n.Neighbors;
            Closing += NeighborSelection_Closing;
            Acceso = false;
        }

        private void NeighborSelection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClosingSelection?.Invoke();
            e.Cancel = true;
            Hide();
        }

        private void Button_send_files(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, Neighbor>> selected = null;
            List<SendingFile> sendingFiles = null;
            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                selected = listNeighborSelection.SelectedItems.Cast<KeyValuePair<string, Neighbor>>().ToList();
                foreach (String file in FileList)
                {
                    sendingFiles = new List<SendingFile>();
                    foreach (KeyValuePair<string, Neighbor> n in selected)
                    {
                        SendingFile sf = new SendingFile(n.Value, file);
                        sendingFiles.Add(sf);
                    }
                    SendSelectedNeighbors(sendingFiles);
                }
                Acceso = false;
                Hide();
            }
            else
                this.ShowMessageAsync("Ops", "Seleziona almeno un contatto");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            String s = b.DataContext as String;
            FileList.Remove(s);
            if (FileList.Count == 0)
            {
                Acceso = false;
                Hide();
            }
        }
    }
}