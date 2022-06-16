using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class RampConstraint : Constraint
    {
        protected GRBConstr[,] UpwardRampingConstr;
        protected GRBConstr[,] DownwardRampingConstr;
        public RampConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver) { }

        public override void AddConstraint()
        {
            if (CC.RampingLimits)
            {
                UpwardRampingConstr = new GRBConstr[totalTime, totalUnits];
                DownwardRampingConstr = new GRBConstr[totalTime, totalUnits];
                ForEachTimeStepAndGenerator((t, u) => AddRampingConstraint(t, u), 1, totalTime);
            }
        }

        public void AddRampingConstraint(int t, int u)
        {
            Unit unit = PS.Units[u];

            var upwardRampingLimitNormal = unit.RU * Variable.Commit[t, u];
            var upwardRampingLimitStartup = (unit.SU - unit.PMin - unit.RU) * Variable.Start[t, u];
            var upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
           // Console.ReadLine();
            //Console.WriteLine("check dit even");
            UpwardRampingConstr[t, u] = Model.AddConstr(Variable.P[t, u] - Variable.P[t - 1, u] <= upwardRampingLimit, "r" + u + "t" + t);
           // UpwardRampingConstr[t, u] = Model.AddConstr(Variable.PotentialP[t, u] - Variable.P[t - 1, u] <= upwardRampingLimit, "r" + u + "t" + t);

            var downwardRampingLimitNormal = unit.RD * Variable.Commit[t - 1, u];
            var downwardRampingLimitShutdown = Variable.Stop[t, u] * (unit.SD - unit.PMin - unit.RD);
            var downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
            DownwardRampingConstr[t, u] = Model.AddConstr(Variable.P[t - 1, u] - Variable.P[t, u] <= downwardRampingLimit, "r" + u + "t" + t);
        }

    }
}
