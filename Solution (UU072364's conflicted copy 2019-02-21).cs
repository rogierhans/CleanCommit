using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtrechtCommitment
{
    class Solution
    {
        PowerSystem PS;

        public int[,] UnitTimeCommit;
        public double[,] UnitTimeDispatch;
        private int totalTime;
        private int totalUnits;
        private int totNodes;

        public static Random RNG = U.RNG;
        public Solution(PowerSystem ps)
        {
            PS = ps;
            totalTime = PS.Nodes[0].Demand.Count;
            totalUnits = PS.Units.Count;
            UnitTimeCommit = new int[totalTime, totalUnits];
            UnitTimeDispatch = new double[totalTime, totalUnits];

        }



        internal void PrintUnitCommit()
        {
            for (int unit = 0; unit < totalUnits; unit++)
            {
                string line = "";
                for (int time = 0; time < totalTime; time++)
                {
                    line += UnitTimeCommit[time, unit];
                }
                Console.WriteLine(line);
            }
        }


        public Solution RandomCrossover(Solution other)
        {
            if (RNG.NextDouble() > 0.5)
            {
                return OnePointCrossover(other);
            }
            {
                return UniformCrossover(other);
            }
        }
        public Solution OnePointCrossover(Solution other)
        {
            Solution newSolution = new Solution(PS);
            int indexPoint = RNG.Next(totalTime);
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    if (time > indexPoint)
                    {
                        newSolution.UnitTimeCommit[time, unit] = UnitTimeCommit[time, unit];
                    }
                    else
                    {
                        newSolution.UnitTimeCommit[time, unit] = other.UnitTimeCommit[time, unit];
                    }
                }
            }

            return newSolution;
        }

        public Solution UniformCrossover(Solution other)
        {
            Solution newSolution = new Solution(PS);
            int indexPoint = RNG.Next(totalTime);
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    newSolution.UnitTimeCommit[time, unit] = RNG.NextDouble() > 0.5 ? UnitTimeCommit[time, unit] : other.UnitTimeCommit[time, unit];
                }
            }

            return newSolution;
        }




        public void LocalSearch()
        {
            while (true) { }
        }


        public double Fitness()
        {
            double totalCost = 0;
            totalCost += CommitCost();
            totalCost += DispathCost();
            return totalCost;
        }

        public bool CheckIfValidSolution()
        {
            bool valid = true;
            valid &= PowerBalance();
            valid &= MinUpNDown();
            valid &= GenerationLimits();
            valid &= RampingLimits();
            return valid;
        }

        private bool MinUpNDown()
        {
            throw new NotImplementedException();
        }

        private bool RampingLimits()
        {
            bool withinBound = true;
            for (int time = 1; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    //bool prevCommit = (time == 0) ? true : UnitTimeCommit[time - 1, unit];
                    //bool currentCommit = UnitTimeCommit[time, unit];
                    //bool nextCommit = (time == totalTime - 1) ? true : UnitTimeCommit[time + 1, unit];
                    //double prevDispatch = UnitTimeDispatch[time - 1, unit];
                    //double currentDispatch = UnitTimeDispatch[time, unit];
                    //double deltaDispatch = prevDispatch - currentDispatch;
                    //withinBound
                    //todo
                }
            }
            return withinBound;
        }

        private bool GenerationLimits()
        {
            bool allWithinBounds = true;
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    var currentUnit = PS.Units[u];
                    int currentCommit = UnitTimeCommit[t, u];
                    var dispatch = UnitTimeDispatch[t, u];
                    bool withinBounds = (currentUnit.PMin * currentCommit <= dispatch) && (dispatch <= currentUnit.PMax * currentCommit);
                    if (!withinBounds) { throw new Exception(U.S("Generation limi 0 * 1 <= 2 <= 3 4", currentUnit.PMin , currentCommit, dispatch, currentUnit.PMax , currentCommit)); }
                    allWithinBounds &= withinBounds;
                }
            }

            return allWithinBounds;
        }

        private bool PowerBalance()
        {
            bool balance = true;
            for (int time = 0; time < totalTime; time++)
            {
                double totalDispatch = 0;
                double demand = PS.Nodes[0].Demand;
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    double dispatch = UnitTimeDispatch[time, unit];
                    totalDispatch += dispatch;
                }
                balance &= totalDispatch == demand;
            }
            return balance;
        }

        private double DispathCost()
        {
            throw new Exception();
            //double totalDispatchCost = 0;
            //for (int time = 0; time < totalTime; time++)
            //{
            //    for (int unit = 0; unit < totalUnits; unit++)
            //    {
            //        var dispatch = UnitTimeDispatch[time, unit];
            //        var currentUnit = PS.Units[unit];
            //        totalDispatchCost += currentUnit.GetGenerationCost(dispatch);
            //    }
            //}
            //return totalDispatchCost;
        }

        private double CommitCost()
        {
            double totalCommitCost = 0;

            return totalCommitCost;
        }
    }
}
