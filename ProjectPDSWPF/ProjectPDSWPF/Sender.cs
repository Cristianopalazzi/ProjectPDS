﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace ProjectPDSWPF
{
    class Sender
    {
        public Sender() { }
        public void sendFile(string ipAddr, string pathFile, Socket sender)
        {
            sender.SendTimeout = 2500;
            sender.ReceiveTimeout = 0;
            string fileName = Path.GetFileName(pathFile);

            //TODO togliamo ping?
            Ping p = new Ping();
            PingReply rep = p.Send(ipAddr, 2000);

            if (rep.Status != IPStatus.Success)
            {
                updateFileState(sender, Constants.FILE_STATE.ERROR);
                fileRejected(fileName, ipAddr, Constants.NOTIFICATION_STATE.NET_ERROR); // 4
                return;
            }


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
            FileStream fs = null;
            try
            {
                sender.Connect(remoteEP);

                SocketError sockError;
                //mando comando + lunghezza nome file
                int sent = sender.Send(request, 0, request.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                //preparo filename + lunghezza file
                byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
                byte[] fileLengthByte = BitConverter.GetBytes(fileLength);
                byte[] fileNameAndLength = fileNameByte.Concat(fileLengthByte).ToArray();
                //mando filename + lunghezza file
                sent = sender.Send(fileNameAndLength, 0, fileNameAndLength.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }
                //aggiunto cambio di stato
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
                    fileRejected(fileName, NeighborProtocol.getInstance.getUserFromIp(ipAddr), Constants.NOTIFICATION_STATE.REFUSED); //3
                    updateFileState(sender, Constants.FILE_STATE.CANCELED);
                    if (File.Exists(zipLocation))
                        File.Delete(zipLocation);
                    releaseResources(sender);
                    return;
                }


                //preparo zip command + zip file neighborName length
                byte[] zipCommand = Encoding.ASCII.GetBytes(Constants.ZIP_COMMAND);
                byte[] zipAndFileNameLength = zipCommand.Concat(BitConverter.GetBytes(zipToSend.Length)).ToArray();

                //mando zip command + zip file neighborName length
                sent = sender.Send(zipAndFileNameLength, 0, zipAndFileNameLength.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                //preparo zip file neighborName + lunghezza file zip
                byte[] zipFileName = Encoding.ASCII.GetBytes(zipToSend);
                byte[] zipFileLength = BitConverter.GetBytes(zipLength);
                byte[] tot = zipFileName.Concat(zipFileLength).ToArray();

                //mando zip file neighborName + lunghezza file zip
                sent = sender.Send(tot, 0, tot.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                int temp = 0, percentage = 0;
                fs = new FileStream(zipLocation, FileMode.Open, FileAccess.Read);

                byte[] data = new byte[1400];
                int readBytes = 0;
                DateTime now = DateTime.Now;
                int inviati = 0;
                List<double> transferRatesList = new List<double>();

                while (temp < zipLength)
                {
                    if (zipLength - temp > 1400)
                        readBytes = fs.Read(data, 0, 1400);
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
                            updateProgress(fileName, sender, tempPercentage);
                            percentage = tempPercentage;
                        }
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
                            string remainingTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);

                            updateRemainingTime(sender, remainingTimeString);
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
                fileRejected(fileName, ipAddr, Constants.NOTIFICATION_STATE.SEND_ERROR); //5
            }

            catch
            {
                updateFileState(sender,Constants.FILE_STATE.ERROR);
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
            if (s.Connected)
                s.Shutdown(SocketShutdown.Both);
            s.Close();
        }


        public delegate void myDelegate(string filename, Socket sock, int percentage);
        public static event myDelegate updateProgress;

        public delegate void myDelegate1(string fileName, string username, Constants.NOTIFICATION_STATE state);
        public static event myDelegate1 fileRejected;

        public delegate void myDelegate3(Socket sock, string remainingTime);
        public static event myDelegate3 updateRemainingTime;

        public delegate void myDelegate4(Socket sock,Constants.FILE_STATE state);
        public static event myDelegate4 updateFileState;

        private decimal milliSeconds = 0;
    }

}