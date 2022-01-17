using CleanCommit.MIP;
using CleanCommit.Instance;
using Gurobi;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace CleanCommit

//die dubble offest kan een probleem worden
{
    public class RollingSolver
    {

        public PowerSystem PS;
        protected GRBEnv env;
        public GRBModel model;
        private Variables Variables;
        private ConstraintConfiguration OGCC;
        private Objective Objective;
        Solution StorageSolution;
        public RollingSolver(Solution storageSolution, ConstraintConfiguration cc)
        {
            StorageSolution = storageSolution;
            PS = StorageSolution.PS;
            OGCC = cc;
            env = new GRBEnv();
        }





        public Solution Roll(int horizon, int forced, int leap, string name)
        {
            if (forced >= horizon || leap>= horizon) throw new Exception("horizon should be bigger than overlap");
            List<Solution> sols = new List<Solution>();
            Solution oldSol = RunFirstModel(horizon);
            sols.Add(oldSol);
            while (oldSol.CC.TimeOffSet + oldSol.CC.TotalTime < OGCC.TotalTime)
            {
                oldSol = RunOtherModels(horizon, forced, leap, oldSol);
                sols.Add(oldSol);
            }
            var PSC = new PartialSolutionCombiner(sols, OGCC,  horizon, forced, leap);
            PSC.Solution.ToCSV(@"E:\Temp3\" + name + ".csv");
            PSC.Solution.ToBin(@"E:\Temp3\" + name + ".bin");
            return PSC.Solution;
        }

        private Solution RunOtherModels(int horizon, int forced, int leap, Solution oldSol)
        {
            int rollingOffset = leap + oldSol.CC.TimeOffSet;
            ConstraintConfiguration CC = CreateCCForRoll(horizon, rollingOffset);
            (var init, var end) = GetStorageValues(StorageSolution, oldSol, rollingOffset, horizon, forced,  leap);
            ConfigureModel(CC, init, end);
            var OC = new OverlapConstraint(PS, CC, model, Variables, horizon, forced, oldSol, StorageSolution.P2GGeneration, rollingOffset);
            var newSol = NewSolve(CC, 600, -1);
            Console.WriteLine("{0} {1} {2} {3}", rollingOffset, horizon, OGCC.TotalTime, forced);
            // Console.ReadLine();
            return newSol;
        }


        private Solution TestSoltuin(Solution combinedSolution)
        {
            var CC = OGCC.Copy();
            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            // model.Parameters.Threads = 1;
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.00001");
            model.Set("OutputFlag", "0");
            model.Set(GRB.IntParam.LogToConsole, 0);
            //model.Set("Method", "1");
            //model.Set("IntFeasTol", "0.000000001");

            Console.WriteLine("Variables...");
            Variables = new Variables(PS, CC, model);
            Variables.IntialiseVariables();

            Console.WriteLine("Objective...");
            Objective = new Objective(PS, CC, model, Variables);
            Objective.AddObjective();

            var GenerationConstraint = new GenerationConstraint(PS, CC, model, Variables);
            GenerationConstraint.AddConstraint();

            var RampingConstraint = new RampConstraint(PS, CC, model, Variables);
            RampingConstraint.AddConstraint();

            var PiecewiseConstraint = new PiecewiseConstraint(PS, CC, model, Variables);
            PiecewiseConstraint.AddConstraint();

            TC = new TransmissionConstraint(PS, CC, model, Variables);
            TC.AddConstraint();

            PBC = new PowerBalanceContraint(PS, CC, model, Variables);
            PBC.AddConstraint();

            var LogicConstraint = new LogicConstraint(PS, CC, model, Variables);
            LogicConstraint.AddConstraint();

            var StorageConstraint = new StorageConstraint(PS, CC, model, Variables);
            StorageConstraint.AddConstraint();

            var MinUpDownConstraint = new MinUpDownConstraint(PS, CC, model, Variables);
            MinUpDownConstraint.AddConstraint();

            var TimeDepStartConstraint = new TimeDepStartConstraint(PS, CC, model, Variables);
            TimeDepStartConstraint.AddConstraint();

            var ReserveConstraint = new ReserveConstraint(PS, CC, model, Variables);
            ReserveConstraint.AddConstraint();

            new OverlapConstraint(PS, CC, model, Variables, CC.TotalTime, CC.TotalTime, combinedSolution, StorageSolution.P2GGeneration, 0);

            var newSol = NewSolve(CC, 3600, -1);

            return newSol;

        }



        private ConstraintConfiguration CreateCCForRoll(int horizon, int offset)
        {
            var CC = OGCC.Copy();
            CC.TotalTime = Math.Min(horizon, (OGCC.TotalTime - offset));
            CC.TimeOffSet = offset + OGCC.TimeOffSet;

           // Console.WriteLine("TT:{0} TO:{1} hor:{2} rollingOffset:{3}", CC.TotalTime, CC.TimeOffSet, horizon, leap);
            return CC;
        }

        private Solution RunFirstModel(int horizon)
        {
            (var init, var end) = GetStorageValuesInit(StorageSolution, horizon);
            var CC = OGCC.Copy();
            CC.TotalTime = Math.Min(horizon, OGCC.TotalTime);
            CC.TimeOffSet = OGCC.TimeOffSet;
            ConfigureModel(CC, init, end);
            var solution = NewSolve(CC, 600, -1);
            return solution;
        }

        private (Dictionary<int, double>, Dictionary<int, double>) GetStorageValuesInit(Solution StorageSolution, int horizon)
        {
            Dictionary<int, double> init = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                var storageUnit = PS.StorageUnits[s];
                init[s] = storageUnit.MaxEnergy / 2;
            }
            Dictionary<int, double> end = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                end[s] = StorageSolution.Storage[Math.Min(horizon, OGCC.TotalTime) - 1, s];
            }
            return (init, end);
        }

        private (Dictionary<int, double>, Dictionary<int, double>)
            GetStorageValues(Solution StorageSolution, Solution Old, int begin, int horizon, int forced, int leap)
        {
            Dictionary<int, double> init = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                init[s] = Old.Storage[leap - 1, s];
            }
            Dictionary<int, double> end = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                end[s] = StorageSolution.Storage[Math.Min(horizon + begin, OGCC.TotalTime) - 1, s];
            }
            return (init, end);
        }
        public void ConfigureModel(ConstraintConfiguration CC, Dictionary<int, double> Storage2Init, Dictionary<int, double> Storage2End)
        {
            if (!(model is null))
                model.Dispose();
            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            // model.Parameters.Threads = 1;
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.00001");
            model.Set("OutputFlag", "0");
            model.Set(GRB.IntParam.LogToConsole, 0);
            //model.Set("Method", "1");
            //model.Set("IntFeasTol", "0.000000001");

            Console.WriteLine("Variables...");
            Variables = new Variables(PS, CC, model);
            Variables.IntialiseVariables();

            Console.WriteLine("Objective...");
            Objective = new Objective(PS, CC, model, Variables);
            Objective.AddObjective();

            Console.WriteLine("AddConstraints...");
            AddConstraints(CC, Storage2Init, Storage2End);
        }



        PowerBalanceContraint PBC;
        TransmissionConstraint TC;

        public virtual void AddConstraints(ConstraintConfiguration CC, Dictionary<int, double> Storage2Init, Dictionary<int, double> Storage2End)
        {
            var GenerationConstraint = new GenerationConstraint(PS, CC, model, Variables);
            GenerationConstraint.AddConstraint();

            var RampingConstraint = new RampConstraint(PS, CC, model, Variables);
            RampingConstraint.AddConstraint();

            var PiecewiseConstraint = new PiecewiseConstraint(PS, CC, model, Variables);
            PiecewiseConstraint.AddConstraint();

            TC = new TransmissionConstraint(PS, CC, model, Variables);
            TC.AddConstraint();

            PBC = new PowerBalanceContraint(PS, CC, model, Variables);
            PBC.AddConstraint();

            var LogicConstraint = new LogicConstraint(PS, CC, model, Variables);
            LogicConstraint.AddConstraint();

            var StorageConstraint = new StorageConstraint(PS, CC, model, Variables);
            StorageConstraint.AddConstraintWithBeginAndEndLimits(Storage2Init, Storage2End);

            var MinUpDownConstraint = new MinUpDownConstraint(PS, CC, model, Variables);
            MinUpDownConstraint.AddConstraint();

            var TimeDepStartConstraint = new TimeDepStartConstraint(PS, CC, model, Variables);
            TimeDepStartConstraint.AddConstraint();

            var ReserveConstraint = new ReserveConstraint(PS, CC, model, Variables);
            ReserveConstraint.AddConstraint();
        }

        public void Kill()
        {
            model.Dispose();
            env.Dispose();
        }

        public Solution NewSolve(ConstraintConfiguration CC, int TimeLimit, int v)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Parameters.Method = v;
            model.Optimize();
            // Do IIS
            if ((model.Status == GRB.Status.INF_OR_UNBD))
            {
                Console.WriteLine("The model is infeasible or unbounded -> setting dual reductions to 0");
                model.Set("DualReductions", "0");
                model.Optimize();
            }

            if ((model.Status == GRB.Status.INFEASIBLE))
            {

                Console.WriteLine("The model is infeasible; computing IIS");
                model.ComputeIIS();
                Console.WriteLine("\nThe following constraint(s) cannot be satisfied:");
                foreach (GRBConstr c in model.GetConstrs())
                {
                    if (c.IISConstr == 1)
                    {
                        Console.WriteLine(c.ConstrName);
                    }
                }
            }
            if (model.Status == GRB.Status.UNBOUNDED)
            {
                model.Set(GRB.IntParam.InfUnbdInfo, 1);
                model.Optimize();
                foreach (var varible in model.GetVars())
                {

                    File.WriteAllText(@"C:\Users\4001184\Desktop\LogFile.txt", string.Format("{0}: {1}", varible.VarName, varible.UnbdRay) + "\n");
                }
                foreach (var varible in model.GetVars())
                {

                    File.WriteAllText(@"C:\Users\4001184\Desktop\LogFile.txt", string.Format("{0}_X: {1}", varible.VarName, varible.X) + "\n");
                }
            }
            return new Solution(model, Objective, Variables, PS, CC, TC, PBC);
        }
    }
}


