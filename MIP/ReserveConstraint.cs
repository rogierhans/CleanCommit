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
            for (int n = 0; n < totalNodes; n++)
            {

                Node node = PS.Nodes[n];
                for (int t = 0; t < totalTime; t++)
                {
                    var totalReserve = new GRBLinExpr();
                    for (int u = 0; u < totalUnits; u++)
                    {
                       if( node.UnitsIndex.Contains(u))
                        totalReserve += (Variable.PotentialP[t, u] - Variable.P[t, u]);
                    }
                    Model.AddConstr(totalReserve + Variable.NodalLossOfReserve[n, t] >= (node.NodalDemand(t) * PS.RatioReserveDemand), "Reserve_" + n + "_" + t);
                }
            }
        }
    }
}
