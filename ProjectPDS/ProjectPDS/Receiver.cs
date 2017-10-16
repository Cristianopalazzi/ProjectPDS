using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Security.Permissions;

namespace ProjectPDS
{
    class Receiver
    {
        public Receiver()
        {
            server = new Thread(startServer);
            server.Name = "server";
            server.Start();
        }
        ~Receiver() { server.Join(); }

        private void startServer()
        {
            //TODO vedere di cambiare le porte in caso di ricezione da più mittenti contemporanemente (teoricamente no)
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);

            //non penso possa servire
            listener.Listen(10);

            Console.WriteLine("Waiting for a connection...");
            while (true)
            {
                Socket handler = listener.Accept();
                Thread myThread = new Thread(() => receiveFromSocket(handler));
                myThread.Start();
            }
        }

        private void receiveFromSocket(Socket handler)
        {
            string ipSender = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
            Console.WriteLine("remote endpoint {0} {1} ", ipSender,
                ((IPEndPoint)handler.RemoteEndPoint).Port.ToString());

            byte[] request = new byte[Constants.FILE_NAME + Constants.FILE_COMMAND.Length];

            //Ricevo comando + lunghezza file name
            int received = handler.Receive(request, Constants.FILE_COMMAND.Length + sizeof(int)
                , SocketFlags.None);

            Array.Resize(ref request, received);

            string requestString = Encoding.UTF8.GetString(request);
            Console.WriteLine("RequestString received {0} ", received);

            //Ricavo comando e dimensione del fileName
            string commandString = requestString.Substring(0, Constants.FILE_COMMAND.Length);
            int fileNameDimension = BitConverter.ToInt32(request, Constants.FILE_COMMAND.Length);

            Console.WriteLine("Command {0} ", commandString);
            Console.WriteLine("fileNameDimension {0} ", fileNameDimension);

            byte[] fileNameAndLength = new byte[fileNameDimension + sizeof(long)];
            received = handler.Receive(fileNameAndLength, fileNameDimension + sizeof(long), SocketFlags.None);


            //Ricevo fileName e lunghezza del file
            string fileNameAndLengthString = Encoding.UTF8.GetString(fileNameAndLength);
            string fileNameString = fileNameAndLengthString.Substring(0, fileNameDimension);
            long fileSize = BitConverter.ToInt64(fileNameAndLength, fileNameDimension);

            Console.WriteLine("fileName {0} ", fileNameString);
            Console.WriteLine("FileSize {0} ", fileSize);

            //TODO integrare con interfaccia grafica per accettare o rifiutare
            //byte[] responseClient = Encoding.ASCII.GetBytes("OK"); // Encoding.ASCII.GetBytes("NO");
            //handler.Send(responseClient, responseClient.Length, SocketFlags.None);
            //relativi controlli per chiudere eventualmente la socket


            //ricevo zip command + zipFileNameLength
            byte[] zipCommand = new byte[Constants.ZIP_COMMAND.Length + sizeof(int)];
            received = handler.Receive(zipCommand, Constants.ZIP_COMMAND.Length + sizeof(int), SocketFlags.None);

            string zipCommandString = Encoding.ASCII.GetString(zipCommand);
            int zipFileNameLength = BitConverter.ToInt32(zipCommand, Constants.ZIP_COMMAND.Length);


            //ricevo zip file name e lunghezza
            byte[] zipFileNameAndLength = new byte[zipFileNameLength + sizeof(long)];

            received = handler.Receive(zipFileNameAndLength, zipFileNameLength + sizeof(long), SocketFlags.None);
            string zipFileNameAndLengthString = Encoding.ASCII.GetString(zipFileNameAndLength);
            string zipFileName = zipFileNameAndLengthString.Substring(0, zipFileNameLength);
            long zipFileSize = BitConverter.ToInt64(zipFileNameAndLength, zipFileNameLength);
            Console.WriteLine("zip file name: {0}", zipFileName);
            Console.WriteLine("zip file size {0} ", zipFileSize);


            //preparo struttura per contenere il file
            byte[] fileContent = new byte[zipFileSize];
            int temp = 0;
            SocketError error;


            while (true)
            {
                Console.WriteLine("Attendo file");
                int bytesRec = handler.Receive(fileContent, temp, (int)(zipFileSize - temp), SocketFlags.None, out error);
                temp += bytesRec;

                //condizione uscita while
                if (temp == zipFileSize) break;
            }

            //controllo se FIL o DIR per vedere come estrarre

            //scrivo il file zip a prescindere, poi devo vedere i file dentro e vedere se già
            //ne esistono altri con lo stesso nome
            if (String.Compare(commandString, Constants.FILE_COMMAND) == 0)
            {
                Console.WriteLine("FILE");
                //scrivo zip file nella directory di default
                //devo rivedere bene questo permission, forse non serve
                FileIOPermission fileIOPerm = new FileIOPermission(FileIOPermissionAccess.Write, Constants.DEFAULT_DIRECTORY + "\\" + zipFileName);
                FileStream fs = new FileStream(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName, FileMode.Create);
                fs.Write(fileContent, 0, fileContent.Length);
                fs.Flush(true);
                fs.Close();

                //controllo se esiste già il file dentro lo zip
                ZipArchive archive = ZipFile.OpenRead(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (File.Exists(Constants.DEFAULT_DIRECTORY + "\\" + entry.Name))
                    {
                        //TESTTTTTT
                        NeighborProtocol n = new NeighborProtocol();
                        n.insert(Environment.UserName + "@" + ipSender);
                        string user = n.getUserFromIp(ipSender);
                        string extension = Path.GetExtension(Constants.DEFAULT_DIRECTORY + "\\" + entry.Name);
                        string onlyName = Path.GetFileNameWithoutExtension(Constants.DEFAULT_DIRECTORY + "\\" + entry.Name);
                        string newName = onlyName + user + extension;
                        if (File.Exists(Constants.DEFAULT_DIRECTORY + "\\" + newName))
                        {
                            string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss").Replace(" ", "_");
                            entry.ExtractToFile(Constants.DEFAULT_DIRECTORY + "\\" + onlyName + user + timeStamp + extension);
                        }
                        else entry.ExtractToFile(Constants.DEFAULT_DIRECTORY + "\\" + newName);
                    }
                }
                archive.Dispose();
            }
            else if (String.Compare(commandString, Constants.DIR_COMMAND) == 0)
            {
                Console.WriteLine("DIR");

                //scrivo zip file nella directory di default
                FileIOPermission fileIOPerm1 = new FileIOPermission(FileIOPermissionAccess.Write, Constants.DEFAULT_DIRECTORY + "\\" + zipFileName);
                FileStream fs = new FileStream(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName, FileMode.Create);
                fs.Write(fileContent, 0, fileContent.Length);
                fs.Flush(true);
                fs.Close();
                ZipArchive archive = ZipFile.OpenRead(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    int index = entry.FullName.IndexOf("/");
                    string folder = entry.FullName.Substring(0, index);
                    if (Directory.Exists(Constants.DEFAULT_DIRECTORY + "\\" + folder))
                    {
                        if (Directory.Exists(Constants.DEFAULT_DIRECTORY + "\\" + folder + Environment.UserName))
                        {
                            string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss").Replace(" ", "_");
                            ZipFile.ExtractToDirectory(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName, Constants.DEFAULT_DIRECTORY + "\\" + folder + Environment.UserName + timeStamp);
                        }
                        else ZipFile.ExtractToDirectory(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName, Constants.DEFAULT_DIRECTORY + "\\" + folder + Environment.UserName);
                    }
                    else ZipFile.ExtractToDirectory(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName, Constants.DEFAULT_DIRECTORY);
                }
                archive.Dispose();
            }
            //TODO agguingere controllo sulla dimensione massima dei nomi dei file e cartelle
            //cancella lo zip
            File.Delete(Constants.DEFAULT_DIRECTORY + "\\" + zipFileName);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        private Thread server;
    }
}