using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPDS
{

    class MyCounter
    {
        public MyCounter(string ip, int val)
        {
            this.IpAddr = ip;
            this.Counter = val;
        }
        public void plusCount()
        {
            if (this.counter < Constants.MAX_COUNTER)
                this.counter++;
        }

        public void minusCount()
        {
            if (this.counter > 0)
                this.counter--;
        }
        public void resetCount()
        {
            this.counter = 0;
        }
        public string IpAddr { get => ipAddr; set => ipAddr = value; }
        public int Counter { get => counter; set => counter = value; }


        private string ipAddr;
        private int counter;
    }
}
