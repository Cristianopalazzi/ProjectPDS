using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace ProjectPDS
{
    class Receiver
    {
        public Receiver()
        {
            settings = Settings.getInstance;
            server = new Thread(startServer)
            {
                Name = "server"
            };
            server.Start();
        }
        ~Receiver() { server.Join(); }

        private void startServer()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);

            listener.Listen(10);

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();
                Thread myThread = new Thread(() => receiveFromSocket(handler));
                myThread.SetApartmentState(ApartmentState.STA);
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
            int received = handler.Receive(request, Constants.FILE_COMMAND.Length + sizeof(int), SocketFlags.None);

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


            if (settings.AutoAccept)
            {
                byte[] responseClient = Encoding.ASCII.GetBytes(Constants.ACCEPT_FILE);
                handler.Send(responseClient, responseClient.Length, SocketFlags.None);
            }
            else
            {
                byte[] responseToClient = new byte[Constants.ACCEPT_FILE.Length];
                string adjustedSize = null;
                if (fileSize < 1024)
                    adjustedSize = fileSize + " B";
                else if (fileSize > 1024 && fileSize < (1024 * 1024))
                    adjustedSize = fileSize / 1024 + " KB";
                else if (fileSize > (1024 * 1024))
                    adjustedSize = fileSize / (1024 * 1024) + " MB";

                DialogResult dialogResult = MessageBox.Show(fileNameString + " di: " + adjustedSize, "Vuoi accettare:", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    responseToClient = Encoding.ASCII.GetBytes(Constants.ACCEPT_FILE);
                    handler.Send(responseToClient, responseToClient.Length, SocketFlags.None);
                }
                else if (dialogResult == DialogResult.No)
                {
                    responseToClient = Encoding.ASCII.GetBytes(Constants.DECLINE_FILE);
                    handler.Send(responseToClient, responseToClient.Length, SocketFlags.None);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    return;
                }
            }

            string currentDirectory = String.Empty;
            while (String.IsNullOrEmpty(currentDirectory))
            {
                if (!settings.DefaultDir)
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog
                    {
                        Description = "Selezione la cartella in cui salvare il file"
                    };
                    if (fbd.ShowDialog() == DialogResult.OK)
                        currentDirectory = fbd.SelectedPath;
                }
                else
                    currentDirectory = settings.DefaultDirPath;

            }


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

            Console.WriteLine("Ricevo il file");
            ReceivingFile rF = new ReceivingFile(NeighborProtocol.getInstance.getUserFromIp(ipSender), fileNameString, ipSender);
            int index = -1;
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(MainForm))
                {
                    index = ((MainForm)form).apriFileToReceive(rF);
                }
            }
            int percentage = 0;
            while (true)
            {
                int bytesRec = handler.Receive(fileContent, temp, (int)(zipFileSize - temp), SocketFlags.None, out SocketError error);
                temp += bytesRec;
                ulong temporary = (ulong) temp * 100;
                if (index != -1)
                    if (temp < 0 )
                        Console.WriteLine("ciaone bellissimo");

               
               int  tempPercentage = (int ) (temporary / (ulong)fileContent.Length);
               if (tempPercentage > percentage)
                {
                    updateProgress(index, tempPercentage);
                    percentage = tempPercentage;
                }
                //condizione uscita while
                if (temp == zipFileSize) break;

                //receive == 0, finito file oppure chiusura dal client
                if (bytesRec == 0) break;
            }

            if (temp != zipFileSize)
            {
                Console.WriteLine("Connessione interrotta dal client");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }

            //controllo se FIL o DIR per vedere come estrarre
            //scrivo il file zip a prescindere, poi devo vedere i file dentro e vedere se già
            //ne esistono altri con lo stesso nome

            //scrivo zip file nella directory di default
            FileStream fs = new FileStream(currentDirectory + "\\" + zipFileName, FileMode.OpenOrCreate);
            fs.Write(fileContent, 0, fileContent.Length);
            fs.Flush(true);
            fs.Close();
            File.SetAttributes(currentDirectory + "\\" + zipFileName, FileAttributes.Hidden);
            NeighborProtocol n = NeighborProtocol.getInstance;
            if (String.Compare(commandString, Constants.FILE_COMMAND) == 0)
            {
                Console.WriteLine("FILE");
                //controllo se esiste già il file dentro lo zip
                ZipArchive archive = ZipFile.OpenRead(currentDirectory + "\\" + zipFileName);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (File.Exists(currentDirectory + "\\" + entry.Name))
                    {

                        string user = n.getUserFromIp(ipSender);
                        string extension = Path.GetExtension(currentDirectory + "\\" + entry.Name);
                        string onlyName = Path.GetFileNameWithoutExtension(currentDirectory + "\\" + entry.Name);
                        string newName = onlyName + user + extension;
                        if (File.Exists(currentDirectory + "\\" + newName))
                        {
                            string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-ffffff");
                            entry.ExtractToFile(currentDirectory + "\\" + onlyName + user + timeStamp + extension, true);
                        }
                        else entry.ExtractToFile(currentDirectory + "\\" + newName);
                    }
                    else entry.ExtractToFile(currentDirectory + "\\" + entry.Name);
                }
                archive.Dispose();
            }
            else if (String.Compare(commandString, Constants.DIR_COMMAND) == 0)
            {
                Console.WriteLine("DIR");
                ZipArchive archive = ZipFile.OpenRead(currentDirectory + "\\" + zipFileName);
                if (Directory.Exists(currentDirectory + "\\" + fileNameString))
                {
                    if (Directory.Exists(currentDirectory + "\\" + fileNameString + n.getUserFromIp(ipSender)))
                    {
                        string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-ffffff");
                        ZipFile.ExtractToDirectory(currentDirectory + "\\" + zipFileName, currentDirectory + "\\" + fileNameString + n.getUserFromIp(ipSender) + timeStamp);
                    }
                    else ZipFile.ExtractToDirectory(currentDirectory + "\\" + zipFileName, currentDirectory + "\\" + fileNameString + n.getUserFromIp(ipSender));
                }
                else ZipFile.ExtractToDirectory(currentDirectory + "\\" + zipFileName, currentDirectory + "\\" + fileNameString);

                archive.Dispose();
            }
            //TODO aggiungere controllo sulla dimensione massima dei nomi dei file e cartelle
            //cancella lo zip
            File.Delete(currentDirectory + "\\" + zipFileName);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        private Thread server;
        private Settings settings;

        public delegate void myDelegate(int index, int percentage);
        public static event myDelegate updateProgress;
    }
}