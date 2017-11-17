﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Media.Imaging;

namespace ProjectPDSWPF
{
    class NeighborProtocol
    {
        private NeighborProtocol()
        {
            Neighbors = new ConcurrentDictionary<string, Neighbor>();
            settings = Settings.getInstance;
            senderEvent = new ManualResetEvent(settings.Online);

            listener = new Thread(listen)
            {
                Name = "listener",
                IsBackground = true
            };
            listener.Start();
            sender = new Thread(sendMe)
            {
                Name = "sender",
                IsBackground = true
            };
            sender.Start();
            clean = new Thread(cleanMap)
            {
                Name = "cleaner",
                IsBackground = true
            };
            clean.Start();
        }
        ~NeighborProtocol() { listener.Join(); sender.Join(); clean.Join(); }

        private void listen()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket socketImg = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string recv, remoteIpAddress, remotePort;

            //EndPoint con le info del client che si collega
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP);

            //join ad un gruppo multicast
            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                new MulticastOption(IPAddress.Parse(Constants.MULTICAST)));

            //Non ricevo pacchetti da me stesso
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);

            socket.Bind(localEndPoint);

            while (true)
            {
                byte[] buffer = new byte[Constants.BUFLEN];
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
                Console.WriteLine("HELLO FROM {0} ", senderID);
                //Console.WriteLine("senderIPPPP {0} ", remoteIpAddress);
                if (String.Compare(command, Constants.HELL, false) == 0)
                {
                    if (!Neighbors.ContainsKey(senderID))
                        Neighbors.TryAdd(senderID, new Neighbor(senderID, null));
                    Neighbors[senderID].Counter = Constants.MAX_COUNTER;
                }
                else if (String.Compare(command, Constants.QUIT, false) == 0)
                    if (Neighbors.TryRemove(senderID, out Neighbor n))
                        neighborsEvent(senderID, null, false);


                byte[] requestImage;
                IPEndPoint ipImg = new IPEndPoint(((IPEndPoint)senderRemote).Address, Constants.PORT_UDP_IMG);
                socketImg.Connect(ipImg);
                if (Neighbors.TryGetValue(senderID, out Neighbor n1))
                {
                    if (n1.NeighborImage == null)
                    {

                        //chiedo la foto 
                        //Console.WriteLine("Non ho la foto del tizio, gliela chiedo");
                        requestImage = Encoding.ASCII.GetBytes(Constants.NEED_IMG);
                        socketImg.SendTo(requestImage, requestImage.Length, SocketFlags.None, ipImg);
                        receiveImg(senderID);
                    }
                    else
                    {
                        //TODO vedere se gestire updateFoto del tizio
                        requestImage = Encoding.ASCII.GetBytes(Constants.DONT_NEED_IMG);
                        socketImg.SendTo(requestImage, requestImage.Length, SocketFlags.None, ipImg);
                    }
                }
                else
                {
                    requestImage = Encoding.ASCII.GetBytes(Constants.DONT_NEED_IMG);
                    socketImg.SendTo(requestImage, requestImage.Length, SocketFlags.None, ipImg);
                }
            }
            socket.Close();
            socketImg.Close();
        }

        public string getUserFromIp(string ipSender)
        {
            foreach (KeyValuePair<string, Neighbor> pair in Neighbors)
                if (String.Compare(pair.Value.NeighborIp, ipSender) == 0)
                    return pair.Value.NeighborName;
            return null;
        }

        void cleanMap()
        {
            while (true)
            {
                if (Neighbors.Count == 0)
                    Console.WriteLine("Niente da pulire");

                //lista temporanea degli oggetti da rimuovere dalla mappa
                List<string> toRemove = new List<string>();

                foreach (KeyValuePair<string, Neighbor> pair in Neighbors)
                {
                    if (pair.Value.Counter == 0)
                    {
                        Console.WriteLine("Rimuovo {0} ", pair.Key);
                        toRemove.Add(pair.Key);
                    }
                    else if (pair.Value.Counter == Constants.MAX_COUNTER)
                        Neighbors[pair.Key].Counter = 0;
                }

                //rimuovo dalla mappa gli oggetti della lista temporanea
                foreach (string tmp in toRemove)
                {
                    if (Neighbors.TryRemove(tmp, out Neighbor value))
                        neighborsEvent(tmp, null, false);
                }
                Thread.Sleep(Constants.CLEAN_TIME);
            }
        }

        private void sendMe()
        {

            IPEndPoint ipMulticast = new IPEndPoint(IPAddress.Parse(Constants.MULTICAST), Constants.PORT_UDP);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket socketImg = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipImg = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP_IMG);

            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            string remoteIpAddress, remotePort;

            socketImg.Bind(ipImg);
            socketImg.ReceiveTimeout = 2000;
            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.HELL + Environment.UserName);

            while (senderEvent.WaitOne())
            {
                byte[] requestImg = new byte[Constants.NEED_IMG.Length];
                socket.SendTo(toBytes, toBytes.Length, SocketFlags.None, ipMulticast);
                try
                {
                    socketImg.ReceiveFrom(requestImg, requestImg.Length, SocketFlags.None, ref senderRemote);
                }
                catch (SocketException s)
                {
                    //Console.WriteLine("Eccezione {0} ", s.ToString());
                    //TODO cercare eccezioni timeout
                }

                if (String.Compare(Encoding.UTF8.GetString(requestImg), Constants.NEED_IMG) == 0)
                {
                    remoteIpAddress = ((IPEndPoint)senderRemote).Address.ToString();
                    remotePort = ((IPEndPoint)senderRemote).Port.ToString();
                    sendImg(remoteIpAddress);
                    Console.WriteLine("Foto Inviata");
                }
                Thread.Sleep(Constants.HELLO_TIME);
            }
            socket.Close();
            socketImg.Close();
        }


        public void quitMe()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Constants.MULTICAST), Constants.PORT_UDP);
            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.QUIT + Environment.UserName);
            socket.SendTo(toBytes, toBytes.Length, SocketFlags.None, ipep);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private void receiveImg(string neighbor)
        {

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP_IMG);
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);

            listener.Listen(1);

            Console.WriteLine("Waiting for img...");
            Socket handler = listener.Accept();
            byte[] buffer = new byte[sizeof(int)];
            handler.Receive(buffer, buffer.Length, SocketFlags.None);
            int sizeImg = BitConverter.ToInt32(buffer, 0);
            //Non ha la foto profilo
            if (sizeImg == -1)
            {
                Console.WriteLine("Prendo placeholder perchè il tizio nn ha la foto");
                string placeholderPath = Constants.PLACEHOLDER_IMAGE;
                int placeholderLength = (int)new FileInfo(placeholderPath).Length;
                FileStream fsp = File.OpenRead(placeholderPath);
                byte[] placeholderByte = new byte[placeholderLength];
                placeholderByte = File.ReadAllBytes(placeholderPath);
                fsp.Close();
                if (Neighbors.TryGetValue(neighbor, out Neighbor n))
                {
                    n.setImage(placeholderByte);
                    neighborsEvent(neighbor, placeholderByte, true);
                }
                return;
            }
            byte[] img = new byte[sizeImg];
            int temp = 0;
            while (true)
            {
                int bytesRec = handler.Receive(img, temp, sizeImg - temp, SocketFlags.None, out SocketError error);
                temp += bytesRec;
                if (temp == sizeImg) break;
            }
            if (Neighbors.TryGetValue(neighbor, out Neighbor n1))
            {
                n1.setImage(img);
                neighborsEvent(neighbor, img, true);
            }
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            listener.Close();
        }


        private void sendImg(string ipAddress)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), Constants.PORT_TCP_IMG);
            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
            DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("AppData") + Constants.ACCOUNT_IMAGE);

            FileInfo[] images = dir.GetFiles("*.accountpicture-ms");
            if (images.Length == 0)
            {
                sender.Send(BitConverter.GetBytes(-1), sizeof(int), SocketFlags.None);
                return;
            }

            var accountImage = images
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
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private byte[] GetImage(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
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

        public static NeighborProtocol getInstance
        {
            get
            {
                if (instance == null)
                    instance = new NeighborProtocol();
                return instance;
            }
        }

        public ConcurrentDictionary<string, Neighbor> Neighbors { get => neighbors; set => neighbors = value; }

        private ConcurrentDictionary<string, Neighbor> neighbors;
        private Thread listener, clean, sender;
        private static NeighborProtocol instance = null;
        private Settings settings;
        public static ManualResetEvent senderEvent;
        public delegate void modifyNeighbors(string neighborID, byte [] image, bool addOrRemove);
        public static event modifyNeighbors neighborsEvent;
    }
}