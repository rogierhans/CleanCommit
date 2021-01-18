using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class TimeDepStartConstraint : Constraint
    {
        public TimeDepStartConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver)
        {

        }

        public override void AddConstraint()
        {
            if (CC.TimeDependantStartUpCost)
            {
                OneStartUpTypePerStartup();
                RelateStartupTypeWithShutdown();
            }
        }


        public void OneStartUpTypePerStartup()
        {
            ForEachTimeStepAndGenerator((t, u) =>
            {
                var sum = new GRBLinExpr();
                for (int e = 0; e < Variable.StartCostIntervall[0, u].Count(); e++)
                {
                    sum += Variable.StartCostIntervall[t, u][e];
                }
                Model.AddConstr(Variable.Start[t, u] == sum, "StartCostContraint4.25" + t + "u:" + u);
            });
        }

        public void RelateStartupTypeWithShutdown()
        {
            ForEachTimeStepAndGenerator((t, u) =>
            {
                var unit = PS.Units[u];
                for (int e = 0; e < unit.StartInterval.Length - 1; e++)
                {
                    var sum = new GRBLinExpr();
                    int from = t - unit.StartInterval[e + 1];
                    int to = t - unit.StartInterval[e];
                    if (from < 0) return;
                    for (int t2 = from; t2 < to; t2++)
                    {
                        sum += Variable.Stop[t2, u];
                    }
                    Model.AddConstr(Variable.StartCostIntervall[t, u][e] <= sum, "StartCostContraint4.26" + t + "u:" + u + "e" + e);
                }
            });
        }
    }
}
