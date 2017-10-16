using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;



namespace ClassLibrary1
{
    public class Class1
    {
        public static void ClientTcp(Int32 port,string server)
        {
            TcpClient client = new TcpClient(server, port);

        }
       
    }
}
