using MatrixServerDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplicationClient
{
    class Client
    {
        public static MatrixServer server;
        public static string clientIP = GetLocalIPAddress();
        public static string clientName;

        private static HttpChannel channel = null;
        public static void Register(string serverIP)
        {
            if (channel == null)
            {
                channel = new HttpChannel();
                ChannelServices.RegisterChannel(channel);
                RemotingConfiguration.RegisterWellKnownClientType(
                typeof(MatrixServer), "http://" + serverIP + ":12345/MatrixServer");
            }
            
            //typeof(MatrixServer), "http://" + serverIP + ":12345/ChatServer");
            server = new MatrixServer();
            server.AddClient(GetNewName());
        }

        public static string GetNewName()
        {
            string name = GetLocalIPAddress();
            while (server.isNameAlready(name))
                name += "A";
            clientName = name;
            return name;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Dirección IP Local no encontrada");
        }
    }
}
