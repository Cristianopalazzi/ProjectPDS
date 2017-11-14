using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;

namespace ProjectPDSWPF
{
    public class Work : INotifyPropertyChanged
    {


        //public Work(string fileName, ArrayList receivers)
        //{
        //    this.fileName = fileName;
        //    sendingFiles = new ArrayList();
        //    foreach (string variable in receivers)
        //    {
        //        int index = variable.LastIndexOf("@");
        //        string name = variable.Substring(0, index);
        //        string ipaddr = variable.Substring(index + 1);
        //        SendingFile sf = new SendingFile(ipaddr, name);
        //        sendingFiles.Add(sf);
        //    }
        //}


        public Work(string fileName, List<Neighbor> neighbors)
        {
            this.fileName = fileName;
            sendingFiles = new ArrayList();
            foreach (Neighbor n in neighbors)
            {
                string name = n.NeighborName;
                string ipaddr = n.NeighborIp;
                SendingFile sf = new SendingFile(ipaddr, name, fileName, n.NeighborImage);
                sendingFiles.Add(sf);
            }
        }

        public string FileName
        {
            get => fileName;
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }

        }
        public ArrayList SendingFiles
        {
            get => sendingFiles; set
            {
                if (sendingFiles != value)
                {
                    sendingFiles = value;
                    NotifyPropertyChanged("SendingFiles");
                }
            }
        }


        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string fileName;
        private ArrayList sendingFiles;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SendingFile : INotifyPropertyChanged
    {
        public SendingFile(string ipAddr, string name, string fileName, BitmapImage immagine)
        {
            IpAddr = ipAddr;
            Name = name;
            FileName = fileName;
            Immagine = immagine;

            progress = new ProgressBar();

            Sock = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream, ProtocolType.Tcp);
        }

        public string IpAddr { get => ipAddr; set => ipAddr = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string Name { get => name; set => name = value; }

        public ProgressBar Progress
        {
            get => progress;
            set
            {
                if (progress.Value != value.Value)
                {
                    progress.Value = value.Value;
                    NotifyPropertyChanged("Progress.Value");
                }
            }
        }


        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        public Socket Sock { get => sock; set => sock = value; }
        public BitmapImage Immagine { get => immagine; set => immagine = value; }


        private string ipAddr, name, fileName;
        private ProgressBar progress;
        private BitmapImage immagine;
        private Socket sock;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}