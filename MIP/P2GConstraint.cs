using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class P2GConstraint : Constraint
    {
        public P2GConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables) : base(ps, cc, model, variables) { }

        public override void AddConstraint()
        {
            GRBLinExpr TotalP2G = new GRBLinExpr();
            for (int n = 0; n < totalNodes; n++)
            {
                for (int t = 0; t < totalTime; t++)
                {
                    TotalP2G.Add(Variable.P2GGeneration[n,t]);
                }
            }
            Model.AddConstr(TotalP2G >= PS.P2GYearlyDemand * ((double)totalTime / 8760.0),"P2g yearly constraint");
        }
    }
}
