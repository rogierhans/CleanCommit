using CleanCommit.MIP;
using CleanCommit.Instance;
using Gurobi;
using System.Diagnostics;
using System;
using System.IO;
namespace CleanCommit
{
    public class TightSolver
    {

        public PowerSystem PS;
        protected GRBEnv env;
        public GRBModel model;
        private Variables Variables;
        public ConstraintConfiguration CC;
        private Objective Objective;

        public TightSolver(PowerSystem ps, ConstraintConfiguration cc)
        {

            PS = ps;
            CC = cc;
            env = new GRBEnv();
            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            // model.Parameters.Threads = 1;
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.001");
            model.Set(GRB.IntParam.LogToConsole, 0);
            //model.Set("Method", "1");
            //model.Set("IntFeasTol", "0.000000001");
        }
        public TightSolver(PowerSystem ps, ConstraintConfiguration cc, double gap)
        {

            PS = ps;
            CC = cc;
            env = new GRBEnv();
            model = new GRBModel(env);
            model.SetCallback(new ConsoleOverwrite());
            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            model.Set("DisplayInterval", "1");
            //model.Set("MIPGap", gap.ToString());
            //model.Set("IntFeasTol", "0.000000001");
        }

        public void SetOutputFlag(bool OutputFlag)
        {
            model.Set("OutputFlag", OutputFlag ? "1" : "0");
        }

        public virtual void ConfigureModel()
        {
            Console.WriteLine("Variables...");
            Variables = new Variables(PS, CC, model, this);
            Variables.IntialiseVariables();

            Console.WriteLine("Objective...");
            Objective = new Objective(PS, CC, model, this, Variables);
            Objective.AddObjective();

            Console.WriteLine("AddConstraints...");
            AddConstraints();
        }

        PowerBalanceContraint PBC;
        public virtual void AddConstraints()
        {
            var GenerationConstraint = new GenerationConstraint(PS, CC, model, Variables);
            GenerationConstraint.AddConstraint();
            var RampingConstraint = new RampConstraint(PS, CC, model, Variables);
            RampingConstraint.AddConstraint();
            var PiecewiseConstraint = new PiecewiseConstraint(PS, CC, model, Variables);
            PiecewiseConstraint.AddConstraint();
            var TransmissionConstraint = new TransmissionConstraint(PS, CC, model, Variables);
            TransmissionConstraint.AddConstraint();
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
        }


        public void Kill()
        {
            model.Dispose();
            env.Dispose();
        }
        public SolverOutput SolveMin(int TimeLimit, double fraction)
        {
            model.Parameters.TimeLimit = TimeLimit;
            string filename = @"C:\Users\4001184\Desktop\gurobi.lp";
            model.Write(filename);
            model.Optimize();
            Objective.OnlyAnalyses();
            var output = new SolverOutput(Variables, Objective, model, model.Runtime);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", Objective.AltObjective.Value.ToString() + " " + output.GurobiCost.ToString() + "\n");
            model.AddConstr(Objective.CurrentObjective <= output.GurobiCost * fraction, "");
            new SolverOutput(Variables, Objective, model, model.Runtime).WriteToCSV(@"C:\Users\4001184\Desktop\temp4\", "1" + fraction);
            Objective.AddAlternativeObjective();
            model.Optimize();
            var output2 = new SolverOutput(Variables, Objective, model, model.Runtime);
            output2.WriteToCSV(@"C:\Users\4001184\Desktop\temp4\", "2" + fraction);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", "min:" + Objective.AltObjective.Value.ToString() + " " + output2.GurobiCost.ToString() + "\n");
            return new SolverOutput(Variables, Objective, model, model.Runtime); ;
        }

        public SolverOutput SolveMax(int TimeLimit, double fraction)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            Objective.OnlyAnalyses();
            var output = new SolverOutput(Variables, Objective, model, model.Runtime);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", Objective.AltObjective.Value.ToString() + " " + output.GurobiCost.ToString() + "\n");
            model.AddConstr(Objective.CurrentObjective <= output.GurobiCost * fraction, "");
            new SolverOutput(Variables, Objective, model, model.Runtime).WriteToCSV(@"C:\Users\4001184\Desktop\temp4\", "1" + fraction);
            Objective.AddAlternativeObjectiveMax();
            model.Optimize();
            var output3 = new SolverOutput(Variables, Objective, model, model.Runtime);
            output3.WriteToCSV(@"C:\Users\4001184\Desktop\temp4\", "3" + fraction);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", "max:" + Objective.AltObjective.Value.ToString() + " " + output3.GurobiCost.ToString() + "\n");
            return new SolverOutput(Variables, Objective, model, model.Runtime); ;
        }

        public SolverOutput SolveMinCO2(int TimeLimit, double fraction)
        {
            model.Parameters.TimeLimit = TimeLimit;
            string filename = @"C:\Users\4001184\Desktop\gurobi.lp";
            model.Write(filename);
            model.Optimize();
            Objective.OnlyAnalysesCO2();
            var output = new SolverOutput(Variables, Objective, model, model.Runtime);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", Objective.AltObjective.Value.ToString() + " " + output.GurobiCost.ToString() + "\n");
            model.AddConstr(Objective.CurrentObjective <= output.GurobiCost * fraction, "");
            new SolverOutput(Variables, Objective, model, model.Runtime).WriteToCSV(@"C:\Users\4001184\Desktop\temp2\", "1" + fraction);
            Objective.AddCO2ObjectiveMin();
            model.Optimize();
            var output2 = new SolverOutput(Variables, Objective, model, model.Runtime);
            output2.WriteToCSV(@"C:\Users\4001184\Desktop\temp2\", "2" + fraction);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", "min:" + Objective.AltObjective.Value.ToString() + " " + output2.GurobiCost.ToString() + "\n");
            return new SolverOutput(Variables, Objective, model, model.Runtime); ;
        }

        public SolverOutput SolveMaxCO2(int TimeLimit, double fraction)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            Objective.OnlyAnalysesCO2();
            var output = new SolverOutput(Variables, Objective, model, model.Runtime);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", Objective.AltObjective.Value.ToString() + " " + output.GurobiCost.ToString() + "\n");
            model.AddConstr(Objective.CurrentObjective <= output.GurobiCost * fraction, "");
            new SolverOutput(Variables, Objective, model, model.Runtime).WriteToCSV(@"C:\Users\4001184\Desktop\temp2\", "1" + fraction);
            Objective.AddCO2ObjectiveMax();
            model.Optimize();
            var output3 = new SolverOutput(Variables, Objective, model, model.Runtime);
            output3.WriteToCSV(@"C:\Users\4001184\Desktop\temp2\", "3" + fraction);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", "max:" + Objective.AltObjective.Value.ToString() + " " + output3.GurobiCost.ToString() + "\n");
            return new SolverOutput(Variables, Objective, model, model.Runtime); ;
        }

        public SolverOutput Solve(int TimeLimit, int v)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Parameters.Method = v;
            model.Optimize();
            return new SolverOutput(Variables, Objective, model, model.Runtime);
        }

        public SolverOutput GetTransmissionOutput(int TimeLimit)
        {
            model.Parameters.LazyConstraints = 1;

            //string filename = @"C:\Users\4001184\Desktop\gurobi.lp";
            //model.Write(filename);
            //Process myProcess = new Process();
            //Process.Start("notepad++.exe", filename);
            //Console.ReadLine();
            model.Parameters.TimeLimit = TimeLimit;

            model.SetCallback(new CallBackTrans(PS, CC, model, Variables, PBC.NodalResidualDemand, PBC.Boven));
            model.Optimize();
            //  saved.Print();
            //new Output(this);

            return new SolverOutput(Variables, Objective, model, model.Runtime);
        }

        //  public SolverOutput TransFix(int TimeLimit) { }
    }
}
