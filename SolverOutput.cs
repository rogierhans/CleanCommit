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
    class SolverOutput
    {
        public double time;
        public double GurobiCost;
        public double GurobiCostGeneration;
        public double GurobiCostCycle;
        public double GurobiCostLOL;
        public double TotalLossOfLoad = 0;
        public bool[,] BinaryCommitStatus;       // time * units
        public bool[,] BinaryStartStatus;        // time * units
        public bool[,] BinaryStopStatus;         // time * units
        public double[,] Dispatch;           // time * units
        public double[,] Reserve;           // time * units
        public double[] TotalDispatch;           // time 
        public double[] TotalReserve;           // time 
        public GRBModel Model;
        public PowerSystem PS;
        public Variables Variables;
        public ConstraintConfiguration CC;
        public PiecewiseGeneration[] PiecewiseGeneration;

        private Objective objective;
        public SolverOutput(Variables variables, Objective objective, GRBModel model, double runTime)
        {
            this.objective = objective;
            time = runTime;
            Variables = variables;
            SetBasicSolverOutput(variables);
            Model = model;
        }
        public double[,] NodalInjection; // node x time;
        public void SetBasicSolverOutput(Variables variables)
        {
            GurobiCost = objective.CurrentObjective.Value;
            GurobiCostGeneration = objective.GenerationCost.X;
            GurobiCostCycle = objective.CycleCost.X;
            GurobiCostLOL = objective.LOLCost.X;

            PS = variables.PS;
            CC = variables.CC;
            var totalTime = CC.TotalTime;
            var totalUnits = PS.Units.Count;
            var totalNodes = PS.Nodes.Count;
            var totalLines = PS.LinesAC.Count;
            var totalStorageUnits = PS.StorageUnits.Count;
            var totatRES = PS.ResGenerations.Count;

            Dispatch = new double[totalTime, totalUnits];
            Reserve = new double[totalTime, totalUnits];
            TotalDispatch = new double[totalTime];
            TotalReserve = new double[totalTime];
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
                TotalReserve[t] = 0;
                for (int u = 0; u < totalUnits; u++)
                {
                    var unit = PS.Units[u];
                    Dispatch[t, u] = variables.P[t, u].X + variables.Commit[t, u].X * unit.pMin;
                    TotalDispatch[t] += Dispatch[t, u];
                    Reserve[t, u] = variables.PotentialP[t, u].X - variables.P[t, u].X;
                    TotalReserve[t] += Reserve[t, u];
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
                "MIPGap=" + (CC.Relax ? 0 : Model.MIPGap),
                "Constraints=" + Model.NumConstrs,
                "Variables=" + Model.NumVars,
                "BinVariables=" + Model.NumBinVars,
                "Costs=" + GurobiCost,
                "GCycle=" + GurobiCostCycle,
                "GGen=" + GurobiCostGeneration,
                "GLOL=" + GurobiCostLOL
            };
            lines.AddRange(MArrayToString("P", Variables.P));
            lines.AddRange(MArrayToString("Potential", Variables.PotentialP));
            lines.AddRange(MArrayToString("Dispatch", Dispatch));
            lines.AddRange(MArrayToString("Reserve", Reserve));
            lines.AddRange(MArrayToString("CommitStatus", Variables.Commit));
            lines.AddRange(MArrayToString("StartStatus", Variables.Start));
            lines.AddRange(MArrayToString("StopStatus", Variables.Stop));
            lines.AddRange(MArrayToString("TotalDispatch", TotalDispatch));
            lines.AddRange(MArrayToString("TotalReserve", TotalReserve));
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
            Process myProcess = new Process();
            Process.Start("notepad++.exe", outputsubfolder + donderdag + ".csv");
            Console.ReadLine();
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
    }
}
