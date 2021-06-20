using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class ReserveConstraint : Constraint
    {
        public ReserveConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables) : base(ps, cc, model, variables) { }

        public override void AddConstraint()
        {

            for (int t = 0; t < totalTime; t++)
            {
                var totalReserve = new GRBLinExpr();
                for (int u = 0; u < totalUnits; u++)
                {

                    totalReserve += (Variable.PotentialP[t, u] - Variable.P[t, u]);
                }
                Model.AddConstr(totalReserve + Variable.LossOfReserve[t] >=  PS.GetReserve(t), "Reserve_" + t);
                //                Model.AddConstr(totalReserve >= PS.GetReserve(t), "Reserve_" + t);
            }

        }
        //Sub 5 min, 100% spin              1% of demand
        //10 min, 50% spin                  Maximum of 6% of demand and the largest contingency
        //1 h, 100% spin                    10% of wind generation+7.5% of solar generation
    }
}
