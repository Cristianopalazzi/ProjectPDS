using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO.Compression;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyShare
{
    class Sender
    {
        public void sendFile(string ipAddr, string pathFile, Socket sender)
        {
            sender.SendTimeout = 2500;
            sender.ReceiveTimeout = 1000 * 5 * 60;
            string fileName = Path.GetFileName(pathFile);
            byte[] fileNameByte = Encoding.UTF8.GetBytes(fileName);
            byte[] fileNameLength = BitConverter.GetBytes(fileNameByte.Length);
            long fileLength = 0;

            byte[] command = new byte[Constants.FILE_COMMAND.Length];

            string zipToSend = RandomStr() + Constants.ZIP_EXTENSION;
            FileAttributes attr = File.GetAttributes(pathFile);
            string zipLocation = App.defaultFolder + "\\" + zipToSend;

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                command = Encoding.ASCII.GetBytes(Constants.DIR_COMMAND);
                DirectoryInfo dInfo = new DirectoryInfo(pathFile);
                fileLength = DirSize(dInfo);
                ZipFile.CreateFromDirectory(pathFile, zipLocation, CompressionLevel.NoCompression, false);
            }
            else
            {
                command = Encoding.ASCII.GetBytes(Constants.FILE_COMMAND);

                fileLength = new FileInfo(pathFile).Length;
                ZipArchive newFile = ZipFile.Open(zipLocation, ZipArchiveMode.Create);
                newFile.CreateEntryFromFile(pathFile, fileName, CompressionLevel.NoCompression);
                newFile.Dispose();
            }

            long zipLength = new FileInfo(zipLocation).Length;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddr), Constants.PORT_TCP);
            FileStream fs = null;
            try
            {
                sender.Connect(remoteEP);
                SocketError sockError;
                int sent = 0;

                byte[] fileLine = Combine(command, fileNameLength, fileNameByte, BitConverter.GetBytes(fileLength));
                sent = sender.Send(fileLine, 0, fileLine.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }
                updateFileState(sender, Constants.FILE_STATE.ACCEPTANCE);
                byte[] responseFromServer = new byte[Constants.ACCEPT_FILE.Length];
                sender.Receive(responseFromServer, 0, responseFromServer.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                string response = Encoding.ASCII.GetString(responseFromServer);
                if (String.Compare(response, Constants.DECLINE_FILE) == 0)
                {
                    fileRejected(fileName, NeighborProtocol.getInstance.getUserFromIp(ipAddr), Constants.NOTIFICATION_STATE.REFUSED);
                    updateFileState(sender, Constants.FILE_STATE.REJECTED);
                    if (File.Exists(zipLocation))
                        File.Delete(zipLocation);
                    releaseResources(sender);
                    return;
                }

                byte[] zipLine = Combine(Encoding.ASCII.GetBytes(Constants.ZIP_COMMAND), BitConverter.GetBytes(zipToSend.Length),
                    Encoding.ASCII.GetBytes(zipToSend), BitConverter.GetBytes(zipLength));
                sent = sender.Send(zipLine, 0, zipLine.Length, SocketFlags.None, out sockError);


                int temp = 0, percentage = 0;
                fs = new FileStream(zipLocation, FileMode.Open, FileAccess.Read);

                byte[] data = new byte[Constants.PACKET_SIZE];
                int readBytes = 0;
                DateTime now = DateTime.Now;
                int inviati = 0;
                List<double> transferRatesList = new List<double>();

                while (temp < zipLength)
                {
                    if (zipLength - temp > Constants.PACKET_SIZE)
                        readBytes = fs.Read(data, 0, Constants.PACKET_SIZE);
                    else
                        readBytes = fs.Read(data, 0, (int)zipLength - temp);

                    sent = sender.Send(data, 0, readBytes, SocketFlags.None, out sockError);

                    if (sockError == SocketError.Success)
                    {
                        temp += sent;
                        inviati += sent;
                        ulong temporary = (ulong)temp * 100;
                        int tempPercentage = (int)(temporary / (ulong)zipLength);
                        if (tempPercentage > percentage)
                        {
                            string remainingTimeString = null;
                            var elapsedSeconds = (DateTime.Now - now).TotalSeconds;
                            if (elapsedSeconds >= 1)
                            {
                                var transferRate = inviati / elapsedSeconds;
                                transferRatesList.Add(transferRate);
                                if (transferRatesList.Count == 6)
                                    transferRatesList.RemoveAt(0);
                                double avg = transferRatesList.Average();

                                var remainingTime = (zipLength - temp) / avg;
                                inviati = 0;
                                now = DateTime.Now;
                                TimeSpan t = TimeSpan.FromSeconds(remainingTime);
                                remainingTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
                            }
                            updateProgress(fileName, sender, tempPercentage, remainingTimeString);
                            percentage = tempPercentage;
                        }

                    }

                    else if (sockError == SocketError.Shutdown)
                        return;
                    else
                    {
                        throw new SocketException();
                    }
                }
            }
            catch (SocketException e)
            {
                //TODO eseguire sempre in debug perchè qualcosa va storto
                updateFileState(sender, Constants.FILE_STATE.ERROR);
                fileRejected(fileName, ipAddr, Constants.NOTIFICATION_STATE.SEND_ERROR);
            }

            catch (Exception e)
            {
                Console.WriteLine("Sender");
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
                updateFileState(sender, Constants.FILE_STATE.ERROR);
                fileRejected(fileName, ipAddr, Constants.NOTIFICATION_STATE.FILE_ERROR); //6 
            }

            finally
            {
                if (fs != null)
                    fs.Close();
                if (File.Exists(zipLocation))
                    File.Delete(zipLocation);
                releaseResources(sender);
            }
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


        private void releaseResources(Socket s)
        {
            try
            {
                if (s.Connected)
                    s.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sender");
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
            finally
            {
                s.Close();
            }
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }


        public delegate void myDelegate(string filename, Socket sock, int percentage, string remainingTime);
        public static event myDelegate updateProgress;

        public delegate void myDelegate1(string fileName, string username, Constants.NOTIFICATION_STATE state);
        public static event myDelegate1 fileRejected;

        public delegate void myDelegate2(Socket sock, Constants.FILE_STATE state);
        public static event myDelegate2 updateFileState;
    }

}