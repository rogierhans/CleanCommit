using CleanCommit.MIP;
using CleanCommit.Instance;
using Gurobi;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace CleanCommit
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
        public RollingSolver(Solution storageSolution, ConstraintConfiguration cc, int horizon, int overlap)
        {
            StorageSolution = storageSolution;
            if (overlap >= horizon) throw new Exception("horizon should be bigger than overlap");
            PS = StorageSolution.PS;
            OGCC = cc;
            env = new GRBEnv();
            Roll(horizon, overlap);
        }


        public void Roll(int horizon, int overlap)
        {
            List<Solution> sols = new List<Solution>();
            Solution oldSol;
            oldSol = RunFirstModel(horizon, sols);
            oldSol = RunOtherModels(horizon, overlap, sols, oldSol);

        }

        private Solution RunOtherModels(int horizon, int overlap, List<Solution> sols, Solution oldSol)
        {
            Console.ReadLine();
            for (int rollingOffset = (horizon - overlap); rollingOffset < (OGCC.TotalTime - horizon); rollingOffset += (horizon - overlap))
            {
                ConfigureModel(rollingOffset, horizon, overlap, oldSol);
                var newSol = NewSolve(600, -1);

                for (int t = 0; t < newSol.Dispatch.GetLength(0); t++)
                {
                    string line = "";
                    for (int g = 0; g < newSol.Dispatch.GetLength(1); g++)
                    {
                        line += "\t" + newSol.Dispatch[t, g];
                    }
                    Console.WriteLine(line);
                }
                oldSol = newSol;
                sols.Add(oldSol);
                Console.ReadLine();
            }
            return oldSol;
        }

        private Solution RunFirstModel(int horizon, List<Solution> sols)
        {
            Solution oldSol;
            (var init, var end) = GetStorageValuesInit(StorageSolution, horizon);

            FirstConfigureModel(horizon, init, end);
            oldSol = NewSolve(600, -1);
            sols.Add(oldSol);


            for (int t = 0; t < oldSol.Dispatch.GetLength(0); t++)
            {
                string line = "";
                for (int g = 0; g < oldSol.Dispatch.GetLength(1); g++)
                {
                    line += "\t" + oldSol.Dispatch[t, g];
                }
                Console.WriteLine(line);
            }

            return oldSol;
        }

        private (Dictionary<int, double>, Dictionary<int, double>) GetStorageValuesInit(Solution StorageSolution, int horizon)
        {
            Dictionary<int, double> init = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                init[s] = StorageSolution.Storage[0, s];
            }
            Dictionary<int, double> end = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                end[s] = StorageSolution.Storage[Math.Min(horizon , OGCC.TotalTime) - 1, s];
            }
            return (init, end);
        }

        private (Dictionary<int, double>, Dictionary<int, double>) GetStorageValues(Solution StorageSolution, Solution Old,int begin,int overlap, int horizon)
        {
            Dictionary<int, double> init = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                init[s] = Old.Storage[horizon-overlap-1 , s];
            }
            Dictionary<int, double> end = new Dictionary<int, double>();
            for (int s = 0; s < PS.StorageUnits.Count; s++)
            {
                end[s] = StorageSolution.Storage[Math.Min(horizon + begin, OGCC.TotalTime) - 1, s];
            }
            return (init, end);
        }
        public void FirstConfigureModel(int horizon, Dictionary<int, double> Storage2Init, Dictionary<int, double> Storage2End)
        {
            var CC = OGCC.Copy();
            CC.TotalTime = Math.Min(horizon, OGCC.TotalTime);
            CC.TimeOffSet = OGCC.TimeOffSet;

            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            // model.Parameters.Threads = 1;
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.00001");
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


        public virtual void ConfigureModel(int rollingOffset, int horizon, int overlap, Solution Old)
        {
            var CC = OGCC.Copy();
            CC.TotalTime = Math.Min(horizon, (OGCC.TotalTime - rollingOffset));
            CC.TimeOffSet = rollingOffset + OGCC.TimeOffSet;
            Console.WriteLine("TT:{0} TO:{1} hor:{2} overlap:{3} rollingOffset:{4}", CC.TotalTime, CC.TimeOffSet, horizon, overlap, rollingOffset);


            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            // model.Parameters.Threads = 1;
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.00001");
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
            (var init, var end) = GetStorageValues(StorageSolution,Old, rollingOffset,overlap, horizon);
            AddConstraints(CC, horizon, overlap, Old, init, end);
        }

        PowerBalanceContraint PBC;
        TransmissionConstraint TC;

        public virtual void AddConstraints(ConstraintConfiguration CC, int horizon, int overlap, Solution Old, Dictionary<int, double> Storage2Init, Dictionary<int, double> Storage2End)
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

            var overlapConstraint = new OverlapConstraint(PS, CC, model, Variables, horizon, overlap, Old);
        }
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

        public Solution NewSolve(int TimeLimit, int v)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Parameters.Method = v;
            model.Optimize();
            // Do IIS
            if ((model.Status == GRB.Status.INF_OR_UNBD) ||
          (model.Status == GRB.Status.INFEASIBLE))
            {
                Console.WriteLine("The model is infeasible; computing IIS");
                model.ComputeIIS();
                Console.WriteLine("\nThe following constraint(s) "
                    + "cannot be satisfied:");
                foreach (GRBConstr c in model.GetConstrs())
                {
                    if (c.IISConstr == 1)
                    {
                        Console.WriteLine(c.ConstrName);
                    }
                }
            }

            return new Solution(model, Objective, Variables, PS, OGCC, TC, PBC);
        }
    }
}


