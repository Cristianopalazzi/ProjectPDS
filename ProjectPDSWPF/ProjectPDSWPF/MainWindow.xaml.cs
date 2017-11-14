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

namespace ProjectPDSWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static bool closedByExit = false;
        private ObservableCollection<test> people;
        private ObservableCollection<Neighbor> neighborsValues;
        private ObservableCollection<SendingFile> fileToSend;

        public delegate void del(List<SendingFile> sf);
        public static event del sendSelectedNeighbors;

        private System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();

        public ObservableCollection<test> People { get => people; set => people = value; }
        public ObservableCollection<Neighbor> NeighborsValues { get => neighborsValues; set => neighborsValues = value; }
        public ObservableCollection<SendingFile> FileToSend { get => fileToSend; set => fileToSend = value; }

        public MainWindow()
        {
            Closing += MainWindow_Closing;
            InitializeComponent();
            NeighborProtocol.neighborsEvent += modify_neighbors;
            Sender.updateProgress += updateProgressBar;
            MyQueue.openNeighbors += neighbor_selection;
            //Settings instance = Settings.getInstance;
            //Receiver r = new Receiver();
            NeighborProtocol n = NeighborProtocol.getInstance;
            Settings s = Settings.getInstance;
            NeighborsValues = new ObservableCollection<Neighbor>();
            FileToSend = new ObservableCollection<SendingFile>();


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
            people = new ObservableCollection<test>();
            DataContext = this;
            test t = new test();
            test t1 = new test();
            test t2 = new test();
            t.Name = "Cristiano";
            t.Surname = "Palazzi";
            t.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
            t.Prog.Value = 0;
            t1.Name = "Gianmaria";
            t1.Surname = "Tremigliozzi";
            t1.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
            t1.Prog.Value = 0;
            t2.Name = "Antonella";
            t2.Surname = "Palazzi";
            t2.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
            t2.Prog.Value = 0;
            people.Add(t);
            people.Add(t1);
            people.Add(t2);
            Neighbors.ItemsSource = neighborsValues;
            sendingFiles.ItemsSource = FileToSend;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(fileToSend);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("FileName");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void updateProgressBar(string filename, System.Net.Sockets.Socket sock, int percentage)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (SendingFile sf in FileToSend)
                {
                    if (sf.Sock == sock)
                        sf.Value = percentage;
                    if (sf.Value == 100)
                        sf.Pic = new BitmapImage(new Uri(@"C:\Users\Cristiano\Desktop\check.ico"));
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
            test y = b.DataContext as test;
            people.Remove(y);
            //var item = (sender as FrameworkElement).DataContext;
            //int index = Persone.Items.IndexOf(item);
            //test t = people.ElementAt(index);
            //MessageBox.Show(""+t.Name);
        }


        private void Menu_delete_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(NeighborsValues.Count + "");
            if (Persone.SelectedIndex == -1)
                return;
            people.Remove(Persone.SelectedItem as test);
        }


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
                    FileToSend.Add(sf);
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
                    Neighbor n1 = new Neighbor(id, bytes); neighborsValues.Add(n1);
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

    public class test : INotifyPropertyChanged
    {
        public test()
        {
            prog = new ProgressBar();
        }
        private string name, surname;
        private BitmapImage pic;
        private ProgressBar prog;


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string Surname
        {
            get => surname;
            set
            {
                if (surname != value)
                {
                    surname = value;
                    NotifyPropertyChanged("Surname");
                }
            }
        }

        public BitmapImage Pic
        {
            get { return pic; }
            set
            {
                pic = value;
                NotifyPropertyChanged("Pic");
            }
        }

        public ProgressBar Prog
        {
            get => prog;
            set
            {
                if (prog.Value != value.Value)
                {
                    prog.Value = value.Value;
                    NotifyPropertyChanged("Prog.Value");
                }
            }
        }
    }
}