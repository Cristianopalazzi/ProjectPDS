﻿using System.Collections.ObjectModel;
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
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Threading;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //TODO cercare come mettere le immagini come risorse
        //TODO gestire errori delle socket e propagare le modifica alla GUI
        // fare prove per la dimensione dei nomi nella schermata dei file in invio e in ricezione ( maxWidth, ellipsize e tooltip)
        // resize della finestra principale
        //TODO tooltip progress bar


        public MainWindow()
        {
            InitializeComponent();
            sets = Settings.getInstance;
            gridSettings.DataContext = sets;

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

      

        private void Sender_fileRejectedGUI(Socket sender)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
           {
               foreach (SendingFile sf in FilesToSend)
               {
                   if (sf.Sock == sender)
                   {
                       sf.File_state = Constants.FILE_STATE.CANCELED;
                       sf.Ready = false;
                       sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
                       break;
                   }
               }
           }));
        }

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
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.File_state = Constants.FILE_STATE.CANCELED;
                        r.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico"));
                        triggerBalloon(r.Filename, r.Name, 2);
                        break;
                    }
            }));
        }

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

        private void updateReceivingFiles(string senderID, byte[] image, string fileName, string id)
        {
            listReceivingFiles.Dispatcher.Invoke(new Action(() =>
            {
                ReceivingFile rf = new ReceivingFile(new Neighbor(senderID, image), fileName, id);
                FilesToReceive.Add(rf);
            }));
        }


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


        private void updateProgressBar(string filename, Socket sock, int percentage)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                    if (sf.Sock == sock)
                    {
                        sf.Value = percentage;
                        if (sf.Ready)
                            sf.Ready = false;
                        if (sf.Value == 100)
                        {
                            sf.File_state = Constants.FILE_STATE.COMPLETED;
                            sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/check.ico"));
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
                sf.Ready = false;
                sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico", UriKind.RelativeOrAbsolute));
            }
        }

        private void Sender_sendingFailure(Socket sock)
        {
            sendingFiles.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FilesToSend)
                {
                    if (sf.Sock == sock)
                    {
                        sf.File_state = Constants.FILE_STATE.CANCELED;
                        sf.Ready = false;
                        sf.Pic = new BitmapImage(new Uri(App.defaultResourcesFolder + "/cross.ico", UriKind.RelativeOrAbsolute));
                    }
                }
            }));
        }



        private void receiving_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (listReceivingFiles.SelectedIndex == -1)
                return;
            ReceivingFile rf = listReceivingFiles.SelectedItem as ReceivingFile;
            if (rf.File_state == Constants.FILE_STATE.PROGRESS)
                this.ShowMessageAsync("Ops", "Attendi il completamento del file");
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
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (ReceivingFile rf in tmp)
                FilesToReceive.Remove(rf);
        }

        private void sending_files_menu_delete_click(object sender, RoutedEventArgs e)
        {
            if (sendingFiles.SelectedIndex == -1)
                return;
            SendingFile sf = sendingFiles.SelectedItem as SendingFile;
            if (sf.File_state == Constants.FILE_STATE.PROGRESS)
                this.ShowMessageAsync("Ops", "Non puoi cancellare un file in invio.\nPremi annulla per fermarlo.");
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
            if (tmp.Count == 0)
            {
                this.ShowMessageAsync("Ops", "Non ci sono file da archiviare.");
            }
            foreach (SendingFile sf in tmp)
                FilesToSend.Remove(sf);
        }


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
