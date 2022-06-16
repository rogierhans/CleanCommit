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
        public List<double> Reserves;
        public double P2GYearlyDemand = 1400 * 1000 * 1000;
       // private double RatioReserveDemand = 0;//0.01;

        public double DRSCost = 300;

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
         //   Reserves = GetTotalDemand().Select(x => x * RatioReserveDemand).ToList(); 
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
            return Name.Split('.').First();//+ ConstraintConfiguration;
        }

        public string NameOK()
        {
            return Name.Split('.').First();// + PercentageDemandForReserve;//+ ConstraintConfiguration;
        }
        public void PrintUnits()
        {
            Units.ForEach(unit => unit.GetInfo());
        }

     
        public PowerSystem GetPowerSystemAtNode(Node node, List<double> nodalInjection, List<double> nodalReserve)
        {
            var newNode  = node.CopyWithExport(nodalInjection);
            var newUnits = node.Units;
            var newRES = node.RES;
            var newStore = node.StorageUnits;
            var newPS = new PowerSystem("", newUnits, new List<Node>() { newNode }, new List<TransmissionLineAC>(), new List<TransmissionLineDC>(), newStore, newRES, new double[0, 0]);
            newPS.Reserves = nodalReserve.ToList();
            return newPS;
        }

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
        public void Dump() {
            Nodes.ForEach(node => node.WriteInfo(LinesDC, LinesAC));
        }

        public void REDUCEUNITS()
        {
            var newUnits = new List<Unit>();
            for (int nodeID = 0; nodeID < Nodes.Count; nodeID++)
            {
                var node = Nodes[nodeID];
                var unit = new Unit();
                var OGUnits = node.Units;
                var pmax = OGUnits.Sum(x => x.PMax);
                var a = OGUnits.Sum(x => x.A);
                var b = OGUnits.Sum(x => x.B);
                var start = OGUnits.Sum(x => x.StartCostInterval.First());
                double[] costs = new double[] { start };
                int[] intervall = new int[] { 0 };


                //if (Demand.Keys.Contains(NUTS1Name))
                {
                    unit.SetGenerationCost(a, b, 0.0);
                    unit.SetGenerationLimits(0, pmax);
                    unit.SetRampLimits(pmax, pmax, pmax, pmax);
                    unit.SetMinTime(1, 1);
                    unit.SetSUInterval(costs, intervall);
                    unit.PrintType = "MIXED";
                    newUnits.Add(unit);
                }
                node.Units = new List<Unit>() { unit };
            }
            Units = newUnits;
        }




        //public void WriteToFile(string name, Node n, List<double> injection, int totalT)
        //{

        //    List<string> lines = new List<string>();
        //    bool quadratic = Units.Where(unit => unit.C >= 0.001).Count() > 0;
        //    bool timeDep = Units[0].StartCostInterval.Count() > 1 || Units[0].StartCostInterval[0] == -1;
        //    bool transmission = false;

        //    AddToList(lines, "type", new List<string>() { "quadratic=" + quadratic, "timeDep=" + timeDep, "transmission=" + transmission, "time=" + Nodes[0].Demand.Count() });
        //    string unitHeader = "ID;Count;pMin;pMax;a;b;c;RU;RD;SU;SD;MinUp;MinDown;FSC;VSC;Lambda;SCV;SCI";
        //    AddToList(lines, "units", n.Units.Select(unit => unit.ToFile()).ToList(), unitHeader);
        //    string storageHeader = "ID;Name;Max Charge;Max Discharge;Max Enenergy;Charge Efficiency;Discharge Efficiency";
        //    AddToList(lines, "storage", n.StorageUnits.Select(sUnit => sUnit.ToFile()).ToList(), storageHeader);
        //    string infowheader = "ID;Storage ID; Inflow Values";
        //    AddToList(lines, "inflows", n.StorageUnits.Select(su => "0" + ";" + su.ID + ";[" + String.Join(":", su.Inflow) + "]").ToList(), infowheader);
        //    string RESHeader = "ID;Name; RES Values";
        //    AddToList(lines, "RESgeneration", n.RES.Select(res => res.ToFile()).ToList(), RESHeader);
        //    string demandHeader = "ID;Node ID;Demand Values";
        //    List<double> newDemand = new List<double>();
        //    for (int t = 0; t < totalT; t++)
        //    {
        //        newDemand.Add(Math.Round(n.NodalDemand(t) - injection[t],3));
        //    }
        //    AddToList(lines, "demands", new List<string>() { 0 + ";" + 0 + ";[" + String.Join(":", newDemand) + "]" } , demandHeader);
        //    string nodeHeader = "ID;Name;Unit IDs;Storage IDs;RES IDs";
        //    AddToList(lines, "nodes", new List<string>() { n.ToFile() }, nodeHeader);
        //    //lines.Add(b("nodes"));
        //    //Nodes.ForEach(node => { lines.AddRange(node.ToFile()); });
        //    //lines.Add(e("nodes"));

        //    //Utils.UnitsBin.AddRange(Units);
        //    File.WriteAllLines( @"C:\Users\Rogier\Desktop\TransTemp\" + name + ".uc", lines);

        //    //File.WriteAllLines(Utils.Folder + @"\NewInstances\" + IOBas.year + IOBas.stats + ".uc", lines);
        //}

        public void AddToList(List<string> lines, string name, List<string> linesToAdd)
        {
            if (linesToAdd.Count == 0) return;
            lines.Add(B(name));
            lines.AddRange(linesToAdd);
            lines.Add(E(name));
        }
        public void AddToList(List<string> lines, string name, List<string> linesToAdd, string header)
        {
            if (linesToAdd.Count == 0) return;
            lines.Add(B(name));
            lines.Add(header);
            lines.AddRange(linesToAdd);
            lines.Add(E(name));
        }

        public string B(string input)
        {
            return "<" + input + ">";
        }
        public string E(string input)
        {
            return "</" + input + ">";
        }
    }
}
