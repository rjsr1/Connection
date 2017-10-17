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
    public class Connection
    {
        public Connection(string option,string ip,int port)
        {
            option = option.ToLower();
            if (option == "server")
            {
                this.startServer(ip,port);
            }
            else if(option=="client")
            {
                this.startClient(ip,port);
            }
            else
            {
                throw new Exception("invalide opition");
            }
        }

        private void startClient(string ip, int port)
        {
            Socket c = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);


            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint IPEnd = new IPEndPoint(ipAddress, port);
           

        }

        private void startServer(string ip, int port)
        {
            throw new NotImplementedException();
        }

       
        
    }
}
