using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class MinUpDownConstraint : Constraint
    {
        public MinUpDownConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver) { }
        public override void AddConstraint()
        {
            if (CC.MinUpMinDown)
            {
                ForEachTimeStepAndGenerator((t, u) => AddMinimumUpTime(t, u));
                ForEachTimeStepAndGenerator((t, u) => AddMinimumDownTime(t, u));
            }
        }

        private void AddMinimumUpTime(int t, int u)
        {
            var unit = PS.Units[u];
            var amountOfTimeStartedInPeriod = new GRBLinExpr();
            int maxLookBack = Math.Max(0, t  - unit.minUpTime);
            for (int t2 = t; t2 > maxLookBack; t2--)
            {
                amountOfTimeStartedInPeriod += Variable.Start[t2, u];
            }
            Model.AddConstr(Variable.Commit[t, u] >= amountOfTimeStartedInPeriod, "MinUpTime" + t + "u" + u);
        }

        private void AddMinimumDownTime(int t, int u)
        {
            var unit = PS.Units[u];
            var amountOfTimeStoppedInPeriod = new GRBLinExpr();
            int maxLookBack = Math.Max(0, t  - unit.minDownTime);
            for (int t2 = t; t2 > maxLookBack; t2--)
            {
                amountOfTimeStoppedInPeriod += Variable.Stop[t2, u];
            }
                Model.AddConstr(1 - Variable.Commit[t, u] >= amountOfTimeStoppedInPeriod, "MinDownTime" + t + "u" + u);
        }
    }
}
