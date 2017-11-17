using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ProjectPDSWPF
{
    [Serializable]
    public class Settings :INotifyPropertyChanged
    {
        public Boolean DefaultDir
        {
            get { return _defaultDir; }
            set { _defaultDir = value; NotifyPropertyChanged("DefaultDir"); }
        }

      

        public Boolean AutoAccept
        {
            get { return _autoAccept; }
            set { _autoAccept = value; NotifyPropertyChanged("AutoAccept"); }
        }

        public string DefaultDirPath
        {
            get { return _defaultDirPath; }
            set { _defaultDirPath = value; NotifyPropertyChanged("DefaultDirPath"); }
        }

        public Boolean Online
        {
            get { return _online; }
            set
            {
                _online = value;
                NotifyPropertyChanged("Online");
            }
        }

        private Settings()
        {
            DefaultDir = false;
            AutoAccept = false;
            DefaultDirPath = String.Empty;
            Online = true;
        }

        public static Settings getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                    readSettings();
                }
                return instance;
            }
        }


        private static void readSettings()
        {
            if (File.Exists(Constants.SETTINGS))
            {
                using (FileStream s = new FileStream(Constants.SETTINGS, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(Settings));
                    instance = (Settings)xSer.Deserialize(s);
                    s.Dispose();
                    s.Close();
                }
            }
        }


        public static void writeSettings(Settings values)
        {
            using (FileStream s = new FileStream(Constants.SETTINGS, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(Settings));
                xSer.Serialize(s, values);
                s.Dispose();
                s.Close();
            }
        }


        private void NotifyPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        private static Settings instance = null;
        private Boolean _online;
        private Boolean _defaultDir;
        private Boolean _autoAccept;
        private string _defaultDirPath;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}