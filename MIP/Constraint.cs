using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;

namespace CleanCommit.MIP
{
    public  abstract class Constraint
    {
        protected int totalNodes;
        protected int totalTime;
        protected int totalUnits;
        protected int totalLinesAC;
        protected int totalLinesDC;
        protected int totalStorageUnits;
        protected int totalRESUnits;
        protected int totalPiecewiseSegments;

        protected GRBModel Model;
        protected PowerSystem PS;
        protected ConstraintConfiguration CC;
        protected Variables Variable;

        protected double[,] PDTF;
        public Constraint(PowerSystem ps, ConstraintConfiguration cc ,  GRBModel model, Variables variable)
        {
            Model = model;
            PS = ps;
            CC = cc;
            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalLinesDC = PS.LinesDC.Count;
            totalLinesAC = PS.LinesAC.Count;
            totalStorageUnits = PS.StorageUnits.Count;
            totalRESUnits = PS.ResGenerations.Count;
            totalPiecewiseSegments =CC.PiecewiseSegments;
            PDTF = PS.PDTF;
            Variable = variable;
        }

        public abstract void AddConstraint();

        protected void ForEachNodeAndTimeStep(Action<int, int> action)
        {
            for (int n = 0; n < totalNodes; n++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    action(n, t);
                }
            }
        }
        protected void ForEachTimeStepAndGenerator(Action<int, int> action)
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    action(t, u);
                }
            }
        }

        protected void ForEachTimeStepAndGenerator(Action<int, int> action, int start, int stop)
        {
            for (int t = start; t < stop; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    action(t, u);
                }
            }
        }
        protected GRBLinExpr MaybeVar(GRBVar[,] variable, int t, int u)
        {
            return t < totalTime ? variable[t, u] : new GRBLinExpr(0);
        }

        protected GRBLinExpr Summation(int n, Func<int, GRBLinExpr> func)
        {
            if (n < 0) return new GRBLinExpr(0);
            else if (n == 0) return func(0);
            else return U.GetNumbers(n + 1).Select(i => func(i)).Aggregate((a, b) => a + b);
        }
        protected GRBLinExpr SummationTotal(int total, Func<int, GRBLinExpr> func)
        {
            //Console.WriteLine(total + " "+ func.ToString());
            if (total == 0) return func(0);
            else return U.GetNumbers(total).Select(i => func(i)).Aggregate((a, b) => a + b);
        }
    }
}
