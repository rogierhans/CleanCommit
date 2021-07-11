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
    public class Objective
    {

        public GRBLinExpr CurrentObjective;
        public GRBVar GenerationCost;
        public GRBVar CycleCost;
        public GRBVar DRCost;
        public GRBVar LOLCost;
        public GRBVar LORCost;

        private readonly int totalNodes;
        private readonly int totalTime;
        private readonly int totalUnits;
        private readonly int totalPiecewiseSegments;
        private readonly Variables Vars;
        private readonly GRBModel Model;
        private readonly PowerSystem PS;
        private readonly ConstraintConfiguration CC;
        private readonly double[,] PDTF;
        private readonly char Type;
        public Objective(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, TightSolver solver, Variables vars)
        {
            Model = model;
            PS = ps;
            CC = cc;
            Vars = vars;
            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalPiecewiseSegments = CC.PiecewiseSegments;
            PDTF = PS.PDTF;
            Type = CC.Relax ? GRB.CONTINUOUS : GRB.BINARY;
        }

        public void AddObjective()
        {
            CurrentObjective = new GRBLinExpr();
            LinkGenerationCost();
            LinkStartUpCost();
            LinkLossOfLoad();
            LinkLossOfReserve();
            LinkDRCost();
            //Objective += GenerationCostVariable + CycleCostVariable + LOLCostVariable;
            if (CC.Adequacy)
            {
                CurrentObjective += LOLCost + LORCost + DRCost;
            }
            else
            {
                CurrentObjective += GenerationCost + CycleCost + LOLCost + LORCost + DRCost;
            }
            Model.SetObjective(CurrentObjective, GRB.MINIMIZE);
        }

        public void OnlyAnalyses()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    if (unit.PrintType == "GAS")
                    {
                        AltObjective += Vars.P[t, u];
                    }
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                }
            }
        }

        public void OnlyAnalysesCO2()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    AltObjective += Vars.P[t, u] * unit.CO2Variable + Vars.Commit[t, u] * unit.CO2Fixed;
                }
            }
        }

        public void AddCO2ObjectiveMin()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    AltObjective += Vars.P[t, u] * unit.CO2Variable + Vars.Commit[t, u] * unit.CO2Fixed;

                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                }
            }
            Model.SetObjective(AltObjective, GRB.MINIMIZE);
        }

        public void AddCO2ObjectiveMax()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    AltObjective += Vars.P[t, u] * unit.CO2Variable + Vars.Commit[t, u] * unit.CO2Fixed;

                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                }
            }
            Model.SetObjective(AltObjective, GRB.MAXIMIZE);
        }


        public void AddAlternativeObjective()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    if (unit.PrintType == "GAS")
                    {
                        AltObjective += Vars.P[t, u];
                    }
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                }
            }
            Model.SetObjective(AltObjective, GRB.MINIMIZE);
        }

        public GRBLinExpr AltObjective;
        public void AddAlternativeObjectiveMax()
        {
            AltObjective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    if (unit.PrintType == "GAS")
                    {
                        AltObjective += Vars.P[t, u];
                    }
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                }
            }
            Model.SetObjective(AltObjective, GRB.MAXIMIZE);
        }

        private void LinkLossOfLoad()
        {
            LOLCost = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "LOLCostVariable");
            GRBLinExpr lolCost = new GRBLinExpr();

            for (int t = 0; t < totalTime; t++)
            {
                for (int n = 0; n < totalNodes; n++)
                {
                    if (CC.Adequacy)
                        lolCost += Vars.NodalLossOfLoad[n, t] * PS.VOLL;
                    else
                        lolCost += Vars.NodalLossOfLoad[n, t] * PS.VOLL;

                }
            }
            Model.AddConstr(LOLCost == lolCost, "");
        }

        private void LinkLossOfReserve()
        {
            LORCost = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "LOLCostVariable");
            GRBLinExpr lorcost = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                if (CC.Adequacy)
                    lorcost += Vars.LossOfReserve[t] * PS.VOLR;
                else
                    lorcost += Vars.LossOfReserve[t] * PS.VOLR;
            }
            Model.AddConstr(LORCost == lorcost, "");
        }

        private void LinkGenerationCost()
        {
            GenerationCost = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "GenerationCost");
            GRBLinExpr GenCost = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    //Console.WriteLine(Vars.PiecewiseGeneration[u]);
                    GenCost += Vars.Commit[t, u] * (Vars.PiecewiseGeneration[u].GetCost(unit.pMin) + unit.A);
                    GenCost += SummationTotal(totalPiecewiseSegments, s => Vars.Piecewise[t, u, s] * Vars.PiecewiseGeneration[u].PiecewiseSlope[s]);

                }
            }
            Model.AddConstr(GenerationCost == GenCost, "");
        }

        private void LinkDRCost()
        {
            DRCost = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "DRCost");
            GRBLinExpr cost = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int n = 0; n < totalNodes; n++)
                {
                    cost += Vars.DemandShed[n, t] * PS.DRSCost;
                }
            }
            Model.AddConstr(DRCost == cost, "");
        }

        public void LinkStartUpCost()
        {
            CycleCost = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "CycleCostVariable");
            GRBLinExpr cycleCost = new GRBLinExpr();
            if (!CC.TimeDependantStartUpCost)
            {

                ApplyFunction((t, u) =>
                {
                    Unit unit = PS.Units[u];
                    cycleCost += unit.StartCostInterval.First() * Vars.Start[t, u];
                });
            }
            else
            {
                ApplyFunction((t, u) =>
                {
                    Unit unit = PS.Units[u];
                    for (int e = 0; e < unit.StartInterval.Length; e++)
                    {
                        cycleCost += Vars.StartCostIntervall[t, u][e] * unit.StartCostInterval[e];
                    }
                });
            }
            Model.AddConstr(CycleCost == cycleCost, "");
        }



        public void ApplyFunction(Action<int, int> action)
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    action(t, u);
                }
            }
        }
        public GRBLinExpr SummationTotal(int total, Func<int, GRBLinExpr> func)
        {
            //Console.WriteLine(total + " "+ func.ToString());
            if (total == 0) return func(0);
            else return U.GetNumbers(total).Select(i => func(i)).Aggregate((a, b) => a + b);
        }
    }
}

