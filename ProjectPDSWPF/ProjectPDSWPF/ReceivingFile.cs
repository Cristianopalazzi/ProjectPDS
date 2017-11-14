using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System;


namespace ProjectPDSWPF
{
    public class ReceivingFile : INotifyPropertyChanged
    {
        public ReceivingFile(Neighbor neighbor, string filename, string guid)
        {
            Name = neighbor.NeighborName;
            Filename = filename;
            Ipaddr = neighbor.NeighborIp;
            Image = new BitmapImage();
            Image = neighbor.NeighborImage;
            Guid = guid;
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

        public BitmapImage Image { get => image; set => image = value; }
        public string Name { get => name; set => name = value; }
        public string Filename { get => filename; set => filename = value; }
        public string Ipaddr { get => ipaddr; set => ipaddr = value; }
        public string Guid { get => guid; set => guid = value; }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        private string name, filename, ipaddr;
        private double value;
        private BitmapImage image, pic;
        private string guid;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}