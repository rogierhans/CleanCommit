using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace UtrechtCommitment
{
    class Solver
    {

        PowerSystem PS;
        Solution Solution;
        GRBEnv env;
        GRBModel model;
        GRBVar[,] UnitCommitmentVars;
        GRBVar[,] UnitCommitmentStartVars;
        GRBVar[,] UnitCommitmentStopVars;
        GRBVar[,] DispatchsVars;
        private int totalTime;
        private int totalUnits;
        public Solver(PowerSystem ps, Solution s)
        {
            PS = ps;
            Solution = s;

            env = new GRBEnv();
            model = new GRBModel(env);
            UnitCommitmentVars = new GRBVar[PS.Times.Count, PS.Units.Count];
            UnitCommitmentStartVars = new GRBVar[PS.Times.Count, PS.Units.Count];
            UnitCommitmentStopVars = new GRBVar[PS.Times.Count, PS.Units.Count];
            DispatchsVars = new GRBVar[PS.Times.Count, PS.Units.Count];

            totalTime = PS.Times.Count;
            totalUnits = PS.Units.Count;

            //GRBCallback cb = new Grb
            //model.SetCallback();
        }



        public void ConfigureModel()
        {
            AddVariables();
            AddConstraints();
            AddObjective();
        }

        public void AddVariables()
        {
            for (int t = 0; t < totalTime; t++)
            {

                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    UnitCommitmentVars[t, u] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "u" + u + "t" + t);
                    UnitCommitmentStartVars[t, u] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "u" + u + "t" + t);
                    UnitCommitmentStopVars[t, u] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "u" + u + "t" + t);
                    DispatchsVars[t, u] = model.AddVar(0, unit.PMax, 0.0, GRB.CONTINUOUS, "p" + u + "t" + t);

                }
            }
        }

        //GRBConstr[,] UnitCommitmentStatus;

        public void SetVariable(bool[,] CommitStatus)
        {
            //UnitCommitmentStatus = new GRBConstr[totalTime, totalUnits];
            for (int t = 0; t < totalTime; t++)
            {

                for (int u = 0; u < totalUnits; u++)
                {
                    float commitstatus = CommitStatus[t, u] ? 1 : 0;
                    UnitCommitmentVars[t, u].LB = commitstatus;
                    UnitCommitmentVars[t, u].UB = commitstatus;
                    //UnitCommitmentStatus[t, u] = model.AddConstr(commitstatus == UnitCommitmentVars[t, u],"Commit"  + t + " "+ u);
                }
            }
        }
        public void ChangeVariable(int t, int u, bool status)
        {
            float commitstatus = status ? 1 : 0;
            UnitCommitmentVars[t, u].LB = commitstatus;
            UnitCommitmentVars[t, u].UB = commitstatus;
        }
        GRBLinExpr Objective;
        public void AddObjective()
        {
            Objective = new GRBLinExpr();
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    if (t == 0)
                    {
                        Objective += (UnitCommitmentStartVars[t, u]) * unit.HC;
                    }
                    if (t > 0)
                    {

                        Objective += UnitCommitmentStartVars[t, u] * unit.HC;
                    }
                    Objective += unit.A * UnitCommitmentVars[t, u] + DispatchsVars[t, u] * unit.B;
                }
            }
            model.SetObjective(Objective, GRB.MINIMIZE);
        }

        public void AddConstraints()
        {
            AddRampingConstraints();
            AddGenerationConstraints();
            AddPowerConstraints();
            PowerPlantLogicConstraint();
        }

        private void PowerPlantLogicConstraint()
        {
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    if (t == 0)
                    {
                        var ppLogic = 0 - UnitCommitmentVars[t, u] + UnitCommitmentStartVars[t, u] + UnitCommitmentStopVars[t, u] == 0;
                        model.AddConstr(ppLogic, "Power Plat Logic" + t + " " + u);
                    }
                    else if (t != 0)
                    {
                        var ppLogic = UnitCommitmentVars[t - 1, u] - UnitCommitmentVars[t, u] + UnitCommitmentStartVars[t, u] + UnitCommitmentStopVars[t, u] == 0;
                        model.AddConstr(ppLogic, "Power Plat Logic" + t + " " + u);
                    }
                }
            }
        }

        GRBConstr[] PowerBalance;
        private void AddPowerConstraints()
        {
            PowerBalance = new GRBConstr[totalTime];
            for (int t = 0; t < totalTime; t++)
            {
                Time time = PS.Times[t];
                GRBLinExpr Load = new GRBLinExpr();
                for (int u = 0; u < totalUnits; u++)
                {
                    Load += DispatchsVars[t, u];
                }
                PowerBalance[t] = model.AddConstr(Load == time.Demand, "PowerBalance" + t);
            }
        }

        GRBConstr[,] GenerationConstrMin;
        GRBConstr[,] GenerationConstrMax;
        private void AddGenerationConstraints()
        {
            GenerationConstrMin = new GRBConstr[totalTime, totalUnits];
            GenerationConstrMax = new GRBConstr[totalTime, totalUnits];
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    var dispatch = DispatchsVars[t, u];
                    var upLimit = UnitCommitmentVars[t, u] * unit.PMax;
                    var downLimit = UnitCommitmentVars[t, u] * unit.PMin;
                    GenerationConstrMax[t, u] = model.AddConstr(dispatch <= upLimit, "cp" + u + "t" + t);
                    GenerationConstrMax[t, u] = model.AddConstr(downLimit <= dispatch, "cp" + u + "t" + t);
                }
            }
        }

        GRBConstr[,] UpwardRampingConstr;
        GRBConstr[,] DownwardRampingConstr;
        private void AddRampingConstraints()
        {
            UpwardRampingConstr = new GRBConstr[totalTime, totalUnits];
            DownwardRampingConstr = new GRBConstr[totalTime, totalUnits];
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {
                    Unit unit = PS.Units[u];
                    if (t > 0)
                    {
                        var deltaDispatch = DispatchsVars[t, u] - DispatchsVars[t - 1, u];




                        var downwardRampingLimit = unit.RampDown * UnitCommitmentVars[t, u] + (unit.ShutDown * UnitCommitmentStopVars[t,u]);
                        DownwardRampingConstr[t, u] = model.AddConstr(-deltaDispatch <= downwardRampingLimit, "r" + u + "t" + t);


                        var upwardRampingLimit = unit.RampUp * UnitCommitmentVars[t, u] + (unit.StartUp * UnitCommitmentStartVars[t, u]);
                        UpwardRampingConstr[t, u] = model.AddConstr(deltaDispatch <= upwardRampingLimit, "r" + u + "t" + t);
                        model.Remove(UpwardRampingConstr[t, u]);
                    }
                }
            }
        }

        public double Solve()
        {


            model.Optimize();
            if (model.Status == 3) return Double.MaxValue;
            return Objective.Value;
            List<string> lines = new List<string>();
            for (int u = 0; u < totalUnits; u++)
            {
                string line = "";
                Console.WriteLine();
                for (int t = 0; t < totalTime; t++)
                {
                    line += DispatchsVars[t, u].X + " \t; ";
                }
                lines.Add(line);
            }

            //File.WriteAllLines(@"C:\Users\Rogier\Desktop\UCTest\output.csv", lines);
        }
    }
}
