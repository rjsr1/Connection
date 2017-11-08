using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{

    public class Receive_Args:EventArgs
    {
        private string message;
        //private string date_hour;

        public Receive_Args(string m)
        {
            message = m;
            
        }
        public string Message
        {
            get { return message; }
        }
       // public string Data_Hour
       //  {
       //     get { return this.date_hour; }
       // }
    }
}
