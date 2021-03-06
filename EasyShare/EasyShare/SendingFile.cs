﻿using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Net.Sockets;

namespace EasyShare
{
    public class SendingFile : INotifyPropertyChanged
    {
        public SendingFile(Neighbor neighbor, string fileName)
        {
            IpAddr = neighbor.NeighborIp;
            Name = neighbor.NeighborName;
            FileName = fileName;
            Immagine = neighbor.NeighborImage;
            Immagine.Freeze();
            Value = 0.0;
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            File_state = Constants.FILE_STATE.PREPARATION;
        }

        public string IpAddr { get => ipAddr; set => ipAddr = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string Name { get => name; set => name = value; }
        public Socket Sock { get => sock; set => sock = value; }
        public BitmapImage Immagine { get => immagine; set => immagine = value; }

        public Constants.FILE_STATE File_state
        {
            get => file_state;
            set
            {
                file_state = value;
                NotifyPropertyChanged("File_state");
            }
        }

        public BitmapImage Pic
        {
            get => pic;
            set
            {
                pic = value;
                NotifyPropertyChanged("Pic");
            }
        }

        public double Value
        {
            get => value;
            set
            {
                this.value = value;
                NotifyPropertyChanged("Value");
            }
        }

        public string RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                NotifyPropertyChanged("RemainingTime");
            }
        }

       

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string ipAddr, name, fileName, remainingTime;
        private BitmapImage immagine, pic;
        private Socket sock;
        private double value;
        private Constants.FILE_STATE file_state;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}