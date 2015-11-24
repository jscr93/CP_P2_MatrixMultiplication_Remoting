using ChatServerDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplicationClient
{
    class Client
    {
        public static void Register(string serverIp)
        {
            HttpChannel channel = new HttpChannel();
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownClientType(
                typeof(MatrixServer), "http://" + serverIp + ":12345/ChatServer");
        }
    }
}
