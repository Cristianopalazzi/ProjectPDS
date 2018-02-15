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
using System.Linq;
using System.Diagnostics;

namespace EasyShare
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            sets = Settings.GetInstance;
            gridSettings.DataContext = sets;
            Deactivated += MainWindow_Deactivated;
            Closing += MainWindow_Closing;

            Sender.UpdateProgress += UpdateProgressBar;
            Sender.UpdateFileState += Sender_updateFileState;
            Receiver.UpdateProgress += UpdateReceivingProgressBar;
            Receiver.UpdateReceivingFiles += UpdateReceivingFiles;
            Receiver.FileCancel += File_cancel;
            Receiver.Acceptance += File_to_accept;
            UserSettings.OpenTabSettings += TabChange;
            NeighborSelection.SendSelectedNeighbors += AddSendingFiles;
            Queue.QueueUpdateState += Sender_updateFileState;
            App.AskForExit += App_askForExit;

            FilesToSend = new ObservableCollection<SendingFile>();
            FilesToReceive = new ObservableCollection<ReceivingFile>();
            DataContext = this;
            Neighbors.ItemsSource = NeighborProtocol.GetInstance.Neighbors;
            sendingFiles.ItemsSource = FilesToSend;
            listReceivingFiles.ItemsSource = FilesToReceive;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(filesToSend);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("FileName");
            view.GroupDescriptions.Add(groupDescription);
            infoImage.Source = new BitmapImage(new Uri(App.currentDirectoryResources + "/info.ico"));
        }

        //lista dei file in attesa di essere accettati o meno
        private void File_to_accept(string userName, string fileName, string dimension, string id)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Acceptance acceptance = null;
                acceptance = Application.Current.Windows.OfType<Acceptance>().SingleOrDefault(w => w.IsActive);
                if (acceptance == null)
                {
                    acceptance = new Acceptance();
                    acceptance.Topmost = true;
                    acceptance.Topmost = false;
                    acceptance.Activate();
                    acceptance.WindowState = WindowState.Normal;
                    acceptance.Show();
                }
                acceptance.AcceptingFiles.Add(new Acceptance.FileToAccept(fileName, userName, dimension, id));
            }));
        }

        private void Sender_updateFileState(Socket sock, Constants.FILE_STATE state)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                {
                    if (sf.Sock == sock)
                    {
                        sf.File_state = state;
                        if (state == Constants.FILE_STATE.CANCELED || state == Constants.FILE_STATE.ERROR || state == Constants.FILE_STATE.REJECTED)
                            sf.Pic = new BitmapImage(new Uri(App.currentDirectoryResources + "/cross.ico"));
                    }
                }
            }));
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            Topmost = false;
        }

        //mostro la tab delle impostazioni dopo aver cliccato su "impostazioni"
        private void TabChange()
        {
            Show();
            WindowState = WindowState.Normal;
            tabControl.SelectedIndex = 3;
        }

        //aggiungo file da inviare alla lista dopo aver cliccato invia nella finestra di selezione dei vicini
        private void AddSendingFiles(List<SendingFile> sf)
        {
            foreach (SendingFile s in sf)
                FilesToSend.Add(s);
        }

        //file annullato da parte del sender => aggiorno la lista dei file in ricezione
        private void File_cancel(string id, Constants.NOTIFICATION_STATE state)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.File_state = Constants.FILE_STATE.CANCELED;
                        r.Pic = new BitmapImage(new Uri(App.currentDirectoryResources + "/cross.ico"));
                        TriggerBalloon?.Invoke(r.Filename, r.Name, state);
                        break;
                    }
            }));
        }


        // aggiungo un nuovo file in ricezione alla lista
        private void UpdateReceivingFiles(ReceivingFile file)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                FilesToReceive.Add(file);
            }));
        }

        //aggiorno la progress bar del file in invio
        private void UpdateProgressBar(string filename, Socket sock, int percentage, string remainingTime)
        {
            try
            {
                sendingFiles.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (SendingFile sf in FilesToSend)
                        if (sf.Sock == sock)
                        {
                            if (!String.IsNullOrEmpty(remainingTime))
                                if (String.Compare(sf.RemainingTime, remainingTime) != 0)
                                    sf.RemainingTime = remainingTime;
                            sf.Value = percentage;
                            sf.File_state = Constants.FILE_STATE.PROGRESS;
                            if (sf.Value == 100)
                            {
                                sf.File_state = Constants.FILE_STATE.COMPLETED;
                                sf.Pic = new BitmapImage(new Uri(App.currentDirectoryResources + "/check.ico"));
                                if (WindowState != WindowState.Normal || tabControl.SelectedIndex != 1)
                                    TriggerBalloon?.Invoke(sf.FileName, sf.Name, Constants.NOTIFICATION_STATE.SENT); //1
                            }
                            break;
                        }

                }));
            }
            catch (Exception e)
            {
                Console.WriteLine("Update Progress Bar SENDER");
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }

        }

        //aggiorno la progress bar del file in ricezione
        private void UpdateReceivingProgressBar(string id, int percentage)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    foreach (ReceivingFile r in FilesToReceive)
                    {
                        if (String.Compare(r.Guid, id) == 0)
                        {
                            r.Value = percentage;
                            if (r.Value == 100)
                            {
                                r.File_state = Constants.FILE_STATE.COMPLETED;
                                r.Pic = new BitmapImage(new Uri(App.currentDirectoryResources + "/check.ico"));
                                TriggerBalloon?.Invoke(r.Filename, r.Name, 0);
                                break;
                            }
                        }
                    }
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine("Update Progress Bar RECEIVER");
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
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
            catch (ObjectDisposedException o)
            {
                Console.WriteLine("main");
                var st = new StackTrace(o, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MainWindwos");
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
            finally
            {
                sf.File_state = Constants.FILE_STATE.CANCELED;
                sf.Pic = new BitmapImage(new Uri(App.currentDirectoryResources + "/cross.ico"));
            }
        }

        //context menu sui file in ricezione (cancella un file ricevuto correttamente)
        private void Receiving_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            ReceivingFile rf = listReceivingFiles.SelectedItem as ReceivingFile;
            if (rf.File_state == Constants.FILE_STATE.PROGRESS)
                this.ShowMessageAsync("Ops", "Attendi il completamento del file");
            else FilesToReceive.Remove(rf);
        }

        //context menu sui file in ricezione (cancella tutti i file ricevuti correttamente)
        private void Receiving_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            List<ReceivingFile> tmp = new List<ReceivingFile>();
            foreach (ReceivingFile rf in FilesToReceive)
                if (rf.File_state == Constants.FILE_STATE.ERROR || rf.File_state == Constants.FILE_STATE.COMPLETED || rf.File_state == Constants.FILE_STATE.CANCELED)
                    tmp.Add(rf);
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (ReceivingFile rf in tmp)
                FilesToReceive.Remove(rf);
        }

        //context menu sui file in invio (cancella un file inviato correttamente)
        private void Sending_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            SendingFile sf = sendingFiles.SelectedItem as SendingFile;
            if (sf.File_state == Constants.FILE_STATE.PROGRESS || sf.File_state == Constants.FILE_STATE.ACCEPTANCE || sf.File_state == Constants.FILE_STATE.PREPARATION)
                this.ShowMessageAsync("Ops", "Non puoi cancellare un file in invio.\nPremi annulla per fermarlo.");
            else FilesToSend.Remove(sf);
        }

        //context menu sui file in invio (cancella tutti i file inviati correttamente)
        private void Sending_files_menu_all_delete_click(object sender, RoutedEventArgs e)
        {
            List<SendingFile> tmp = new List<SendingFile>();
            foreach (SendingFile sf in FilesToSend)
                if (sf.File_state == Constants.FILE_STATE.CANCELED || sf.File_state == Constants.FILE_STATE.COMPLETED || sf.File_state == Constants.FILE_STATE.ERROR || sf.File_state == Constants.FILE_STATE.REJECTED)
                    tmp.Add(sf);
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (SendingFile sf in tmp)
                FilesToSend.Remove(sf);
        }

        //mostro il dialog per scegliere la cartella in cui salvare il file in ricezione
        public void OpenFolderBrowserDialog(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.Cancel)
                    sets.DefaultDirPath = dialog.SelectedPath;
            }
        }


        //scrivo su file i settings ogni volta che cambio tab (rispetto a quella delle impostazioni)
        public void TabChanged(object Sender, EventArgs e)
        {
            Settings.WriteSettings(sets);
        }

        public bool CheckForFilesInProgress()
        {
            foreach (SendingFile sf in FilesToSend)
                if (sf.File_state == Constants.FILE_STATE.ACCEPTANCE || sf.File_state == Constants.FILE_STATE.PREPARATION || sf.File_state == Constants.FILE_STATE.PROGRESS)
                    return true;
            foreach (ReceivingFile rf in FilesToReceive)
                if (rf.File_state == Constants.FILE_STATE.ACCEPTANCE || rf.File_state == Constants.FILE_STATE.PREPARATION || rf.File_state == Constants.FILE_STATE.PROGRESS)
                    return true;
            return false;
        }

        private bool App_askForExit()
        {
            MessageDialogResult result = MessageDialogResult.Negative;
            MessageDialogStyle style = MessageDialogStyle.AffirmativeAndNegative;
            Show();
            result = this.ShowModalMessageExternal("Il trasferimento di alcuni file non è completato", "Vuoi davvero uscire?", style);
            if (result == MessageDialogResult.Affirmative)
                return true;
            return false;
        }

        private ObservableCollection<SendingFile> filesToSend;
        private ObservableCollection<ReceivingFile> filesToReceive;
        private Settings sets;

        public ObservableCollection<SendingFile> FilesToSend { get => filesToSend; set => filesToSend = value; }
        public ObservableCollection<ReceivingFile> FilesToReceive { get => filesToReceive; set => filesToReceive = value; }

        public delegate void myDelegate(string filename, string username, Constants.NOTIFICATION_STATE state);
        public static event myDelegate TriggerBalloon;
    }
}