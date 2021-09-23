using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class StorageConstraint : Constraint
    {
        public StorageConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables solver) : base(ps, cc, model, solver)
        {

        }

        public override void AddConstraint()
        {
            AddStorageConstraints();
        }

        GRBConstr[,] StorageLevelConstaints;
        private void AddStorageConstraints()
        {
            StorageLevelConstaints = new GRBConstr[totalTime, totalStorageUnits];
            for (int t = 0; t < totalTime; t++)
            {
                for (int s = 0; s < totalStorageUnits; s++)
                {
                    var StorageUnit = PS.StorageUnits[s];
                    var inflowValue = StorageUnit.GetInflow(t);
                    // Console.WriteLine(inflowValue);
                    var inflowVar = Model.AddVar(0, inflowValue, 0.0, GRB.CONTINUOUS, "auxiliaryVariableStorageInflow_" + t + "_" + s);
                    var init = StorageUnit.MaxEnergy / 2;
                    if (t == 0)
                    {
                        StorageLevelConstaints[t, s] = Model.AddConstr(Variable.Storage[0, s] == Variable.Charge[0, s] * StorageUnit.ChargeEffiency - Variable.Discharge[0, s] * StorageUnit.DischargeEffiencyInverse + inflowVar + init, "InitalStorageLevel" + s);
                    }
                    else if (t < totalTime - 1)
                    {
                        StorageLevelConstaints[t, s] = Model.AddConstr(Variable.Storage[t, s] == Variable.Storage[t - 1, s] + Variable.Charge[t, s] * StorageUnit.ChargeEffiency - Variable.Discharge[t, s] * StorageUnit.DischargeEffiencyInverse + inflowVar, "StorageLevel" + t + "s" + s);
                    }
                    else {
                        StorageLevelConstaints[t, s] = Model.AddConstr(Variable.Storage[t, s] == Variable.Storage[t - 1, s] + Variable.Charge[t, s] * StorageUnit.ChargeEffiency - Variable.Discharge[t, s] * StorageUnit.DischargeEffiencyInverse + inflowVar, "StorageLevel" + t + "s" + s);
                        Model.AddConstr(Variable.Storage[t, s] == init, "");
                    }
                }
            }
        }
    }
}
