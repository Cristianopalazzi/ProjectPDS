using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProjectPDS
{
    [Serializable]
    public class Settings
    {


        public Boolean DefaultDir
        {
            get { return this._defaultDir; }
            set { this._defaultDir = value; }
        }

        public Boolean AutoAccept
        {
            get { return this._autoAccept; }
            set { this._autoAccept = value; }
        }

        public string DefaultDirPath
        {
            get { return this._defaultDirPath; }
            set { this._defaultDirPath = value; }
        }

        public Boolean Online
        {
            get { return this._online; }
            set { this._online = value; }
        }

        private Settings()
        {
            DefaultDir = true;
            AutoAccept = true;
            DefaultDirPath = "";
            Online = true;
        }


        private static void getUpdatedValues()
        {
            if (File.Exists("Customer.bin"))
            {

                // If the file exists, restore the data from the file  
                using (FileStream s = new FileStream("Customer.xml", FileMode.Open)) 
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(Settings));
                    instance = (Settings) xSer.Deserialize(s);
                }
            }
        }


        public static Settings getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                    getUpdatedValues();
                }
                return instance;
            }
        }

      

        public static void writeSettings(Settings values)
        {
            using (FileStream s = new FileStream("Customer.xml", FileMode.Create))
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
