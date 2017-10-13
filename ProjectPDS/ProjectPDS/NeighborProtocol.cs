using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace ProjectPDS
{
    class NeighborProtocol
    {
        public NeighborProtocol()
        {
            neighbors = new Dictionary<string, MyCounter>();

            Console.WriteLine("Socket creata");
            listener = new Thread(listen);
            listener.Name = "listener";
            listener.Start();
            //sender = new Thread(sendMe);
            //sender.Name = "sender";
            //sender.Start();
            clean = new Thread(cleanMap);
            clean.Name = "cleaner";
            clean.Start();
        }
        ~NeighborProtocol() { listener.Join(); sender.Join(); clean.Join(); }

        public void listen()
        {

            //IPHostEntry ipHostInfo = Dns.GetHostEntryGetHostEntry(); Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            //IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            byte[] buffer = new byte[Constants.BUFLEN];
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
                        neighbors[senderID].plusCount();
                    else
                        neighbors.Add(senderID, new MyCounter(remoteIpAddress, Constants.MAX_COUNTER));
                }
                else if (String.Compare(command, Constants.QUIT, false) == 0)
                {
                    if (neighbors.ContainsKey(senderID))
                        neighbors.Remove(senderID);
                }
            }
            socket.Close();
        }
        void threadSend(int s) { }
        void threadListen(int s) { }
        void threadClean() { }
        public void print()
        {
            foreach (KeyValuePair<string, MyCounter> pair in neighbors)
                Console.WriteLine("Key: {0} Values: {1}", pair.Key, pair.Value.Counter);
        }


        void cleanMap()
        {
            while (true)
            {
                if (neighbors.Count == 0)
                    Console.WriteLine("Niente da pulire");
                //lista temporanea degli oggetti da rimuovere dalla mappa
                List<string> toRemove = new List<string>();

                foreach (KeyValuePair<string, MyCounter> pair in neighbors.ToList())
                {
                    if (pair.Value.Counter == 0)
                    {
                        toRemove.Add(pair.Key);
                    }
                    else if (pair.Value.Counter == Constants.MAX_COUNTER)
                    {
                        pair.Value.resetCount();
                        Console.WriteLine("Rimesso a 0");
                    }
                }
                //rimuovo dalla mappa gli oggetti della lista temporanea
                foreach (string tmp in toRemove)
                    neighbors.Remove(tmp);

                print();
                Thread.Sleep(Constants.CLEAN_TIME);
            }
        }

        public void sendMe()
        {
            IPAddress ip = IPAddress.Parse(Constants.MULTICAST);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(ip, Constants.PORT_UDP);
            socket.Connect(ipep);


            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.HELL + "SCEMO");
            while (true)
            {
                socket.Send(toBytes, toBytes.Length, SocketFlags.None);
                Thread.Sleep(Constants.HELLO_TIME);
            }
            //byte[] toBytes1 = Encoding.ASCII.GetBytes(Constants.QUIT + Environment.UserName);
            //socket.Send(toBytes1, toBytes1.Length, SocketFlags.None);
            socket.Close();
        }


        private void receive(int s) { }
        Dictionary<string, MyCounter> neighbors;

        private Thread listener, sender, clean;
    }

    //TODO ho usato 2 socket per sender e receiver
    //TODO ho cambiato il max counter da 3 a 1
}