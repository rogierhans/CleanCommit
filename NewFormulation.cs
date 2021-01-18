using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.Instance;
namespace CleanCommit
{
    public class NetworkFormulation
    {
        Unit Unit;
        GRBModel Model;
        // List<double> Commitsolution = new List<double>();
        int totalTime;
        public NetworkFormulation(GRBModel model,Unit unit, int timesteps, char type, GRBVar[] p, GRBVar[] commit, GRBVar[] start, GRBVar[]  stop)
        {
            this.Type = type;
            P = p;
            Commit = commit;
            Start = start;
            Stop = stop;
            Unit = unit;
            totalTime = timesteps;
            Model = model;
            AddVariables();
            AddNetwork();
            AddLimits();
            AddRampingLimits();
            AddLogic();
            //Print();


        }

        public GRBVar[,] PCopy;
        public GRBVar[] P;
        public GRBVar[] Commit;
        public GRBVar[] Start;
        public GRBVar[] Stop;
        private readonly char Type;
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
        private void AddVariables()
        {
            PCopy = new GRBVar[totalTime, Unit.minUpTime];
            for (int t = 0; t < totalTime; t++)
            {
                // P[t] = Model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, "");
                var sum = new GRBLinExpr();
                for (int tau = 0; tau < Unit.minUpTime; tau++)
                {
                    PCopy[t, tau] = Model.AddVar(0.0, double.MaxValue, 0.0, GRB.CONTINUOUS, "Copy");
                    sum += PCopy[t, tau];
                }
                Model.AddConstr(sum == P[t] + Unit.pMin * Commit[t], "PCopy match to sum");
            }

        }

        private void AddLimits()
        {

            {
                var t = 0;
                for (int tau = 0; tau < Unit.minUpTime; tau++)
                {
                    Model.AddConstr(PCopy[t, tau] <= Unit.pMax * beginOnTrans[tau], "");
                    Model.AddConstr(PCopy[t, tau] >= Unit.pMin * beginOnTrans[tau], "");
                }
            }
            for (int t = 1; t < totalTime; t++)
            {
                {
                    var tau = 0;
                    var max = Math.Min(Unit.SU, Unit.pMax);
                    Model.AddConstr(PCopy[t, tau] <= max * offTrans[t - 1, Unit.minDownTime], "");
                    Model.AddConstr(PCopy[t, tau] >= Unit.pMin * offTrans[t - 1, Unit.minDownTime], "");
                }
                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    var max = Math.Min(Unit.SU + Unit.RU * tau, Unit.pMax);
                    Model.AddConstr(PCopy[t, tau] <= max * onTrans[t - 1, tau - 1], "");
                    Model.AddConstr(PCopy[t, tau] >= Unit.pMin * onTrans[t - 1, tau - 1], "");
                }
                {
                    var tau = Unit.minUpTime - 1;
                    Model.AddConstr(PCopy[t, tau] <= Unit.pMax * (onTrans[t - 1, tau - 1] + onTrans[t - 1, tau]), "");
                    Model.AddConstr(PCopy[t, tau] >= Unit.pMin * (onTrans[t - 1, tau - 1] + onTrans[t - 1, tau]), "");
                }
            }
        }
        private void AddRampingLimits()
        {
            //for (int t = 0; t < totalTime; t++)
            //{
            //    if (t > 0)
            //    {
            //        GRBLinExpr downwardRampingLimitNormal = Unit.RampDown * Commit[t - 1];
            //        GRBLinExpr downwardRampingLimitShutdown = Stop[t] * (Unit.SD - Unit.RampDown);
            //        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
            //        GRBLinExpr sumt_1 = new GRBLinExpr();
            //        GRBLinExpr sumt = new GRBLinExpr();
            //        for (int tau = 0; tau < Unit.minUpTime; tau++)
            //        {
            //            sumt_1 += P[t - 1, tau];
            //            sumt += P[t, tau];
            //        }
            //        Model.AddConstr(sumt_1 - sumt <= downwardRampingLimit, "t" + t);


            //        GRBLinExpr upwardRampingLimitNormal = Unit.RampUp * Commit[t];
            //        GRBLinExpr upwardRampingLimitStartup = Start[t] * (Unit.SU - Unit.RampUp);
            //        GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
            //        Model.AddConstr(sumt - sumt_1 <= upwardRampingLimit, "t" + t);
            //    }
            //}

            for (int t = 0; t < totalTime - 1; t++)
            {



                for (int tau = 0; tau < Unit.minUpTime - 2; tau++)
                {
                    GRBLinExpr downwardRampingLimitNormal = Unit.RD * onTrans[t, tau];
                    GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal;
                    Model.AddConstr(PCopy[t, tau] - PCopy[t + 1, tau + 1] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                }
                {
                    {
                        var tau = Unit.minUpTime - 2;
                        GRBLinExpr downwardRampingLimitNormal = Unit.RD * onTrans[t, tau];
                        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal - Unit.pMin * onTrans[t, tau + 1];
                        Model.AddConstr(PCopy[t, tau] - PCopy[t + 1, tau + 1] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                    }
                    {
                        var tau = Unit.minUpTime - 1;
                        GRBLinExpr downwardRampingLimitNormal = Unit.RD * onTrans[t, tau];
                        GRBLinExpr downwardRampingLimitShutdown = onTrans[t, tau + 1] * (Unit.SD);
                        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown - Unit.pMin * onTrans[t, tau - 1]; ;
                        Model.AddConstr(PCopy[t, tau] - PCopy[t + 1, tau] <= downwardRampingLimit, "ramdown" + t + " " + tau);
                    }
                }




                for (int tau = 0; tau < Unit.minUpTime - 2; tau++)
                {
                    var limit = Math.Min(Unit.pMax, Unit.SU + Unit.RU * (tau + 1));
                    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onTrans[t, tau];
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal;
                    Model.AddConstr(PCopy[t + 1, tau + 1] - PCopy[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }
                {
                    //var Pminmin = P[t , Unit.minUpTime - 2];
                    //var Pmin = P[t, Unit.minUpTime - 1];
                    //var Pfull = P[t+1, Unit.minUpTime - 1];

                    var tau = Unit.minUpTime - 2;
                    var limit = Unit.pMax;

                    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onTrans[t, tau];
                    GRBLinExpr upwardRampingLimitStartup = onTrans[t, tau + 1] * limit;
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                    Model.AddConstr(PCopy[t + 1, tau + 1] - PCopy[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }

                {
                    var tau = Unit.minUpTime - 1;
                    var limit = Math.Min(Unit.pMax, Unit.SU + Unit.RU * (tau));
                    GRBLinExpr upwardRampingLimitNormal = Unit.RU * onTrans[t, tau];
                    GRBLinExpr upwardRampingLimitStartup = onTrans[t, tau - 1] * limit;
                    GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                    Model.AddConstr(PCopy[t + 1, tau] - PCopy[t, tau] <= upwardRampingLimit, "rampup" + t + " " + tau);
                }


                //Model.AddConstr(sumt_1 - sumt <= downwardRampingLimit, "t" + t);


                //GRBLinExpr upwardRampingLimitNormal = Unit.RampUp * Commit[t];
                //GRBLinExpr upwardRampingLimitStartup = Start[t] * (Unit.SU - Unit.RampUp);
                //GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
                //Model.AddConstr(sumt - sumt_1 <= upwardRampingLimit, "t" + t);

            }

        }

        private GRBVar[,] onStates;
        private GRBVar[,] offStates;
        private GRBVar[] beginOnTrans;
        private GRBVar[] beginOffTrans;
        private GRBVar[] endOnTrans;
        private GRBVar[] endOffTrans;
        private GRBVar[,] onTrans;
        private GRBVar[,] offTrans;
        private void AddNetwork()
        {
            onStates = new GRBVar[totalTime, Unit.minUpTime];
            offStates = new GRBVar[totalTime, Unit.minDownTime];
            beginOnTrans = new GRBVar[Unit.minUpTime];
            beginOffTrans = new GRBVar[Unit.minDownTime];
            endOnTrans = new GRBVar[Unit.minUpTime];
            endOffTrans = new GRBVar[Unit.minDownTime];
            onTrans = new GRBVar[totalTime - 1, Unit.minUpTime + 1];
            offTrans = new GRBVar[totalTime - 1, Unit.minDownTime + 1];
            {
                //Init
                for (int tau = 0; tau < Unit.minUpTime; tau++)
                {
                    beginOnTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    endOnTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                }

                for (int tau = 0; tau < Unit.minDownTime; tau++)
                {
                    beginOffTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    endOffTrans[tau] = Model.AddVar(0, 1, 0.0, Type, "");
                }
                for (int t = 0; t < totalTime - 1; t++)
                {
                    for (int tau = 0; tau < Unit.minUpTime + 1; tau++)
                    {
                        onTrans[t, tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    }
                    for (int tau = 0; tau < Unit.minDownTime + 1; tau++)
                    {
                        offTrans[t, tau] = Model.AddVar(0, 1, 0.0, Type, "");
                    }
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
                    var outflow = onTrans[t, tau];
                    Model.AddConstr(inflow == outflow, "");
                }

                {
                    var tau = Unit.minUpTime - 1;
                    var inflow = beginOnTrans[tau];
                    var outflow = onTrans[t, tau] + onTrans[t, tau + 1];
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
                    var outflow = offTrans[t, tau];
                    Model.AddConstr(inflow == outflow, "");
                    // Model.AddConstr(Commit[t] == outflow, "");
                    //Model.AddConstr(Commit[t] == inflow, "");
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var inflow = beginOffTrans[tau];
                    var outflow = offTrans[t, tau] + offTrans[t, tau + 1];
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
                    var inflow = offTrans[t - 1, Unit.minDownTime];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                {
                    var outflow = endOnTrans[tau];
                    var inflow = onTrans[t - 1, tau - 1];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    var tau = Unit.minUpTime - 1;
                    var outflow = endOnTrans[tau];
                    var inflow = onTrans[t - 1, tau - 1] + onTrans[t - 1, tau];
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
                    var inflow = onTrans[t - 1, Unit.minUpTime];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                {
                    var inflow = endOffTrans[tau];
                    var outflow = offTrans[t - 1, tau - 1];
                    Model.AddConstr(inflow == outflow, "");
                    totalInFlow += inflow;
                }
                {
                    var tau = Unit.minDownTime - 1;
                    var outflow = endOffTrans[tau];
                    var inflow = offTrans[t - 1, tau - 1] + offTrans[t - 1, tau];
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
                        var inflow = offTrans[t - 1, Unit.minDownTime];
                        var outflow = onTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minUpTime - 1; tau++)
                    {
                        var inflow = onTrans[t - 1, tau - 1];
                        var outflow = onTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minUpTime - 1;
                        var inflow = onTrans[t - 1, tau] + onTrans[t - 1, tau - 1];
                        var outflow = onTrans[t, tau] + onTrans[t, tau + 1];
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
                        var inflow = onTrans[t - 1, Unit.minUpTime];
                        var outflow = offTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    for (int tau = 1; tau < Unit.minDownTime - 1; tau++)
                    {
                        var inflow = offTrans[t - 1, tau - 1];
                        var outflow = offTrans[t, tau];
                        Model.AddConstr(inflow == outflow, "");
                        totalInFlow += inflow;
                    }
                    {
                        var tau = Unit.minDownTime - 1;
                        var inflow = offTrans[t - 1, tau - 1] + offTrans[t - 1, tau];
                        var outflow = offTrans[t, tau] + offTrans[t, tau + 1];
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

        private void Print()
        {
            for (int tau = 0; tau < Unit.minDownTime + 1; tau++)
            {
                string line = "";
                if (tau < Unit.minDownTime)
                {
                    line += Math.Round(beginOffTrans[tau].X, 2) + "\t";
                }
                else
                {
                    line += "x" + "\t";
                }
                for (int t = 0; t < totalTime - 1; t++)
                {
                    line += Math.Round(offTrans[t, tau].X, 2) + "\t";
                }
                if (tau < Unit.minDownTime)
                {
                    line += Math.Round(endOffTrans[tau].X, 2) + "\t";
                }
                else
                {
                    line += "x" + "\t";
                }
                Console.WriteLine(line);
            }
            for (int tau = 0; tau < Unit.minUpTime + 1; tau++)
            {
                string line = "";
                if (tau < Unit.minUpTime)
                {
                    line += Math.Round(beginOnTrans[tau].X, 2) + "\t";
                }
                else
                {
                    line += "x" + "\t";
                }
                for (int t = 0; t < totalTime - 1; t++)
                {
                    line += Math.Round(onTrans[t, tau].X, 2) + "\t";
                }
                if (tau < Unit.minUpTime)
                {
                    line += Math.Round(endOnTrans[tau].X, 2) + "\t";
                }
                else
                {
                    line += "x" + "\t";
                }
                Console.WriteLine(line);
            }

            Console.WriteLine(string.Join(" ", Commit.Select(com => Math.Round(com.X, 4))));
            Console.WriteLine(string.Join(" ", Start.Select(com => Math.Round(com.X, 4))));
            Console.WriteLine(string.Join(" ", Stop.Select(com => Math.Round(com.X, 4))));

            for (int tau = 0; tau < Unit.minUpTime; tau++)

            {
                string line = "";
                for (int t = 0; t < totalTime; t++)
                {
                    line += Math.Round(PCopy[t, tau].X, 4) + "\t";
                }
                Console.WriteLine(line);
            }
            // Console.WriteLine(string.Join(" ", P.Select(com => com.X)));
        }

    }
}



