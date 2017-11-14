using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO cercare come mettere le immagini come risorse
        // eliminazione delle righe
        // controllare invii e ricezioni con placeholder al posto della foto utente
        // placeholder per le liste vuote
        //rifare la window per le impostazioni
        //vedere se lasciare una sola window per i contatti online o separarle
        //resize della finestra principale
        

        public static bool closedByExit = false;
        private ObservableCollection<Neighbor> neighborsValues;
        private ObservableCollection<SendingFile> filesToSend;
        private ObservableCollection<ReceivingFile> filesToReceive;

        public delegate void del(List<SendingFile> sf);
        public static event del sendSelectedNeighbors;


        private System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();

        public ObservableCollection<Neighbor> NeighborsValues { get => neighborsValues; set => neighborsValues = value; }
        public ObservableCollection<SendingFile> FilesToSend { get => filesToSend; set => filesToSend = value; }
        public ObservableCollection<ReceivingFile> FilesToReceive { get => filesToReceive; set => filesToReceive = value; }

        public MainWindow()
        {
            Closing += MainWindow_Closing;
            InitializeComponent();
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Sender.updateProgress += updateProgressBar;
            Receiver.updateProgress += updateReceivingProgressBar;
            Receiver.updateReceivingFiles += updateReceivingFiles;
            Receiver.fileCancel += file_cancel;
            MyQueue.openNeighbors += neighbor_selection;
            //Settings instance = Settings.getInstance;
            Receiver r = new Receiver();
            NeighborProtocol n = NeighborProtocol.getInstance;
            Settings s = Settings.getInstance;
            NeighborsValues = new ObservableCollection<Neighbor>();
            FilesToSend = new ObservableCollection<SendingFile>();
            FilesToReceive = new ObservableCollection<ReceivingFile>();

            MyQueue queue = new MyQueue();
            nIcon.Icon = new System.Drawing.Icon(@"C:\Users\Cristiano\Desktop\check.ico");
            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item3 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item4 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.ContextMenu cMenu = new System.Windows.Forms.ContextMenu();
            cMenu.MenuItems.Add(item1);
            cMenu.MenuItems.Add(item2);
            cMenu.MenuItems.Add(item3);
            cMenu.MenuItems.Add(item4);
            nIcon.ContextMenu = cMenu;
            item1.Text = "File in ricezione";
            item2.Text = "File in invio";
            item3.Text = "Tizi online";
            item4.Text = "Esci";
            item1.Click += delegate { Show(); WindowState = WindowState.Normal; tabControl.SelectedIndex = 0; };
            item2.Click += delegate { Show(); WindowState = WindowState.Normal; tabControl.SelectedIndex = 1; };
            item3.Click += delegate { Show(); WindowState = WindowState.Normal; tabControl.SelectedIndex = 2; };
            //item4.Click += delegate { closedByExit = true; Close(); };
            nIcon.Visible = true;
            DataContext = this;
            Neighbors.ItemsSource = NeighborsValues;
            sendingFiles.ItemsSource = FilesToSend;
            listReceivingFiles.ItemsSource = FilesToReceive;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(filesToSend);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("FileName");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void file_cancel(string id)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (ReceivingFile r in FilesToReceive)
                    if (String.Compare(r.Guid, id) == 0)
                    {
                        r.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\cross.ico"));
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
                            r.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
                            break;
                        }
                    }
                }
            }));
        }

        private void updateReceivingFiles(string senderID, byte[] image, string fileName, string id)
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
                            sf.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
                        break;
                    }
            }));
        }


        //private void Doubleanimation_Completed(object sender, EventArgs e, ProgressBar progress)
        //{
        //    if (progress.Value == 100)
        //        foreach (SendingFile sf in FileToSend)
        //            if (sf.Progress == progress)
        //            {
        //                sf.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
        //                break;
        //            }
        //}


        private void neighbor_selection(string file)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                sendingFile.Text = file;
                //TODO sistemare la scritta del file che stiamo mandando
                tabControl.SelectedIndex = 2;
                Show();
            }));

        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //if (closedByExit)
            //{
            //    e.Cancel = false;
            //    nIcon.Dispose();
            //}
            //else
            //{
            //    e.Cancel = true;
            //    WindowState = WindowState.Minimized;
            //    Hide();
            //}
            nIcon.Dispose();
            Environment.Exit(0);
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
                b.Visibility = Visibility.Hidden;
                sf.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\cross.ico"));
            }
        }


        //private void Menu_delete_click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show(NeighborsValues.Count + "");
        //    if (Persone.SelectedIndex == -1)
        //        return;
        //    people.Remove(Persone.SelectedItem as test);
        //}


        //private void Menu_modify_click(object sender, RoutedEventArgs e)
        //{
        //    if (Persone.SelectedIndex == -1)
        //        return;
        //    test tmp = Persone.SelectedItem as test;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        double t = tmp.Prog.Value += 1;
        //        Duration duration = new Duration(TimeSpan.FromSeconds(0.5));
        //        DoubleAnimation doubleanimation = new DoubleAnimation(t, duration);
        //        doubleanimation.Completed += delegate (object sender1, EventArgs e1)
        //        {
        //            Doubleanimation_Completed(sender1, e1, tmp);
        //        };
        //        tmp.Prog.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
        //    }
        //}

        //private void Doubleanimation_Completed(object sender, EventArgs e, test tmp)
        //{
        //    if (tmp.Prog.Value == 100)
        //        tmp.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\cross.ico"));
        //}



        private void button_send_files(object sender, RoutedEventArgs e)
        {
            string file = sendingFile.Text;
            List<Neighbor> selected = null;
            List<SendingFile> sendingFiles = null;
            if (Neighbors.SelectedItems.Count > 0)
            {
                sendingFiles = new List<SendingFile>();
                selected = Neighbors.SelectedItems.Cast<Neighbor>().ToList();
                foreach (Neighbor n in selected)
                {
                    SendingFile sf = new SendingFile(n.NeighborIp, n.NeighborName, file, n.NeighborImage);
                    sendingFiles.Add(sf);
                }
                sendSelectedNeighbors(sendingFiles);
                foreach (SendingFile sf in sendingFiles)
                    FilesToSend.Add(sf);
                tabControl.SelectedIndex = 1;
            }
            else MessageBox.Show("Seleziona almeno un vicino");
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
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Neighbor n1 = new Neighbor(id, bytes);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                    neighborsValues.Add(n1);
                }));
        }
    }
}