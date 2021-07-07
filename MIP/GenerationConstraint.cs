using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;

namespace CleanCommit.MIP
{
    class GenerationConstraint : Constraint
    {
        public GenerationConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables) : base(ps, cc, model, variables)
        {

        }

        public override void AddConstraint()
        {
            if (CC.Tight && CC.RampingLimits && CC.MinUpMinDown)
            {
                ForEachTimeStepAndGenerator((t, u) => AddTightConstraint(t, u));
            }
            else
            {
                ForEachTimeStepAndGenerator((t, u) => AddNormalConstraint(t, u));
            }

        }

        private void AddTightConstraint(int t, int u)
        {

            Unit unit = PS.Units[u];
            //constraint 23a 
            var maxGeneration = (unit.pMax - unit.pMin) * Variable.Commit[t, u];
            var startupConstraint23a = (unit.pMax - unit.SU) * Variable.Start[t, u];
            var stopNextConstraint23a = U.ZeroOrGreater(unit.SU - unit.SD) * MaybeVar(Variable.Stop, t + 1, u);
            var upperbound23a = maxGeneration - startupConstraint23a - stopNextConstraint23a;


            //constraint 23b
            var startupConstraint23b = U.ZeroOrGreater(unit.SD - unit.SU) * Variable.Start[t, u];
            var stopNextConstraint23b = (unit.pMax - unit.SD) * MaybeVar(Variable.Stop, t + 1, u);
            var upperbound23b = maxGeneration - startupConstraint23b - stopNextConstraint23b;


            //constraint 38 
            int TRU = (int)Math.Floor((unit.pMax - unit.SU) / unit.RU);
            int maxInt38 = U.MaxLookback(Math.Min(unit.minUpTime - 2, TRU), t);
            var summation38 = Summation(maxInt38, i => (unit.pMax - unit.SU - i * unit.RU) * Variable.Start[t - i, u]);
            var upperbound38 = maxGeneration - stopNextConstraint23b - summation38;

            //constraint 40
            int maxInt40 = U.MaxLookback(Math.Min(unit.minUpTime - 1, TRU), t);
            var summation40 = Summation(maxInt40, i => (unit.pMax - unit.SU - i * unit.RU) * Variable.Start[t - i, u]);
            var upperbound40 = maxGeneration - summation40;


            //constraint 41
            int TRD = (int)Math.Floor((unit.pMax - unit.SD) / unit.RD);
            int KSD = U.Min(TRD, unit.minUpTime - 1, totalTime - t - 2);
            int KSU = U.Min(TRU, unit.minUpTime - 2 - U.ZeroOrGreater(KSD), t - 1);
            var upperbound41 = maxGeneration
                - Summation(KSD, i => (unit.pMax - (unit.SD + i * unit.RD)) * Variable.Stop[t + 1 + i, u])
                - Summation(KSU, i => (unit.pMax - (unit.SU + i * unit.RU)) * Variable.Start[t - i, u]);



            if (CC.RampingLimits)
            {
                if (unit.minUpTime == 1)
                {
                    Model.AddConstr(Variable.P[t, u] <= upperbound23a, "");
                    Model.AddConstr(Variable.P[t, u] <= upperbound23b, "");
                }
                Model.AddConstr(Variable.P[t, u] <= upperbound38, "");
                if (TRU > unit.minUpTime - 2)
                {
                    Model.AddConstr(Variable.P[t, u] <= upperbound40, "");
                }
                if (KSD > 0)
                {
                    Model.AddConstr(Variable.P[t, u] <= upperbound41, "");
                }
            }
            else
            {
                Model.AddConstr(Variable.P[t, u] <= maxGeneration, "");
            }
            Model.AddConstr(Variable.P[t, u] <= maxGeneration, "");

        }
        private void AddNormalConstraint(int t, int u)
        {
            Unit unit = PS.Units[u];
            GRBLinExpr maxGeneration = (unit.pMax - unit.pMin) * Variable.Commit[t, u];
            Model.AddConstr(Variable.P[t, u] <= maxGeneration, "");
        }
    }
}
