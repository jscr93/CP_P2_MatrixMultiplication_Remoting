using MatrixServerDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create and register the channel
            HttpChannel channel = new HttpChannel(12345);
            ChannelServices.RegisterChannel(channel);
            Console.WriteLine("Starting MatrixServer");

            // Register the ChatServer for remoting
            RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(MatrixServer),
                    "MatrixServer",
                    WellKnownObjectMode.Singleton);

            Console.WriteLine("Press return to exit MatrixServer.");
            Console.ReadLine();
        }
    }
}
