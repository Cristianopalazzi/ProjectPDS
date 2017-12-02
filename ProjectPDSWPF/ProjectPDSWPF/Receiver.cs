using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

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
            string user = String.Empty;
            try
            {
                SocketError sockError;
                int received = 0;
                ipSender = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                user = NeighborProtocol.getInstance.getUserFromIp(ipSender);
                if (String.IsNullOrEmpty(user))
                    user = Constants.UTENTE_ANONIMO;
                byte[] command = new byte[Constants.FILE_COMMAND.Length];
                received = handler.Receive(command, 0, command.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                string commandString = Encoding.ASCII.GetString(command);
                byte[] fileNameLength = new byte[sizeof(int)];
                received = handler.Receive(fileNameLength, 0, sizeof(int), SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                int fileNameDimension = BitConverter.ToInt32(fileNameLength, 0);
                byte[] fileNameAndFileLength = new byte[fileNameDimension + sizeof(long)];
                received = handler.Receive(fileNameAndFileLength, 0, fileNameAndFileLength.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }

                fileNameString = Encoding.UTF8.GetString(fileNameAndFileLength, 0, fileNameDimension);
                long fileSize = BitConverter.ToInt64(fileNameAndFileLength, fileNameDimension);
                id = Guid.NewGuid().ToString();
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
                    acceptance(user, fileNameString, adjustedSize, id);

                    while (mre.WaitOne())
                        if (String.Compare(Receiver.idFileToAccept, id) == 0)
                            break;

                    if (Receiver.accepted)
                    {
                        responseToClient = Encoding.ASCII.GetBytes(Constants.ACCEPT_FILE);
                        handler.Send(responseToClient, 0, responseToClient.Length, SocketFlags.None, out sockError);
                        if (sockError != SocketError.Success)
                            throw new SocketException();
                    }
                    else if (!Receiver.accepted)
                    {
                        responseToClient = Encoding.ASCII.GetBytes(Constants.DECLINE_FILE);
                        handler.Send(responseToClient, 0, responseToClient.Length, SocketFlags.None, out sockError);
                        if (sockError != SocketError.Success)
                            throw new SocketException();
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

                byte[] zipCommandZipNameLength = new byte[Constants.ZIP_COMMAND.Length + sizeof(int)];
                received = handler.Receive(zipCommandZipNameLength, 0, zipCommandZipNameLength.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success || received == 0)
                {
                    throw new SocketException();
                }
                int zipFileNameLength = BitConverter.ToInt32(zipCommandZipNameLength, Constants.ZIP_COMMAND.Length);
                byte[] zipNameAndZipLength = new byte[zipFileNameLength + sizeof(long)];
                received = handler.Receive(zipNameAndZipLength, 0, zipNameAndZipLength.Length, SocketFlags.None, out sockError);
                if (sockError != SocketError.Success)
                {
                    throw new SocketException();
                }
                string zipFileName = Encoding.ASCII.GetString(zipNameAndZipLength, 0, zipFileNameLength);
                zipFileSize = BitConverter.ToInt64(zipNameAndZipLength, zipFileNameLength);

                string senderID = user + "@" + ipSender;
                byte[] image;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                if (NeighborProtocol.getInstance.Neighbors.TryGetValue(senderID, out Neighbor ne))
                {
                    if (ne.NeighborImage == null)
                        ne.setImage(File.ReadAllBytes(App.defaultResourcesFolder + "/guest.png"));
                    encoder.Frames.Add(BitmapFrame.Create(ne.NeighborImage));
                }
                else
                {
                    BitmapImage bitmap = Neighbor.ToImage(File.ReadAllBytes(App.defaultResourcesFolder + "/uu.png"));
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    image = ms.ToArray();
                    ms.Close();
                }

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
                    fileCancel(id, Constants.NOTIFICATION_STATE.CANCELED);
                    fs.Close();
                    releaseResources(handler);
                    return;
                }
                fs.Close();

                string str = String.Empty;

                if (String.Compare(commandString, Constants.FILE_COMMAND) == 0)
                {
                    archive = ZipFile.OpenRead(zipLocation);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        str = currentDirectory + "\\" + entry.Name;
                        if (File.Exists(str))
                        {
                            string extension = Path.GetExtension(str);
                            string onlyName = Path.GetFileNameWithoutExtension(str);
                            str = currentDirectory + "\\" + onlyName + user + extension;

                            if (File.Exists(str))
                            {
                                string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-ffffff");
                                str = currentDirectory + "\\" + onlyName + user + timeStamp + extension;
                            }
                        }
                        entry.ExtractToFile(str, true);
                    }
                }
                else if (String.Compare(commandString, Constants.DIR_COMMAND) == 0)
                {
                    str = currentDirectory + "\\" + fileNameString;
                    if (Directory.Exists(str))
                    {
                        str += user;
                        if (Directory.Exists(str))
                        {
                            string timeStamp = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss-ffffff");
                            str += timeStamp;
                        }
                    }
                    try
                    {
                        ZipFile.ExtractToDirectory(zipLocation, str);
                    }
                    catch (PathTooLongException e)
                    {
                        //scegli tu il percorso
                    }
                }
                //TODO aggiungere controllo sulla dimensione massima dei nomi dei file e cartelle
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.SocketErrorCode);
                if (zipFileSize == 0)
                    receivingFailure(fileNameString, ipSender, Constants.NOTIFICATION_STATE.NET_ERROR); // era 4
                else
                    fileCancel(id, Constants.NOTIFICATION_STATE.REC_ERROR);
            }
            catch
            {
                receivingFailure(fileNameString, ipSender, Constants.NOTIFICATION_STATE.FILE_ERROR); // era 6
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


        static string SizeSuffix(Int64 value)
        {
            if (value < 0) return "-" + SizeSuffix(-value);
            if (value == 0) return string.Format("{0:n" + 2 + "} bytes", 0);

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            if (Math.Round(adjustedSize, 2) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + 2 + "} {1}", adjustedSize, SizeSuffixes[mag]);
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

        public static ManualResetEvent mre = new ManualResetEvent(false);

        public delegate void myDelegate(string id, int percentage);
        public static event myDelegate updateProgress;

        public delegate void myDelegate1(string senderID, byte[] image, string fileName, string id);
        public static event myDelegate1 updateReceivingFiles;

        public delegate void myDelegate2(string id, Constants.NOTIFICATION_STATE state);
        public static event myDelegate2 fileCancel;

        public delegate void myDelegate4(string fileName, string userName, Constants.NOTIFICATION_STATE state);
        public static event myDelegate4 receivingFailure;

        public delegate void myDelegate5(string userName, string fileName, string dimension, string id);
        public static event myDelegate5 acceptance;

        public static string idFileToAccept = String.Empty;
        public static bool accepted = false;
    }
}