using CleanCommit.MIP;
using CleanCommit.Instance;
using Gurobi;
using System.Diagnostics;
using System;
namespace CleanCommit
{
    class TightSolver
    {

        public PowerSystem PS;
        protected GRBEnv env;
        public GRBModel model;
        public Variables Variables;
        public ConstraintConfiguration CC;
        public Objective Objective;

        public TightSolver(PowerSystem ps, ConstraintConfiguration cc)
        {

            PS = ps;
            CC = cc;
            env = new GRBEnv();
            model = new GRBModel(env);

            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.001");
            model.Set("Method", "1");
            //model.Set("IntFeasTol", "0.000000001");
        }
        public TightSolver(PowerSystem ps, ConstraintConfiguration cc, double gap)
        {

            PS = ps;
            CC = cc;
            env = new GRBEnv();
            model = new GRBModel(env);

            //model.Set("LogFile", @"C:\Users\4001184\Desktop\Glog.txt");
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", gap.ToString());
            //model.Set("IntFeasTol", "0.000000001");
        }

        public void SetOutputFlag(bool OutputFlag)
        {
            model.Set("OutputFlag", OutputFlag ? "1" : "0");
        }

        public virtual void ConfigureModel()
        {
            Variables = new Variables(PS, CC, model, this);
            Variables.IntialiseVariables();
            Objective = new Objective(PS, CC, model, this, Variables);
            Objective.AddObjective();
            AddConstraints();
        }

        TightGenerationConstraint saved;
        public virtual void AddConstraints()
        {
            //if (CC.SuperTight)
            {
                saved = new TightGenerationConstraint(PS, CC, model, Variables);
                saved.AddConstraint();
            }
            var GenerationConstraint = new GenerationConstraint(PS, CC, model, Variables);
            GenerationConstraint.AddConstraint();
            var RampingConstraint = new RampConstraint(PS, CC, model, Variables);
            RampingConstraint.AddConstraint();
            var PiecewiseConstraint = new PiecewiseConstraint(PS, CC, model, Variables);
            PiecewiseConstraint.AddConstraint();
            var TransmissionConstraint = new TransmissionConstraint(PS, CC, model, Variables);
            TransmissionConstraint.AddConstraint();
            var PowerBalanceContraint = new PowerBalanceContraint(PS, CC, model, Variables);
            PowerBalanceContraint.AddConstraint();
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
        public SolverOutput Solve2(int TimeLimit)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            //  saved.Print();
            //new Output(this);
            return new SolverOutput(Variables, Objective, model, model.Runtime);
        }

        public double[,] GetTransmissionOutput(int TimeLimit)
        {
            string filename = @"C:\Users\4001184\Desktop\gurobi.lp";
            model.Write(filename);
            Process myProcess = new Process();
            Process.Start("notepad++.exe", filename);
            Console.ReadLine();
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            //  saved.Print();
            //new Output(this);

            var solveroutput = new SolverOutput(Variables, Objective, model, model.Runtime);
            return solveroutput.NodalInjection;
        }
    }
}
