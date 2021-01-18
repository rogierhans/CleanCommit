using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class PowerBalanceContraint : Constraint
    {
        public PowerBalanceContraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver)
        {
        }

        GRBConstr[,] NodalPowerBalance;
        public override void AddConstraint()
        {
            NodalPowerBalance = new GRBConstr[totalNodes, totalTime];
            ForEachNodeAndTimeStep((n, t) => AddPowerBalanceConstraint(n, t));
        }
        private void AddPowerBalanceConstraint(int n, int t)
        {
            Node node = PS.Nodes[n];
            GRBLinExpr generation = new GRBLinExpr();
            generation += NodalGeneration(n, t);
            generation += NodalDischarge(n, t);
            generation += NodalResGeneration(n, t);
            generation += Variable.NodalInjectionAC[n, t];
            generation += Variable.NodalInjectionDC[n, t];
            generation += Variable.NodalLossOfLoad[n, t];

            GRBLinExpr consumption = new GRBLinExpr();
            consumption += node.NodalDemand(t) * CC.DemandMultiplier * (n == 0 ? CC.FistNodeM : 1);
            consumption += GetNodalTotalCharge(n, t);

            NodalPowerBalance[n, t] = Model.AddConstr(generation == consumption, "NodalPowerBalance" + t);
        }

        private GRBLinExpr GetNodalTotalCharge(int n, int t)
        {
            var totalCharge = new GRBLinExpr();

            var node = PS.Nodes[n];
            foreach (int s in node.StorageUnitsIndex)
            {
                totalCharge += Variable.Charge[t, s];
            }

            return totalCharge;
        }
        private GRBLinExpr NodalGeneration(int nodeIndex, int t)
        {
            var totalGeneration = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (int u in node.UnitsIndex)
            {
                var unit = PS.Units[u];
                totalGeneration += Variable.P[t, u] + unit.pMin * Variable.Commit[t, u];
            }
            return totalGeneration;
        }
        private GRBLinExpr NodalResGeneration(int nodeIndex, int t)
        {
            GRBLinExpr ResGeneration = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (int r in node.RESindex)
            {
                ResGeneration += Variable.RESDispatch[t, r];
            }
            return ResGeneration;
        }
        private GRBLinExpr NodalDischarge(int nodeIndex, int t)
        {
            var NodalDischarge = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (int s in node.StorageUnitsIndex)
            {
                NodalDischarge += Variable.Discharge[t, s];
            }
            return NodalDischarge;
        }
    }
}
