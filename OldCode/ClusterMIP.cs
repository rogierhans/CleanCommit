//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Gurobi;

//namespace UtrechtCommitment
//{
//    class ClusterMIP : Solver
//    {

//        bool MinUpDownTime;
//        public ClusterMIP(PowerSystem PS)
//            : base(PS)
//        {
//            MinUpDownTime = PS.ConstraintConfiguration.MinUpMinDown;
//        }


//        public GRBVar[,] UnitCommitmentVars;
//        public GRBVar[,] UnitCommitmentStartVars;
//        public GRBVar[,] UnitCommitmentStopVars;

//        public override void AddVariables()
//        {
//            base.AddVariables();
//            UnitCommitmentVars = new GRBVar[totalTime, PS.Units.Count];
//            //help variable for start and stop of a generator
//            UnitCommitmentStartVars = new GRBVar[totalTime, PS.Units.Count];
//            UnitCommitmentStopVars = new GRBVar[totalTime, PS.Units.Count];
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];


//                    UnitCommitmentVars[t, u] = model.AddVar(0.0, unit.Count, 0.0, GRB.INTEGER, "u" + u + "t" + t);
//                    UnitCommitmentStartVars[t, u] = model.AddVar(0.0, unit.Count, 0.0, GRB.INTEGER, "u" + u + "t" + t);
//                    UnitCommitmentStopVars[t, u] = model.AddVar(0.0, unit.Count, 0.0, GRB.INTEGER, "u" + u + "t" + t);
//                }
//            }
//        }

//        public override void AddObjective()
//        {
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];
//                    GRBLinExpr baseGeneration = UnitCommitmentVars[t, u] * unit.A;
//                    GRBLinExpr startCost = UnitCommitmentStartVars[t, u] * unit.StartCostInterval.Last();
//                    Objective += startCost + baseGeneration;
//                }
//            }

//            base.AddObjective();
//        }


//        public override void AddConstraints()
//        {
//            base.AddConstraints();

//            if (MinUpDownTime)
//            {
//                AddMinimumDownTime();
//                AddMinimumUpTime();
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
//                    var unit = PS.Units[u];
//                    var amountOfTimeStopped = new GRBLinExpr();
//                    for (int t2 = Math.Max(0, (t + 1) - unit.MinDownTime); t2 < t; t2++)
//                    {
//                        amountOfTimeStopped += UnitCommitmentStopVars[t2, u];
//                    }

//                    model.AddConstr(unit.Count - UnitCommitmentVars[t, u] >= amountOfTimeStopped, "MinDownTime" + t + "u" + u);
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
//                        var ppLogic = 0 - UnitCommitmentVars[t, u] + UnitCommitmentStartVars[t, u] - UnitCommitmentStopVars[t, u] == 0;
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


 


//        public void NodalConsoleOutput()
//        {




//            string line = "";
//            double totalREScurtailment = 0;
//            double totalLossOfLoad = 0;
//            for (int n = 0; n < totalNodes; n++)
//            {
//                var node = PS.Nodes[n];

//                double NodalTotalREScurtailment = 0;
//                double NodalTotalLossOfLoad = 0;

//                double NodalTotalDemand = 0;
//                line = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    for (int r = 0; r < totatRESTypes; r++)
//                    {
//                        NodalTotalREScurtailment += NodalRESGeneration[n, r, t].X;
//                        totalREScurtailment += NodalRESGeneration[n, r, t].X;
//                    }

//                    NodalTotalLossOfLoad += NodalLossOfLoad[n, t].X;
//                    Console.Write(Math.Round(NodalLossOfLoad[n, t].X, 2) + "\t");
//                    totalLossOfLoad += NodalLossOfLoad[n, t].X;

//                    NodalTotalDemand += node.Demand[t];
//                    line += NodalInjection[n, t].X + "\t";
//                }
//                Console.WriteLine(node.Name);
//                Console.WriteLine("Node {0}, Demand {1}", n, NodalTotalDemand);
//                Console.WriteLine("Node {0}, RES{1}", n, NodalTotalREScurtailment);
//                Console.WriteLine("Node {0}, Loss of load {1}", n, NodalTotalLossOfLoad);
//                Console.WriteLine("Node {0}, Injection {1}", n, line);
//            }
//            Console.WriteLine("REScurtailment {0}", totalREScurtailment);
//            Console.WriteLine("Loss of load {0}", totalLossOfLoad);
//            var totalDemand = PS.Nodes.Select(nodes => nodes.Demand.Sum()).Sum();
//            Console.WriteLine("Total Demand:{0} \t Ratio{1}", totalDemand, totalLossOfLoad / totalDemand);



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
//            //string line2 = "";
//            //string line3 = "";
//            for (int u = 0; u < totalUnits; u++)
//            {
//                var sunit = PS.Units[u];
//                //line1 = sunit.Name + "\t " + PS.Nodes.Where(x => x.UnitsIndex.Contains(u)).First().Name + "\t";
//                //line2 = "";
//                //line3 = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    line1 += Math.Round(DispatchsVars[t, u].X, 3) + "\t";
//                    //line2 += Math.Round(UnitCommitmentStartVars[t, u].X, 3) + "\t";
//                    //line3 += Math.Round(UnitCommitmentStopVars[t, u].X, 3) + "\t";
//                }
//                Console.WriteLine(line1);
//                //Utils.Write(IOUtils.OutputFolder + @"units.csv", line1);
//                //Utils.Write(IOUtils.OutputFolder + @"unitsSta.csv", line2);
//                //Utils.Write(IOUtils.OutputFolder + @"unitsSto.csv", line3);
//            }

//            Console.ReadLine();

//        }

//        public override Output GetAnswer()
//        {

//            return new Output(this);
//            // output.GetGenerationMixPerTime();
//        }
//    }
//}
