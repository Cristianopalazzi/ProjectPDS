﻿using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System;

namespace ProjectPDSWPF
{
    public class SendingFile : INotifyPropertyChanged
    {
        public SendingFile(string ipAddress, string name, string fileName, BitmapImage immagine)
        {
            IpAddr = ipAddress;
            Name = name;
            FileName = fileName;
            Immagine = immagine;
            Value = 0.0;
            Sock = new Socket(AddressFamily.InterNetwork,
              SocketType.Stream, ProtocolType.Tcp);
            File_state = Constants.FILE_STATE.PROGRESS;
        }

        public string IpAddr { get => ipAddr; set => ipAddr = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string Name { get => name; set => name = value; }
        public Socket Sock { get => sock; set => sock = value; }
        public BitmapImage Immagine { get => immagine; set => immagine = value; }
        internal Constants.FILE_STATE File_state { get => file_state; set => file_state = value; }

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

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string ipAddr, name, fileName;
        private BitmapImage immagine, pic;
        private Socket sock;
        private double value;
        private Constants.FILE_STATE file_state;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}