using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class ReserveConstraint : Constraint
    {
        public ReserveConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables) : base(ps, cc, model, variables) { }

        public override void AddConstraint()
        {
            AddThermalLimits();
            AddRampLimits();
            AddStorageLimits();
            AddStorageRampLimits();

            for (int t = 0; t < totalTime; t++)
            {
                for (int rIndex = 0; rIndex < CC.Reserves.Count; rIndex++)
                {

                    var totalReserve = new GRBLinExpr();
                    for (int u = 0; u < totalUnits; u++)
                    {
                        totalReserve += (Variable.ReserveThermal[t,u, rIndex]);
                    }
                    for (int u = 0; u < totalStorageUnits; u++)
                    {
                        totalReserve += (Variable.ReserveStorage[t, u, rIndex]);
                    }
                    var reservequirement = CC.Reserves[rIndex].GetReserve(PS, t);
                    Console.WriteLine("{0} {1} {2}", t, rIndex, reservequirement);
                    Model.AddConstr(totalReserve + Variable.LossOfReserve[t] >= reservequirement, "Reserve_" + t);
                }

                //                Model.AddConstr(totalReserve >= PS.GetReserve(t), "Reserve_" + t);
            }

        }
        private void AddThermalLimits()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    var unit = PS.Units[u];
                    var sum = new GRBLinExpr();
                    for (int reserve = 0; reserve < CC.Reserves.Count; reserve++)
                    {
                        sum += Variable.ReserveThermal[t, u, reserve];
                    }
                    sum += Variable.P[t, u];
                    Model.AddConstr(sum <= unit.pMax - unit.pMin,"" );
                }
            }
        }

        private void AddRampLimits()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    var unit = PS.Units[u];
                    for (int reserve = 0; reserve < CC.Reserves.Count; reserve++)
                    {
                        Model.AddConstr(Variable.ReserveThermal[t, u, reserve] <= unit.RU * CC.Reserves[reserve].RatioRamped, "");
                    }

                }
            }
        }

        private void AddStorageLimits()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalStorageUnits; u++)
                {
                    var unit = PS.StorageUnits[u];
                    var sum = new GRBLinExpr();
                    for (int reserve = 0; reserve < CC.Reserves.Count; reserve++)
                    {
                        sum += Variable.ReserveStorage[t, u, reserve];
                    }
                    sum += Variable.Discharge[t, u];
                    Model.AddConstr(sum <= unit.MaxDischarge, "");
                }
            }
        }
        private void AddStorageRampLimits()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalStorageUnits; u++)
                {
                    var unit = PS.StorageUnits[u];
                    for (int reserve = 0; reserve < CC.Reserves.Count; reserve++)
                    {
                        
                        Model.AddConstr(Variable.ReserveStorage[t, u, reserve] <= unit.MaxDischarge * CC.Reserves[reserve].RatioRamped, "");
                        if (t > 1)
                        {
                            Model.AddConstr(Variable.ReserveStorage[t, u, reserve] <= Variable.Storage[t-1, u] * unit.DischargeEffiency, "");
                        }
                    }
                }
            }
        }




        //Sub 5 min, 100% spin              1% of demand
        //10 min, 50% spin                  Maximum of 6% of demand and the largest contingency
        //1 h, 100% spin                    10% of wind generation+7.5% of solar generation
    }
}
