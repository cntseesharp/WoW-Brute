using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;

    class SocketStreamer:IDisposable
    {
        Socket socket;
        byte[] respond = new byte[0xffff];

        public SocketStreamer(string address)
        {
            IPAddress ipAddr = Dns.GetHostEntry(address).AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 3724);
            this.socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(ipEndPoint);
            
        }

        public Packet SendPacket(Packet p)
        {
            socket.Send(p.Content);
            socket.Receive(respond);
            p.Flush();
            p.Write(respond);
            return p;
        }

        public void Dispose()
        {
            socket.Close();
        }
    }
