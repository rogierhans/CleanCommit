using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class LogicConstraint : Constraint
    {
        public LogicConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver)
        {

        }

        public override void AddConstraint()
        {
            ForEachTimeStepAndGenerator((t, u) => AddLogicConstraint(t,u),1,totalTime);
        }
        public void AddLogicConstraint(int t, int u) {

                var PowerPlantLogic = Variable.Commit[t - 1, u] - Variable.Commit[t, u] + Variable.Start[t, u] - Variable.Stop[t, u] == 0;
                Model.AddConstr(PowerPlantLogic, "Power Plant Logic" + t + " " + u);
        }
    }
}
