using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gurobi;
using CleanCommit.Instance;
using CleanCommit.MIP;
using System.IO;
namespace CleanCommit
{
    class PartialSolutionCombiner
    {
        public Solution Solution;
        protected int totalNodes;
        protected int totalTime;
        protected int totalUnits;
        protected int totalLinesAC;
        protected int totalLinesDC;
        protected int totalStorageUnits;
        protected int totalReserveTypes;
        protected int totalRES;
        protected int totalPiecewiseSegments;
        public PartialSolutionCombiner(List<Solution> PartialSolutions, ConstraintConfiguration OGCC, int horizon, int forced, int leap)
        {
            Solution = new Solution
            {
                ComputationTime = -1,
                Gap = -1,
                PiecewiseGeneration = PartialSolutions.First().PiecewiseGeneration,
                PS = PartialSolutions.First().PS,
                CC = PartialSolutions.First().CC.Copy()
            };
            Solution.CC.TotalTime = OGCC.TotalTime;
            Solution.CC.TimeOffSet = OGCC.TimeOffSet;

            totalTime = Solution.CC.TotalTime;
            totalUnits = Solution.PS.Units.Count;
            totalNodes = Solution.PS.Nodes.Count;
            totalLinesAC = Solution.PS.LinesAC.Count;
            totalLinesDC = Solution.PS.LinesDC.Count;
            totalStorageUnits = Solution.PS.StorageUnits.Count;
            totalReserveTypes = Solution.CC.Reserves.Count;
            totalRES = Solution.PS.ResGenerations.Count;
            totalPiecewiseSegments = Solution.CC.PiecewiseSegments;

            Solution.LossOfReserve = new double[totalTime]; //  time
            Solution.P = new double[totalTime, totalUnits]; // time x units
            Solution.Dispatch = new double[totalTime, totalUnits];  // time x units
            Solution.Commit = new double[totalTime, totalUnits];  // time x units
            Solution.Start = new double[totalTime, totalUnits];  // time x units
            Solution.Stop = new double[totalTime, totalUnits];  // time x units
            Solution.RESDispatch = new double[totalTime, totalRES]; ; // time x resunits
            Solution.TransmissionFlowAC = new double[totalLinesAC, totalTime]; // lines x time
            Solution.TransmissionFlowDC = new double[totalLinesDC, totalTime]; // lines x time
            Solution.NodeVoltAngle = new double[totalNodes, totalTime];  // node x time
            Solution.Charge = new double[totalTime, totalStorageUnits]; // time x storageunits
            Solution.Discharge = new double[totalTime, totalStorageUnits];  // time x storageunits
            Solution.Storage = new double[totalTime, totalStorageUnits];   // time x storageunits
            Solution.NodalLossOfLoad = new double[totalNodes, totalTime]; // node x time
            Solution.RESIDUALDemand = new double[totalNodes, totalTime]; // node x time
            Solution.DemandShed = new double[totalNodes, totalTime]; // node x time;
            Solution.NodalInjectionAC = new double[totalNodes, totalTime]; // node x time
            Solution.NodalInjectionDC = new double[totalNodes, totalTime]; // node x time
            Solution.NodalShadowPrice = new double[totalNodes, totalTime]; // node x time
            Solution.LineUpperLimitShadowPrice = new double[totalLinesAC, totalTime]; // lines x time
            Solution.LineLowerLimitShadowPrice = new double[totalLinesAC, totalTime]; // lines x time
            Solution.ReserveThermal = new double[totalTime, totalUnits, totalReserveTypes];  // time x units x reservetype;
            Solution.ReserveStorage = new double[totalTime, totalStorageUnits, totalReserveTypes]; // time x Sunits x reservetype;
            Solution.Piecewise = new double[totalTime, totalUnits, totalPiecewiseSegments];  // time x units x segments
            Solution.StartCostIntervall = new double[totalTime, totalUnits, Solution.PS.Units[0].StartInterval.Length];  //time x units x segments
            Solution.P2GGeneration = new double[totalNodes, totalTime];// node x time

            CopySolution(PartialSolutions.First(), PartialSolutions.First().CC.TimeOffSet, horizon, leap, 0);
            foreach (var sol in PartialSolutions.Skip(1))
            {
                Console.WriteLine(sol.CC.TimeOffSet + " " +  (sol.CC.TimeOffSet + sol.CC.TotalTime));
                CopySolution(sol, sol.CC.TimeOffSet, horizon,  leap,  forced);
            }
            var recalc = new RecalcSolution(Solution);
            Solution.GurobiCost = recalc.TotalCost;
            Solution.GurobiCostGeneration = recalc.GenerationCost;
            Solution.GurobiCostCycle = recalc.CycleCost;
            Solution.GurobiCostDR = recalc.DRCost;
            Solution.GurobiCostLOL = recalc.LOLCost;
            Solution.GurobiCostLOR = recalc.LORCost;
            Solution.ComputationTime = PartialSolutions.Sum(x => x.ComputationTime);
        }
        HashSet<int> IsTimesetSet = new HashSet<int>();
        private void CopySolution(Solution Old, int offset, int horizon, int leap, int forced)
        {
            for (int indexold = forced; indexold < horizon; indexold++)
            {
                if (indexold + offset >= totalTime) return;
                int t = offset + indexold;
                Solution.LossOfReserve[t] = Old.LossOfReserve[indexold];

                for (int g = 0; g < totalUnits; g++)
                {
                    Unit unit = Old.PS.Units[g];
                    Solution.P[t, g] = Old.P[indexold, g];
                    Solution.Dispatch[t, g] = Old.P[indexold, g] + Old.Commit[indexold, g] * unit.PMin;
                    Solution.Commit[t, g] = Old.Commit[indexold, g];
                    Solution.Start[t, g] = Old.Start[indexold, g];
                    Solution.Stop[t, g] = Old.Stop[indexold, g];
                    for (int res = 0; res < Solution.CC.Reserves.Count; res++)
                    {
                        Solution.ReserveThermal[t, g, res] = Old.ReserveThermal[indexold, g, res];
                    }
                    for (int pwsSegment = 0; pwsSegment < totalPiecewiseSegments; pwsSegment++)
                    {
                        Solution.Piecewise[t, g, pwsSegment] = Old.Piecewise[indexold, g, pwsSegment];
                    }
                    for (int segment = 0; segment < Solution.PS.Units[0].StartInterval.Length; segment++)
                    {
                        Solution.StartCostIntervall[t, g, segment] = Old.StartCostIntervall[indexold, g, segment];
                    }
                }
                for (int r = 0; r < totalRES; r++)
                {
                    Solution.RESDispatch[t, r] = Old.RESDispatch[indexold, r];
                }
                for (int l = 0; l < totalLinesAC; l++)
                {
                    Solution.TransmissionFlowAC[l, t] = Old.TransmissionFlowAC[l, indexold];
                    Solution.LineLowerLimitShadowPrice[l, t] = Old.LineLowerLimitShadowPrice[l, indexold];
                    Solution.LineUpperLimitShadowPrice[l, t] = Old.LineUpperLimitShadowPrice[l, indexold];
                }
                for (int l = 0; l < totalLinesDC; l++)
                {
                    Solution.TransmissionFlowDC[l, t] = Old.TransmissionFlowDC[l, indexold];
                }
                for (int n = 0; n < totalNodes; n++)
                {
                    Solution.NodeVoltAngle[n, t] = Old.NodeVoltAngle[n, indexold];
                }
                for (int s = 0; s < totalStorageUnits; s++)
                {
                    Solution.Charge[t, s] = Old.Charge[indexold, s];
                    Solution.Discharge[t, s] = Old.Discharge[indexold, s];
                    Solution.Storage[t, s] = Old.Storage[indexold, s];
                    for (int res = 0; res < Solution.CC.Reserves.Count; res++)
                    {
                        Solution.ReserveStorage[t, s, res] = Old.ReserveStorage[indexold, s, res];
                    }
                }
                for (int n = 0; n < totalNodes; n++)
                {
                    Solution.NodalLossOfLoad[n, t] = Old.NodalLossOfLoad[n, indexold];
                    Solution.RESIDUALDemand[n, t] = Old.RESIDUALDemand[n, indexold];
                    Solution.DemandShed[n, t] = Old.DemandShed[n, indexold];
                    Solution.NodalInjectionAC[n, t] = Old.NodalInjectionAC[n, indexold];
                    Solution.NodalInjectionDC[n, t] = Old.NodalInjectionDC[n, indexold];
                    Solution.NodalShadowPrice[n, t] = Old.NodalShadowPrice[n, indexold];
                    Solution.P2GGeneration[n, t] = Old.P2GGeneration[n, indexold];
                }
            }
        }
    }
}
