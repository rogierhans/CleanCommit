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

        public bool[,] UnitTimeCommit;
        public double[,] UnitTimeDispatch;
        private int totalTime;
        private int totalUnits;
        public Solution(PowerSystem ps)
        {
            PS = ps;
            totalTime = PS.Times.Count;
            totalUnits = PS.Units.Count;
            UnitTimeCommit = new bool[totalTime, totalUnits];
            UnitTimeDispatch = new double[totalTime, totalUnits];
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    UnitTimeCommit[time,unit] = true;
                }
            }
        }

        Solver S;
        public void Solve()
        {
            S = new Solver(PS, this);
            S.ConfigureModel();
            S.Solve();
            Console.ReadLine();

            S.SetVariable(UnitTimeCommit);
            while (Improve()) { };
        }

        public bool Improve()
        {
            double fitness = S.Solve();
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    //Console.ReadLine();
                    ChangeVariable(time, unit);
                    double newFitness = S.Solve();
                    Console.WriteLine(newFitness - fitness);
                    if (newFitness < fitness) {

                        Console.WriteLine(newFitness - fitness);
                        Console.ReadLine();
                        return true;
                    }
                    ChangeVariable(time, unit);

                }
            }
            return false;
        }

        private void ChangeVariable(int time, int unit)
        {
            UnitTimeCommit[time, unit] = !UnitTimeCommit[time, unit];
            S.ChangeVariable(time, unit, UnitTimeCommit[time, unit]);

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
            valid &= GenerationLimits();
            valid &= RampingLimits();
            return valid;
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
            bool withinBounds = true;
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    var currentUnit = PS.Units[unit];
                    bool currentCommit = UnitTimeCommit[time, unit];
                    var dispatch = UnitTimeDispatch[time, unit];
                    if (currentCommit)
                    {
                        withinBounds &= (currentUnit.PMin <= dispatch) && (dispatch <= currentUnit.PMax);
                    }
                }
            }
            return withinBounds;
        }

        private bool PowerBalance()
        {
            bool balance = true;
            for (int time = 0; time < totalTime; time++)
            {
                double totalDispatch = 0;
                double demand = PS.Times[time].Demand;
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
            double totalDispatchCost = 0;
            for (int time = 0; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    var dispatch = UnitTimeDispatch[time, unit];
                    var currentUnit = PS.Units[unit];
                    totalDispatchCost += currentUnit.GetGenerationCost(dispatch);
                }
            }
            return totalDispatchCost;
        }

        private double CommitCost()
        {
            double totalCommitCost = 0;
            for (int time = 1; time < totalTime; time++)
            {
                for (int unit = 0; unit < totalUnits; unit++)
                {
                    bool prevCommit = UnitTimeCommit[time - 1, unit];
                    bool currentCommit = UnitTimeCommit[time, unit];
                    var currentUnit = PS.Units[unit];
                    if (!prevCommit && currentCommit)
                    {
                        totalCommitCost += currentUnit.GetStartCost();
                    }
                }
            }
            return totalCommitCost;
        }
    }
}
