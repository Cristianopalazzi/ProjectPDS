using System.ComponentModel;
using System.Windows.Media.Imaging;


namespace EasyShare
{
    public class Neighbor : INotifyPropertyChanged
    {
        public Neighbor(string neighorID, byte[] bytes)
        {
            int temp = neighorID.LastIndexOf("@");
            NeighborName = neighorID.Substring(0, temp);
            NeighborIp = neighorID.Substring(temp + 1);
            Counter = 0;
            neighborImage = null;
            if (bytes != null)
                setImage(bytes);
        }

        public static BitmapImage ToImage(byte[] array)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new System.IO.MemoryStream(array);
            image.EndInit();
            return image;
        }

        public string NeighborName
        {
            get => neighborName;
            set
            {
                if (neighborName != value)
                {
                    neighborName = value;
                    NotifyPropertyChanged("NeighborName");
                }
            }
        }


        public BitmapImage NeighborImage
        {
            get => neighborImage;
        }

        public void setImage(byte[] bytes)
        {
            BitmapImage bitmap = ToImage(bytes);
            if (NeighborImage != bitmap)
            {
                neighborImage = bitmap;
                NotifyPropertyChanged("NeighborImage");
            }
        }

        public int Counter { get => counter; set => counter = value; }
        public string NeighborIp { get => neighborIp; set => neighborIp = value; }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        private string neighborName, neighborIp;
        private BitmapImage neighborImage;
        private int counter;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}