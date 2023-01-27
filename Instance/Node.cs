using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CleanCommit.Instance
{

    [Serializable]
    public class Node
    {
        public int ID;
        public string Name;
        public List<Unit> Units;
        public List<StorageUnit> StorageUnits;
        public List<ResGeneration> RES;
        public List<double> Demand;
        public double DemandResonsePotential;
        public double P2GCapacity;

        public Node() { }
        public Node(int iD, string name, List<Unit> UnitList, List<StorageUnit> StorageList, List<ResGeneration> RESList)
        {
            ID = iD;
            Name = name;
            Units = UnitList;
            StorageUnits = StorageList;
            RES = RESList;
        }

        public Node CopyWithExport(List<double> Export)
        {
            var newNode = new Node(ID, Name, Units, StorageUnits, RES);
            if (Demand == null)
            {
                newNode.Demand = Export;
            }
            else
            {
                newNode.Demand = Demand.Zip(Export, (a, b) => a + b).ToList();
            }
            return newNode;
        }

        public void PeturbDemand(Random RNG, double factor)
        {
            if (Demand != null)
            {
                for (int t = 0; t < Demand.Count(); t++)
                {
                    double demand = Demand[t];
                    double range = demand / factor;
                    double delta = RNG.NextDouble() * range * 2 - range;
                    //Console.WriteLine("demand:{0} naar {1}", demand, demand + delta);
                    Demand[t] = demand + delta;
                }
            }
        }

        public void SetDemand(List<double> values)
        {
            Demand = values;
        }

        public double NodalDemand(int time, int timeOFFset)
        {
            if (Demand != null)
            {
                return Demand[(time + timeOFFset) % Demand.Count];
            }
            return 0;
        }

        public double GetTotalDemand(int totalTime)
        {
            if (Demand != null)
            {
                return Demand.Take(totalTime).Sum();
            }
            return 0;
        }

        public double PotentialExport(PowerSystem PS)
        {
            return Units.Sum(x => x.pMax) + RES.Sum(x => x.ResValues.Max()) + StorageUnits.Sum(x => x.MaxDischarge) ;
        }
        public override string ToString()
        {
            List<string> Properties = new List<string>
            {
                0.ToString(),
                Name,
                "[" + String.Join(":", Units.Select(unit => unit.ID)) + "]",
                "[" + String.Join(":", StorageUnits.Select(unit => unit.ID)) + "]",
                "[" + String.Join(":", RES.Select(unit => unit.ID)) + "]"
            };
            return String.Join(";", Properties);
        }

    }
}
