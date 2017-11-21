using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO.Compression;
using System.Diagnostics;

namespace ProjectPDSWPF
{
    class Sender
    {
        public Sender() { }
        public void sendFile(string ipAddr, string pathFile, Socket sender)
        {
            int idx = pathFile.LastIndexOf('\\');

            string fileName = Path.GetFileName(pathFile);
            string path = pathFile.Substring(0, idx + 1);

            byte[] command;

            byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
            long fileLength = 0;

            string zipToSend = RandomStr() + Constants.ZIP_EXTENSION;
            FileAttributes attr = File.GetAttributes(pathFile);
            string zipLocation = App.defaultFolder + "\\" + zipToSend;

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                command = Encoding.ASCII.GetBytes(Constants.DIR_COMMAND);
                DirectoryInfo dInfo = new DirectoryInfo(pathFile);
                fileLength = DirSize(dInfo);

                //zippo cartella
                ZipFile.CreateFromDirectory(pathFile, zipLocation, CompressionLevel.NoCompression, false);
            }
            else
            {
                //creo comando da mandare
                command = Encoding.ASCII.GetBytes(Constants.FILE_COMMAND);

                //calcolo grandezza file
                fileLength = new FileInfo(pathFile).Length;
                ZipArchive newFile = ZipFile.Open(zipLocation, ZipArchiveMode.Create);
                newFile.CreateEntryFromFile(pathFile, fileName, CompressionLevel.NoCompression);
                newFile.Dispose();
            }

            //preparo comando + lunghezza nome file
            byte[] request = command.Concat(fileNameLength).ToArray();
            long zipLength = new FileInfo(zipLocation).Length;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddr), Constants.PORT_TCP);
            sender.Connect(remoteEP);

            //mando comando + lunghezza nome file
            int sent = sender.Send(request, request.Length, SocketFlags.None);

            //preparo filename + lunghezza file
            byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
            byte[] fileLengthByte = BitConverter.GetBytes(fileLength);
            byte[] fileNameAndLength = fileNameByte.Concat(fileLengthByte).ToArray();
            //mando filename + lunghezza file
            sent = sender.Send(fileNameAndLength, fileNameAndLength.Length, SocketFlags.None);


            byte[] responseFromServer = new byte[Constants.ACCEPT_FILE.Length];
            sender.Receive(responseFromServer, responseFromServer.Length, SocketFlags.None);
            string response = Encoding.ASCII.GetString(responseFromServer);

            if (String.Compare(response, Constants.DECLINE_FILE) == 0)
            {
                fileRejected(fileName, NeighborProtocol.getInstance.getUserFromIp(ipAddr), 3);
                fileRejectedGUI(sender);
                File.Delete(zipLocation);
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                return;
            }


            //preparo zip command + zip file neighborName length
            byte[] zipCommand = Encoding.ASCII.GetBytes(Constants.ZIP_COMMAND);
            byte[] zipAndFileNameLength = zipCommand.Concat(BitConverter.GetBytes(zipToSend.Length)).ToArray();

            //mando zip command + zip file neighborName length
            sent = sender.Send(zipAndFileNameLength, zipAndFileNameLength.Length, SocketFlags.None);

            //preparo zip file neighborName + lunghezza file zip
            byte[] zipFileName = Encoding.ASCII.GetBytes(zipToSend);
            byte[] zipFileLength = BitConverter.GetBytes(zipLength);
            byte[] tot = zipFileName.Concat(zipFileLength).ToArray();

            //mando zip file neighborName + lunghezza file zip
            sent = sender.Send(tot, tot.Length, SocketFlags.None);

            int temp = 0, percentage = 0;
            SocketError error;

            FileStream fs = new FileStream(zipLocation, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[1400];
            int readBytes = 0;

            while (temp < zipLength)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (zipLength - temp > 1400)
                {
                    readBytes = fs.Read(data, 0, 1400);
                }
                else
                {
                    readBytes = fs.Read(data, 0, (int)zipLength - temp);
                }

                sent = sender.Send(data, 0, readBytes, SocketFlags.None, out error);

                if (error == SocketError.Success)
                {
                    temp += sent;
                    ulong temporary = (ulong)temp * 100;
                    int tempPercentage = (int)(temporary / (ulong)zipLength);
                    if (tempPercentage > percentage)
                    {
                        updateProgress(fileName, sender, tempPercentage);
                        percentage = tempPercentage;

                        timer.Stop();
                        long ticks = timer.ElapsedTicks;


                        if (milliSeconds == 0)
                            milliSeconds = (decimal)(ticks * 1000) / (decimal)Stopwatch.Frequency;

                        decimal transferRate = (decimal)(sent * 1000) / ((decimal)milliSeconds);


                        //TODO provare con countdown 
                        decimal remainingTime = (zipLength - temp) / transferRate;
                        TimeSpan t = TimeSpan.FromSeconds(Convert.ToDouble(remainingTime));

                        string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds);
                        updateRemainingTime(sender, answer);
                    }
                }
                else
                {
                    //TODO cose
                    Console.WriteLine("****** DEBUG ******* THREAD SHUTDOWN ********");
                    fs.Close();
                    if (File.Exists(zipLocation))
                        File.Delete(zipLocation);
                    sender.Close();
                    return;
                }
                if (temp == zipLength) break;
            }

            fs.Close();
            //cancello zip temporaneo
            if (File.Exists(zipLocation))
                File.Delete(zipLocation);
            // Release the socket.
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public static string RandomStr()
        {
            string rStr = Path.GetRandomFileName();
            rStr = rStr.Replace(".", "");
            return rStr;
        }


        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
                size += fi.Length;

            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
                size += DirSize(di);
            return size;
        }


        public delegate void myDelegate(string filename, Socket sock, int percentage);
        public static event myDelegate updateProgress;

        public delegate void myDelegate1(string fileName, string username, int type);
        public static event myDelegate1 fileRejected;

        public delegate void myDelegate2(Socket sender);
        public static event myDelegate2 fileRejectedGUI;


        public delegate void myDelegate3(Socket sock, string remainingTime);
        public static event myDelegate3 updateRemainingTime;

        private decimal milliSeconds = 0;
    }

}