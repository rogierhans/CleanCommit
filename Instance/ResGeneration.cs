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
        }

        public double GetValue(int t, int timeOffset) {
            return ResValues[(t+ timeOffset) % ResValues.Count];
        }


        public override string ToString()
        {
            return ID + ";" + Name + ";[" + String.Join(":", ResValues) + "]";
        }
    }
}
