using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls.Dialogs;

namespace ProjectPDSWPF
{
    class Receiver
    {
        public Receiver()
        {
            settings = Settings.getInstance;
            server = new Thread(startServer)
            {
                Name = "server",
                IsBackground = true
            };
            server.Start();
        }
        ~Receiver() { server.Join(); }

        private void startServer()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP);
            // Create a TCP/IP socket.
            Socket listener = null;
            try
            {
                listener = new Socket(AddressFamily.InterNetwork,
                       SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);

                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();
                    Thread myThread = new Thread(() => receiveFromSocket(handler));
                    myThread.SetApartmentState(ApartmentState.STA);
                    myThread.IsBackground = true;
                    myThread.Start();
                }
            }
            catch
            {
                //GUI (Controlla la tua connessione) ?????
            }
            finally
            {
                if (listener != null)
                {
                    listener.Shutdown(SocketShutdown.Both);
                    listener.Close();
                }
            }

        }

        private void receiveFromSocket(Socket handler)
        {
            handler.ReceiveTimeout = 2500;
            handler.SendTimeout = 2500;
            string zipLocation = String.Empty, fileNameString = String.Empty, ipSender = String.Empty, id = String.Empty;
            ZipArchive archive = null;
            int temp = 0;
            long zipFileSize = 0;
            FileStream fs = null;
            try
            {
                ipSender = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                byte[] request = new byte[Constants.FILE_NAME + Constants.FILE_COMMAND.Length];
                SocketError sockError;

                //Ricevo comando + lunghezza file neighborName
                int received = handler.Receive(request, 0, Constants.FILE_COMMAND.Length + sizeof(int), SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                Array.Resize(ref request, received);
                string requestString = Encoding.UTF8.GetString(request);
                //Ricavo comando e dimensione del fileName
                string commandString = requestString.Substring(0, Constants.FILE_COMMAND.Length);
                int fileNameDimension = BitConverter.ToInt32(request, Constants.FILE_COMMAND.Length);

                byte[] fileNameAndLength = new byte[fileNameDimension + sizeof(long)];
                received = handler.Receive(fileNameAndLength, 0, fileNameDimension + sizeof(long), SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                //Ricevo fileName e lunghezza del file
                string fileNameAndLengthString = Encoding.UTF8.GetString(fileNameAndLength);
                fileNameString = fileNameAndLengthString.Substring(0, fileNameDimension);
                long fileSize = BitConverter.ToInt64(fileNameAndLength, fileNameDimension);

                if (settings.AutoAccept)
                {
                    byte[] responseClient = Encoding.ASCII.GetBytes(Constants.ACCEPT_FILE);
                    handler.Send(responseClient, 0, responseClient.Length, SocketFlags.None, out sockError);
                    if (sockError != SocketError.Success)
                    {
                        throw new SocketException();
                    }
                }
                else
                {
                    byte[] responseToClient = new byte[Constants.ACCEPT_FILE.Length];
                    string adjustedSize = SizeSuffix(fileSize);

                    MessageDialogResult dialogResult = askToAccept(NeighborProtocol.getInstance.getUserFromIp(ipSender), fileNameString, adjustedSize);
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        responseToClient = Encoding.ASCII.GetBytes(Constants.ACCEPT_FILE);
                        handler.Send(responseToClient, 0, responseToClient.Length, SocketFlags.None, out sockError);
                        if (sockError != SocketError.Success)
                        {
                            throw new SocketException();
                        }
                    }
                    else if (dialogResult == MessageDialogResult.Negative)
                    {
                        responseToClient = Encoding.ASCII.GetBytes(Constants.DECLINE_FILE);
                        handler.Send(responseToClient, 0, responseToClient.Length, SocketFlags.None, out sockError);
                        if (sockError != SocketError.Success)
                        {
                            throw new SocketException();
                        }
                        releaseResources(handler);
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
                received = handler.Receive(zipCommand, 0, Constants.ZIP_COMMAND.Length + sizeof(int), SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                string zipCommandString = Encoding.ASCII.GetString(zipCommand);
                int zipFileNameLength = BitConverter.ToInt32(zipCommand, Constants.ZIP_COMMAND.Length);


                //ricevo zip file neighborName e lunghezza
                byte[] zipFileNameAndLength = new byte[zipFileNameLength + sizeof(long)];

                received = handler.Receive(zipFileNameAndLength, 0, zipFileNameLength + sizeof(long), SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }
                string zipFileNameAndLengthString = Encoding.ASCII.GetString(zipFileNameAndLength);
                string zipFileName = zipFileNameAndLengthString.Substring(0, zipFileNameLength);
                zipFileSize = BitConverter.ToInt64(zipFileNameAndLength, zipFileNameLength);


                string senderID = NeighborProtocol.getInstance.getUserFromIp(ipSender) + "@" + ipSender;
                byte[] image;
                NeighborProtocol.getInstance.Neighbors.TryGetValue(senderID, out Neighbor ne);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                if (ne.NeighborImage == null)
                    ne.setImage(File.ReadAllBytes(App.defaultResourcesFolder + "/guest.png"));
                encoder.Frames.Add(BitmapFrame.Create(ne.NeighborImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    image = ms.ToArray();
                    ms.Close();
                }
                id = Guid.NewGuid().ToString();

                // INIZIA A SERVIRE IL CONTROLLO GUI
                updateReceivingFiles(senderID, image, fileNameString, id);
                int percentage = 0;
                zipLocation = App.defaultFolder + "\\" + zipFileName;
                fs = new FileStream(zipLocation, FileMode.Create, FileAccess.Write);
                byte[] data = new byte[1400];
                int bytesRec = 0;

                while (temp < zipFileSize)
                {
                    if (zipFileSize - temp > 1400)
                    {
                        bytesRec = handler.Receive(data, 0, 1400, SocketFlags.None, out sockError);
                    }
                    else bytesRec = handler.Receive(data, 0, (int)zipFileSize - temp, SocketFlags.None, out sockError);



                    if (sockError == SocketError.Success)
                    {
                        temp += bytesRec;
                        fs.Write(data, 0, bytesRec);
                        fs.Flush();
                        ulong temporary = (ulong)temp * 100;
                        int tempPercentage = (int)(temporary / (ulong)zipFileSize);
                        if (tempPercentage > percentage)
                        {
                            updateProgress(id, tempPercentage);
                            percentage = tempPercentage;
                        }
                    }
                    else
                    {
                        if (fs != null)
                            fs.Close();
                        throw new SocketException();
                    }

                    if (bytesRec == 0)
                        break;
                }

                if (temp != zipFileSize)
                {
                    fileCancel(id, Constants.FILE_STATE.CANCELED.ToString());
                    fs.Close();
                    releaseResources(handler);
                    return;
                }
                fs.Close();

                NeighborProtocol n = NeighborProtocol.getInstance;
                string user = n.getUserFromIp(ipSender);
                if (String.Compare(commandString, Constants.FILE_COMMAND) == 0)
                {
                    archive = ZipFile.OpenRead(zipLocation);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (File.Exists(currentDirectory + "\\" + entry.Name))
                        {
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
                }
                else if (String.Compare(commandString, Constants.DIR_COMMAND) == 0)
                {
                    if (Directory.Exists(currentDirectory + "\\" + fileNameString))
                    {
                        if (Directory.Exists(currentDirectory + "\\" + fileNameString + user))
                        {
                            string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-ffffff");
                            ZipFile.ExtractToDirectory(zipLocation, currentDirectory + "\\" + fileNameString + user + timeStamp);
                        }
                        else ZipFile.ExtractToDirectory(zipLocation, currentDirectory + "\\" + fileNameString + user);
                    }
                    else ZipFile.ExtractToDirectory(zipLocation, currentDirectory + "\\" + fileNameString);
                }
                //TODO aggiungere controllo sulla dimensione massima dei nomi dei file e cartelle
                //cancella lo zip
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.SocketErrorCode);
                if (zipFileSize == 0)
                {
                    receivingFailure(fileNameString, ipSender, 4);
                }
                else
                {
                    //TODO cambiare
                    fileCancel(id, "ERROR");
                }
            }
            catch
            {
                receivingFailure(fileNameString, ipSender, 6);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                if (archive != null)
                    archive.Dispose();
                if (!String.IsNullOrEmpty(zipLocation))
                    if (File.Exists(zipLocation))
                        File.Delete(zipLocation);
                releaseResources(handler);
            }
        }


        static string SizeSuffix(Int64 value, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0) throw new ArgumentOutOfRangeException("decimalPlaces");
            if (value < 0) return "-" + SizeSuffix(-value);
            if (value == 0) return string.Format("{0:n" + decimalPlaces + "} bytes", 0);

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        private void releaseResources(Socket sock)
        {
            if (sock.Connected)
                sock.Shutdown(SocketShutdown.Both);
            sock.Close();

        }
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        private Thread server;
        private Settings settings;

        public delegate void myDelegate(string id, int percentage);
        public static event myDelegate updateProgress;

        public delegate void myDelegate1(string senderID, byte[] image, string fileName, string id);
        public static event myDelegate1 updateReceivingFiles;

        public delegate void myDelegate2(string id, string statusFile);
        public static event myDelegate2 fileCancel;

        public delegate MessageDialogResult myDelegate3(string userName, string fileName, string dimension);
        public static event myDelegate3 askToAccept;

        public delegate void myDelegate4(string fileName, string userName, int type);
        public static event myDelegate4 receivingFailure;
    }
}