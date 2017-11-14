using System.ComponentModel;
using System.Windows.Media.Imaging;


namespace ProjectPDSWPF
{
    public class Neighbor : INotifyPropertyChanged
    {
        public Neighbor(string neighorID, byte[] image)
        {
            int temp = neighorID.LastIndexOf("@");
            NeighborName = neighorID.Substring(0, temp);
            NeighborIp = neighorID.Substring(temp + 1);
            if (image != null)
                NeighborImage = ToImage(image);
        }

        private BitmapImage ToImage(byte[] array)
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

        public string NeighborIp { get => neighborIp; set => neighborIp = value; }
        public BitmapImage NeighborImage
        {
            get => neighborImage; set
            {
                if (neighborImage != value)
                {
                    neighborImage = value;
                    NotifyPropertyChanged("NeighborImage");
                }
            }
        }


        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        private string neighborName, neighborIp;
        private BitmapImage neighborImage;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
