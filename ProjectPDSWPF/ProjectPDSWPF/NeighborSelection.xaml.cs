using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per NeighborSelection.xaml
    /// </summary>
    public partial class NeighborSelection : MetroWindow
    {

        public delegate void del(List<SendingFile> sf);
        public static event del sendSelectedNeighbors;

        public delegate void myDelegate();
        public static event myDelegate closingSelection;

        private Boolean acceso;

        public ObservableCollection<Neighbor> Neighbors { get => neighbors; set => neighbors = value; }
        public ObservableCollection<string> FileList { get => fileList; set => fileList = value; }
        public bool Acceso { get => acceso; set => acceso = value; }

        private ObservableCollection<Neighbor> neighbors;
        private ObservableCollection<String> fileList;




        public NeighborSelection()
        {
            InitializeComponent();
            DataContext = this;
            NeighborProtocol n = NeighborProtocol.getInstance;
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Neighbors = new ObservableCollection<Neighbor>();
            FileList = new ObservableCollection<string>();
            listNeighborSelection.ItemsSource = Neighbors;
            ListFiles.ItemsSource = FileList;
            Closing += NeighborSelection_Closing;
            Acceso = false;

        }

        private void NeighborSelection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closingSelection();
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }



        private void button_send_files(object sender, RoutedEventArgs e)
        {

            
            List<Neighbor> selected = null;
            List<SendingFile> sendingFiles = null;
            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                foreach (string file in FileList)
                {
                    sendingFiles = new List<SendingFile>();
                    selected = listNeighborSelection.SelectedItems.Cast<Neighbor>().ToList();
                    foreach (Neighbor n in selected)
                    {
                        SendingFile sf = new SendingFile(n.NeighborIp, n.NeighborName, file, n.NeighborImage);
                        sendingFiles.Add(sf);
                    }
                    sendSelectedNeighbors(sendingFiles);
                }
                Acceso = false;
                Hide();
            }
            else
                this.ShowMessageAsync("Ops", "Seleziona almeno un contatto");

        }

        public void modify_neighbors(string id, byte[] bytes, bool addOrRemove)
        {
            bool isPresent = false;
            //AddOrRemove = true per neighbor da aggiungere e false da cancellare
            foreach (Neighbor n in Neighbors)
            {
                if (String.Compare(id, n.NeighborName + "@" + n.NeighborIp) == 0)
                {
                    isPresent = true;
                    if (!addOrRemove)
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            Neighbors.Remove(n);
                        }));
                    break;
                }
            }
            if (addOrRemove && !isPresent)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Neighbor n1 = new Neighbor(id, bytes);
                    Neighbors.Add(n1);
                }));
        }


       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int index = FileList.IndexOf(b.Tag.ToString());
            if (index < 0)
                return;
            FileList.RemoveAt(index);
            if (FileList.Count == 0)
            {
                Acceso = false;
                Hide();
            }
        }
    }
}