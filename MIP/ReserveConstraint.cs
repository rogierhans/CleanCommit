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
                Model.AddConstr(totalReserve + Variable.LossOfReserve[t] >=  PS.Reserves[t], "Reserve_" + t);
            }

        }
    }
}
