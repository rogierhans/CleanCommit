using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.Instance;
using Gurobi;
namespace CleanCommit.MIP
{
    class TightGenerationConstraint : Constraint
    {
        public TightGenerationConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables) : base(ps, cc, model, variables)
        {

        }
        public NewFormulation2[] Saved;
        public override void AddConstraint()
        {
            Saved = new NewFormulation2[totalUnits];
            for (int u = 0; u < totalUnits; u++)
            { 
                GRBVar[] CopyP = new GRBVar[totalTime];
                GRBVar[] CopyCommit = new GRBVar[totalTime];
                GRBVar[] CopyStart = new GRBVar[totalTime];
                GRBVar[] CopyStop = new GRBVar[totalTime];
                for (int t = 0; t < totalTime; t++)
                {
                    CopyP[t] = Variable.P[t,u];
                    CopyCommit[t] = Variable.Commit[t, u];
                    CopyStart[t] = Variable.Start[t, u];
                    CopyStop[t] = Variable.Stop[t, u];

                }

                Saved[u] =new NewFormulation2(Model, PS.Units[u], totalTime, CC.Relax ? GRB.CONTINUOUS : GRB.BINARY, CopyP, CopyCommit, CopyStart, CopyStop);
            }

        }

        public void Print()
        {
            for (int u = 0; u < totalUnits; u++)
            {
                Saved[u].Print();
            }

        }
    }
}
