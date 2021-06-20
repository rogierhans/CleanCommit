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
        public GRBVar[,] NodalResidualDemand;
        public GRBVar[,] Boven;
        public override void AddConstraint()
        {

            NodalPowerBalance = new GRBConstr[totalNodes, totalTime];
            NodalResidualDemand = new GRBVar[totalNodes, totalTime];
            Boven = new GRBVar[totalNodes, totalTime];
            ForEachNodeAndTimeStep((n, t) => AddPowerBalanceConstraint(n, t));
          //  ForEachNodeAndTimeStep((n, t) => Link(n, t));
           // ForEachNodeAndTimeStep((n, t) => AddBoven(n, t));
            //var test = NodalResidualDemand[0, 0] == Boven[0,0];
            //Model.AddConstr(test, "");
        }
        private void AddBoven(int n, int t)
        {
            var node = PS.Nodes[0];
            var min = node.Units.Count > 0 ? node.Units.Min(x => x.pMin) : 0;
            var max = node.Units.Count > 0 ? node.Units.Sum(x => x.pMax) : 0;
            Boven[n,t]= Model.AddVar(min, max, 0, GRB.SEMICONT, "");
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

        private void Link(int n, int t)
        {
            Node node = PS.Nodes[n];
            GRBLinExpr generation = new GRBLinExpr();
            // generation += NodalGeneration(n, t);
            generation += NodalDischarge(n, t);
            generation += NodalResGeneration(n, t);
            generation += Variable.NodalInjectionAC[n, t];
            generation += Variable.NodalInjectionDC[n, t];
            generation += Variable.NodalLossOfLoad[n, t];

            GRBLinExpr consumption = new GRBLinExpr();
            consumption += node.NodalDemand(t) * CC.DemandMultiplier * (n == 0 ? CC.FistNodeM : 1);
            consumption += GetNodalTotalCharge(n, t);
            var demand = consumption - generation;

            NodalResidualDemand[n, t] = Model.AddVar(double.MinValue, double.MaxValue, 0, GRB.CONTINUOUS, "Residual Demand No Generation");
            Model.AddConstr(NodalResidualDemand[n, t] == demand, "Residual Demand No Generation");
            //NodalPowerBalance[n, t] = Model.AddConstr(demand <= boven * max, "NodalPowerBalanceYolo" + t);
        }



        private GRBLinExpr GetNodalTotalCharge(int n, int t)
        {
            var totalCharge = new GRBLinExpr();

            var node = PS.Nodes[n];
            foreach (var StorageID in node.StorageUnits)
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
            foreach (var unitID in node.Units)
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
            foreach (var RESID in node.RES)
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
            foreach (var StorageID in node.StorageUnits)
            {
                var storageindex = Variable.SUnitID2Index[StorageID];
                NodalDischarge += Variable.Discharge[t, storageindex];
            }
            return NodalDischarge;
        }
    }
}
