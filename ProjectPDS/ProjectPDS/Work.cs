using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ProjectPDS
{
    class Work
    {
        public Work(string fileName, ArrayList receivers)
        {
            this.fileName = fileName;
            this.receivers = new ArrayList();
            this.receivers = receivers;
        }

        public string FileName { get => fileName; set => fileName = value; }
        public ArrayList Receivers { get => receivers; set => receivers = value; }

        private string fileName;
        private ArrayList receivers;


    }
}
