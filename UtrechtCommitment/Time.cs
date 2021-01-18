using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtrechtCommitment
{
    class Time
    {
        public int ID;
        public double Demand;
        public Time(string cell) {
            Demand = double.Parse(cell);
            Console.WriteLine(Demand);
            //Dit gaat voor problemen zorgen
            ID = Utils.GetTimeID();
        }
    }
}
