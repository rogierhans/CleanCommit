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
                    var inflow = PS.Inflows.FirstOrDefault(unit => unit.StorageID == s);
                    var inflowValue = inflow != null ? inflow.Inflows[t] : 0;
                    if (t == 0)
                    {
                        StorageLevelConstaints[t, s] = Model.AddConstr(Variable.Storage[0, s] == Variable.Charge[0, s] * StorageUnit.ChargeEffiency - Variable.Discharge[0, s] * StorageUnit.DischargeEffiencyInverse + inflowValue, "InitalStorageLevel" + s);
                    }
                    else
                    {
                        StorageLevelConstaints[t, s] = Model.AddConstr(Variable.Storage[t, s] == Variable.Storage[t - 1, s] + Variable.Charge[t, s] * StorageUnit.ChargeEffiency - Variable.Discharge[t, s] * StorageUnit.DischargeEffiencyInverse + inflowValue, "StorageLevel" + t + "s" + s);
                    }
                }
            }
        }
    }
}
