using System;

namespace EasyShare
{
    class ZipInfo
    {
        private string zipToSend, zipLocation;
        private long zipLength;
        private bool isFile;
        private DateTime lastWrite;

        public ZipInfo(string zipToSend, string zipLocation, long zipLength, bool isFile, DateTime lastWrite)
        {
            ZipToSend = zipToSend;
            ZipLocation = zipLocation;
            ZipLength = zipLength;
            IsFile = isFile;
            LastWrite = lastWrite;
        }

        public string ZipToSend { get => zipToSend; set => zipToSend = value; }
        public string ZipLocation { get => zipLocation; set => zipLocation = value; }
        public long ZipLength { get => zipLength; set => zipLength = value; }
        public bool IsFile { get => isFile; set => isFile = value; }
        public DateTime LastWrite { get => lastWrite; set => lastWrite = value; }
    }
}