using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit
{
   public class NewFormulation2
    {
        Unit Unit;
        GRBModel Model;
        // List<double> Commitsolution = new List<double>();
        int totalTime;
        public NewFormulation2(GRBModel model, Unit unit, int timesteps, char type, GRBVar[] p, GRBVar[] commit, GRBVar[] start, GRBVar[] stop)
        {
            this.Type = type;
            P = p;
            Commit = commit;
            Start = start;
            Stop = stop;
            Unit = unit;
            totalTime = timesteps;
            Model = model;
            Rup = (int)Math.Ceiling((Unit.pMax - Unit.pMin) / (double)Unit.RU);
            Rup = Math.Max(Rup, Unit.minUpTime);
            Rextra = 1 + Math.Max(0, Rup - Unit.minUpTime);
            RDup = (int)Math.Ceiling((Unit.pMax - Unit.pMin) / (double)Unit.RD);
            RDup = Math.Max(RDup, Unit.minUpTime);
            RDextra = 1 + Math.Max(0, RDup - Unit.minUpTime);
            //Print();
            AddP_Forward();
            AddP_Back();
            LinkObjectives_Forward();
            AddNetwork();
            AddNetwork_Back();
            AddLimits();
            AddLimits_Back();
            AddRampingLimits();
            //AddRampingLimits_Back();
            AddLogic();

        }
        private void LinkObjectives_Forward()
        {
            for (int t = 0; t < totalTime; t++)
            {
                GRBLinExpr sum = new GRBLinExpr();
                for (int tau = 0; tau < Rup; tau++)
                {

                    sum += P_forward[t, tau];
                }
                Model.AddConstr(P[t] + Unit.pMin * Commit[t] == sum, "Pbacksum2m");
            }
            for (int t = 0; t < totalTime; t++)
            {
                GRBLinExpr sum = new GRBLinExpr();
                for (int tau = 0; tau < RDup; tau++)
                {

                    sum += P_Back[t, tau];
                }
                Model.AddConstr(P[t] + Unit.pMin * Commit[t] == sum, "Pbacksum2m");
            }
        }
        readonly int Rup;
        readonly int Rextra;
        readonly int RDup;
        readonly int RDextra;
        public GRBVar[,] P_forward;
        public GRBVar[] P;
        public GRBVar[] Commit;
        public GRBVar[] Start;
        public GRBVar[] Stop;
        private GRBVar[] beginOnTrans;
        private GRBVar[] beginOffTrans;
        private GRBVar[] endOnTrans;
        private GRBVar[] endOffTrans;
        private GRBVar[,] onOnTrans;
        private GRBVar[,] offOffTrans;
        private GRBVar[,] onOffTrans;
        private GRBVar[] offOnTrans;
        private GRBVar[] beginOnTrans_back;
        private GRBVar[] beginOffTrans_back;
        private GRBVar[] endOnTrans_back;
        private GRBVar[] endOffTrans_back;
        private GRBVar[,] onOnTrans_back;
        private GRBVar[,] offOffTrans_back;
        private GRBVar[] onOffTrans_back;
        private GRBVar[,] offOnTrans__back;
        private readonly char Type;

        private void AddP_Forward()
        {
            P_forward = new GRBVar[totalTime, Rup];
            for (int t = 0; t < totalTime; t++)
            {
                for (int tau = 0; tau < Rup; tau++)
                {
                    P_forward[t, tau] = Model.AddVar(0.0, Unit.pMax, 0.0, GRB.CONTINUOUS, "PF" + t + " " + tau);
                }
            }
        }
        GRBVar[,] P_Back;
        private void AddP_Back()
        {
            P_Back = new GRBVar[totalTime, RDup];
            for (int t = 0; t < totalTime; t++)
            {
                for (int tau = 0; tau < RDup; tau++)
                {
                    P_Back[t, tau] = Model.AddVar(0.0, Unit.pMax, 0.0, GRB.CONTINUOUS, "PB" + t + " " + tau);
                }
            }
        }

        private void AddLogic()
        {
            for (int t = 0; t < totalTime; t++)
            {
                if (t != 0)
                {
                    var ppLogic = Commit[t - 1] - Commit[t] + Start[t] - Stop[t] == 0;
                    Model.AddConstr(ppLogic, "Power Plant Logic" + t);
                }
            }
        }


        private GRBLinExpr Sum(GRBVar[] vars)
        {
            GRBLinExpr sum = new GRBLinExpr();
            for (int i = 0; i < vars.Length; i++)
            {
                sum += vars[i];
            }
            return sum;
        }
        private GRBLinExpr Sum1(GRBVar[,] vars, int index)
        {
            GRBLinExpr sum = new GRBLinExpr();
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                sum += vars[i, index];
            }
            return sum;
        }

        private GRBLinExpr Sum2(GRBVar[,] vars, int index)
        {
            GRBLinExpr sum = new GRBLinExpr();
            for (int i = 0; i < vars.GetLength(1); i++)
            {
                sum += vars[index, i];
            }
            return sum;
        }
        private void AddNetwork()
        {
            // onStates = new GRBVar[totalTime, Unit.minUpTime];
            //offStates = new GRBVar[totalTime, Unit.minDownTime];
            beginOnTrans = new GRBVar[Rup];
            endOnTrans = new GRBVar[Rup];
            beginOffTrans = new GRBVar[Unit.minDownTime];
            endOffTrans = new GRBVar[Unit.minDownTime];
            onOnTrans = new GRBVar[totalTime - 1, Rup];
            onOffTrans = new GRBVar[totalTime - 1, Rextra];
            offOffTrans = new GRBVar[totalTime - 1, Unit.minDownTime];
            offOnTrans = new GRBVar[totalTime - 1];
            {
                //Init
                for (int tau = 0; tau < Rup; tau++)
                {
                    beginOnTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    endOnTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                }

                for (int tau = 0; tau < Unit.minDownTime; tau++)
                {
                    beginOffTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "beginon" + tau);
                    endOffTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "endoff" + tau);
                }
                for (int t = 0; t < totalTime - 1; t++)
                {
                    for (int tau = 0; tau < Rup; tau++)
                    {
                        onOnTrans[t, tau] = Model.AddVar(0, 1, 0.0, Type, "onon" + t + " " + tau);
                    }
                    for (int tau = 0; tau < Unit.minDownTime; tau++)
                    {
                        offOffTrans[t, tau] = Model.AddVar(0, 1, 0.0, Type, "offoff" + t + " " + tau);
                    }
                    for (int r = 0; r < Rextra; r++)
                    {
                        onOffTrans[t, r] = Model.AddVar(0, 1, 0.0, Type, "onOff" + t + " " + r);
                    }
                    offOnTrans[t] = Model.AddVar(0, 1, 0.0, Type, "offon" + t);
                }
            }
            {
                //beginTrans
                Model.AddConstr(Sum(beginOnTrans) + Sum(beginOffTrans) == 1, "");
                Model.AddConstr(Sum(endOnTrans) + Sum(endOffTrans) == 1, "");
            }


            {
                //first node On
                int t = 0;
                for (int tau = 0; tau < Unit.minUpTime - 1; tau++)
                {
                    var inflow = beginOnTrans[tau];
                    var outflow = onOnTrans[t, tau];
                    Model.AddConstr(inflow == outflow, "");
                }

                for (int r = 0; r < Rextra; r++)
                {
                    var tau = Unit.minUpTime - 1 + r;
                    var inflow = beginOnTrans[tau];
                    var outflow = onOnTrans[t, tau] + onOffTrans[t, r];
                    Model.AddConstr(inflow == outflow, "");
                }
                {
                    // total inflow
                    Model.AddConstr(Sum(beginOnTrans) == Commit[t], "");
                }
            }

            {
                //first node Off
                int t = 0;
                for (int tau = 0; tau < Unit.minDownTime - 1; tau++)
                {
                    var inflow = beginOffTrans[tau];
                    var outflow = offOffTrans[t, tau];
                    Model.AddConstr(inflow == outflow, "");
                    // Model.AddConstr(Commit[t] == outflow, "");
                    //Model.AddConstr(Commit[t] == inflow, "");
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var inflow = beginOffTrans[tau];
                    var outflow = offOffTrans[t, tau] + offOnTrans[t];
                    Model.AddConstr(inflow == outflow, "");
                    // Model.AddConstr(Commit[t] == outflow, "");
                    // Model.AddConstr(Commit[t] == inflow, "");
                }
                {
                    // total inflow
                    Model.AddConstr(Sum(beginOffTrans) == 1 - Commit[t], "");

                }
            }
            {
                //last node On
                int t = totalTime - 1;
                var totalInFlow = new GRBLinExpr();
                {
                    var tau = 0;
                    var outflow = endOnTrans[tau];
                    var inflow = offOnTrans[t - 1];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    var outflow = endOnTrans[tau];
                    var inflow = onOnTrans[t - 1, tau - 1];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                for (int r = 0; r < Rextra - 1; r++)
                {
                    var tau = Unit.minUpTime - 1 + r;

                    var inflow = onOnTrans[t - 1, tau - 1];
                    var outflow = endOnTrans[tau];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    var tau = Unit.minUpTime - 1 + Rextra - 1;

                    var inflow = onOnTrans[t - 1, tau - 1] + onOnTrans[t - 1, tau];
                    var outflow = endOnTrans[tau];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    // total inflow
                    Model.AddConstr(totalInFlow == Commit[t], "");
                }
            }

            {
                //last node Off
                int t = totalTime - 1;
                var totalInFlow = new GRBLinExpr();
                {
                    var tau = 0;
                    var outflow = endOffTrans[tau];
                    var inflow = new GRBLinExpr();
                    for (int r = 0; r < Rextra; r++)
                    {
                        //var ontau = Unit.minUpTime - 1 + r;
                        inflow += onOffTrans[t - 1, r];
                    }

                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                {
                    var inflow = endOffTrans[tau];
                    var outflow = offOffTrans[t - 1, tau - 1];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var outflow = endOffTrans[tau];
                    var inflow = offOffTrans[t - 1, tau - 1] + offOffTrans[t - 1, tau];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    // total inflow
                    Model.AddConstr(totalInFlow == 1 - Commit[t], "");
                }
            }
            {
                // t<1 ontrans

                for (int t = 1; t < totalTime - 1; t++)
                {
                    var totalInFlow = new GRBLinExpr();
                    {
                        var tau = 0;
                        var inflow = offOnTrans[t - 1];
                        var outflow = onOnTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                    {
                        var inflow = onOnTrans[t - 1, tau - 1];
                        var outflow = onOnTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    for (int r = 0; r < Rextra - 1; r++)
                    {
                        var tau = Unit.minUpTime - 1 + r;
                        var inflow = onOnTrans[t - 1, tau - 1];
                        var outflow = onOnTrans[t, tau] + onOffTrans[t, r];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minUpTime - 1 + Rextra - 1;
                        var inflow = onOnTrans[t - 1, tau] + onOnTrans[t - 1, tau - 1];
                        var outflow = onOnTrans[t, tau] + onOffTrans[t, Rextra - 1];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        // total inflow
                        Model.AddConstr(totalInFlow == Commit[t], "");
                    }
                }
            }
            {
                //t<1  offtrans
                for (int t = 1; t < totalTime - 1; t++)
                {
                    var totalInFlow = new GRBLinExpr();
                    {
                        var tau = 0;
                        var inflow = new GRBLinExpr();
                        for (int r = 0; r < Rextra; r++)
                        {
                            //var ontau = Unit.minUpTime - 1 + r;
                            inflow += onOffTrans[t - 1, r];
                        }
                        var outflow = offOffTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                    {
                        var inflow = offOffTrans[t - 1, tau - 1];
                        var outflow = offOffTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minDownTime - 1;
                        var inflow = offOffTrans[t - 1, tau - 1] + offOffTrans[t - 1, tau];
                        var outflow = offOffTrans[t, tau] + offOnTrans[t];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        // total inflow
                        Model.AddConstr(totalInFlow == 1 - Commit[t], "");
                    }
                }
            }
        }
        private void AddNetwork_Back()
        {
            beginOnTrans_back = new GRBVar[RDup];
            endOnTrans_back = new GRBVar[RDup];
            beginOffTrans_back = new GRBVar[Unit.minDownTime];
            endOffTrans_back = new GRBVar[Unit.minDownTime];
            onOnTrans_back = new GRBVar[totalTime - 1, RDup];
            onOffTrans_back = new GRBVar[totalTime - 1];
            offOffTrans_back = new GRBVar[totalTime - 1, Unit.minDownTime];
            offOnTrans__back = new GRBVar[totalTime - 1, RDextra];
            {
                //Init
                for (int tau = 0; tau < RDup; tau++)
                {
                    beginOnTrans_back[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    endOnTrans_back[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                }

                for (int tau = 0; tau < Unit.minDownTime; tau++)
                {
                    beginOffTrans_back[tau] = Model.AddVar(0, 1, 0.0, Type, "beginon" + tau);
                    endOffTrans_back[tau] = Model.AddVar(0, 1, 0.0, Type, "endoff" + tau);
                }
                for (int t = 0; t < totalTime - 1; t++)
                {
                    for (int tau = 0; tau < RDup; tau++)
                    {
                        onOnTrans_back[t, tau] = Model.AddVar(0, 1, 0.0, Type, "onon" + t + " " + tau);
                    }
                    for (int tau = 0; tau < Unit.minDownTime; tau++)
                    {
                        offOffTrans_back[t, tau] = Model.AddVar(0, 1, 0.0, Type, "offoff" + t + " " + tau);
                    }
                    onOffTrans_back[t] = Model.AddVar(0, 1, 0.0, Type, "onOff" + t);
                    for (int r = 0; r < RDextra; r++)
                    {
                        offOnTrans__back[t, r] = Model.AddVar(0, 1, 0.0, Type, "offon" + t + " " + r);
                    }
                }
            }
            {
                //beginTrans
                Model.AddConstr(Sum(beginOnTrans_back) + Sum(beginOffTrans_back) == 1, "");
                Model.AddConstr(Sum(endOnTrans_back) + Sum(endOffTrans_back) == 1, "");
            }


            {
                //first node On
                int t = 0;
                {
                    int tau = 0;
                    var inflow = beginOnTrans_back[tau];
                    var outflow = onOffTrans_back[t];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                }
                for (int tau = 1; tau < RDup - 1; tau++)
                {
                    var inflow = beginOnTrans_back[tau];
                    var outflow = onOnTrans_back[t, tau - 1];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                }

                {
                    var tau = RDup - 1;
                    var inflow = beginOnTrans_back[tau];
                    var outflow = onOnTrans_back[t, tau] + onOnTrans_back[t, tau - 1];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                }
                {
                    // total inflow
                    Model.AddConstr(Sum(beginOnTrans_back) == Commit[t], "");
                }
            }

            {
                //first node Off
                int t = 0;
                for (int tau = 0; tau < Unit.minDownTime - 1; tau++)
                {
                    var inflow = beginOffTrans_back[tau];
                    var outflow = offOffTrans_back[t, tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    // Model.AddConstr(Commit[t] == outflow, "");
                    //Model.AddConstr(Commit[t] == inflow, "");
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var inflow = beginOffTrans_back[tau];
                    var outflows = new GRBLinExpr();
                    for (int r = 0; r < RDextra; r++)
                    {
                        outflows += offOnTrans__back[t, r];
                    }
                    var outflow = outflows + offOffTrans_back[t, tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    // Model.AddConstr(Commit[t] == outflow, "");
                    // Model.AddConstr(Commit[t] == inflow, "");
                }
                {
                    // total inflow
                    Model.AddConstr(Sum(beginOffTrans_back) == 1 - Commit[t], "");

                }
            }
            {
                //last node On
                int t = totalTime - 1;
                var totalInFlow = new GRBLinExpr();
                {
                    var tau = 0;
                    var outflow = endOnTrans_back[tau];
                    var inflow = onOnTrans_back[t - 1, tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    var outflow = endOnTrans_back[tau];
                    var inflow = onOnTrans_back[t - 1, tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                for (int r = 0; r < RDextra; r++)
                {
                    var tau = Unit.minUpTime - 1 + r;

                    var inflow = onOnTrans_back[t - 1, tau] + offOnTrans__back[t - 1, r];
                    var outflow = endOnTrans_back[tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                {
                    // total inflow
                    Model.AddConstr(totalInFlow == Commit[t], "LastOn");
                }
            }

            {
                //last node Off
                int t = totalTime - 1;
                var totalInFlow = new GRBLinExpr();
                {
                    var tau = 0;
                    var outflow = endOffTrans_back[tau];
                    var inflow = new GRBLinExpr();
                    inflow += onOffTrans_back[t - 1];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                {
                    var inflow = endOffTrans_back[tau];
                    var outflow = offOffTrans_back[t - 1, tau - 1];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var outflow = endOffTrans_back[tau];
                    var inflow = offOffTrans_back[t - 1, tau - 1] + offOffTrans_back[t - 1, tau];
                    Model.AddConstr(inflow == outflow, t + " " + tau);
                    totalInFlow += inflow;
                }
                {
                    // total inflow
                    Model.AddConstr(totalInFlow == 1 - Commit[t], "");
                }
            }
            {
                // t<1 ontrans

                for (int t = 1; t < totalTime - 1; t++)
                {
                    var totalInFlow = new GRBLinExpr();
                    {
                        var tau = 0;
                        var inflow = onOnTrans_back[t - 1, tau];
                        var outflow = onOffTrans_back[t];
                        Model.AddConstr(inflow == outflow, t + "ON" + tau);
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                    {
                        var inflow = onOnTrans_back[t - 1, tau];
                        var outflow = onOnTrans_back[t, tau - 1];
                        Model.AddConstr(inflow == outflow, t + "ON" + tau);
                        totalInFlow += inflow;
                    }
                    for (int r = 0; r < RDextra - 1; r++)
                    {
                        var tau = Unit.minUpTime - 1 + r;
                        var inflow = onOnTrans_back[t - 1, tau] + offOnTrans__back[t - 1, r];
                        var outflow = onOnTrans_back[t, tau - 1];
                        Model.AddConstr(inflow == outflow, t + "ON" + tau);
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minUpTime - 1 + RDextra - 1;
                        var inflow = onOnTrans_back[t - 1, tau] + offOnTrans__back[t - 1, RDextra - 1];
                        var outflow = onOnTrans_back[t, tau] + onOnTrans_back[t, tau - 1];
                        Model.AddConstr(inflow == outflow, t + "ON" + tau);
                        totalInFlow += inflow;
                    }
                    {
                        // total inflow
                        Model.AddConstr(totalInFlow == Commit[t], "");
                    }
                }
            }
            {
                //t<1  offtrans
                for (int t = 1; t < totalTime - 1; t++)
                {
                    var totalInFlow = new GRBLinExpr();
                    {
                        var tau = 0;
                        var inflow = new GRBLinExpr();

                        //var ontau = Unit.minUpTime - 1 + r;
                        inflow += onOffTrans_back[t - 1];
                        var outflow = offOffTrans_back[t, tau];
                        Model.AddConstr(inflow == outflow, t + "OFF" + tau);
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                    {
                        var inflow = offOffTrans_back[t - 1, tau - 1];
                        var outflow = offOffTrans_back[t, tau];
                        Model.AddConstr(inflow == outflow, t + "OFF" + tau);
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minDownTime - 1;
                        var inflow = offOffTrans_back[t - 1, tau - 1] + offOffTrans_back[t - 1, tau];
                        GRBLinExpr outflow = offOffTrans_back[t, tau];
                        for (int r = 0; r < RDextra; r++)
                        {
                            outflow += offOnTrans__back[t, r];
                        }
                        Model.AddConstr(inflow == outflow, t + "OFF" + tau);
                        totalInFlow += inflow;
                    }
                    {
                        // total inflow
                        Model.AddConstr(totalInFlow == 1 - Commit[t], "");
                    }
                }
            }
        }
        private void AddLimits()
        {

            {
                var t = 0;
                for (int tau = 0; tau < Rup; tau++)
                {
                    Model.AddConstr(P_forward[t, tau] <= Unit.pMax * beginOnTrans[tau], "pMax" + t + " " + tau);
                    Model.AddConstr(P_forward[t, tau] >= Unit.pMin * beginOnTrans[tau], "pMin" + t + " " + tau);
                }
            }
            for (int t = 1; t < totalTime; t++)
            {
                {
                    var tau = 0;
                    var max = Unit.SU;
                    Model.AddConstr(P_forward[t, tau] <= max * offOnTrans[t - 1], "pMax" + t + " " + tau);
                    Model.AddConstr(P_forward[t, tau] >= Unit.pMin * offOnTrans[t - 1], "pMin" + t + " " + tau);
                }
                for (int tau = 1; tau < Rup - 1; tau++)
                {
                    var limit = Math.Min(Unit.SU + Unit.RU * tau, Unit.pMax);
                    //  int tau = Unit.minUpTime - 1 + r;
                    Model.AddConstr(P_forward[t, tau] <= limit * onOnTrans[t - 1, tau - 1], "pMax" + t + " " + tau);
                    Model.AddConstr(P_forward[t, tau] >= Unit.pMin * onOnTrans[t - 1, tau - 1], "pMin" + t + " " + tau);
                }
                {
                    int tau = Rup - 1;
                    var limit = Math.Min(Unit.SU + Unit.RU * tau, Unit.pMax);
                    Model.AddConstr(P_forward[t, tau] <= (limit * (onOnTrans[t - 1, tau - 1]) + (Unit.pMax * onOnTrans[t - 1, tau])), "pMax" + t + " " + tau);
                    Model.AddConstr(P_forward[t, tau] >= Unit.pMin * (onOnTrans[t - 1, tau - 1] + onOnTrans[t - 1, tau]), "pMin" + t + " " + tau);
                }

                //for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                //{
                //    var max = Math.Min(Unit.SU + Unit.RU * tau, Unit.pMax);
                //    Model.AddConstr(P[t, tau] <= max * onOnTrans[t - 1, tau - 1], "");
                //    Model.AddConstr(P[t, tau] >= Unit.pMin * onOnTrans[t - 1, tau - 1], "");
                //}
                //{
                //    var tau = Unit.minUpTime - 1;
                //    Model.AddConstr(P[t, tau] <= Unit.pMax * (onOnTrans[t - 1, tau - 1] + onOnTrans[t - 1, tau]), "");
                //    Model.AddConstr(P[t, tau] >= Unit.pMin * (onOnTrans[t - 1, tau - 1] + onOnTrans[t - 1, tau]), "");
                //}
            }
        }
        private void AddLimits_Back()
        {
            {
                var t = 0;
                {
                    int tau = 0;
                    Model.AddConstr(P_Back[t, tau] <= Unit.SD * beginOnTrans[tau], "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * beginOnTrans[tau], "pMin" + t + " " + tau);
                }
                for (int tau = 1; tau < RDup; tau++)
                {
                    Model.AddConstr(P_Back[t, tau] <= Unit.pMax * beginOnTrans_back[tau], "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * beginOnTrans_back[tau], "pMin" + t + " " + tau);
                }
            }
            for (int t = 1; t < totalTime; t++)
            {
                {
                    var tau = 0;
                    var max = Unit.SD;
                    Model.AddConstr(P_Back[t, tau] <= max * onOnTrans_back[t - 1, tau], "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * onOnTrans_back[t - 1, tau], "pMin" + t + " " + tau);
                }
                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    var limit = Math.Min(Unit.pMax, Unit.SU + tau * Unit.RD);
                    Model.AddConstr(P_Back[t, tau] <= limit * onOnTrans_back[t - 1, tau], "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * onOnTrans_back[t - 1, tau], "pMin" + t + " " + tau);
                }
                for (int r = 0; r < RDextra - 1; r++)
                {
                    int tau = Unit.minUpTime - 1 + r;
                    var limit = Math.Min(Unit.pMax, Unit.SU + tau * Unit.RD);
                    Model.AddConstr(P_Back[t, tau] <= limit * (onOnTrans_back[t - 1, tau] + offOnTrans__back[t - 1, r]), "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * onOnTrans_back[t - 1, tau], "pMin" + t + " " + tau);
                }
                if (t < totalTime - 1)
                {
                    int tau = RDup - 1;
                    var limit = Math.Min(Unit.pMax, Unit.SU + tau * Unit.RD);
                    Model.AddConstr(P_Back[t, tau] <= (limit * (onOnTrans_back[t, tau - 1]) + (Unit.pMax * onOnTrans_back[t, tau])), "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * (onOnTrans_back[t, tau - 1] + onOnTrans_back[t, tau]), "pMin" + t + " " + tau);
                }
                else
                {
                    int tau = Unit.minUpTime - 1 + RDextra - 1;
                    var limit = Math.Min(Unit.pMax, Unit.SU + tau * Unit.RD);
                    Model.AddConstr(P_Back[t, tau] <= Unit.SU * offOnTrans__back[t - 1, RDextra - 1] + (Unit.pMax * onOnTrans_back[t - 1, tau]), "pMax" + t + " " + tau);
                    Model.AddConstr(P_Back[t, tau] >= Unit.pMin * (offOnTrans__back[t - 1, RDextra - 1] + onOnTrans_back[t - 1, tau]), "pMin" + t + " " + tau);
                }
            }
        }
        private void AddRampingLimits()
        {
            //Model.AddConstr(Commit[0] == 0, "bla");
            //Model.AddConstr(Commit[1] == 0, "bla");
            //Model.AddConstr(Commit[2] == 0, "bla");
            //Model.AddConstr(Commit[3] == 0, "bla");
            //for (int t = 4; t < totalTime - 1; t++)
            //{
            //    Model.AddConstr(Commit[t] == 1, "bla");
            //}

            //for (int t = 0; t < totalTime; t++)
            //{
            //    if (t > 0)
            //    {
            //        GRBLinExpr downwardRampingLimitNormal = Unit.RD * Commit[t - 1];
            //        GRBLinExpr downwardRampingLimitShutdown = Stop[t] * (Unit.SD - Unit.RD);
            //        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
            //        GRBLinExpr sumt_1 = new GRBLinExpr();
            //        GRBLinExpr sumt = new GRBLinExpr();
            //        for (int tau = 0; tau < Rup; tau++)
            //        {
            //            sumt_1 += P[t - 1, tau];
            //            sumt += P[t, tau];
            //        }
            //        Model.AddConstr(sumt_1 - sumt <= downwardRampingLimit, "rampup" + t);


            //        GRBLinExpr upwardRampingLimitNormal = Unit.RU * Commit[t];
            //        GRBLinExpr upwardRampingLimitStartup = Start[t] * (Unit.SU - Unit.RU);
            //        GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
            //        Model.AddConstr(sumt - sumt_1 <= upwardRampingLimit, "rampdown" + t);
            //    }
            //}

            for (int t = 0; t < totalTime - 1; t++)
            {



                for (int tau = 0; tau < Unit.minUpTime - 1; tau++)
                {
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans[t, tau];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal;
                    Model.AddConstr(P_forward[t, tau] - P_forward[t + 1, tau + 1] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                }
                for (int r = 0; r < Rextra - 1; r++)
                {
                    int tau = Unit.minUpTime - 1 + r;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans[t, tau];
                    GRBLinExpr downwardRampingLimitShutdown = onOffTrans[t, r] * (Unit.SD);
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
                    Model.AddConstr(P_forward[t, tau] - P_forward[t + 1, tau + 1] <= downwardRampingLimit, "ramdownRE" + t + " " + tau);
                }
                {
                    int tau = Unit.minUpTime - 1 + Rextra - 1;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans[t, tau];
                    GRBLinExpr downwardRampingLimitShutdown = onOffTrans[t, Rextra - 1] * (Unit.SD);
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
                    Model.AddConstr(P_forward[t, tau] - P_forward[t + 1, tau] <= downwardRampingLimit, "ramdownRE" + t + " " + tau);
                }



                for (int tau = 0; tau < Rup - 2; tau++)
                {
                    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onOnTrans[t, tau];
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal;
                    Model.AddConstr(P_forward[t + 1, tau + 1] - P_forward[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }
                {

                    var tau = Rup - 2;
                    var limit = Unit.pMax;

                    GRBLinExpr upwardRampingLimitNormal = -(limit - Unit.RU) * onOnTrans[t, tau];
                    GRBLinExpr upwardRampingLimitStartup = limit;
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                    Model.AddConstr(P_forward[t + 1, tau + 1] - P_forward[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }

                {

                    var tau = Rup - 1;
                    // var limit = Unit.pMax;
                    var limit = Math.Min(Unit.pMax, Unit.SU + Unit.RU * (tau));
                    GRBLinExpr upwardRampingLimitNormal = -(limit - Unit.RU) * onOnTrans[t, tau];
                    GRBLinExpr upwardRampingLimitStartup = limit;
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                    Model.AddConstr(P_forward[t + 1, tau] - P_forward[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }

            }
        }
        private void AddRampingLimits_Back()
        {

            //for (int t = 0; t < totalTime; t++)
            //{
            //    if (t > 0)
            //    {
            //        GRBLinExpr downwardRampingLimitNormal = Unit.RD * Commit[t - 1];
            //        GRBLinExpr downwardRampingLimitShutdown = Stop[t] * (Unit.SD - Unit.RD);
            //        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
            //        GRBLinExpr sumt_1 = new GRBLinExpr();
            //        GRBLinExpr sumt = new GRBLinExpr();
            //        for (int tau = 0; tau < RDup; tau++)
            //        {
            //            sumt_1 += P_Back[t - 1, tau];
            //            sumt += P_Back[t, tau];
            //        }
            //        Model.AddConstr(sumt_1 - sumt <= downwardRampingLimit, "rampup" + t);


            //        GRBLinExpr upwardRampingLimitNormal = Unit.RU * Commit[t];
            //        GRBLinExpr upwardRampingLimitStartup = Start[t] * (Unit.SU - Unit.RU);
            //        GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup - Unit.RU * Stop[t];
            //        Model.AddConstr(sumt - sumt_1 <= upwardRampingLimit, "rampdown" + t);
            //    }
            //}

            //realcde:
            for (int t = 0; t < totalTime; t++)
            {
                if (t > 0)
                {
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * Commit[t - 1];
                    GRBLinExpr downwardRampingLimitShutdown = Stop[t] * (Unit.SD - Unit.RD);
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
                    GRBLinExpr sumt_1 = new GRBLinExpr();
                    GRBLinExpr sumt = new GRBLinExpr();
                    for (int tau = 0; tau < RDup; tau++)
                    {
                        sumt_1 += P_Back[t - 1, tau];
                        sumt += P_Back[t, tau];
                    }
                    Model.AddConstr(sumt_1 - sumt <= downwardRampingLimit, "rampup" + t);


                    GRBLinExpr upwardRampingLimitNormal = Unit.RU * Commit[t];
                    GRBLinExpr upwardRampingLimitStartup = Start[t] * (Unit.SU - Unit.RU);
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                    Model.AddConstr(sumt - sumt_1 <= upwardRampingLimit, "rampdown" + t);
                }
            }

            //realcde:
            for (int t = 0; t < totalTime - 1; t++)
            {


                for (int tau = 1; tau < RDup - 1; tau++)
                {
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans_back[t, tau - 1];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal;
                    Model.AddConstr(P_Back[t, tau] - P_Back[t + 1, tau - 1] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                }
                {
                    int tau = RDup - 1;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans_back[t, tau - 1];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + Unit.pMax * onOnTrans_back[t, tau];
                    Model.AddConstr(P_Back[t, tau] - P_Back[t + 1, tau - 1] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                }
                {

                    int tau = RDup - 1;
                    var limit = Math.Min(Unit.pMax, Unit.SD + Unit.RD * tau);
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans_back[t, tau];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + limit * onOnTrans_back[t, tau - 1];
                    Model.AddConstr(P_Back[t, tau] - P_Back[t + 1, tau] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                }

                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    GRBLinExpr downwardRampingLimitNormal = Unit.RU * onOnTrans_back[t, tau - 1];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal;
                    Model.AddConstr(P_Back[t + 1, tau - 1] - P_Back[t, tau] <= downwardRampingLimit, "rampup" + t + " " + tau);
                }

                for (int r = 0; r < RDextra - 1; r++)
                {
                    int tau = Unit.minUpTime - 1 + r;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RU * onOnTrans_back[t, tau - 1];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + Unit.SU * offOnTrans__back[t, r];
                    Model.AddConstr(P_Back[t + 1, tau - 1] - P_Back[t, tau] <= downwardRampingLimit, "rampup" + t + " " + tau);
                }
                {
                    int tau = RDup - 1;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RU * onOnTrans_back[t, tau - 1];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal;
                    Model.AddConstr(P_Back[t + 1, tau - 1] - P_Back[t, tau] <= downwardRampingLimit, "rampup" + t + " " + tau);
                }
                {

                    int tau = RDup - 1;
                    GRBLinExpr downwardRampingLimitNormal = Unit.RU * onOnTrans_back[t, tau];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + Unit.SU * offOnTrans__back[t, RDextra - 1];
                    Model.AddConstr(P_Back[t + 1, tau] - P_Back[t, tau] <= downwardRampingLimit, "rampup" + t + " " + tau);
                }
                //for (int r = 0; r < RDextra - 1; r++)
                //{
                //    int tau = Unit.minUpTime - 1 + r;
                //    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans[t, tau];
                //    GRBLinExpr downwardRampingLimitShutdown = onOffTrans[t, r] * (Unit.SD);
                //    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
                //    Model.AddConstr(P[t, tau] - P[t + 1, tau + 1] <= downwardRampingLimit, "ramdownRE" + t + " " + tau);
                //}
                //{
                //    int tau = Unit.minUpTime - 1 + RDextra - 1;
                //    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onOnTrans[t, tau];
                //    GRBLinExpr downwardRampingLimitShutdown = onOffTrans[t, RDextra - 1] * (Unit.SD);
                //    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
                //    Model.AddConstr(P[t, tau] - P[t + 1, tau] <= downwardRampingLimit, "ramdownRE" + t + " " + tau);
                //}



                //for (int tau = 0; tau < RDup - 2; tau++)
                //{
                //    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onOnTrans[t, tau];
                //    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal;
                //    Model.AddConstr(P[t + 1, tau + 1] - P[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                //}
                //{

                //    var tau = RDup - 2;
                //    var limit = Unit.pMax;

                //    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onOnTrans[t, tau];
                //    GRBLinExpr upwardRampingLimitStartup = onOnTrans[t, tau + 1] * limit;
                //    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                //    Model.AddConstr(P[t + 1, tau + 1] - P[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                //}

                //{

                //    var tau = RDup - 1;
                //    // var limit = Unit.pMax;
                //    var limit = Math.Min(Unit.pMax, Unit.SU + Unit.RU * (tau));
                //    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onOnTrans[t, tau];
                //    GRBLinExpr upwardRampingLimitStartup = onOnTrans[t, tau - 1] * limit;
                //    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                //    Model.AddConstr(P[t + 1, tau] - P[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                //}

            }
        }
        public void Print()
        {
            for (int tau = 0; tau < Unit.minDownTime; tau++)
            {
                string line = "";
                line += Math.Round(beginOffTrans[tau].X, 2) + "\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(offOffTrans[t, tau].X, 2) + "\t";
                }
                line += Math.Round(endOffTrans[tau].X, 2) + "\t";
                Console.WriteLine(line);
            }
            {
                string line = "x\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(offOnTrans[t].X, 2) + "\t";
                }
                Console.WriteLine(line + "x");
            }
            for (int tau = 0; tau < Rup; tau++)
            {
                string line = "";
                line += Math.Round(beginOnTrans[tau].X, 2) + "\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(onOnTrans[t, tau].X, 2) + "\t";
                }
                line += Math.Round(endOnTrans[tau].X, 2) + "\t";
                Console.WriteLine(line);
            }
            for (int r = 0; r < Rextra; r++)
            {
                string line = "x\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(onOffTrans[t, r].X, 2) + "\t";
                }
                Console.WriteLine(line + "x");
            }

            Console.WriteLine(" ");
            Console.WriteLine(" ");
            for (int tau = 0; tau < Unit.minDownTime; tau++)
            {
                string line = "";
                line += Math.Round(beginOffTrans_back[tau].X, 2) + "\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(offOffTrans_back[t, tau].X, 2) + "\t";
                }
                line += Math.Round(endOffTrans_back[tau].X, 2) + "\t";
                Console.WriteLine(line);
            }
            for (int r = 0; r < RDextra; r++)

            {
                string line = "x\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(offOnTrans__back[t, r].X, 2) + "\t";
                }
                Console.WriteLine(line + "x");
            }
            for (int tau = 0; tau < RDup; tau++)
            {
                string line = "";
                line += Math.Round(beginOnTrans_back[tau].X, 2) + "\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(onOnTrans_back[t, tau].X, 2) + "\t";
                }
                line += Math.Round(endOnTrans_back[tau].X, 2) + "\t";
                Console.WriteLine(line);
            }
            {
                string line = "x\t";
                for (int t = 0; t <totalTime- 1; t++)
                {
                    line += Math.Round(onOffTrans_back[t].X, 2) + "\t";
                }
                Console.WriteLine(line + "x");
            }

            Console.WriteLine(string.Join(" ", Commit.Select(com => Math.Round(com.X, 4))));
            Console.WriteLine(string.Join(" ", Start.Select(com => Math.Round(com.X, 4))));
            Console.WriteLine(string.Join(" ", Stop.Select(com => Math.Round(com.X, 4))));

            {
                string line = "\t";
                for (int t = 0; t < totalTime; t++)
                {
                    line += t + "\t";
                }
                Console.WriteLine(line);
            }
            for (int tau = 0; tau < Rup; tau++)
            {
                string line = tau + "\t";
                for (int t = 0; t < totalTime; t++)
                {
                    line += Math.Round(P_forward[t, tau].X, 4) + "\t";
                }
                Console.WriteLine(line);
            }
            Console.WriteLine(" ");
            Console.WriteLine(string.Join("\t", Commit.Select(com => Math.Round(com.X, 4))));
            Console.WriteLine(" ");
            {
                string line = "\t";
                for (int t = 0; t < totalTime; t++)
                {
                    line += t + "\t";
                }
                Console.WriteLine(line);
            }
            for (int tau = 0; tau < RDup; tau++)

            {
                string line = tau + "\t";
                for (int t = 0; t < totalTime; t++)
                {
                    line += Math.Round(P_Back[t, tau].X, 4) + "\t";
                }
                Console.WriteLine(line);
            }
            Console.WriteLine("[{0},{1}]  RU {2} RD {3}    SU{4} SD{5}", Unit.pMin, Unit.pMax, Unit.RU, Unit.RD, Unit.SU, Unit.SD);
            // Console.WriteLine(string.Join(" ", P.Select(com => com.X)));
        }
    }
}
