using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace CleanCommit.Instance
{
    [Serializable]
    public class PowerSystem
    {

        public string Name;
        public List<Unit> Units;
        public List<Node> Nodes;
        public List<ResGeneration> ResGenerations;
        public List<TransmissionLineAC> LinesAC;
        public List<TransmissionLineDC> LinesDC;
        public List<StorageUnit> StorageUnits;
        public double[,] PDTF;
        public double VOLL = 10000;
        public double VOLR = 1000;
        public double DSRCost = 300;
        public List<double> Reserves;



        public PowerSystem() { }
        public PowerSystem(string name, List<Unit> units, List<Node> nodes, List<TransmissionLineAC> linesAC, List<TransmissionLineDC> linesDC, List<StorageUnit> storageUnits, List<ResGeneration> resgenerations, double[,] pDTF)
        {
            Name = name;
            Units = units;
            ResGenerations = resgenerations;
            Nodes = nodes;
            LinesAC = linesAC;
            LinesDC = linesDC;
            StorageUnits = storageUnits;
            PDTF = pDTF;
        }


        public List<double> GetTotalDemand()
        {
            Dictionary<int, double> time2Demand = new Dictionary<int, double>();
            foreach (var node in Nodes)
            {
                if (node.Demand != null)
                    for (int t = 0; t < node.Demand.Count; t++)
                    {
                        if (!time2Demand.ContainsKey(t)) { time2Demand[t] = 0; }
                        time2Demand[t] += node.Demand[t];
                    }
            }
            return time2Demand.Values.ToList();
        }
        public double GetReserve(int t) {
            return Reserves[t % Reserves.Count()];
        }

        public void PeturbDemand(double factor)
        {
            Random Rng = new Random();
            Nodes.ForEach(node => node.PeturbDemand(Rng, factor));
        }

        public double TotalDemand()
        {
            return Nodes.Where(node => node.Demand != null).Sum(node => node.Demand.Sum());
        }


        public override string ToString()
        {
            return Name.Split('.').First();
        }
      
    }
}
