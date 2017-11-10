using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Connection
{
    class Connection_Util
    {
        public static string ASCIITag(int n)
        {
            int unicode = n;
            char character = (char)unicode;
            return character.ToString();
        }
        public static void WriteOnLog(StreamWriter stream, string s)
        {

            stream.WriteLineAsync(DateTime.Now+" "+s);
            stream.Flush();
        }


    }
}
