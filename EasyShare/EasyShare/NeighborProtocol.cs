﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.IO;
using System.Diagnostics;

namespace EasyShare
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
            cleanT = new Thread(cleanMap)
            {
                Name = "cleaner",
                IsBackground = true
            };
            cleanT.Start();
            waitForImage = new Thread(waitForImageRequest)
            {
                Name = "waitImage",
                IsBackground = true
            };
            waitForImage.Start();
        }

        private void listen()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string recv, remoteIpAddress;

            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_UDP);

            socket.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                new MulticastOption(IPAddress.Parse(Constants.MULTICAST)));

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);

            socket.Bind(localEndPoint);
            while (!ShutDown)
            {
                try
                {
                    byte[] buffer = new byte[Constants.BUFLEN];
                    int i = socket.ReceiveFrom(buffer, ref senderRemote);

                    Array.Resize(ref buffer, i);
                    recv = Encoding.UTF8.GetString(buffer);
                    remoteIpAddress = ((IPEndPoint)senderRemote).Address.ToString();
                    string command = recv.Substring(0, 4);
                    string senderName = recv.Substring(4);

                    string senderID = String.Concat(senderName, String.Concat("@", remoteIpAddress));
                    Console.WriteLine("HELLO FROM {0} ", senderID);
                    if (String.Compare(command, Constants.HELL) == 0)
                    {
                        if (!Neighbors.ContainsKey(senderID))
                            requestImg(senderID);

                        Neighbors[senderID].Counter = Constants.MAX_COUNTER;
                    }
                    else if (String.Compare(command, Constants.QUIT) == 0)
                        if (Neighbors.TryRemove(senderID, out Neighbor n))
                            if (neighborsEvent != null)
                                neighborsEvent(senderID, null, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("NeighborProtocol");
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(st.FrameCount - 1);
                    var line = frame.GetFileLineNumber();
                    Console.WriteLine("Error at line {0} ", line);
                    continue;
                }
            }
            socket.Close();
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
            while (!ShutDown)
            {
                List<string> toRemove = new List<string>();

                foreach (KeyValuePair<string, Neighbor> pair in Neighbors)
                {
                    if (pair.Value.Counter == 0)
                    {
                        Console.WriteLine("Rimuovo {0} ", pair.Key);
                        toRemove.Add(pair.Key);
                    }
                    else
                        Neighbors[pair.Key].Counter = 0;
                }


                foreach (string tmp in toRemove)
                    if (Neighbors.TryRemove(tmp, out Neighbor value))
                        if (neighborsEvent != null)
                            neighborsEvent(tmp, null, false);

                Thread.Sleep(Constants.CLEAN_TIME);
            }
        }

        public void clean()
        {
            List<string> toRemove = new List<string>();

            foreach (KeyValuePair<string, Neighbor> pair in Neighbors)
            {
                if (pair.Value.Counter == 0)
                {
                    Console.WriteLine("Rimuovo {0} ", pair.Key);
                    toRemove.Add(pair.Key);
                }
            }

            foreach (string tmp in toRemove)
                if (Neighbors.TryRemove(tmp, out Neighbor value))
                    if (neighborsEvent != null)
                        neighborsEvent(tmp, null, false);
        }



        private void sendMe()
        {
            IPEndPoint ipMulticast = new IPEndPoint(IPAddress.Parse(Constants.MULTICAST), Constants.PORT_UDP);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            byte[] toBytes = Encoding.UTF8.GetBytes(Constants.HELL + Environment.UserName);

            while (senderEvent.WaitOne())
            {
                if (ShutDown) break;
                try
                {
                    socket.SendTo(toBytes, toBytes.Length, SocketFlags.None, ipMulticast);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("NeighborProtocol");
                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(st.FrameCount - 1);
                    var line = frame.GetFileLineNumber();
                    Console.WriteLine("Error at line {0} ", line);
                    continue;
                }
                finally
                {
                    Thread.Sleep(Constants.HELLO_TIME);
                }
            }
            socket.Close();
        }

        public void quitMe()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Constants.MULTICAST), Constants.PORT_UDP);
            byte[] toBytes = Encoding.ASCII.GetBytes(Constants.QUIT + Environment.UserName);
            try
            {
                socket.SendTo(toBytes, toBytes.Length, SocketFlags.None, ipep);
            }
            catch
            {
            }
            finally
            {
                socket.Close();
            }

        }

        private void waitForImageRequest()
        {
            //TODO aggiungere timeout alle socket per evitare alte latenze nella fase di NeighborProtocol
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Constants.PORT_TCP_IMG);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket handler = null;
            listener.Bind(localEndPoint);
            listener.Listen(10);
            SocketError error;
            while (true)
            {
                try
                {
                    handler = listener.Accept();
                    DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("AppData") + Constants.ACCOUNT_IMAGE);

                    FileInfo[] images = dir.GetFiles("*.accountpicture-ms");
                    if (images.Length == 0)
                    {
                        handler.Send(BitConverter.GetBytes(-1), 0, sizeof(int), SocketFlags.None, out error);

                        if (error != SocketError.Success)
                            throw new SocketException();
                    }
                    else
                    {

                        var accountImage = images
                     .OrderByDescending(f => f.LastWriteTime)
                     .First();

                        string imgPath = Environment.GetEnvironmentVariable("AppData") + Constants.ACCOUNT_IMAGE + accountImage.Name;
                        byte[] imgLength = BitConverter.GetBytes((int)accountImage.Length);

                        int sent = handler.Send(imgLength, 0, sizeof(int), SocketFlags.None, out error);

                        if (error != SocketError.Success)
                            throw new SocketException();

                        byte[] img = GetImage(imgPath);

                        sent = 0;

                        while (sent < img.Length)
                        {
                            if (accountImage.Length - sent >= Constants.PACKET_SIZE)
                                sent += handler.Send(img, sent, Constants.PACKET_SIZE, SocketFlags.None, out error);
                            else
                                sent += handler.Send(img, sent, img.Length - sent, SocketFlags.None, out error);

                            if (error != SocketError.Success)
                                throw new SocketException();
                        }
                    }

                }
                catch (SocketException e)
                {
                    Console.WriteLine("NeighborProtocol");
                    var st = new StackTrace(e, true);
                    var frame = st.GetFrame(st.FrameCount - 1);
                    var line = frame.GetFileLineNumber();
                    Console.WriteLine("Error at line {0} ", line);
                }
                finally
                {
                    if (handler.Connected)
                        handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
        }

        private void requestImg(string neighbor)
        {
            String address = neighbor.Substring(neighbor.IndexOf("@") + 1);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(address), Constants.PORT_TCP_IMG);
            Socket receiver = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            int received = 0;

            try
            {
                byte[] img;
                receiver.Connect(iPEndPoint);

                byte[] buffer = new byte[sizeof(int)];
                received = receiver.Receive(buffer, 0, buffer.Length, SocketFlags.None, out SocketError sockError);

                if (sockError != SocketError.Success)
                    throw new SocketException();

                int sizeImg = BitConverter.ToInt32(buffer, 0);
                if (sizeImg == -1)
                {
                    string placeholderPath = App.currentDirectoryResources + "/guest.png";
                    if (String.IsNullOrEmpty(placeholderPath)) return;
                    img = File.ReadAllBytes(placeholderPath);
                }
                else
                {
                    img = new byte[sizeImg];
                    received = 0;
                    while (received < img.Length)
                    {
                        if (img.Length - received > Constants.PACKET_SIZE)
                            received += receiver.Receive(img, received, Constants.PACKET_SIZE, SocketFlags.None, out sockError);

                        else received += receiver.Receive(img, received, img.Length - received, SocketFlags.None, out sockError);

                        if (sockError != SocketError.Success)
                            throw new SocketException();
                    }
                }

                //TODO TESTARE
                Neighbor n = new Neighbor(neighbor, img);
                if (Neighbors.TryAdd(neighbor, n))
                    if (neighborsEvent != null)
                        neighborsEvent(neighbor, img, true);
            }
            catch (SocketException e)
            {
                Console.WriteLine("NeighborProtocol");
                var st = new StackTrace(e, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
            catch (Exception e)
            {
                Console.WriteLine("NeighborProtocol");
                var st = new StackTrace(e, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Error at line {0} ", line);
            }
            finally
            {
                if (receiver != null)
                {
                    if (receiver.Connected)
                        receiver.Shutdown(SocketShutdown.Both);
                    receiver.Close();
                }
            }
        }


        private byte[] GetImage(string path)
        {
            byte[] b = null;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open);
                long position = Seek(fs, "JFIF", 0);
                b = new byte[Convert.ToInt32(fs.Length)];
                fs.Seek(position - 6, SeekOrigin.Begin);
                fs.Read(b, 0, b.Length);
                return b;
            }
            catch
            {
                Console.WriteLine("NeighborProtocol");
                return BitConverter.GetBytes(-1);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
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
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new NeighborProtocol();
                    }
                }
                return instance;
            }
        }

        public ConcurrentDictionary<string, Neighbor> Neighbors { get => neighbors; set => neighbors = value; }

        private ConcurrentDictionary<string, Neighbor> neighbors;
        private Thread listener, cleanT, sender, waitForImage;
        private static NeighborProtocol instance = null;
        private Settings settings;
        public static ManualResetEvent senderEvent;
        public delegate void modifyNeighbors(string neighborID, byte[] image, bool addOrRemove);
        public static event modifyNeighbors neighborsEvent;
        public static bool ShutDown = false;
        private static object syncLock = new object();
    }
}
