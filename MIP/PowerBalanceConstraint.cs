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
            foreach (string StorageID in node.StorageUnitIDs)
            {
                var storageindex = Variable.SUnitID2Index[StorageID];
                totalCharge += Variable.Charge[t, storageindex];
            }

            return totalCharge;
        }
        private GRBLinExpr NodalGeneration(int nodeIndex, int t)
        {
            var totalGeneration = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (string unitID in node.UnitsIndex)
            {
                int index = Variable.UnitID2Index[unitID];
                var unit = PS.Units[index];
                totalGeneration += Variable.P[t, index] + unit.pMin * Variable.Commit[t, index];
            }
            return totalGeneration;
        }
        private GRBLinExpr NodalResGeneration(int nodeIndex, int t)
        {
            GRBLinExpr ResGeneration = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (string RESID in node.RESIDs)
            {
                var resIndex = Variable.RUnitID2Index[RESID];
                ResGeneration += Variable.RESDispatch[t, resIndex];
            }
            return ResGeneration;
        }
        private GRBLinExpr NodalDischarge(int nodeIndex, int t)
        {
            var NodalDischarge = new GRBLinExpr();
            var node = PS.Nodes[nodeIndex];
            foreach (string StorageID in node.StorageUnitIDs)
            {
                var storageindex = Variable.SUnitID2Index[StorageID];
                NodalDischarge += Variable.Discharge[t, storageindex];
            }
            return NodalDischarge;
        }
    }
}
