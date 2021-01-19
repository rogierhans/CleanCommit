using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{
    class ResGeneration
    {
        public string Name;
        public string ID;
        public List<double> ResValues;
        public ResGeneration(string id, List<double> resValues,string name)
        {
            ID = id;
            ResValues = resValues;
            Name = name;//.Replace("\t", "").Replace(" ","");
            //Console.WriteLine("*{0}*", name); Console.WriteLine("*{0}*", Name);
        }

        public void CombineGeneration(ResGeneration res2)
        {
            if (res2.Name != Name || res2.ResValues.Count != ResValues.Count)
            {
                throw new Exception("Go fuck yourself!" + res2.ResValues.Count + ResValues.Count + res2.Name + Name);
            }
            for (int i = 0; i < res2.ResValues.Count; i++)
            {
                ResValues[i] += res2.ResValues[i];
            }
        }
    }
}
