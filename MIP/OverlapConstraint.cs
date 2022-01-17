using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class OverlapConstraint : Constraint
    {
        public OverlapConstraint(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variables, int horizon, int forced, Solution Old, double[,] ForcedP2G, int rollingOffset) : base(ps, cc, model, variables)
        {
            //for (int t = 0; t < CC.TotalTime; t++)
            //{
            //    int indexold = t + rollingOffset;
            //    for (int n = 0; n < totalNodes; n++)
            //    {
            //       // Model.AddConstr(variables.P2GGeneration[n, t] == ForcedP2G[n, indexold], "P2G Overlap_" + n + "_" + t + "_" +indexold + "_" + cc.TimeOffSet );
            //    }
            //}
            for (int t = 0; t < forced; t++)
            {
                int indexold = (CC.TimeOffSet - Old.CC.TimeOffSet) + t ;

               // string info = String.Format("t:{0} overlap:{1} totalTime:{2} horizon:{3} indexold:{4}", t, overlap, totalTime, horizon, indexold);
                Model.AddConstr(variables.LossOfReserve[t] == Old.LossOfReserve[indexold], "LossOfReserve Overlap" + t);

                for (int g = 0; g < totalUnits; g++)
                {
                    Model.AddConstr(variables.P[t,g] == Old.P[indexold,g], "P Overlap_" + t  + "_" +g);
                    Model.AddConstr(variables.Commit[t, g] == Old.Commit[indexold, g], "Commit Overlap" + t + "_" + g);
                    Model.AddConstr(variables.Start[t, g] == Old.Start[indexold, g], "Start Overlap_" + t + "_" + g);
                    Model.AddConstr(variables.Stop[t, g] == Old.Stop[indexold, g], "Stop Overlap_" + t + "_" + g);
                    for (int res = 0; res < CC.Reserves.Count; res++)
                    {
                        Model.AddConstr(variables.ReserveThermal[t, g, res] == Old.ReserveThermal[indexold, g, res], "Storage Reserve Overlap_" + t + "_" + g + "_" + res);
                    }
                }
                for (int r = 0; r < totalRESUnits; r++)
                {
                    Model.AddConstr(variables.RESDispatch[t, r] == Old.RESDispatch[indexold, r], "RESDispatch Overlap_t:" + t + "_r:" + r +"_oldT:"+indexold + "_offset"+ CC.TimeOffSet);
                }
                for (int l = 0; l < totalLinesAC; l++)
                {
                    Model.AddConstr(variables.TransmissionFlowAC[l,t] == Old.TransmissionFlowAC[l, indexold], "AC Overlap_" + l + "_" + t);

                }
                for (int l = 0; l < totalLinesDC; l++)
                {
                    Model.AddConstr(variables.TransmissionFlowDC[l, t] == Old.TransmissionFlowDC[l, indexold], "DC Overlap_" + l + "_" + t);
                }
                for (int n = 0; n < totalNodes; n++)
                {
                    Model.AddConstr(variables.NodeVoltAngle[n, t] == Old.NodeVoltAngle[n, indexold], "VoltAngles Overlap_" + n + "_" + t);
                }
                for (int s = 0; s < totalStorageUnits; s++)
                {
                    Model.AddConstr(variables.Charge[t, s] == Old.Charge[indexold, s], "Charge Overlap_" + t + "_" + s);
                    Model.AddConstr(variables.Discharge[t, s] == Old.Discharge[indexold, s], "Discharge Overlap_" + t + "_" + s);
                    Model.AddConstr(variables.Storage[t, s] == Old.Storage[indexold, s], "Storage Overlap_" + t + "_" + s);
                    for (int res = 0; res < CC.Reserves.Count; res++)
                    {
                        Model.AddConstr(variables.ReserveStorage[t, s,res] == Old.ReserveStorage[indexold, s,res], "Storage Reserve Overlap_" + t + "_" + s + "_" + res);
                    }
                }
                for (int n = 0; n < totalNodes; n++)
                {
                    Model.AddConstr(variables.NodalLossOfLoad[n, t] == Old.NodalLossOfLoad[n, indexold], "NodalLossOfLoad Overlap_" + n + "_" + t);
                    Model.AddConstr(variables.RESIDUALDemand[n, t] == Old.RESIDUALDemand[n, indexold], "RESIDUALDemand Overlap_" + n + "_" + t +"_" );
                    Model.AddConstr(variables.DemandShed[n, t] == Old.DemandShed[n, indexold], "DemandResponse Overlap_" + n + "_" + t);
                    Model.AddConstr(variables.NodalInjectionAC[n, t] == Old.NodalInjectionAC[n, indexold], "NodalInjectionAC Overlap_" + n + "_" + t);
                    Model.AddConstr(variables.NodalInjectionDC[n, t] == Old.NodalInjectionDC[n, indexold], "NodalInjectionDC Overlap_" + n + "_" + t);
                    //Model.AddConstr(variables.P2GGeneration[n, t] == Old.P2GGeneration[n, indexold], "P2GGeneration Overlap_" + n + "_" + t);
                }
            }
        }
        public override void AddConstraint()
        {
            throw new NotImplementedException();
        }
    }
}
