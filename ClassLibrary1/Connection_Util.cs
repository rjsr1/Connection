using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
