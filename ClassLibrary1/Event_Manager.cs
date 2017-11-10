using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Connection
{

    public class Receive_Args:EventArgs
    {
        private string message;
        

        public Receive_Args(string m)
        {
            message = m;
            
        }
        public string Message
        {
            get { return message; }
        }
       
    }
    public class Receive_Args_Server : EventArgs
    {
        private string message;
        private DateTime dateTime;
        private Socket clientSocket;

        public Receive_Args_Server(string m,DateTime dateTime,Socket socket)
        {
            this.message = m;
            this.dateTime = dateTime;
            this.clientSocket = socket;
        }

        public string GetMessage
        {
            get { return message; }
        }

        public DateTime GetDateTime
        {
            get { return this.dateTime; }
        }
        public Socket GetSocket
        {
            get { return this.clientSocket; }
        }

    }
}
