﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO.Compression;
using System.Threading;

namespace ProjectPDS
{
    class Sender
    {
        public Sender() { }
        public void sendFile(string ipAddr, string pathFile)
        {
            //TODO vedere se fare questo parse dopo aver ricevuto con il tasto destro o qui
            //estraggo nome file dal path assoluto
            int idx = pathFile.LastIndexOf('\\');

            //ricavo fileName
            string fileName = Path.GetFileName(pathFile);
            //ricavo root
            string path = pathFile.Substring(0, idx + 1);
            Console.WriteLine("PAth {0} ", path);
            Console.WriteLine("FileName {0} ", fileName);

            byte[] command;

            //lunghezza nome file
            byte[] fileNameLength = BitConverter.GetBytes(fileName.Length);
            long fileLength = 0;

            //creo nome random dello zip da mandare
            string zipToSend = RandomStr() + Constants.ZIP_EXTENSION;

            //detect whether its a directory or file
            FileAttributes attr = File.GetAttributes(pathFile);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Console.WriteLine("SONO UNA DIRECTORY");
                //creo comando da mandare
                command = Encoding.ASCII.GetBytes(Constants.DIR_COMMAND);
                //calcolo grandezza della directory (compresi i file dentro)
                fileLength = DirSize(new DirectoryInfo(pathFile));
                Console.WriteLine("dimensione directory {0} ", fileLength);

                //zippo cartella
                ZipFile.CreateFromDirectory(pathFile, path + zipToSend, CompressionLevel.NoCompression, false);
                File.SetAttributes(path + zipToSend, FileAttributes.Hidden);
            }
            else
            {
                Console.WriteLine("SONO UN FILE");
                //creo comando da mandare
                command = Encoding.ASCII.GetBytes(Constants.FILE_COMMAND);

                //calcolo grandezza file
                fileLength = new FileInfo(pathFile).Length;
                Console.WriteLine("dimensione file {0} ", fileLength);

                //zippo file
                ZipArchive newFile = ZipFile.Open(path + zipToSend, ZipArchiveMode.Create);
                newFile.CreateEntryFromFile(pathFile, fileName, CompressionLevel.NoCompression);
                File.SetAttributes(path + zipToSend, FileAttributes.Hidden);
                newFile.Dispose();
            }

            //preparo comando + lunghezza nome file
            byte[] request = command.Concat(fileNameLength).ToArray();


            //leggo contenuto dello zip e lo salvo in fileContent
            FileStream fs = File.OpenRead(path + zipToSend);
            byte[] fileContent = new byte[fileLength];
            fileContent = File.ReadAllBytes(path + zipToSend);
            fs.Close();


            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddr), Constants.PORT_TCP);

            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(remoteEP);
            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());


            //mando comando + lunghezza nome file
            int sent = sender.Send(request, request.Length, SocketFlags.None);

            //preparo filename + lunghezza file
            byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
            byte[] fileLengthByte = BitConverter.GetBytes(fileLength);
            byte[] fileNameAndLength = fileNameByte.Concat(fileLengthByte).ToArray();
            //mando filename + lunghezza file
            sent = sender.Send(fileNameAndLength, fileNameAndLength.Length, SocketFlags.None);


            //preparo zip command + zip file name length
            byte[] zipCommand = Encoding.ASCII.GetBytes(Constants.ZIP_COMMAND);
            byte[] zipAndFileNameLength = zipCommand.Concat(BitConverter.GetBytes(zipToSend.Length)).ToArray();

            //mando zip command + zip file name length
            sent = sender.Send(zipAndFileNameLength, zipAndFileNameLength.Length, SocketFlags.None);

            //preparo zip file name + lunghezza file zip
            byte[] zipFileName = Encoding.ASCII.GetBytes(zipToSend);
            byte[] zipFileLength = BitConverter.GetBytes((long)fileContent.Length);
            byte[] tot = zipFileName.Concat(zipFileLength).ToArray();

            //mando zip file name + lunghezza file zip
            sent = sender.Send(tot, tot.Length, SocketFlags.None);

            int temp = 0;
            SocketError error;

            //Thread per mandare shutdown ( prova ) try per non farlo rompere se ci mette meno di quei millisecondi

            //mando zip
            //new Thread(() => { Thread.Sleep(10); try { sender.Shutdown(SocketShutdown.Both); } catch (System.ObjectDisposedException e) { return; } }).Start();
            while (true)
            {
                if (fileContent.Length - temp >= 1400)
                    sent = sender.Send(fileContent, temp, 1400, SocketFlags.None, out error);
                else
                    sent = sender.Send(fileContent, temp, fileContent.Length - temp, SocketFlags.None, out error);

                temp += sent;
                updateProgress(fileName, ipAddr,((temp*100)/fileContent.Length));
                if (error == SocketError.Shutdown)
                {
                    Console.WriteLine("thread fa cose");
                    File.Delete(path + zipToSend);
                    sender.Close();
                    return;
                }

                if (temp == fileContent.Length) break;
            }


            //cancello zip temporaneo
            File.Delete(path + zipToSend);
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


        public delegate void myDelegate(string filename, string receiver, int percentage);
        public static event myDelegate updateProgress;
    }
}