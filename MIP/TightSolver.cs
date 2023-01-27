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
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", "0.00001");
        }
        public TightSolver(PowerSystem ps, ConstraintConfiguration cc, double gap)
        {

            PS = ps;
            CC = cc;
            env = new GRBEnv();
            model = new GRBModel(env);
            model.Set("DisplayInterval", "1");
            model.Set("MIPGap", gap.ToString());
        }


        public virtual void ConfigureModel()
        {
            Console.WriteLine("Variables...");
            Variables = new Variables(PS, CC, model);
            Variables.IntialiseVariables();

            Console.WriteLine("Objective...");
            Objective = new Objective(PS, CC, model, Variables);
            Objective.AddObjective();

            Console.WriteLine("AddConstraints...");
            AddConstraints();
        }

        PowerBalanceContraint PBC;
        TransmissionConstraint TC;

        public virtual void AddConstraints()
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

        public Solution CFOptimzation(int TimeLimit, double fraction, string generatorType)
        {
            string RootFolder = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\CFMax\";
            string Folder = RootFolder + generatorType;
            Directory.CreateDirectory(Folder);

            //Solve Original Model
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            var solution1 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            double totalGenerationForSpecifiedType = SolutionAnalyssis.GetTotalGeneration(solution1, generatorType);
            solution1.ToCSV(Folder + @"\OG_" + PS);

            // optimize <generatorType> CF while staying <fraction> within the original model objective value
            model.AddConstr(Objective.CurrentObjective <= solution1.GurobiCost * fraction, "");
            Objective.AddAlternativeObjective(generatorType, GRB.MINIMIZE);
            model.Optimize();
            var solution2 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            solution2.ToCSV(Folder + @"\" + GRB.MINIMIZE + "_" + PS);
            double totalGenerationForSpecifiedType2 = SolutionAnalyssis.GetTotalGeneration(solution2, generatorType);



            Objective.AddObjective();
            model.Optimize();


            Objective.AddAlternativeObjective(generatorType, GRB.MAXIMIZE);
            model.Optimize();
            var solution3 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            solution3.ToCSV(Folder + @"\" + GRB.MAXIMIZE + "_" + PS);
            double totalGenerationForSpecifiedType3 = SolutionAnalyssis.GetTotalGeneration(solution3, generatorType);
            int year = int.Parse(PS.ToString().Split('_')[2]);
            DateTime dt = new DateTime(year, 1, 1);
            dt = dt.AddHours(CC.TimeOffSet);
            File.AppendAllText(RootFolder + @"\text.txt",
               generatorType + "\t"
                + fraction + "\t"
                + PS + "\t"
                + year + "\t" +
                +CC.TimeOffSet + "\t"
                + dt.ToString() + "\t"
                + solution2.ComputationTime + "\t"
                + solution3.ComputationTime + "\t"
                + totalGenerationForSpecifiedType + "\t"
                + totalGenerationForSpecifiedType2 + "\t"
                + totalGenerationForSpecifiedType3 + "\t"
                + totalGenerationForSpecifiedType2 / totalGenerationForSpecifiedType3 + "\t"
                + solution1.GurobiCost + "\t"
                + solution2.GurobiCost + "\t"
                + solution3.GurobiCost + "\t"
                + "\n");

            return solution2;
        }

        public Solution LOLHOptimzation(int TimeLimit, double fraction, string folderName, Action<Objective> a)
        {
            string extraComment = "";
            string RootFolder = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\2022Results\extra14dagen\" + folderName + @"\";
            string Folder = RootFolder;
            Directory.CreateDirectory(Folder);

            //Solve Original Model
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            Solution solution1 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            solution1.ToCSV(Folder + @"\OG_" + PS + "_" + CC.TimeOffSet);
            Solution solution2 = solution1;
            extraComment = "skip";
            if (solution2.LOLCounter > 0)
            {
                // optimize <generatorType> CF while staying <fraction> within the original model objective value
                model.AddConstr(Objective.CurrentObjective <= Math.Ceiling(solution1.GurobiCost * fraction), "");
                a(Objective);
                model.Optimize();
                extraComment = model.Status.ToString();
                if (model.Status == GRB.Status.NUMERIC)
                {
                    extraComment += "_nummeric";
                }
                else if (model.Status == GRB.Status.OPTIMAL)
                    solution2 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            }
            solution2.ToCSV(Folder + @"\" + GRB.MINIMIZE + "_" + PS + "_" + CC.TimeOffSet);



            int year = int.Parse(PS.ToString().Split('_')[2]);
            DateTime dt = new DateTime(year, 1, 1);
            dt = dt.AddHours(CC.TimeOffSet);
            File.AppendAllText(RootFolder + @"\text.txt",
                +fraction + "\t"
                + PS + "\t"
                + year + "\t" +
                +CC.TimeOffSet + "\t"
                + dt.ToString() + "\t"
                + solution2.ComputationTime + "\t"
                + solution1.GurobiCost + "\t"
                + solution2.GurobiCost + "\t"
                + solution1.LOLCounter + "\t"
                + solution2.LOLCounter + "\t"
                + extraComment + "\t"
                + "\n");

            return solution2;
        }


        public SolverOutput CO2Optimzation(int TimeLimit, double fraction, int GRBMINMAXMode)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Optimize();
            Objective.OnlyAnalysesCO2();
            var output = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", Objective.AltObjective.Value.ToString() + " " + output.GurobiCost.ToString() + "\n");
            model.AddConstr(Objective.CurrentObjective <= output.GurobiCost * fraction, "");
            new SolverOutput(Variables, Objective, model, model.Runtime).WriteToCSV(@"C:\Users\4001184\Desktop\temp2\", "1" + fraction);
            Objective.AddCO2Objective(GRBMINMAXMode);
            model.Optimize();
            var output2 = new Solution(model, Objective, Variables, PS, CC, TC, PBC);
            output2.ToCSV(@"C:\Users\4001184\Desktop\temp2\2_" + fraction + "_");
            File.AppendAllText(@"C:\Users\4001184\Desktop\text.txt", GRBMINMAXMode + ":" + Objective.AltObjective.Value.ToString() + " " + output2.GurobiCost.ToString() + "\n");
            return new SolverOutput(Variables, Objective, model, model.Runtime); ;
        }

        public SolverOutput Solve(int TimeLimit, int methodCode)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Parameters.Method = methodCode;
            model.Optimize();
            return new SolverOutput(Variables, Objective, model, model.Runtime);
        }

        public Solution NewSolve(int TimeLimit, int methodCode)
        {
            model.Parameters.TimeLimit = TimeLimit;
            model.Parameters.Method = methodCode;
            model.Optimize();
            return new Solution(model, Objective, Variables, PS, CC, TC, PBC);
        }
    }
}
