using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.MIP;
using CleanCommit.Instance;
using System.IO;
using System.Diagnostics;
namespace CleanCommit
{
    [Serializable]
    public class SolverOutput
    {
        public double time;

        public double TotalLossOfLoad = 0;
        public bool[,] BinaryCommitStatus;      // time * units
        public bool[,] BinaryStartStatus;       // time * units
        public bool[,] BinaryStopStatus;        // time * units
        public double[,] Dispatch;              // time * units
        public double[,] RDispatch;             // time * units
        public double[,] SDispatch;             // time * units
        public double[,,] ThermalReserve;      // time x units x reservetype;
        public double[,,] HydroReserve;         // time x Sunits x reservetype;
        public double[] TotalDispatch;          // time                                        // public double[] TotalReserve;           // time 
        public double[,] NodalInjection; // node x time;

        public double GurobiCost;
        public double GurobiCostGeneration;
        public double GurobiCostCycle;
        public double GurobiCostLOL;
        public double GurobiCostLOR;
        public double Gap;
        public int NumConstrs;
        public int NumVars;
        public int NumBinVars;
        readonly int totalTime;
        readonly int totalUnits;
        readonly int totalNodes;
        readonly  int totalLines;
        readonly  int totalStorageUnits;
        readonly  int totalRES;
        readonly int totalReserveTypes;

        [NonSerialized]
        private PowerSystem PS;
        [NonSerialized]
        private Variables Variables;
        [NonSerialized]
        private ConstraintConfiguration CC;
        [NonSerialized]
        readonly private PiecewiseGeneration[] PiecewiseGeneration;
        [NonSerialized]
        readonly private Objective objective;

        public SolverOutput(Variables variables, Objective objective, GRBModel model, double runTime)
        {
            this.objective = objective;
            time = runTime;
            Variables = variables;
            NumConstrs = model.NumConstrs;
            NumVars = model.NumVars;
            NumBinVars = model.NumBinVars;
            CC = variables.CC;

            GurobiCost = objective.CurrentObjective.Value;
            GurobiCostGeneration = objective.GenerationCost.X;
            GurobiCostCycle = objective.CycleCost.X;
            GurobiCostLOL = objective.LOLCost.X;
            GurobiCostLOR = objective.LORCost.X;
            Gap = (CC.Relax ? 0 : model.MIPGap);
            PS = variables.PS;

            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalLines = PS.LinesAC.Count;
            totalStorageUnits = PS.StorageUnits.Count;
            totalRES = PS.ResGenerations.Count;
            totalReserveTypes = CC.Reserves.Count();
            Dispatch = new double[totalTime, totalUnits];
            RDispatch = new double[totalTime, totalRES];
            SDispatch = new double[totalTime, totalStorageUnits];
            //Reserve = new double[totalTime, totalUnits, totalReserveTypes];
            ThermalReserve = new double[totalTime, totalUnits, totalReserveTypes];
            HydroReserve = new double[totalTime, totalStorageUnits, totalReserveTypes];
            TotalDispatch = new double[totalTime];
            //TotalReserve = new double[totalTime];
            BinaryCommitStatus = new bool[totalTime, totalUnits];
            BinaryStartStatus = new bool[totalTime, totalUnits];
            BinaryStopStatus = new bool[totalTime, totalUnits];
            NodalInjection = new double[totalNodes, totalTime];
            PiecewiseGeneration = variables.PiecewiseGeneration;
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    BinaryCommitStatus[t, u] = variables.Commit[t, u].X > 0.5;
                    BinaryStartStatus[t, u] = variables.Start[t, u].X > 0.5;
                    BinaryStopStatus[t, u] = variables.Stop[t, u].X > 0.5;
                }
            }

            for (int t = 0; t < totalTime; t++)
            {
                TotalDispatch[t] = 0;
                //TotalReserve[t] = 0;
                for (int u = 0; u < totalUnits; u++)
                {
                    var unit = PS.Units[u];
                    Dispatch[t, u] = variables.P[t, u].X + variables.Commit[t, u].X * unit.pMin;
                    TotalDispatch[t] += Dispatch[t, u];
                    // Reserve[t, u] = variables.PotentialP[t, u].X - variables.P[t, u].X;
                    //TotalReserve[t] += Reserve[t, u];
                    for (int r = 0; r < totalReserveTypes; r++)
                    {
                        ThermalReserve[t, u, r] = Variables.ReserveThermal[t, u, r].X;
                    }
                }
            }

            for (int t = 0; t < totalTime; t++)
            {
                for (int g = 0; g < totalRES; g++)
                {
                    RDispatch[t, g] = Variables.RESDispatch[t, g].X;
                }
            }

            for (int t = 0; t < totalTime; t++)
            {
                for (int g = 0; g < totalStorageUnits; g++)
                {
                    SDispatch[t, g] = Variables.Discharge[t, g].X;
                    for (int r = 0; r < totalReserveTypes; r++)
                    {
                        HydroReserve[t, g, r] = Variables.ReserveStorage[t, g, r].X;
                    }
                }
            }
            for (int n = 0; n < totalNodes; n++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    TotalLossOfLoad += variables.NodalLossOfLoad[n, t].X;
                    NodalInjection[n, t] = variables.NodalInjectionAC[n, t].X + variables.NodalInjectionDC[n, t].X;
                }
            }

        }

        public void WriteToCSV(string folder, string donderdag)
        {


            List<string> lines = new List<string>
            {
                "Name=" + PS.Name,
                "Timesteps=" + CC.TotalTime,
                "Units=" + PS.Units.Count,
                "Storage=" + PS.StorageUnits.Count,
                "RES=" + PS.ResGenerations.Count,
                "Tight=" + CC.Tight,
                "Relax=" + CC.Relax,
                "Ramp=" + CC.RampingLimits,
                "MinUpDown=" + CC.MinUpMinDown,
                "TimeDepStart=" + CC.TimeDependantStartUpCost,
                "Transmission=" + CC.TransmissionMode,
                "Segments=" + CC.PiecewiseSegments,
                "Time=" + time,
                "MIPGap=" + Gap,
                "Constraints=" + NumConstrs,
                "Variables=" + NumVars,
                "BinVariables=" + NumBinVars,
                "Costs=" + GurobiCost,
                "GCycle=" + GurobiCostCycle,
                "GGen=" + GurobiCostGeneration,
                "GLOL=" + GurobiCostLOL,
                "GLOR=" + GurobiCostLOR,
                "TotalDemand=" + PS.Nodes.Sum(node => node.GetTotalDemand(CC.TotalTime))
            };
            lines.AddRange(MArrayToString("P", Variables.P));
            lines.AddRange(MArrayToString("Dispatch", Dispatch));
            // lines.AddRange(MArrayToString("Reserve", Reserve));
            lines.AddRange(MArrayToString("CommitStatus", Variables.Commit));
            lines.AddRange(MArrayToString("StartStatus", Variables.Start));
            lines.AddRange(MArrayToString("StopStatus", Variables.Stop));
            lines.AddRange(MArrayToString("TotalDispatch", TotalDispatch));
            //  lines.AddRange(MArrayToString("TotalReserve", TotalReserve));
            lines.AddRange(MArrayToStringAlt("ThermalReserve", ThermalReserve));
            lines.AddRange(MArrayToStringAlt("HydroReserve", HydroReserve));
            lines.AddRange(MArrayToString("ResDispatch", Variables.RESDispatch));
            lines.AddRange(MArrayToString("NodalLossOfLoad", Variables.NodalLossOfLoad));
            lines.AddRange(MArrayToString("NodalLossOfReserve", Variables.LossOfReserve.Select(x => x.X).ToArray()));
            lines.AddRange(MArrayToString("StorageLevel", Variables.Storage));
            lines.AddRange(MArrayToString("StorageCharge", Variables.Charge));
            lines.AddRange(MArrayToString("StorageDischarge", Variables.Discharge));
            lines.AddRange(MArrayToString("NodalInjection", Variables.NodalInjectionAC));
            lines.AddRange(MArrayToString("TransmissionFlow", Variables.TransmissionFlowAC));
            string outputsubfolder = folder + PS.Name.Split('.')[0] + "\\";
            if (!Directory.Exists(outputsubfolder))
                Directory.CreateDirectory(outputsubfolder);
            File.WriteAllLines(outputsubfolder + donderdag + ".csv", lines);
            //Process myProcess = new Process();
            //Process.Start("notepad++.exe", outputsubfolder + donderdag + ".csv");
            //Console.ReadLine();
        }

        public List<string> MArrayToString(string identifier, GRBVar[,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0)
            {
                for (int i = 0; i < input.GetLength(0); i++)
                {
                    List<object> values = new List<object>();
                    for (int j = 0; j < input.GetLength(1); j++)
                    {
                        values.Add(input[i, j].X);
                    }
                    lines.Add(String.Join(";", values.Select(x => x.ToString())));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public List<string> MArrayToString(string identifier, GRBVar[,,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0 && input.GetLength(2) != 0)
            {
                for (int n = 0; n < input.GetLength(0); n++)
                {
                    GRBVar[,] newValues = new GRBVar[input.GetLength(1), input.GetLength(2)];
                    for (int i = 0; i < input.GetLength(1); i++)
                    {
                        for (int j = 0; j < input.GetLength(2); j++)
                        {
                            newValues[i, j] = input[n, i, j];
                        }
                    }
                    lines.AddRange(MArrayToString(identifier + n, newValues));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public List<string> MArrayToString(string identifier, double[] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0)
            {
                List<object> values = new List<object>();
                for (int i = 0; i < input.GetLength(0); i++)
                {

                    values.Add(input[i]);

                }
                lines.Add(String.Join(";", values.Select(x => x.ToString())));
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public List<string> MArrayToString(string identifier, double[,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0)
            {
                for (int i = 0; i < input.GetLength(0); i++)
                {
                    List<object> values = new List<object>();
                    for (int j = 0; j < input.GetLength(1); j++)
                    {
                        values.Add(input[i, j]);
                    }
                    lines.Add(String.Join(";", values.Select(x => x.ToString())));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public List<string> MArrayToString(string identifier, double[,,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0 && input.GetLength(2) != 0)
            {
                for (int n = 0; n < input.GetLength(0); n++)
                {
                    double[,] newValues = new double[input.GetLength(1), input.GetLength(2)];
                    for (int i = 0; i < input.GetLength(1); i++)
                    {
                        for (int j = 0; j < input.GetLength(2); j++)
                        {
                            newValues[i, j] = input[n, i, j];
                        }
                    }
                    lines.AddRange(MArrayToString(identifier + n, newValues));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }

        public List<string> MArrayToStringAlt(string identifier, double[,,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            if (input.GetLength(0) != 0 && input.GetLength(1) != 0 && input.GetLength(2) != 0)
            {
                for (int j = 0; j < input.GetLength(2); j++)
                {
                    double[,] newValues = new double[input.GetLength(0), input.GetLength(1)];
                    for (int i = 0; i < input.GetLength(1); i++)
                    {
                        for (int n = 0; n < input.GetLength(0); n++)
                        {
                            newValues[n, i] = input[n, i, j];
                        }
                    }
                    lines.AddRange(MArrayToString(identifier + j, newValues));
                }
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }

        public void Foto()
        {
            var test = new Print();
            test.PrintUCED(PS, CC, Dispatch, SDispatch, RDispatch, "");
        }
    }
}
