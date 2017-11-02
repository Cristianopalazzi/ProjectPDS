using System;
using System.IO;
using System.Xml.Serialization;

namespace ProjectPDS
{
    [Serializable]
    public class Settings
    {
        public Boolean DefaultDir
        {
            get { return _defaultDir; }
            set { _defaultDir = value; }
        }

        public Boolean AutoAccept
        {
            get { return _autoAccept; }
            set { _autoAccept = value; }
        }

        public string DefaultDirPath
        {
            get { return _defaultDirPath; }
            set { _defaultDirPath = value; }
        }

        public Boolean Online
        {
            get { return _online; }
            set
            {
                _online = value;
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

        private static Settings instance = null;
        private Boolean _online;
        private Boolean _defaultDir;
        private Boolean _autoAccept;
        private string _defaultDirPath;
    }
}