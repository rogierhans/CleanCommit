//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using Gurobi;

//namespace UtrechtCommitment
//{
//    class LPSolver : Solver
//    {
//        //TODO: keep track of start up costs and fixed costs of generators


//        public bool[,] DefinedUnitCommitment;
//        public LPSolver(PowerSystem ps, bool[,] definedUnitCommitment) : base(ps)
//        {
//            DefinedUnitCommitment = definedUnitCommitment;
//        }


//        public int GetCommitmentVar(int t, int u)
//        {

//            int unitcommitment = DefinedUnitCommitment[t, u] ? 1 : 0;
//            return unitcommitment;
//        }
//        public int GetStartVar(int t, int u)
//        {


//            int lastUnitCommitment = (t == 0) ? 0 : (DefinedUnitCommitment[t - 1, u] ? 1 : 0);
//            int currentUnitCommitment = (DefinedUnitCommitment[t, u] ? 1 : 0);
//            int diffUnitCommit = lastUnitCommitment - currentUnitCommitment;
//            int startValue = diffUnitCommit == -1 ? 1 : 0;

//            return startValue;
//        }

//        public int GetStopVar(int t, int u)
//        {


//            int lastUnitCommitment = (t == 0) ? 0 : (DefinedUnitCommitment[t - 1, u] ? 1 : 0);
//            int currentUnitCommitment = (DefinedUnitCommitment[t, u] ? 1 : 0);
//            int diffUnitCommit = lastUnitCommitment - currentUnitCommitment;
//            int stopValue = diffUnitCommit == 1 ? 1 : 0;

//            return stopValue;
//        }

//        public void ChangeCommitStatus(bool[,] definedUnitCommitment)
//        {
//            DefinedUnitCommitment = definedUnitCommitment;
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    UpdateSingleCommitStatus(t, u);
//                }
//            }

//        }

//        public void ChangeSingleStatus(int t, int u)
//        {
//            DefinedUnitCommitment[t, u] = !DefinedUnitCommitment[t, u];
//            UpdateSingleCommitStatus(t, u);
//            if (t < totalTime - 1)
//            {
//                UpdateSingleCommitStatus(t + 1, u);

//            }
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
//                    var upLimit = GetCommitmentVar(t, u) * unit.PMax;
//                    var downLimit = GetCommitmentVar(t, u) * unit.PMin;
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

//                        double downwardRampingLimitNormal = unit.RampDown * GetCommitmentVar(t - 1, u);
//                        double downwardRampingLimitShutdown = GetStopVar(t, u) * (unit.ShutDown - unit.RampDown);
//                        GRBLinExpr downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
//                        DownwardRampingConstr[t, u] = model.AddConstr(-deltaDispatch <= downwardRampingLimit, "r" + u + "t" + t);


//                        double upwardRampingLimitNormal = unit.RampUp * GetCommitmentVar(t, u);
//                        double upwardRampingLimitStartup = GetStartVar(t, u) * (unit.StartUp - unit.RampUp);
//                        double upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
//                        UpwardRampingConstr[t, u] = model.AddConstr(deltaDispatch <= upwardRampingLimit, "r" + u + "t" + t);
//                    }
//                }
//            }
//        }



//        public void LSTest()
//        {
//            ////model.Update();
//            //var OGcommitment = CopyCommitment(DefinedUnitCommitment);
//            //SwitchModelOutputOn();
//            //var watch = new Stopwatch();
//            //watch.Start();
//            //model.Optimize();
//            //var oldCopy = new CopySolution(this,StorageMode,"OG"); 
//            //for (int t = 0; t < totalTime; t++)
//            //{
//            //    for (int u = 0; u < totalUnits; u++)
//            //    {
//            //        ChangeSingleStatus(t, u);
//            //        model.Optimize();
//            //        var newCopy = new CopySolution(this, StorageMode, "t"+t +"u"+u);
//            //        Console.WriteLine(Objective.Value);
//            //        newCopy.PrintDispatch(oldCopy, t,u);
//            //        newCopy.PrintUCED(PS);
//            //        DefinedUnitCommitment = CopyCommitment(OGcommitment);
//            //        ChangeCommitStatus(DefinedUnitCommitment);
//            //        //oldCopy = newCopy;
//            //        // Console.ReadLine();
//            //    }
//            //}
//            //Console.WriteLine(watch.ElapsedMilliseconds / 1000);
//        }

//        public bool[,] CopyCommitment(bool[,] originalCommitment) {
//            bool[,] copyCommitment = new bool[originalCommitment.GetLength(0), originalCommitment.GetLength(1)];
//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    copyCommitment[t, u] = originalCommitment[t, u];
//                }
//            }
//            return copyCommitment;
//        }



//        public void UpdateSingleCommitStatus(int t, int u)
//        {
//            //change the generation limits
//            Unit unit = PS.Units[u];
//            var upLimit = unit.PMax;
//            var downLimit = unit.PMin;
//            GenerationConstrMax[t, u].RHS = DefinedUnitCommitment[t, u] ? upLimit : 0;
//            GenerationConstrMin[t, u].RHS = DefinedUnitCommitment[t, u] ? downLimit : 0;

//            //change the ramping limits
//            if (t > 0 && Ramp)
//            {
//                double downwardRampingLimitNormal = unit.RampDown * GetCommitmentVar(t - 1, u);
//                double downwardRampingLimitShutdown = GetStopVar(t, u) * (unit.ShutDown - unit.RampDown);
//                double downwardRampingLimit = downwardRampingLimitNormal + downwardRampingLimitShutdown;
//                DownwardRampingConstr[t, u].RHS = downwardRampingLimit;


//                double upwardRampingLimitNormal = unit.RampUp * GetCommitmentVar(t, u);
//                double upwardRampingLimitStartup = GetStartVar(t, u) * (unit.StartUp - unit.RampUp);
//                double upwardRampingLimit = upwardRampingLimitNormal + upwardRampingLimitStartup;
//                Console.WriteLine(t + " " + u);
//                UpwardRampingConstr[t, u].RHS = upwardRampingLimit;

//            }
//        }


//    }
//}

////        PowerSystem PS;
////        //Solution Solution;
////        GRBEnv env;
////        GRBModel model;

////        GRBVar[,] DispatchsVars;
////        private int totalTime;
////        private int totalUnits;

////        public LPSolver(PowerSystem ps)//, Solution s)
////        {
////            PS = ps;
////            env = new GRBEnv();
////            model = new GRBModel(env);
////            totalTime = PS.Nodes[0].Demand.Count;
////            totalUnits = PS.Units.Count;
////        }

////        GRBVar[] LossOfLoad;
////        GRBVar[] LossOfGeneration;
////        public void ConfigureModel()
////        {
////            bool[,] CommitStatus = new bool[totalTime, totalUnits];
////            AddVariables();
////            AddConstraints(CommitStatus);
////            AddObjective();
////            //model.Parameters.Heuristics = 0;
////            model.Parameters.OutputFlag = 1;
////            //model.Parameters.StartNodeLimit = 20000000;
////        }
////        public void AddVariables()
////        {

////            //decision variables
////            DispatchsVars = new GRBVar[totalTime, PS.Units.Count];


////            //variables for violating constraints
////            LossOfLoad = new GRBVar[totalTime];
////            LossOfGeneration = new GRBVar[totalTime];

////            for (int t = 0; t < totalTime; t++)
////            {
////                for (int u = 0; u < totalUnits; u++)
////                {
////                    Unit unit = PS.Units[u];
////                    DispatchsVars[t, u] = model.AddVar(0, unit.Generation.PMax, 0.0, GRB.CONTINUOUS, "p" + u + "t" + t);
////                }
////                LossOfGeneration[t] = model.AddVar(0, PS.Units.Sum(unit => unit.Generation.PMax), 0.0, GRB.CONTINUOUS, "t" + t);
////                LossOfLoad[t] = model.AddVar(0, time.Demand, 0.0, GRB.CONTINUOUS, "t" + t);
////            }
////        }
////        GRBLinExpr Objective;

////        float VOLL = 100000;
////        float VOLG = 100000;
////        public void AddObjective()
////        {
////            Objective = new GRBLinExpr();
////            for (int t = 0; t < totalTime; t++)
////            {
////                for (int u = 0; u < totalUnits; u++)
////                {
////                    Unit unit = PS.Units[u];
////                    Objective += DispatchsVars[t, u] * unit.Generation.B;
////                }
////                Objective += VOLL * LossOfLoad[t];
////                Objective += VOLG * LossOfGeneration[t];
////            }
////            model.SetObjective(Objective, GRB.MINIMIZE);
////        }
////        public void AddConstraints(bool[,] CommitStatus)
////        {
////            AddPowerConstraints();
////            AddGenerationConstraints(CommitStatus);
////            //AddRampingConstraints();
////        }
////        GRBConstr[] PowerBalance;


////        GRBConstr[,] GenerationConstrMin;
////        GRBConstr[,] GenerationConstrMax;
////        private void AddGenerationConstraints(bool[,] CommitStatus)
////        {
////            GenerationConstrMin = new GRBConstr[totalTime, totalUnits];
////            GenerationConstrMax = new GRBConstr[totalTime, totalUnits];
////            for (int t = 0; t < totalTime; t++)
////            {
////                for (int u = 0; u < totalUnits; u++)
////                {
////                    Unit unit = PS.Units[u];
////                    var dispatch = DispatchsVars[t, u];
////                    var upLimit = CommitStatus[t, u] ? unit.Generation.PMax : 0;
////                    var downLimit = CommitStatus[t, u] ? unit.Generation.PMin : 0;
////                    GenerationConstrMax[t, u] = model.AddConstr(dispatch <= upLimit, "cp" + u + "t" + t);
////                    GenerationConstrMin[t, u] = model.AddConstr(dispatch >= downLimit, "cp" + u + "t" + t);
////                }
////            }
////        }

////        private void SetConstraints(bool[,] commitStatus)
////        {
////            for (int t = 0; t < totalTime; t++)
////            {
////                for (int u = 0; u < totalUnits; u++)
////                {
////                    ChangeGenerationConstraint(t, u, commitStatus[t, u]);
////                }
////            }
////        }
////        public void ChangeConstraints(int t, int u, bool status)
////        {
////            ChangeGenerationConstraint(t, u, status);
////        }



////        GRBConstr[,] UpwardRampingConstr;
////        GRBConstr[,] DownwardRampingConstr;
////        private void AddRampingConstraints(bool[,] CommitStatus)
////        {
////            UpwardRampingConstr = new GRBConstr[totalTime, totalUnits];
////            DownwardRampingConstr = new GRBConstr[totalTime, totalUnits];
////            for (int t = 0; t < totalTime; t++)
////            {
////                for (int u = 0; u < totalUnits; u++)
////                {
////                    //Unit unit = PS.Units[u];
////                    //if (t > 0)
////                    //{
////                    //    var deltaDispatch = DispatchsVars[t, u] - DispatchsVars[t - 1, u];




////                    //    var downwardRampingLimit = unit.RampDown * CommitStatus[t, u] + (unit.ShutDown * UnitCommitmentStopVars[t, u]);
////                    //    DownwardRampingConstr[t, u] = model.AddConstr(-deltaDispatch <= downwardRampingLimit, "r" + u + "t" + t);


////                    //    var upwardRampingLimit = unit.RampUp * CommitStatus[t, u] + (unit.StartUp * UnitCommitmentStartVars[t, u]);
////                    //    UpwardRampingConstr[t, u] = model.AddConstr(deltaDispatch <= upwardRampingLimit, "r" + u + "t" + t);
////                    //    model.Remove(UpwardRampingConstr[t, u]);
////                    //}
////                }
////            }
////        }

////        public double Solve(bool[,] CommitStatus)
////        {
////            SetConstraints(CommitStatus);
////            model.Reset(0);
////            model.Optimize();
////            if (model.Status == 3) return Double.MaxValue;
////            Console.WriteLine("Loss of load {0}", LossOfLoad.ToList().Sum(x => x.X));
////            return Objective.Value;
////        }

////        public double Solve()
////        {
////            model.Optimize();
////            if (model.Status == 3) return Double.MaxValue;
////            //Console.WriteLine("Loss of load {0}", LossOfLoad.ToList().Sum(x => x.X));
////            return Objective.Value;
////        }

////    }
////}
