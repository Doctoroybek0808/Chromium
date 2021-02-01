using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromiumTest
{
    public class FsDLL
    {
        public event EventHandler<string> DataReady;

        public void dofunc(string data)
        {
            //Console.Write("asfasf");
            DataReady?.Invoke(this, data);
        }
    }
}
