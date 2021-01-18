using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace CleanCommit.Instance
{
    class PowerSystem
    {

        public string Name;
        public List<Unit> Units;
        public List<Node> Nodes;
        public List<ResGeneration> ResGenerations;
        public List<TransmissionLineAC> LinesAC;
        public List<TransmissionLineDC> LinesDC;
        public List<StorageUnit> StorageUnits;
        public List<Inflow> Inflows;
        public double[,] PDTF;
        public double VOLL = 10000;
        public double VOLR = 1000;
        public double RatioReserveDemand = 0.1;
        //public ConstraintConfiguration ConstraintConfiguration;

        public PowerSystem(string name, List<Unit> units, List<Node> nodes, List<TransmissionLineAC> linesAC, List<TransmissionLineDC> linesDC, List<StorageUnit> storageUnits, List<Inflow> inflows, List<ResGeneration> resgenerations, double[,] pDTF)
        {
            Name = name;
            Units = units;
            ResGenerations = resgenerations;
            Nodes = nodes;
            LinesAC = linesAC;
            LinesDC = linesDC;
            StorageUnits = storageUnits;
            PDTF = pDTF;
            Inflows = inflows;
        }


        public void UnCluster()
        {
            List<Unit> allNewUnits = new List<Unit>();
            int ID = 0;
            foreach (var node in Nodes)
            {
                var unitNewIndices = new List<int>();
                foreach (var unitIndex in node.UnitsIndex)
                {
                    var unit = Units[unitIndex];
                    int count = unit.Count;
                    var newUnits = unit.CreateCopies(ID, count);
                    allNewUnits.AddRange(newUnits);
                    unitNewIndices.AddRange(newUnits.Select(u => u.ID));
                    ID = ID + count;
                }
                node.UnitsIndex = unitNewIndices;
            }
            Units = allNewUnits;
        }

        public void PeturbDemand(double factor)
        {
            Random Rng = new Random();
            Nodes.ForEach(node => node.PeturbDemand(Rng, factor));
        }

        public double TotalDemand() {
            return Nodes.Where(node => node.Demand != null).Sum(node => node.Demand.Sum());
        }

        private void AddResForCopperPlate(Dictionary<string, ResGeneration> dict, string resName, ResGeneration generation)
        {
            if (!dict.ContainsKey(resName))
            {
                dict[resName] = generation;
            }
            else
            {
                dict[resName].CombineGeneration(generation);
            }

        }

        public override string ToString()
        {
            return Name.Split('.').First() + RatioReserveDemand;//+ ConstraintConfiguration;
        }

        public  string NameOK()
        {
            return Name.Split('.').First();// + PercentageDemandForReserve;//+ ConstraintConfiguration;
        }
        public void PrintUnits()
        {
            Units.ForEach(unit => unit.GetInfo());
        }

        //public void Test(ConstraintConfiguration ConstraintConfiguration)
        //{
        //    for (int t = 0; t < ConstraintConfiguration.totalTime; t++)
        //    {
        //        double totalDemand = 0;
        //        double totalRes = 0;
        //        double totalGenerationCap = 0;
        //        for (int n = 0; n < Nodes.Count; n++)
        //        {

        //            double demandAtNode = 0;
        //            double resAtNode = 0;
        //            double generationAtNode = 0;

        //            demandAtNode += Nodes[n].NodalDemand(t);
        //            resAtNode += Nodes[n].RESindex.Select(id => ResGenerations[id].ResValues[t]).Sum();// .ResGeneration.Select(res => res.ResValues[t]).Sum();
        //            foreach (var unitIndex in Nodes[n].UnitsIndex)
        //            {
        //                var unit = Units[unitIndex];
        //                generationAtNode += unit.PMax * unit.Count;
        //            }
        //            totalDemand += demandAtNode;
        //            totalRes += resAtNode;
        //            totalGenerationCap += generationAtNode;
        //            //Console.WriteLine("{0}\t {1} \t  {2} \t {3} \t {4}", demandAtNode, resAtNode, generationAtNode, (demandAtNode - (resAtNode + generationAtNode)), Nodes[n].Name);
        //        }
        //        Console.WriteLine("Demand: {0}\t Res:{1} \t  Generation{2} \t Rest{3}", totalDemand, totalRes, totalGenerationCap, (totalDemand - (totalRes + totalGenerationCap)));
        //    }

        //}
        public void GetTableUnits()
        {
            ;
            List<string> header = new List<string>();
            bool function = Units[0].FSC != -1;
            header.Add("ID");
            header.Add("$\\productionMax$");
            header.Add("$\\productionMin$");
            header.Add("a");
            header.Add("b");
            header.Add("c");
            header.Add("$\\rampUp$");
            header.Add("$\\rampDown$");
            header.Add("SU");
            header.Add("SD");
            header.Add("Up");
            header.Add("Down");
            if (function)
            {
                header.Add("FSC");
                header.Add("VSC");
                header.Add("$\\lambda$");
            }
            else
            {
                header.Add("Interval");
                header.Add("Cost");
            }
            var lines = new List<string>() { "\\begin{table}[H]", "\\caption{Units of instance " + Name + "}", "\\scalebox{0.7}{", "\\begin{tabular}{" + header.Select(x => "c").Aggregate((a, b) => a + b) + "} \\hline" };
            lines.Add(String.Join("&", header) + "\\\\ \\hline");

            int n = Units.Count;
            if (n > 20)
            {
                lines.AddRange(Units.Take(10).Select(unit => unit.ToRow()));
                lines.Add(String.Join("&", header.Select(h => "...").ToList()) + "\\\\");
                lines.AddRange(Units.Skip(n - 10).Select(unit => unit.ToRow()));
            }
            else
            {
                lines.AddRange(Units.Take(20).Select(unit => unit.ToRow()));
            }
            lines.Add("\\hline \\end{tabular}}");
            lines.Add("\\end{table}");
            U.Write(@"E:\Table\InstancesTable\all", lines);
        }
    }
}
