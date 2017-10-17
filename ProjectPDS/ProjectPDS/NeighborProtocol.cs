using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace ProjectPDS
{
    class NeighborProtocol
    {
        public NeighborProtocol()
        {
            neighbors = new ConcurrentDictionary<string, int>();
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
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP);

            socket.Bind(localEndPoint);
            while (true)
            {
                byte[] buffer = new byte[Constants.BUFLEN];
                Console.WriteLine("Waiting for connections...");
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
            }
            socket.Close();
        }


        private void print()
        {
            foreach (KeyValuePair<string, int> pair in neighbors)
                Console.WriteLine("Key: {0} Values: {1}", pair.Key, pair.Value);
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
            IPEndPoint ipep = new IPEndPoint(ip, Constants.PORT_UDP);
            socket.Connect(ipep);


            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.HELL + Environment.UserName);
            while (true)
            {
                socket.Send(toBytes, toBytes.Length, SocketFlags.None);
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

        ConcurrentDictionary<string, int> neighbors;
        private Thread listener, clean, sender;
    }
}