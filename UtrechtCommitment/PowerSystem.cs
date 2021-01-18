using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace UtrechtCommitment
{
    class PowerSystem
    {
        //public List<Unit> Units = Utils.ReadFile(@"C:\Users\4001184\Documents\UCData\UC.csv").Select(line => new Unit(line)).ToList();
        public List<Unit> Units = Utils.ReadFile(@"C:\Users\Rogier\Desktop\UCTest\units.csv").Skip(1).Select(line => new Unit(line,true)).ToList();
        //public List<Time> Times = Utils.ReadFile(@"C:\Users\4001184\Documents\UCData\Demand.csv")[0].Split(';').ToList().Select(s => new Time(s)).ToList();
        public List<Time> Times = Utils.ReadFile(@"C:\Users\Rogier\Desktop\UCTest\demand.csv").Skip(1).Take(50).Select(s => new Time(s.Split(',')[6])).ToList();


    }
}
