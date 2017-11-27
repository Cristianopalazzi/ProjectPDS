using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Threading;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            sets = Settings.getInstance;
            gridSettings.DataContext = sets;
            Deactivated += MainWindow_Deactivated;
            Closing += MainWindow_Closing;
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Sender.updateProgress += updateProgressBar;
            Sender.updateRemainingTime += updateRemainingTime;
            Receiver.updateProgress += updateReceivingProgressBar;
            Receiver.updateReceivingFiles += updateReceivingFiles;
            UserSettings.openTabSettings += tabChange;
            Receiver.fileCancel += file_cancel;
            NeighborSelection.sendSelectedNeighbors += addSendingFiles;
            Receiver.askToAccept += Receiver_askToAccept;
            Sender.fileRejectedGUI += Sender_fileRejectedGUI;
            Sender.sendingFailure += Sender_sendingFailure;

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

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            Topmost = false;
        }


        //aggiornamento GUI dopo che il file è stato rifiutato dal receiver
        private void Sender_fileRejectedGUI(Socket sender)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
           {
               foreach (SendingFile sf in FilesToSend)
               {
                   if (sf.Sock == sender)
                   {
                       sf.File_state = Constants.FILE_STATE.CANCELED;
                       sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
                       break;
                   }
               }
           }));
        }

        //chiedo al receiver se vuole accettare il file che sto mandando
        private MessageDialogResult Receiver_askToAccept(string userName, string fileName, string dimension)
        {
            MessageDialogResult mr = MessageDialogResult.Negative;
            myMainWindow.Dispatcher.Invoke(new Action(() =>
            {
                MetroWindow mw = Application.Current.MainWindow as MetroWindow;
                mw.Show();
                mr = mw.ShowModalMessageExternal("File in arrivo", userName + " vuole condividere " + fileName + " di: " + dimension + "\nAccetti?", MessageDialogStyle.AffirmativeAndNegative);
                if (mr == MessageDialogResult.Negative)
                    mw.Hide();
                else
                    tabControl.SelectedIndex = 0;
            }));
            return mr;
        }


        //mostro la tab delle impostazioni dopo aver cliccato su "impostazioni"
        private void tabChange()
        {
            Show();
            WindowState = WindowState.Normal;
            tabControl.SelectedIndex = 3;
        }

        //aggiungo file da inviare alla lista dopo aver cliccato invia nella finestra di selezione dei vicini
        private void addSendingFiles(List<SendingFile> sf)
        {
            foreach (SendingFile s in sf)
                FilesToSend.Add(s);
        }

        //file annullato da parte del sender => aggiorno la lista dei file in ricezione
        private void file_cancel(string id, Constants.NOTIFICATION_STATE state)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.File_state = Constants.FILE_STATE.CANCELED;
                        r.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
                        if(state == Constants.NOTIFICATION_STATE.CANCELED)
                            triggerBalloon(r.Filename, r.Name, Constants.NOTIFICATION_STATE.CANCELED); //2
                        else if( state == Constants.NOTIFICATION_STATE.REC_ERROR)
                            triggerBalloon(r.Filename, r.Name, Constants.NOTIFICATION_STATE.REC_ERROR); //7
                        break;
                    }
            }));
        }

        //aggiorno la progress bar del file in ricezione
        private void updateReceivingProgressBar(string id, int percentage)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                {
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.Value = percentage;
                        if (r.Value == 100)
                        {
                            r.File_state = Constants.FILE_STATE.COMPLETED;
                            r.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/check.ico"));
                            triggerBalloon(r.Filename, r.Name, 0);
                            break;
                        }
                    }
                }
            }));
        }

        // aggiungo un nuovo file in ricezione alla lista
        private void updateReceivingFiles(string senderID, byte[] image, string fileName, string id)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                ReceivingFile rf = new ReceivingFile(new Neighbor(senderID, image), fileName, id);
                FilesToReceive.Add(rf);
            }));
        }

        //aggiorno il tempo rimanente ad un file in invio
        private void updateRemainingTime(Socket sock, string remainingTime)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                    if (sf.Sock == sock)
                    {
                        sf.RemainingTime = remainingTime;
                        break;
                    }
            }));
        }

        //aggiorno la progress bar del file in invio
        private void updateProgressBar(string filename, Socket sock, int percentage)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                    if (sf.Sock == sock)
                    {
                        sf.Value = percentage;
                        sf.File_state = Constants.FILE_STATE.PROGRESS;
                        if (sf.Value == 100)
                        {
                            sf.File_state = Constants.FILE_STATE.COMPLETED;
                            sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/check.ico"));
                            if (WindowState != WindowState.Normal || tabControl.SelectedIndex != 1)
                                triggerBalloon(sf.FileName, sf.Name, Constants.NOTIFICATION_STATE.SENT); //1
                        }
                        break;
                    }
            }));
        }

        //chiusura della finestra principale
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
        }

        //click a seguito della cancellazione di un file in invio
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            SendingFile sf = b.DataContext as SendingFile;
            try
            {
                if (sf.Sock.Connected)
                    sf.Sock.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //TODO questo messagebox
                MessageBox.Show("ROTTO");
            }
            finally
            {
                sf.File_state = Constants.FILE_STATE.CANCELED;
                sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
            }
        }

        //errore durante l'invio di un file
        private void Sender_sendingFailure(Socket sock)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                {
                    if (sf.Sock == sock)
                    {
                        sf.File_state = Constants.FILE_STATE.CANCELED;
                        sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
                    }
                }
            }));
        }


        //context menu sui file in ricezione (cancella un file ricevuto correttamente)
        private void receiving_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            ReceivingFile rf = listReceivingFiles.SelectedItem as ReceivingFile;
            if (rf.File_state == Constants.FILE_STATE.PROGRESS)
                this.ShowMessageAsync("Ops", "Attendi il completamento del file");
            else FilesToReceive.Remove(rf);
        }

        //context menu sui file in ricezione (cancella tutti i file ricevuti correttamente)
        private void receiving_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            List<ReceivingFile> tmp = new List<ReceivingFile>();
            foreach (ReceivingFile rf in FilesToReceive)
                if (rf.File_state == Constants.FILE_STATE.CANCELED || rf.File_state == Constants.FILE_STATE.COMPLETED)
                    tmp.Add(rf);
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (ReceivingFile rf in tmp)
                FilesToReceive.Remove(rf);
        }
        //context menu sui file in invio (cancella un file inviato correttamente)
        private void sending_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            SendingFile sf = sendingFiles.SelectedItem as SendingFile;
            if (sf.File_state == Constants.FILE_STATE.PROGRESS)
                this.ShowMessageAsync("Ops", "Non puoi cancellare un file in invio.\nPremi annulla per fermarlo.");
            else FilesToSend.Remove(sf);
        }

        //context menu sui file in invio (cancella tutti i file inviati correttamente)
        private void sending_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            List<SendingFile> tmp = new List<SendingFile>();
            foreach (SendingFile sf in FilesToSend)
                if (sf.File_state == Constants.FILE_STATE.CANCELED || sf.File_state == Constants.FILE_STATE.COMPLETED)
                    tmp.Add(sf);
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (SendingFile sf in tmp)
                FilesToSend.Remove(sf);
        }

        //aggiorno la lista dei vicini online
        public void modify_neighbors(string id, byte[] bytes, bool addOrRemove)
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
                Dispatcher.Invoke(new Action(() =>
                {
                    Neighbor n1 = new Neighbor(id, bytes);
                    neighborsValues.Add(n1);
                }));
        }

        //mostro il dialog per scegliere la cartella in cui salvare il file in ricezione
        public void openFolderBrowserDialog(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                sets.DefaultDirPath = dialog.SelectedPath;
            }
        }


        //scrivo su file i settings ogni volta che cambio tab (rispetto a quella delle impostazioni)
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

        public delegate void myDelegate(string filename, string username, Constants.NOTIFICATION_STATE state);
        public static event myDelegate triggerBalloon;
    }
}
