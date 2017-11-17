using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using MahApps.Metro.Controls;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //TODO cercare come mettere le immagini come risorse
        // controllare invii e ricezioni con placeholder al posto della foto utente
        // placeholder per le liste vuote
        // rifare la window per le impostazioni
        // resize della finestra principale
        // rifare le scritte con il tasto destro sulla liste dei file in invio e in ricezione


        public MainWindow()
        {
            InitializeComponent();
            sets = Settings.getInstance;
            gridSettings.DataContext = sets;
            
            Closing += MainWindow_Closing;
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Sender.updateProgress += updateProgressBar;
            Receiver.updateProgress += updateReceivingProgressBar;
            Receiver.updateReceivingFiles += updateReceivingFiles;
            UserSettings.openTabSettings += tabChange;
            Receiver.fileCancel += file_cancel;
            NeighborSelection.sendSelectedNeighbors += addSendingFiles;

            NeighborsValues = new ObservableCollection<Neighbor>();
            FilesToSend = new ObservableCollection<SendingFile>();
            FilesToReceive = new ObservableCollection<ReceivingFile>();
            DataContext = this;
            Neighbors.ItemsSource = NeighborsValues;
            sendingFiles.ItemsSource = FilesToSend;
            listReceivingFiles.ItemsSource = FilesToReceive;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(filesToSend);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("FileName");
            view.GroupDescriptions.Add(groupDescription);

           
        }

        private void tabChange()
        {
            Show();
            WindowState = WindowState.Normal;
            tabControl.SelectedIndex = 3;
        }

        private void addSendingFiles(List<SendingFile> sf)
        {
            foreach (SendingFile s in sf)
                FilesToSend.Add(s);
        }

        private void file_cancel(string id)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.File_state = Constants.FILE_STATE.CANCELED;
                        r.Pic = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/cross.ico", UriKind.RelativeOrAbsolute));
                        triggerBalloon(r.Filename, r.Name, 2);
                        break;
                    }
            }));
        }

        private void updateReceivingProgressBar(string id, int percentage)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                {
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.Value = percentage;
                        if (r.Value == 100)
                        {
                            r.File_state = Constants.FILE_STATE.COMPLETED;
                            r.Pic = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/check.ico", UriKind.RelativeOrAbsolute));
                            triggerBalloon(r.Filename, r.Name, 0);
                            break;
                        }
                    }
                }
            }));
        }

        private void updateReceivingFiles(string senderID, byte [] image, string fileName, string id)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ReceivingFile rf = new ReceivingFile(new Neighbor(senderID, image), fileName, id);
                FilesToReceive.Add(rf);
            }));
        }

        private void updateProgressBar(string filename, Socket sock, int percentage)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                    if (sf.Sock == sock)
                    {
                        sf.Value = percentage;
                        if (sf.Value == 100)
                        {
                            sf.File_state = Constants.FILE_STATE.COMPLETED;
                            sf.Pic = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/check.ico", UriKind.RelativeOrAbsolute));
                            if (WindowState != WindowState.Normal || tabControl.SelectedIndex != 1)
                                triggerBalloon(sf.FileName, sf.Name, 1);
                        }
                        break;
                    }
            }));
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            SendingFile sf = b.DataContext as SendingFile;
            try
            {
                sf.Sock.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException exc)
            {
                MessageBox.Show(exc.ToString());
            }
            finally
            {
                sf.File_state = Constants.FILE_STATE.CANCELED;
                sf.Pic = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/cross.ico", UriKind.RelativeOrAbsolute));
                b.Visibility = Visibility.Hidden;
            }
        }


 
        private void receiving_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            ReceivingFile rf = listReceivingFiles.SelectedItem as ReceivingFile;
            if (rf.File_state == Constants.FILE_STATE.PROGRESS)
                MessageBox.Show("Attendi il completamento del file");
            else FilesToReceive.Remove(rf);
        }


        private void receiving_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            List<ReceivingFile> tmp = new List<ReceivingFile>();
            foreach (ReceivingFile rf in FilesToReceive)
                if (rf.File_state == Constants.FILE_STATE.CANCELED || rf.File_state == Constants.FILE_STATE.COMPLETED)
                    tmp.Add(rf);
            foreach (ReceivingFile rf in tmp)
                FilesToReceive.Remove(rf);
        }

        private void sending_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            SendingFile sf = sendingFiles.SelectedItem as SendingFile;
            if (sf.File_state == Constants.FILE_STATE.PROGRESS)
                MessageBox.Show("Non puoi cancellare un file in invio\nPremi annulla per fermare l'invio");
            else FilesToSend.Remove(sf);
        }


        private void sending_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            List<SendingFile> tmp = new List<SendingFile>();
            foreach (SendingFile sf in FilesToSend)
                if (sf.File_state == Constants.FILE_STATE.CANCELED || sf.File_state == Constants.FILE_STATE.COMPLETED)
                    tmp.Add(sf);
            foreach (SendingFile sf in tmp)
                FilesToSend.Remove(sf);
        }


        public void modify_neighbors(string id, byte [] bytes, bool addOrRemove)
        {
            bool isPresent = false;
            //AddOrRemove = true per neighbor da aggiungere e false da cancellare
            foreach (Neighbor n in NeighborsValues)
            {
                if (String.Compare(id, n.NeighborName + "@" + n.NeighborIp) == 0)
                {
                    isPresent = true;
                    if (!addOrRemove)
                        Application.Current.Dispatcher.Invoke(new Action(() => { neighborsValues.Remove(n); }));
                    break;
                }
            }
            if (addOrRemove && !isPresent)
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Neighbor n1 = new Neighbor(id, bytes);
                    neighborsValues.Add(n1);
                    //neighborsValues.Add(n1); neighborsValues.Add(n1); neighborsValues.Add(n1); neighborsValues.Add(n1); neighborsValues.Add(n1); neighborsValues.Add(n1);
                }));
        }


        public void openFolderBrowserDialog(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                sets.DefaultDirPath = dialog.SelectedPath;
            }
        }



        public void tabChanged(object Sender, EventArgs e)
        {
            Settings.writeSettings(sets);
        }


        private ObservableCollection<Neighbor> neighborsValues;
        private ObservableCollection<SendingFile> filesToSend;
        private ObservableCollection<ReceivingFile> filesToReceive;
        private Settings sets;

        public ObservableCollection<Neighbor> NeighborsValues { get => neighborsValues; set => neighborsValues = value; }
        public ObservableCollection<SendingFile> FilesToSend { get => filesToSend; set => filesToSend = value; }
        public ObservableCollection<ReceivingFile> FilesToReceive { get => filesToReceive; set => filesToReceive = value; }

        public delegate void myDelegate(string filename, string username, int type);
        public static event myDelegate triggerBalloon;

        
    }
}