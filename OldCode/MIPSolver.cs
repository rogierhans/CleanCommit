//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Gurobi;

//namespace UtrechtCommitment
//{
//    class MIPSolver : Solver
//    {

//        bool MinUpDownTime;
//        bool TimeDependantStartUpCost;
//        Char Type;
//        public MIPSolver(PowerSystem PS)
//            : base(PS)
//        {
//            Type = PS.ConstraintConfiguration.Relax ? GRB.CONTINUOUS : GRB.BINARY;
//            MinUpDownTime = PS.ConstraintConfiguration.MinUpMinDown;
//            TimeDependantStartUpCost = PS.ConstraintConfiguration.TimeDependantStartUpCost;
//        }


//        public GRBVar[,] UnitCommitmentVars;
//        public GRBVar[,] UnitCommitmentStartVars;
//        public GRBVar[,] UnitCommitmentStopVars;


//        public override void AddVariables()
//        {
//            base.AddVariables();
//            AddBinaryVariables();
//            if (TimeDependantStartUpCost)
//            { AddTimeDependantStartUpCostVariables(); }
//        }

//        public void AddBinaryVariables()
//        {
//            UnitCommitmentVars = new GRBVar[totalTime, PS.Units.Count];
//            UnitCommitmentStartVars = new GRBVar[totalTime, PS.Units.Count];
//            UnitCommitmentStopVars = new GRBVar[totalTime, PS.Units.Count];
//            ApplyFunction((t, u) =>
//            {
//                Unit unit = PS.Units[u];
//                UnitCommitmentVars[t, u] = model.AddVar(0.0, 1.0, 0.0, Type, "U" + u + "," + t);
//                UnitCommitmentStartVars[t, u] = model.AddVar(0.0, 1.0, 0.0, Type, "V" + u + "," + t);
//                UnitCommitmentStopVars[t, u] = model.AddVar(0.0, 1.0, 0.0, Type, "W" + u + "," + t);
//            });
//        }

//        protected List<GRBVar>[,] StartCostIntervall;
//        public void AddTimeDependantStartUpCostVariables()
//        {
//            StartCostIntervall = new List<GRBVar>[totalTime, totalUnits];
//            ApplyFunction((t, u) =>
//            {
//                Unit unit = PS.Units[u];
//                StartCostIntervall[t, u] = new List<GRBVar>();
//                for (int e = 0; e < unit.StartInterval.Length; e++)
//                {
//                    StartCostIntervall[t, u].Add(model.AddVar(0.0, 1.0, 0.0, Type, "u" + u + "t" + t + "e" + e));
//                }
//            });
//        }

//        public override void AddObjective()
//        {
//            AddBaseGeneration();
//            if (TimeDependantStartUpCost) { AddTimeDependantStartCost(); }
//            else { AddStartUpCosts(); }
//            base.AddObjective();
//        }
//        public void AddBaseGeneration()
//        {
//            ApplyFunction((t, u) =>
//            {
//                var unit = PS.Units[u];
//                GRBLinExpr baseGeneration = UnitCommitmentVars[t, u] * unit.A;
//                Objective += baseGeneration;
//            });
//        }
//        public void AddStartUpCosts()
//        {
//            ApplyFunction((t, u) =>
//            {
//                Unit unit = PS.Units[u];
//                Objective += unit.StartCostInterval.Last() * UnitCommitmentStartVars[t, u];
//            });
//        }

//        public void AddTimeDependantStartCost()
//        {
//            ApplyFunction((t, u) =>
//            {
//                Unit unit = PS.Units[u];
//                for (int e = 0; e < unit.StartInterval.Length; e++)
//                {
//                    Objective += StartCostIntervall[t, u][e] * unit.StartCostInterval[e];
//                }
//            });
//        }

//        public void OneStartUpTypePerStartup()
//        {
//            ApplyFunction((t, u) =>
//            {
//                var sum = new GRBLinExpr();
//                for (int e = 0; e < StartCostIntervall[0, u].Count(); e++)
//                {
//                    sum += StartCostIntervall[t, u][e];
//                }
//                model.AddConstr(UnitCommitmentStartVars[t, u] == sum, "StartCostContraint4.25" + t + "u:" + u);
//            });
//        }

//        public void RelateStartupTypeWithShutdown()
//        {
//            ApplyFunction((t, u) =>
//            {
//                var unit = PS.Units[u];
//                for (int e = 0; e < unit.StartInterval.Length - 1; e++)
//                {
//                    //var sum = new GRBLinExpr();
//                    //int from = unit.StartInterval[e];
//                    //int to = unit.StartInterval[e + 1];
//                    //if (t < to) return;
//                    //for (int i = from; i < to; i++)
//                    //{
//                    //    int t2 = t - i;
//                    //    sum += UnitCommitmentStopVars[t2, u];
//                    //}
//                    //model.AddConstr(StartCostIntervall[t, u][e] <= sum, "StartCostContraint4.26" + t + "u:" + u + "e" + e);
//                    var sum = new GRBLinExpr();
//                    int from =  t - unit.StartInterval[e + 1];
//                    int to = t - unit.StartInterval[e];
//                    if (from<0) return;
//                    for (int t2 = from; t2 < to; t2++)
//                    {
//                        sum += UnitCommitmentStopVars[t2, u];
//                    }
//                    model.AddConstr(StartCostIntervall[t, u][e] <= sum, "StartCostContraint4.26" + t + "u:" + u + "e" + e);
//                }
//            });
//        }


//        public override void AddConstraints()
//        {
//            base.AddConstraints();

//            if (MinUpDownTime)
//            {
//                AddMinimumDownTime();
//                AddMinimumUpTime();
//            }
//            if (TimeDependantStartUpCost)
//            {
//                OneStartUpTypePerStartup();
//                RelateStartupTypeWithShutdown();
//            }
//            PowerPlantLogicConstraint();
//        }


//        protected override void AddGenerationConstraints()
//        {
//            GenerationConstrMin = new GRBConstr[totalTime, totalUnits];
//            GenerationConstrMax = new GRBConstr[totalTime, totalUnits];
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];
//                    var dispatch = DispatchsVars[t, u];
//                    var upLimit = UnitCommitmentVars[t, u] * unit.PMax;
//                    var downLimit = UnitCommitmentVars[t, u] * unit.PMin;
//                    GenerationConstrMax[t, u] = model.AddConstr(dispatch <= upLimit, "cp" + u + "t" + t);
//                    GenerationConstrMin[t, u] = model.AddConstr(dispatch >= downLimit, "cp" + u + "t" + t);
//                }
//            }
//        }
//        protected override void AddRampingConstraints()
//        {
//            UpwardRampingConstr = new GRBConstr[totalTime, totalUnits];
//            DownwardRampingConstr = new GRBConstr[totalTime, totalUnits];
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];
//                    if (t > 0)
//                    {
//                        GRBLinExpr deltaDispatch = DispatchsVars[t, u] - DispatchsVars[t - 1, u];
//                        GRBLinExpr downwardRampingLimitNormal = unit.RampDown * UnitCommitmentVars[t - 1, u];
//                        GRBLinExpr downwardRampingLimitShutdown = UnitCommitmentStopVars[t, u] * (unit.ShutDown - unit.RampDown);
//                        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
//                        DownwardRampingConstr[t, u] = model.AddConstr(-deltaDispatch <= downwardRampingLimit, "r" + u + "t" + t);


//                        GRBLinExpr upwardRampingLimitNormal = unit.RampUp * UnitCommitmentVars[t, u];
//                        GRBLinExpr upwardRampingLimitStartup = UnitCommitmentStartVars[t, u] * (unit.StartUp - unit.RampUp);
//                        GRBLinExpr upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
//                        UpwardRampingConstr[t, u] = model.AddConstr(deltaDispatch <= upwardRampingLimit, "r" + u + "t" + t);

//                    }
//                }
//            }
//        }

//        private void AddMinimumUpTime()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var units = PS.Units[u];
//                    var amountOfTimeStartedInPeriod = new GRBLinExpr();
//                    for (int t2 = Math.Max(0, (t + 1) - units.MinUpTime); t2 < t; t2++)
//                    {
//                        amountOfTimeStartedInPeriod += UnitCommitmentStartVars[t2, u];
//                    }

//                    model.AddConstr(UnitCommitmentVars[t, u] >= amountOfTimeStartedInPeriod, "MinUpTime" + t + "u" + u);
//                }
//            }
//        }

//        private void AddMinimumDownTime()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var units = PS.Units[u];
//                    var amountOfTimeStopped = new GRBLinExpr();
//                    for (int t2 = Math.Max(0, (t + 1) - units.MinDownTime); t2 < t; t2++)
//                    {
//                        amountOfTimeStopped += UnitCommitmentStopVars[t2, u];
//                    }

//                    model.AddConstr(1 - UnitCommitmentVars[t, u] >= amountOfTimeStopped, "MinDownTime" + t + "u" + u);
//                }
//            }
//        }

//        private void PowerPlantLogicConstraint()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    if (t == 0)
//                    {
//                        var ppLogic = UnitCommitmentStopVars[t, u] == 0;
//                        model.AddConstr(ppLogic, "Power Plant Logic" + t + " " + u);
//                    }
//                    else if (t != 0)
//                    {
//                        var ppLogic = UnitCommitmentVars[t - 1, u] - UnitCommitmentVars[t, u] + UnitCommitmentStartVars[t, u] - UnitCommitmentStopVars[t, u] == 0;
//                        model.AddConstr(ppLogic, "Power Plant Logic" + t + " " + u);
//                    }
//                }
//            }
//        }

//        public void ApplyFunction(Action<int, int> action)
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    action(t, u);
//                }
//            }
//        }


//        public void NodalConsoleOutput()
//        {
//            //    string line = "";
//            //    double totalREScurtailment = 0;
//            //    double totalLossOfLoad = 0;
//            //    for (int n = 0; n < totalNodes; n++)
//            //    {
//            //        var node = PS.Nodes[n];

//            //        double NodalTotalREScurtailment = 0;
//            //        double NodalTotalLossOfLoad = 0;

//            //        double NodalTotalDemand = 0;
//            //        line = "";
//            //        for (int t = 0; t < totalTime; t++)
//            //        {
//            //            NodalTotalREScurtailment += NodalREScurtailment[n, t].X;
//            //            totalREScurtailment += NodalREScurtailment[n, t].X;

//            //            NodalTotalLossOfLoad += NodalLossOfLoad[n, t].X;
//            //            totalLossOfLoad += NodalLossOfLoad[n, t].X;

//            //            NodalTotalDemand += node.Demand[t];
//            //            line += NodalInjection[n, t].X + "\t";
//            //        }
//            //        Console.WriteLine("Node {0}, Demand {1}", n, NodalTotalDemand);
//            //        Console.WriteLine("Node {0}, REScurtailment {1}", n, NodalTotalREScurtailment);
//            //        Console.WriteLine("Node {0}, Loss of load {1}", n, NodalTotalLossOfLoad);
//            //        Console.WriteLine("Node {0}, Injection {1}", n, line);
//            //    }
//            //    Console.WriteLine("REScurtailment {0}", totalREScurtailment);
//            //    Console.WriteLine("Loss of load {0}", totalLossOfLoad);
//            //    var totalDemand = PS.Nodes.Select(nodes => nodes.Demand.Sum()).Sum();
//            //    Console.WriteLine("Total Demand:{0} \t Ratio{1}", totalDemand, totalLossOfLoad / totalDemand);



//            //    for (int l = 0; l < totalLines; l++)
//            //    {
//            //        line = "";
//            //        for (int t = 0; t < totalTime; t++)
//            //        {
//            //            line += Transmission[l, t].X + " \t";.
//            //        }
//            //        Console.WriteLine("Line{0},{1} {2}", PS.Lines[l].From.Name, PS.Lines[l].To.Name, line);
//            //    }
//            //for (int u = 0; u < totalUnits; u++)
//            //{

//            //    for (int t = 0; t < totalTime; t++)
//            //    {
//            //        Console.WriteLine("u{0}t{1} z:{2}, minsp:{3} \t maxsp:{4}", u, t, UnitCommitmentVars[t, u].X, GenerationConstrMin[t, u].Pi, GenerationConstrMax[t, u].Pi);

//            //            }
//            //}

//            //Console.ReadLine();

//            string line1 = "";
//            string line2 = "";
//            string line3 = "";
//            for (int u = 0; u < totalUnits; u++)
//            {
//                line1 = "";
//                line2 = "";
//                line3 = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    line1 += Math.Round(UnitCommitmentVars[t, u].X, 3) + "\t";
//                    line2 += Math.Round(UnitCommitmentStartVars[t, u].X, 3) + "\t";
//                    line3 += Math.Round(UnitCommitmentStopVars[t, u].X, 3) + "\t";
//                }
//                Console.WriteLine(line1);
//                U.Write(IOUtils.OutputFolder + @"units.csv", line1);
//                U.Write(IOUtils.OutputFolder + @"unitsSta.csv", line2);
//                U.Write(IOUtils.OutputFolder + @"unitsSto.csv", line3);
//            }

//            Console.ReadLine();

//        }


//        public bool[,] ReadCommitStatus()
//        {
//            return WriteCommitStatus(UnitCommitmentVars);
//        }
//        public bool[,] ReadStartStatus()
//        {
//            return WriteCommitStatus(UnitCommitmentStartVars);
//        }
//        public bool[,] ReadStopStatus()
//        {
//            return WriteCommitStatus(UnitCommitmentStopVars);
//        }

//        public bool[,] WriteCommitStatus(GRBVar[,] Status)
//        {
//            bool[,] unitCommit = new bool[totalTime, totalUnits];

//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    unitCommit[t, u] = Status[t, u].X > 0.5 ? true : false;
//                }
//            }
//            return unitCommit;
//        }


//        public override Output GetAnswer()
//        {

//            return new Output(this);
//            // output.GetGenerationMixPerTime();
//        }
//    }
//}
