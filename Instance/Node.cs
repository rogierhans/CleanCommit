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
        //public List<double> SpinningReservesUP;
        //public List<double> SpinningReservesDown;

        public string ToFile()
        {
            List<string> Properties = new List<string>
            {
                //Properties.Add("<node>");
                0.ToString(),
                Name,
                "[" + String.Join(":", Units.Select(unit => unit.ID)) + "]",
                "[" + String.Join(":", StorageUnits.Select(unit => unit.ID)) + "]",
                "[" + String.Join(":", RES.Select(unit => unit.ID)) + "]"
            };
            //Properties.Add("Demand;" + String.Join(";", Demand.Select(demand => Math.Round(demand,2).ToString())));
            //Properties.Add("Upward Reserves;" + String.Join(";", SpinningReservesUP.Select(demand => demand.ToString())));
            //Properties.Add("Downward Reserves;" + String.Join(";", SpinningReservesDown.Select(demand => demand.ToString())));
            //ResGeneration.ForEach(resGen => Properties.Add(resGen.Name + ";" + String.Join(";", resGen.ResValues.Select(res => Math.Round(res,2)))));
            //Properties.Add("</node>");
            return String.Join(";", Properties);
        }
        public Node() { }
        public Node(int iD, string name, List<Unit> UnitList, List<StorageUnit> StorageList, List<ResGeneration> RESList)
        {
            ID = iD;
            Name = name;
            Units = UnitList;
            StorageUnits = StorageList;
            RES = RESList;
            //Demand = demand;
            //SpinningReservesUP = spinningReservesUP;
            // SpinningReservesDown = spinningReservesDown;
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

        public void PrintInfo()
        {
            Console.WriteLine("Name:{0}", Name);
            Console.WriteLine("Units:");
            foreach (var unit in Units)
            {
                Console.WriteLine(unit);
            }

            Console.WriteLine("Demand:");
            if (Demand != null)
                Demand.Take(10).ToList().ForEach(demand => Console.WriteLine(demand));

            Console.WriteLine("ResGeneration:");


        }

        public double PotentialExport(PowerSystem PS)
        {
            return Units.Sum(x => x.pMax) + RES.Sum(x => x.ResValues.Max()) + StorageUnits.Sum(x => x.MaxDischarge) ;
        }


        public void PrintStorage()
        {
            Console.WriteLine("StorageUnits {0}:", Name);
            foreach (var storageUnit in StorageUnits)
            {
                Console.WriteLine(storageUnit);
            }
        }

        public void WriteInfo(List<TransmissionLineDC> dclines, List<TransmissionLineAC> aclines)
        {
            List<string> lines = new List<string>();
            lines.Add(Name);
            lines.Add("Total Generation:" + (Units.Sum(x => x.pMax) + RES.Sum(x => x.ResValues.Max()) + StorageUnits.Sum(x => x.MaxCharge)));
            lines.Add("Total Unit:" + (Units.Sum(x => x.pMax)));
            lines.Add("ResValues Unit:" + RES.Sum(x => x.ResValues.Max()));
            lines.Add("StorageUnits Unit:" + StorageUnits.Sum(x => x.MaxCharge));
            bool RESloadfail = false;
            bool RESflowfail = false;
            List<string> longLines = new List<string>();
            double maxResdemand = double.MinValue;
            double maxResload = double.MinValue;
            double maxResflow = double.MinValue;

            if (Demand != null)
                for (int t = 0; t < Demand.Count; t++)
                {
                    string line = t + ":";
                    line += (RESloadfail ? 1 : 0).ToString() + (RESflowfail ? 1 : 0).ToString();
                    double units = Units.Sum(x => x.pMax);
                    double storage = StorageUnits.Sum(x => x.MaxCharge);
                    double R = RES.Sum(x => x.ResValues[t]);
                    double RESload = Demand[t] - units - R - storage;
                    double RESloadInflow = RESload - aclines.Where(tline => tline.From == this || tline.To == this).Sum(tline => tline.MaxCapacity) - dclines.Where(tline => tline.From == this || tline.To == this).Sum(tline => tline.MaxCapacity);
                    line += "\t" + Demand[t] + "\tRESDemand" + (Demand[t] - R) + "\tRESLOAD" + RESload + "\tRESFLOW" + RESloadInflow + "\tUNITS " + units + "\tstorage " + storage + "\tR " + R;
                    bool resloadfail = RESload > 0;
                    bool resINFLOWfail = RESloadInflow > 0;
                    RESloadfail |= resloadfail;
                    RESflowfail |= resINFLOWfail;
                    maxResdemand = Math.Max(maxResdemand, (Demand[t] - R));
                    maxResload = Math.Max(maxResload, RESload);
                    maxResflow = Math.Max(maxResflow, RESloadInflow);
                    longLines.Add(line);
                }
            lines.Add("maxResdemand:" + maxResdemand);
            lines.Add("maxResload:" + maxResload);
            lines.Add("maxResflow:" + maxResflow);
            lines.Add("RESloadfail:" + RESloadfail);
            lines.Add("RESINflowfail:" + RESflowfail);
            lines.AddRange(longLines);
            File.WriteAllLines(@"C:\Users\Rogier\Desktop\TEMPDumo2\" + Name + ".csv", lines);
        }


    }
}
