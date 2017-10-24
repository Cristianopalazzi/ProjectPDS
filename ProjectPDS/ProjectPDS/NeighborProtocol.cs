using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.IO;
using System.Drawing;

namespace ProjectPDS
{
    class NeighborProtocol
    {
        public NeighborProtocol()
        {
            neighbors = new ConcurrentDictionary<string, int>();
            neighborsImage = new Dictionary<string, byte[]>();
            listener = new Thread(listen)
            {
                Name = "listener"
            };
            listener.Start();
            sender = new Thread(sendMe)
            {
                Name = "sender"
            };
            sender.Start();
            clean = new Thread(cleanMap)
            {
                Name = "cleaner"
            };
            clean.Start();
        }
        ~NeighborProtocol() { listener.Join(); sender.Join(); clean.Join(); }

        private void listen()
        {

            //IPHostEntry ipHostInfo = Dns.GetHostEntryGetHostEntry(); Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string recv, remoteIpAddress, remotePort;

            //EndPoint con le info del client che si collega
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            //join ad un gruppo multicast
            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                new MulticastOption(IPAddress.Parse(Constants.MULTICAST)));

            //Non ricevo pacchetti da me stesso
            //socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP);

            socket.Bind(localEndPoint);
            while (true)
            {
                //TODO diminuire buflen, serve meno spazio
                byte[] buffer = new byte[Constants.BUFLEN];
                Console.WriteLine("Waiting for hello...");
                int i = socket.ReceiveFrom(buffer, ref senderRemote);
                Array.Resize(ref buffer, i);
                recv = Encoding.UTF8.GetString(buffer);
                remoteIpAddress = ((IPEndPoint)senderRemote).Address.ToString();
                remotePort = ((IPEndPoint)senderRemote).Port.ToString();
                //Console.WriteLine("mittente {0} ", remoteIpAddress
                //    + "on port number " + remotePort);
                //Console.WriteLine("ricevuto {0} ", recv);
                string command = recv.Substring(0, 4);
                //Console.WriteLine("command {0} ", command);
                string senderName = recv.Substring(4);

                string senderID = String.Concat(senderName, String.Concat("@", remoteIpAddress));
                //Console.WriteLine("senderID {0} ", senderID);
                //Console.WriteLine("senderIPPPP {0} ", remoteIpAddress);
                if (String.Compare(command, Constants.HELL, false) == 0)
                {
                    if (neighbors.ContainsKey(senderID))
                        neighbors[senderID] = Constants.MAX_COUNTER;
                    else
                        neighbors.TryAdd(senderID, Constants.MAX_COUNTER);
                }
                else if (String.Compare(command, Constants.QUIT, false) == 0)
                {
                    int value;
                    if (neighbors.ContainsKey(senderID))
                        neighbors.TryRemove(senderID, out value);
                }

                IPEndPoint ip = new IPEndPoint(((IPEndPoint)senderRemote).Address, Constants.PORT_UDP_IMG);
                byte[] requestImage;
                if (!neighborsImage.ContainsKey(senderID))
                {
                    //richiedo la foto 
                    Console.WriteLine("Non ho la foto del tizio, gliela richiedo");
                    requestImage = Encoding.ASCII.GetBytes(Constants.NEED_IMG);
                    socket.SendTo(requestImage, requestImage.Length, SocketFlags.None, ip);
                    receiveImg(senderID);
                }
                else
                {
                    Console.WriteLine("Ho la foto del tizio");
                    requestImage = Encoding.ASCII.GetBytes(Constants.DONT_NEED_IMG);
                    socket.SendTo(requestImage, requestImage.Length, SocketFlags.None, ip);
                }
            }
            socket.Close();
        }


        private void print()
        {
            foreach (KeyValuePair<string, int> pair in neighbors)
                Console.WriteLine("Key: {0} Values: {1}", pair.Key, pair.Value);
            Console.WriteLine("Dimensione neighborImage {0} ", neighborsImage.Count);
        }

        public void insert(string ipSender)
        {
            neighbors.TryAdd(ipSender, 0);
        }

        public string getUserFromIp(string ipSender)
        {
            foreach (KeyValuePair<string, int> pair in neighbors)
                if (pair.Key.IndexOf(ipSender) != -1)
                    return pair.Key.Substring(0, pair.Key.LastIndexOf("@"));
            return null;
        }

        void cleanMap()
        {
            while (true)
            {
                if (neighbors.Count == 0)
                    Console.WriteLine("Niente da pulire");

                //lista temporanea degli oggetti da rimuovere dalla mappa
                List<string> toRemove = new List<string>();

                foreach (KeyValuePair<string, int> pair in neighbors)
                {
                    if (pair.Value == 0)
                    {
                        Console.WriteLine("Rimuovo {0} ", pair.Key);
                        toRemove.Add(pair.Key);
                    }
                    else if (pair.Value == Constants.MAX_COUNTER)
                    {
                        neighbors[pair.Key] = 0;
                        Console.WriteLine("Rimesso a 0");
                    }
                }
                //rimuovo dalla mappa gli oggetti della lista temporanea
                foreach (string tmp in toRemove)
                {
                    int value;
                    neighbors.TryRemove(tmp, out value);
                }

                print();
                Thread.Sleep(Constants.CLEAN_TIME);
            }
        }

        private void sendMe()
        {
            IPAddress ip = IPAddress.Parse(Constants.MULTICAST);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket socketImg = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipepImg = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP_IMG);
            IPEndPoint ipep = new IPEndPoint(ip, Constants.PORT_UDP);
            socket.Connect(ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;
            string remoteIpAddress, remotePort;
            socketImg.Bind(ipepImg);

            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.HELL + Environment.UserName);
            byte[] requestImg = new byte[Constants.NEED_IMG.Length];
            while (true)
            {
                socket.Send(toBytes, toBytes.Length, SocketFlags.None);
                socketImg.ReceiveFrom(requestImg, requestImg.Length, SocketFlags.None, ref senderRemote);
                if (String.Compare(Encoding.UTF8.GetString(requestImg), Constants.NEED_IMG) == 0)
                {
                    Console.WriteLine("Server vuole la foto");
                    remoteIpAddress = ((IPEndPoint)senderRemote).Address.ToString();
                    remotePort = ((IPEndPoint)senderRemote).Port.ToString();
                    sendImg(remoteIpAddress);
                }
                Thread.Sleep(Constants.HELLO_TIME);
            }
            socket.Close();
        }


        private void quitMe()
        {
            IPAddress ip = IPAddress.Parse(Constants.MULTICAST);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(ip, Constants.PORT_UDP);
            socket.Connect(ipep);
            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.QUIT + Environment.UserName);
            socket.Send(toBytes, toBytes.Length, SocketFlags.None);
            socket.Close();
        }

        private void receiveImg(string neighbor)
        {

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP_IMG);
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);

            listener.Listen(10);

            Console.WriteLine("Waiting for img...");
            Socket handler = listener.Accept();
            byte[] buffer = new byte[sizeof(int)];
            handler.Receive(buffer, buffer.Length, SocketFlags.None);
            int sizeImg = BitConverter.ToInt32(buffer, 0);
            byte[] img = new byte[sizeImg];
            int temp = 0;
            SocketError error;
            while (true)
            {
                int bytesRec = handler.Receive(img, temp, sizeImg - temp, SocketFlags.None, out error);
                temp += bytesRec;
                if (temp == sizeImg) break;
            }
            //TODO non salvare foto che non serve
            FileStream fs = new FileStream(Constants.DEFAULT_DIRECTORY + "\\" + "user.jpg", FileMode.Create);
            fs.Write(img, 0, img.Length);
            fs.Flush(true);
            fs.Close();
            neighborsImage.Add(neighbor, img);
        }


        private void sendImg(string ipAddress)
        {
            //TODO verificare se la foto utente esiste, in caso contrario mandare 0 al server come dimensione e fargli prendere un placeholder da salvare nella mappa
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), Constants.PORT_TCP_IMG);
            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
            DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("AppData") + Constants.ACCOUNT_IMAGE);
            var accountImage = dir.GetFiles("*.accountpicture-ms")
             .OrderByDescending(f => f.LastWriteTime)
             .First();

            string imgPath = Environment.GetEnvironmentVariable("AppData") + Constants.ACCOUNT_IMAGE + accountImage.Name;

            byte[] imgLength = BitConverter.GetBytes((int)accountImage.Length);
            int sent = sender.Send(imgLength, sizeof(int), SocketFlags.None);
            byte[] img = GetImage(imgPath);
            int temp = 0;
            SocketError error;
            while (true)
            {
                if (accountImage.Length - temp >= 1400)
                    sent = sender.Send(img, temp, 1400, SocketFlags.None, out error);
                else
                    sent = sender.Send(img, temp, img.Length - temp, SocketFlags.None, out error);
                temp += sent;
                if (temp == accountImage.Length) break;
            }
        }

        private byte[] GetImage(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            //TODO long position = Seek(fs, "JFIF", 100);  => aumenta grandezza
            long position = Seek(fs, "JFIF", 0);
            byte[] b = new byte[Convert.ToInt32(fs.Length)];
            fs.Seek(position - 6, SeekOrigin.Begin);
            fs.Read(b, 0, b.Length);
            fs.Close();
            fs.Dispose();
            return b;
        }


        private long Seek(FileStream fs, string searchString, int startIndex)
        {
            char[] search = searchString.ToCharArray();
            long result = -1, position = 0, stored = startIndex,
            begin = fs.Position;
            int c;
            while ((c = fs.ReadByte()) != -1)
            {
                if ((char)c == search[position])
                {
                    if (stored == -1 && position > 0 && (char)c == search[0])
                        stored = fs.Position;
                    if (position + 1 == search.Length)
                    {
                        result = fs.Position - search.Length;
                        fs.Position = result;
                        break;
                    }
                    position++;
                }
                else if (stored > -1)
                {
                    fs.Position = stored + 1;
                    position = 1;
                    stored = -1;
                }
                else
                    position = 0;
            }

            if (result == -1)
                fs.Position = begin;
            return result;
        }

        private ConcurrentDictionary<string, int> neighbors;
        private Dictionary<string, byte[]> neighborsImage;
        private Thread listener, clean, sender;
    }
}