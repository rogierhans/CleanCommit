using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{


    class Node
    {
        public int ID;
        public string Name;


        public List<int> UnitsIndex;
        public List<int> StorageUnitsIndex;
        public List<int> RESindex;
        public List<double> Demand;
        //public List<double> SpinningReservesUP;
        //public List<double> SpinningReservesDown;


        public Node(int iD, string name, List<int> unitsIndex, List<int> storageUnitsIndex, List<int> resindex)
        {
            ID = iD;
            Name = name;
            UnitsIndex = unitsIndex;
            StorageUnitsIndex = storageUnitsIndex;
            RESindex = resindex;
            //Demand = demand;
            //SpinningReservesUP = spinningReservesUP;
            // SpinningReservesDown = spinningReservesDown;
        }

        public void PeturbDemand(Random RNG, double factor)
        {
            if (Demand != null)
                for (int t = 0; t < Demand.Count(); t++)
                {
                    double demand = Demand[t];
                    double range = demand / factor;
                    double delta = RNG.NextDouble() * range * 2 - range;
                    //Console.WriteLine("demand:{0} naar {1}", demand, demand + delta);
                    Demand[t] = demand + delta;
                }
        }

        public void SetDemand(List<double> values)
        {
            Demand = values;
        }

        public double NodalDemand(int time)
        {
            if (Demand != null)
            {
                return Demand[time % Demand.Count];
            }
            return 0;
        }

        public void PrintInfo()
        {
            Console.WriteLine("Name:{0}", Name);
            Console.WriteLine("Units:");
            foreach (var unit in UnitsIndex)
            {
                Console.WriteLine(unit);
            }

            Console.WriteLine("Demand:");
            if (Demand != null)
                Demand.Take(10).ToList().ForEach(demand => Console.WriteLine(demand));

            Console.WriteLine("ResGeneration:");


        }


        public void PrintStorage()
        {
            Console.WriteLine("StorageUnits {0}:", Name);
            foreach (var storageUnit in StorageUnitsIndex)
            {
                Console.WriteLine(storageUnit);
            }
        }
    }


}
