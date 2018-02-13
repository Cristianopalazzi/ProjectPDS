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

        public ObservableCollection<Neighbor> Neighbors { get => neighbors; set => neighbors = value; }
        public bool Acceso { get => acceso; set => acceso = value; }
        public ObservableCollection<string> FileList { get => fileList; set => fileList = value; }

        private ObservableCollection<Neighbor> neighbors;
        private ObservableCollection<String> fileList;

        public NeighborSelection()
        {
            InitializeComponent();
            DataContext = this;
            NeighborProtocol n = NeighborProtocol.GetInstance;
            NeighborProtocol.NeighborsEvent += Modify_neighbors;
            Neighbors = new ObservableCollection<Neighbor>();
            FileList = new ObservableCollection<String>();
            listNeighborSelection.ItemsSource = Neighbors;
            Closing += NeighborSelection_Closing;
            Acceso = false;
        }

        private void NeighborSelection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClosingSelection?.Invoke();
            e.Cancel = true;
            //WindowState = WindowState.Minimized;
            Hide();
        }



        private void Button_send_files(object sender, RoutedEventArgs e)
        {
            List<Neighbor> selected = null;
            List<SendingFile> sendingFiles = null;
            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                selected = listNeighborSelection.SelectedItems.Cast<Neighbor>().ToList();
                foreach (String file in FileList)
                {
                    sendingFiles = new List<SendingFile>();
                    foreach (Neighbor n in selected)
                    {
                        SendingFile sf = new SendingFile(n, file);
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

        public void Modify_neighbors(Neighbor neighbor, bool addOrRemove)
        {
            bool isPresent = false;
            //AddOrRemove = true per neighbor da aggiungere e false da cancellare
            foreach (Neighbor n in Neighbors)
            {
                if (String.Compare(neighbor.NeighborIp, n.NeighborIp) == 0 && String.Compare(neighbor.NeighborName, n.NeighborName) == 0)
                {
                    isPresent = true;
                    if (!addOrRemove)
                        Application.Current.Dispatcher.Invoke(new Action(() => { Neighbors.Remove(n); }));
                    break;
                }
            }
            if (addOrRemove && !isPresent)
                Dispatcher.Invoke(new Action(() =>
                {
                    Neighbors.Add(neighbor);
                }));
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