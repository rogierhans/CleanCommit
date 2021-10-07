using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{
    [Serializable]
    public class ResGeneration
    {
        public string Type = "None";
        public string Name;
        public string ID;
        public List<double> ResValues;

        public ResGeneration() { }
        public ResGeneration(string id, List<double> resValues,string name)
        {
            ID = id;
            ResValues = resValues;
            Name = name;//.Replace("\t", "").Replace(" ","");
            //Console.WriteLine("*{0}*", name); Console.WriteLine("*{0}*", Name);
        }

        public double GetValue(int t, int timeOffset) {
            return ResValues[(t+ timeOffset) % ResValues.Count];
        }


        public string ToFile()
        {
            return ID + ";" + Name + ";[" + String.Join(":", ResValues) + "]";
        }
    }
}
