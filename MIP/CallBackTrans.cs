using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit.MIP
{
    class CallBackTrans : GRBCallback
    {
        protected int totalNodes;
        protected int totalTime;
        protected int totalUnits;
        protected int totalLinesAC;
        protected int totalLinesDC;
        protected int totalStorageUnits;
        protected int totatRESTypes;
        protected int totalPiecewiseSegments;

        protected GRBModel Model;
        protected PowerSystem PS;
        protected ConstraintConfiguration CC;
        protected Variables Variable;
        GRBVar[,] NodalPowerBalance;
        GRBVar[,] Boven;
        public CallBackTrans(PowerSystem ps, ConstraintConfiguration cc, GRBModel model, Variables variable, GRBVar[,] nodalPowerBalance, GRBVar[,] boven)
        {
            Model = model;
            PS = ps;
            CC = cc;
            totalTime = CC.TotalTime;
            totalUnits = PS.Units.Count;
            totalNodes = PS.Nodes.Count;
            totalLinesDC = PS.LinesDC.Count;
            totalLinesAC = PS.LinesAC.Count;
            totalStorageUnits = PS.StorageUnits.Count;
            totatRESTypes = PS.ResGenerations.Count;
            totalPiecewiseSegments = CC.PiecewiseSegments;
            Variable = variable;
            NodalPowerBalance = nodalPowerBalance;
            Boven = boven;
        }

        protected override void Callback()
        {
            Console.Write("helllo {0}", where);
            // Console.ReadLine();
            //  try

            {
                if (where == GRB.Callback.MIPSOL)
                {
                    for (int n = 0; n < totalNodes; n++)
                    {
                        for (int t = 0; t < totalTime; t++)
                        {
                            var sol = GetSolution(NodalPowerBalance[n, t]);
                            var node = PS.Nodes[n];
                            var min = node.Units.Count > 0 ? node.Units.Min(x => x.PMin) : 0;
                            if (sol < min + 0.00001 && sol > 0.0000001)
                            {
                                //Console.WriteLine("{0} <= {1} <= {2}", 0, sol, min);
                                //Console.ReadLine();
                                Console.Write("{0} <= {1} <= {2}", 0, sol, min);
                                // AddLazy(NodalPowerBalance[n, t] == Boven[n,t]);
                            }
                        }
                    }
                }
            }
            // catch (GRBException e)
            // {
            //      Console.WriteLine(" Error code : " + e.ErrorCode + ". " + e.Message);
            ////     Console.WriteLine(e.StackTrace);
            // }
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
                totalGeneration += Variable.P[t, index] + unit.PMin * Variable.Commit[t, index];
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


    class ConsoleOverwrite : GRBCallback
    {

        public ConsoleOverwrite()
        {
        }
        protected override void Callback()
        {
            if (this.where == GRB.Callback.MESSAGE)
            {
                String text = this.GetStringInfo(GRB.Callback.MSG_STRING);
                Console.Write(text);
            }
        }
    }
}
