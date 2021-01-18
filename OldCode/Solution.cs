//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UtrechtCommitment
//{
//    class Solution
//    {
//        PowerSystem PS;

//        double[] Demand;
//        public int[,] UnitCommit;
//        public int[,] UnitStart;
//        public int[,] UnitStop;
//        public double[,] Dispatch;
//        public double[,] DispatchUpperBound;
//        public double[,] DistapchLowerBound;
//        public double[] LossOfLoad;
//        private int totalTime;
//        private int totalUnits;

//        public static Random RNG = U.RNG;

//        private DispatchBounds bounds;
//        public Solution(PowerSystem ps, MIPSolver solver)
//        {
//            PS = ps;
//            Demand = PS.Nodes[0].Demand.ToArray();
//            totalTime = Demand.Length;

//            totalUnits = PS.Units.Count;
//            UnitCommit = new int[totalTime, totalUnits];
//            UnitStart = new int[totalTime, totalUnits];
//            UnitStop = new int[totalTime, totalUnits];
//            Dispatch = new double[totalTime, totalUnits];
//            DispatchUpperBound = new double[totalTime, totalUnits];
//            DistapchLowerBound = new double[totalTime, totalUnits];
//            LossOfLoad = new double[totalTime];
//            SetCommit(solver);
//            Configure();
//            //CalculateCost();
//            LocalSearch();
//        }

//        private void SetCommit(MIPSolver solver)
//        {
//            var commit = solver.ReadCommitStatus();
//            var start = solver.ReadStartStatus();
//            var stop = solver.ReadStopStatus();
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    UnitCommit[t, u] = commit[t, u] ? 1 : 0;
//                    UnitStart[t, u] = start[t, u] ? 1 : 0;
//                    UnitStop[t, u] = stop[t, u] ? 1 : 0;
//                }
//            }
//        }



//        internal void PrintUnitCommit()
//        {
//            for (int unit = 0; unit < totalUnits; unit++)
//            {
//                string line = "";
//                for (int time = 0; time < totalTime; time++)
//                {
//                    line += UnitCommit[time, unit];
//                }
//                Console.WriteLine(line);
//            }
//        }


//        //public Solution RandomCrossover(Solution other)
//        //{
//        //    if (RNG.NextDouble() > 0.5)
//        //    {
//        //        return OnePointCrossover(other);
//        //    }
//        //    {
//        //        return UniformCrossover(other);
//        //    }
//        //}
//        //public Solution OnePointCrossover(Solution other)
//        //{
//        //    Solution newSolution = new Solution(PS);
//        //    int indexPoint = RNG.Next(totalTime);
//        //    for (int time = 0; time < totalTime; time++)
//        //    {
//        //        for (int unit = 0; unit < totalUnits; unit++)
//        //        {
//        //            if (time > indexPoint)
//        //            {
//        //                newSolution.UnitCommit[time, unit] = UnitCommit[time, unit];
//        //            }
//        //            else
//        //            {
//        //                newSolution.UnitCommit[time, unit] = other.UnitCommit[time, unit];
//        //            }
//        //        }
//        //    }

//        //    return newSolution;
//        //}

//        //public Solution UniformCrossover(Solution other)
//        //{
//        //    Solution newSolution = new Solution(PS);
//        //    int indexPoint = RNG.Next(totalTime);
//        //    for (int time = 0; time < totalTime; time++)
//        //    {
//        //        for (int unit = 0; unit < totalUnits; unit++)
//        //        {
//        //            newSolution.UnitCommit[time, unit] = RNG.NextDouble() > 0.5 ? UnitCommit[time, unit] : other.UnitCommit[time, unit];
//        //        }
//        //    }

//        //    return newSolution;
//        //}





//        public void LocalSearch()
//        {
//            while (Improve())
//            {
//               // bounds.PrintBounds();
//                //U.PrintArray(Dispatch);

//                //Console.ReadLine();
//            };// Console.ReadLine(); }
//            bounds.PrintBounds();
//            U.PrintArray(Dispatch);
//            LossOfLoad.ToList().ForEach(x => Console.Write("\t "+x));
//        }



//        public void Configure()
//        {
//            SetDispatch();
//            bounds = new DispatchBounds(PS, UnitCommit, UnitStart, UnitStop, Dispatch);
//            bounds.PrintBounds();
//            SetLossOfLoad();
//        }

//        private void SetLossOfLoad()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                double totalGeneration = 0;
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    totalGeneration += Dispatch[t, u];
//                }
//                LossOfLoad[t] = Demand[t] - totalGeneration;
//            }
//        }

//        public void SetDispatch()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];
//                    if (UnitCommit[t, u] == 1)
//                    {
//                        //Console.WriteLine("t={0} u={1} pMin={2}", t, u, unit.PMin);
//                        //unit.GetInfo();
//                        //Console.ReadLine();
//                        Dispatch[t, u] = unit.PMin;
//                    }
//                    else
//                    {
//                        Dispatch[t, u] = 0;
//                    }
//                }
//            }
//        }

//        public bool CheckIfValidSolution()
//        {
//            bool valid = true;
//            valid &= PowerBalance();
//            valid &= MinUpNDown();
//            valid &= GenerationLimits();
//            valid &= RampingLimits();
//            return valid;
//        }

//        private bool MinUpNDown()
//        {
//            return true;
//        }

//        private bool RampingLimits()
//        {
//            bool allWithinBound = true;
//            for (int t = 1; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];

//                    double deltaDispatch = Dispatch[t, u] - Dispatch[t - 1, u];

//                    double downwardRampingLimitNormal = unit.RampDown * UnitCommit[t - 1, u];
//                    double downwardRampingLimitShutdown = UnitStop[t, u] * (unit.ShutDown - unit.RampDown);
//                    double downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
//                    bool DownwardRampingConstr = -deltaDispatch <= downwardRampingLimit;
//                    U.E(!DownwardRampingConstr, "0 \t 1  \t 2 \t 3", deltaDispatch, unit.RampDown, unit.ShutDown, downwardRampingLimit);

//                    double upwardRampingLimitNormal = unit.RampUp * UnitCommit[t, u];
//                    double upwardRampingLimitStartup = UnitStart[t, u] * (unit.StartUp - unit.RampUp);
//                    double upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
//                    bool UpwardRampingConstr = deltaDispatch <= upwardRampingLimit;
//                    U.E(!UpwardRampingConstr, "0 <= 1", deltaDispatch, upwardRampingLimit);
//                    allWithinBound &= DownwardRampingConstr && UpwardRampingConstr;
//                }
//            }
//            return allWithinBound;
//        }

//        private bool GenerationLimits()
//        {
//            bool allWithinBounds = true;
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var currentUnit = PS.Units[u];
//                    int currentCommit = UnitCommit[t, u];
//                    var dispatch = Dispatch[t, u];
//                    bool withinBounds = (currentUnit.PMin * currentCommit <= dispatch) && (dispatch <= currentUnit.PMax * currentCommit);
//                    U.E(!withinBounds, "Violating Generation Limit 0 * 1 < 2 < 3 * 4 (5,6)", currentUnit.PMin, currentCommit, dispatch, currentUnit.PMax, currentCommit, t, u);
//                    allWithinBounds &= withinBounds;
//                }
//            }

//            return allWithinBounds;
//        }

//        private bool PowerBalance()
//        {
//            bool allBalance = true;
//            for (int t = 0; t < totalTime; t++)
//            {
//                double totalDispatch = LossOfLoad[t];
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    double dispatch = Dispatch[t, u];
//                    totalDispatch += dispatch;
//                }

//                bool Balance = Math.Abs(totalDispatch - Demand[t]) < 0.1;
//                U.E(!Balance, "Balance 0 != 1 at time 2, diff 3", totalDispatch, Demand[t], t, Math.Abs(totalDispatch - Demand[t]));
//                allBalance &= Balance;
//            }
//            return allBalance;
//        }
//        public double CalculateGenerationCost()
//        {
//            double cost = 0;
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];
//                    cost += unit.A * UnitCommit[t, u] + unit.B * Dispatch[t, u];
//                }
//            }
//            return cost;
//        }
//        public double CalculateStartUpCosts()
//        {
//            double cost = 0;
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];
//                    cost += UnitStart[t, u] * unit.StartCostInterval[unit.StartInterval.Count() - 1];
//                }
//            }
//            return cost;
//        }
//        public double CalculatePeneltyVOLL()
//        {
//            double cost = 0;
//            for (int t = 0; t < totalTime; t++)
//            {
//                cost += LossOfLoad[t] * PS.VOLL;
//            }
//            return cost;
//        }

//        bool Improve()
//        {
//            CalculateCost();
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];
//                    if (bounds.DispatchUpwardSlack[t, u] > 0.0001)
//                    {

//                        if (LossOfLoad[t] > bounds.DispatchUpwardSlack[t, u])
//                        {
//                            LossOfLoad[t] -= bounds.DispatchUpwardSlack[t, u];
//                            Dispatch[t, u] = bounds.DispatchUpwardSlack[t, u] + Dispatch[t, u];
//                            bounds.UpdateBounds(t, u);
//                            CalculateCost();
//                            Console.WriteLine("LOL1 {0} , {1}", t, u);
//                            return true;
//                        }
//                        else if (LossOfLoad[t] > 0)
//                        {

//                            Dispatch[t, u] = LossOfLoad[t] + Dispatch[t, u];
//                            LossOfLoad[t] = 0;
//                            bounds.UpdateBounds(t, u);
//                            Console.WriteLine("LOL2 {0} , {1}", t, u);
//                            return true;
//                        }
//                        //for (int u2 = 0; u2 < totalUnits; u2++)
//                        //{

//                        //    var unit2 = PS.Units[u2];
//                        //    if (bounds.DispatchDownwardSlack[t, u2] > 0.1 && unit2.B > unit.B)
//                        //    {
//                        //        if (bounds.DispatchUpwardSlack[t, u] > bounds.DispatchDownwardSlack[t, u2])
//                        //        {
//                        //            Dispatch[t, u] = Dispatch[t, u] + bounds.DispatchDownwardSlack[t, u2];
//                        //            Dispatch[t, u2] = Dispatch[t, u2] - bounds.DispatchDownwardSlack[t, u2];
//                        //            if (Dispatch[t, u2] < 0) Console.ReadLine();
//                        //        }
//                        //        else
//                        //        {
//                        //            Dispatch[t, u] = Dispatch[t, u] + bounds.DispatchUpwardSlack[t, u];
//                        //            Dispatch[t, u2] = Dispatch[t, u2] - bounds.DispatchUpwardSlack[t, u];
//                        //            if (Dispatch[t, u2] < 0) Console.ReadLine();
//                        //        }
//                        //        Console.WriteLine("{0} Swap {1} {2}   {3} > {4}  ^{5}  v{6} ", t, u, u2, unit2.B, unit.B, bounds.DispatchUpwardSlack[t, u], bounds.DispatchDownwardSlack[t, u2]);
//                        //        bounds.UpdateBounds(t, u);
//                        //        bounds.UpdateBounds(t, u2);
//                        //        return true;
//                        //    }
//                        //}
//                    }
//                }
//            }
//            return false;
//        }





//        public double CalculateCost()
//        {
//            double generationCost = CalculateGenerationCost();
//            //Console.WriteLine("Generation={0}", generationCost);
//            double cycleCost = CalculateStartUpCosts();
//            //Console.WriteLine("Cycle={0}", cycleCost);
//            double LOLCost = CalculatePeneltyVOLL();
//            Console.WriteLine("LOL={0}", (LOLCost));
//            //Console.WriteLine("Total={0}", generationCost + cycleCost + LOLCost);
//            return (generationCost + cycleCost + LOLCost);
//        }
//    }
//}
