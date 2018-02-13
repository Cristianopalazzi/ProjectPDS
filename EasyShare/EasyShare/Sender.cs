using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasyShare
{
    class Sender
    {
        public void SendFile(string ipAddr, string pathFile, Socket sender, ZipInfo zipInfo)
        {
            sender.SendTimeout = 2500;
            sender.ReceiveTimeout = 1000 * 5 * 60;
            string fileName = Path.GetFileName(pathFile);
            byte[] fileNameByte = Encoding.UTF8.GetBytes(fileName);
            byte[] fileNameLength = BitConverter.GetBytes(fileNameByte.Length);
            long fileLength = zipInfo.IsFile ? new FileInfo(pathFile).Length : DirSize(new DirectoryInfo(pathFile));

            byte[] command = new byte[Constants.FILE_COMMAND.Length];

            string zipToSend = zipInfo.ZipToSend;
            string zipLocation = zipInfo.ZipLocation;
            command = zipInfo.IsFile ? Encoding.ASCII.GetBytes(Constants.FILE_COMMAND) : Encoding.ASCII.GetBytes(Constants.DIR_COMMAND);
            long zipLength = zipInfo.ZipLength;


            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddr), Constants.PORT_TCP);
            FileStream fs = null;
            try
            {
                sender.Connect(remoteEP);
                int sent = 0;

                byte[] fileLine = Combine(command, fileNameLength, fileNameByte, BitConverter.GetBytes(fileLength));
                sent = sender.Send(fileLine, 0, fileLine.Length, SocketFlags.None, out SocketError sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }
                UpdateFileState?.Invoke(sender, Constants.FILE_STATE.ACCEPTANCE);
                byte[] responseFromServer = new byte[Constants.ACCEPT_FILE.Length];
                sender.Receive(responseFromServer, 0, responseFromServer.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                string response = Encoding.ASCII.GetString(responseFromServer);
                if (String.Compare(response, Constants.DECLINE_FILE) == 0)
                {
                    FileRejected?.Invoke(fileName, NeighborProtocol.GetInstance.GetUserFromIp(ipAddr), Constants.NOTIFICATION_STATE.REFUSED);
                    UpdateFileState?.Invoke(sender, Constants.FILE_STATE.REJECTED);
                    ReleaseResources(sender);
                    return;
                }

                byte[] zipLine = Combine(Encoding.ASCII.GetBytes(Constants.ZIP_COMMAND), BitConverter.GetBytes(zipToSend.Length),
                    Encoding.ASCII.GetBytes(zipToSend), BitConverter.GetBytes(zipLength));
                sent = sender.Send(zipLine, 0, zipLine.Length, SocketFlags.None, out sockError);
                int received = 0;
                byte[] data = new byte[Constants.DECLINE_FILE.Length];
                received = sender.Receive(data, 0, Constants.DECLINE_FILE.Length, SocketFlags.None, out sockError);
                if (received == 0 || sockError != SocketError.Success)
                    throw new SocketException();

                if (string.Compare(Encoding.ASCII.GetString(data), Constants.DECLINE_FILE) == 0)
                {
                    UpdateFileState?.Invoke(sender, Constants.FILE_STATE.ERROR);
                    FileRejected?.Invoke(fileName, NeighborProtocol.GetInstance.GetUserFromIp(ipAddr), Constants.NOTIFICATION_STATE.EXISTS);
                    return;
                }

                long temp = 0, percentage = 0;
                fs = File.Open(zipLocation, FileMode.Open, FileAccess.Read, FileShare.Read);

                data = new byte[Constants.PACKET_SIZE];
                int readBytes = 0;
                DateTime now = DateTime.Now;
                int inviati = 0;
                List<double> transferRatesList = new List<double>();

                while (temp < zipLength)
                {
                    if (zipLength - temp > Constants.PACKET_SIZE)
                        readBytes = fs.Read(data, 0, Constants.PACKET_SIZE);
                    else
                        readBytes = fs.Read(data, 0, (int)(zipLength - temp));

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
                            UpdateProgress?.Invoke(fileName, sender, tempPercentage, remainingTimeString);
                            percentage = tempPercentage;
                        }

                    }

                    else if (sockError == SocketError.Shutdown)
                        return;
                    else
                        throw new SocketException();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Sender");
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
                UpdateFileState?.Invoke(sender, Constants.FILE_STATE.ERROR);
                FileRejected?.Invoke(fileName, ipAddr, Constants.NOTIFICATION_STATE.SEND_ERROR);
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
                UpdateFileState?.Invoke(sender, Constants.FILE_STATE.ERROR);
                FileRejected?.Invoke(fileName, ipAddr, Constants.NOTIFICATION_STATE.FILE_ERROR_SEND);
            }

            finally
            {
                if (fs != null)
                    fs.Close();
                ReleaseResources(sender);
            }
        }

        private void ReleaseResources(Socket s)
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

        private long DirSize(DirectoryInfo d)
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


        public delegate void myDelegate(string filename, Socket sock, int percentage, string remainingTime);
        public static event myDelegate UpdateProgress;

        public delegate void myDelegate1(string fileName, string username, Constants.NOTIFICATION_STATE state);
        public static event myDelegate1 FileRejected;

        public delegate void myDelegate2(Socket sock, Constants.FILE_STATE state);
        public static event myDelegate2 UpdateFileState;
    }

}