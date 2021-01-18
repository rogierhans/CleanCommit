using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class PiecewiseConstraint : Constraint
    {


        public PiecewiseConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver)
        {

        }

        public override void AddConstraint()
        {
            if (CC.Tight && CC.RampingLimits && CC.MinUpMinDown)
            {
                AddTightConstraint();
            }
            else
            {
                AddNormalConstraint();
            }
        }

        private void AddNormalConstraint()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Model.AddConstr(SummationTotal(totalPiecewiseSegments, s => Variable.Piecewise[t, u, s]) == Variable.P[t, u], "");
                    var unit = PS.Units[u];
                    for (int s = 0; s < totalPiecewiseSegments; s++)
                    {
                        var maxGeneration = Variable.PiecewiseGeneration[u].PiecewiseLengths[s] * Variable.Commit[t, u];
                        Model.AddConstr(Variable.Piecewise[t, u, s] <= maxGeneration, "");
                    }
                }
            }
        }

        private void AddTightConstraint()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Model.AddConstr(SummationTotal(totalPiecewiseSegments, s => Variable.Piecewise[t, u, s]) == Variable.P[t, u], "");
                    var unit = PS.Units[u];
                    for (int s = 0; s < totalPiecewiseSegments; s++)
                    {
                        var piece = Variable.PiecewiseGeneration[u];
                        var maxGeneration = piece.PiecewiseLengths[s] * Variable.Commit[t, u];
                        var maxStartLimit = piece.PiecewiseStartUpLimit[s];
                        var maxStopLimit = piece.PiecewiseShutDownLimit[s];
                        var startLimit = maxStartLimit * Variable.Start[t, u];
                        var stopLimit = maxStopLimit * MaybeVar(Variable.Stop, t + 1, u);
                        var positiveDiffernceStartStopLimit = U.ZeroOrGreater(maxStopLimit - maxStartLimit) * MaybeVar(Variable.Stop, t + 1, u);
                        var positiveDiffernceStopStartLimit = U.ZeroOrGreater(maxStartLimit - maxStopLimit) * Variable.Start[t, u];
                        if (unit.minUpTime > 1)
                        {
                            Model.AddConstr(Variable.Piecewise[t, u, s] <= maxGeneration - startLimit - stopLimit, "");
                        }
                        else
                        {
                            Model.AddConstr(Variable.Piecewise[t, u, s] <= maxGeneration - startLimit - positiveDiffernceStartStopLimit, "");
                            Model.AddConstr(Variable.Piecewise[t, u, s] <= maxGeneration - stopLimit - positiveDiffernceStopStartLimit, "");
                        }
                    }
                }
            }
        }
    }
}
