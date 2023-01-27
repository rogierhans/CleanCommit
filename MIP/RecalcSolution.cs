using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
using System.Diagnostics;

namespace CleanCommit.MIP
{
    public class RecalcSolution
    {
        private readonly int totalNodes;
        private readonly int totalTime;
        private readonly int totalUnits;
        private readonly int totalPiecewiseSegments;
        private readonly PowerSystem PS;
        private readonly ConstraintConfiguration CC;
        private readonly double[,] PDTF;
        private readonly char Type;

        public double TotalCost;
        public double GenerationCost;
        public double GenerationLinCost;
        public double CycleCost;
        public double DRCost;
        public double LOLCost;
        public double LORCost;

        public double CO2Cost;

        public double[] GenerationCostPerTime;
        public double[] CycleCostPerTime;
        public double[] DRCostPerTime;
        public double[] LOLCostPerTime;
        public double[] LORCostPerTime;

        Solution Solution;
        public RecalcSolution(Solution sol)
        {
            Solution = sol;
            PS = sol.PS;
            CC = sol.CC;
            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalPiecewiseSegments = CC.PiecewiseSegments;
            PDTF = PS.PDTF;
            Type = CC.Relax ? GRB.CONTINUOUS : GRB.BINARY;
            SetLossOfLoad();
            SetLossOfReserve();
            LinkGenerationCost();
            LinkGenerationLinCost();
            LinkGenerationCO2Cost();
            LinkDRCost();
            LinkStartUpCost(); 
            TotalCost += GenerationCost + CycleCost + DRCost + LOLCost + LORCost;
        }



        private void SetLossOfLoad()
        {
            LOLCost = 0;
            LOLCostPerTime = new double[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                double costPerTime = 0.0;
                for (int n = 0; n < totalNodes; n++)
                {
                    costPerTime += Solution.NodalLossOfLoad[n, t] * PS.VOLL;
                    LOLCostPerTime[t] = costPerTime;
                }
                LOLCost += costPerTime;
            }
        }

        private void SetLossOfReserve()
        {
            LORCost = 0;
            LORCostPerTime = new double[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                LOLCostPerTime[t] = Solution.LossOfReserve[t] * PS.VOLR;
                LORCost += LOLCostPerTime[t];
            }
        }

        private void LinkGenerationCost()
        {
            GenerationCost = 0;
            GenerationCostPerTime = new double[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                double costPerTime = 0.0;
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                    costPerTime += Solution.Commit[t, u] * (Solution.PiecewiseGeneration[u].GetCost(unit.PMin) + unit.A);

                    for (int s = 0; s < totalPiecewiseSegments; s++)
                    {
                        costPerTime += Solution.Piecewise[t, u, s] * Solution.PiecewiseGeneration[u].PiecewiseSlope[s];
                    }
                }
                GenerationCost += costPerTime;
            }
        }
        private void LinkGenerationLinCost()
        {
            GenerationLinCost = 0;
            for (int t = 0; t < totalTime; t++)
            {
                double costPerTime = 0.0;
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                    costPerTime += Solution.Commit[t, u] * unit.A;
                    costPerTime += Solution.Dispatch[t, u] * unit.B;
                }
                GenerationLinCost += costPerTime;
            }
        }
        private void LinkGenerationCO2Cost()
        {
            CO2Cost = 0;
            for (int t = 0; t < totalTime; t++)
            {
                double costPerTime = 0.0;
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                    costPerTime += Solution.Commit[t, u] * unit.CO2Fixed;
                    costPerTime += Solution.Dispatch[t, u] * unit.CO2Variable;
                }
                CO2Cost += costPerTime;
            }
        }

        private void LinkDRCost()
        {
            DRCost = 0;
            DRCostPerTime = new double[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                double perTime = 0;
                for (int n = 0; n < totalNodes; n++)
                {
                    perTime += Solution.DemandShed[n, t] * PS.DSRCost;
                }
                DRCostPerTime[t] = perTime;
                DRCost += perTime;
            }
        }

        public void LinkStartUpCost()
        {
            CycleCost = 0;
            CycleCostPerTime =   new double[totalTime];
            if (!CC.TimeDependantStartUpCost)
            {

                for (int t = 0; t < totalTime; t++)
                {
                    double perTimestep = 0;
                    for (int u = 0; u < totalUnits; u++)
                    {
                        Unit unit = PS.Units[u];
                        perTimestep += unit.StartCostInterval.First() * Solution.Start[t, u];
                    }
                    CycleCostPerTime[t] = perTimestep;
                    CycleCost += perTimestep;
                }
            }
            else
            {
                for (int t = 0; t < totalTime; t++)
                {
                    double perTimestep = 0;
                    for (int u = 0; u < totalUnits; u++)
                    {
                        Unit unit = PS.Units[u];
                        for (int e = 0; e < unit.StartInterval.Length; e++)
                        {
                            perTimestep += Solution.StartCostIntervall[t, u,e] * unit.StartCostInterval[e];
                        }
                    }
                    CycleCostPerTime[t] = perTimestep;
                    CycleCost += CycleCost;
                }
            }
        }

    }
}
