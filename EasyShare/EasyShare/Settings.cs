﻿using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace EasyShare
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
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

        public bool AutoRename
        {
            get { return _autoRename; }
            set { _autoRename = value; NotifyPropertyChanged("AutoRename"); }
        }

        public string DefaultDirPath
        {
            get { return _defaultDirPath; }
            set { _defaultDirPath = value; NotifyPropertyChanged("DefaultDirPath"); }
        }

        public bool EnableNotification
        {
            get => _enableNotification;
            set { _enableNotification = value; NotifyPropertyChanged("EnableNotification"); }
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
            AutoRename = false;
            EnableNotification = true;
        }

        public static Settings GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                        {
                            instance = new Settings();
                            ReadSettings();
                        }
                    }
                }
                return instance;
            }
        }



        private static void ReadSettings()
        {
            if (File.Exists(App.defaultResourcesFolder + "\\" + Constants.SETTINGS))
            {
                using (FileStream s = new FileStream(App.defaultResourcesFolder + "\\" + Constants.SETTINGS, FileMode.Open))
                {
                    try
                    {
                        XmlSerializer xSer = new XmlSerializer(typeof(Settings));
                        instance = (Settings)xSer.Deserialize(s);
                        s.Dispose();
                        s.Close();
                    }
                    catch
                    {
                        s.Dispose();
                        s.Close();
                        WriteSettings(instance);
                    }
                }
            }
        }


        public static void WriteSettings(Settings values)
        {
            using (FileStream s = new FileStream(App.defaultResourcesFolder + "\\" + Constants.SETTINGS, FileMode.Create, FileAccess.Write, FileShare.None, 1024, FileOptions.WriteThrough))
            {
                try
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(Settings));
                    xSer.Serialize(s, values);
                }
                catch
                {
                    s.Dispose();
                    s.Close();
                    File.Delete(App.defaultResourcesFolder + "\\" + Constants.SETTINGS);
                }
                finally
                {
                    if (s != null)
                    {
                        s.Dispose();
                        s.Close();
                    }
                }
            }
        }


        private void NotifyPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        private static Settings instance = null;
        private Boolean _online, _defaultDir, _autoAccept, _autoRename, _enableNotification;
        private string _defaultDirPath;
        private static object syncLock = new object();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}