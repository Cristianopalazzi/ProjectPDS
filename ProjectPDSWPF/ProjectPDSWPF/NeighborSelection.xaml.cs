using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per NeighborSelection.xaml
    /// </summary>
    public partial class NeighborSelection : MetroWindow
    {

        public delegate void del(List<SendingFile> sf);
        public static event del sendSelectedNeighbors;

        public ObservableCollection<Neighbor> Neighbors { get => neighbors; set => neighbors = value; }
        private ObservableCollection<Neighbor> neighbors;

        public NeighborSelection()
        {
            InitializeComponent();
            DataContext = this;
            NeighborProtocol n = NeighborProtocol.getInstance;
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Neighbors = new ObservableCollection<Neighbor>();
            listNeighborSelection.ItemsSource = Neighbors;
            Closing += neighborSelectinClosing;
        }

        private void neighborSelectinClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }

        private void button_send_files(object sender, RoutedEventArgs e)
        {
            string file = sendingFile.Text;
            List<Neighbor> selected = null;
            List<SendingFile> sendingFiles = null;
            if (listNeighborSelection.SelectedItems.Count > 0)
            {
                sendingFiles = new List<SendingFile>();
                selected = listNeighborSelection.SelectedItems.Cast<Neighbor>().ToList();
                foreach (Neighbor n in selected)
                {
                    SendingFile sf = new SendingFile(n.NeighborIp, n.NeighborName, file, n.NeighborImage);
                    sendingFiles.Add(sf);
                }
                sendSelectedNeighbors(sendingFiles);
                Hide();
            }
            //TODO cambiare
            else MessageBox.Show("Seleziona almeno un vicino");
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
                    n1.NeighborName = "francischiellobello";
                    Neighbors.Add(n1);
                }));
        }
    }
}