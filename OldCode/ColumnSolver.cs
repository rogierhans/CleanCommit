//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Gurobi;

//namespace UtrechtCommitment
//{
//    class ColumnSolver : Solver
//    {

//        bool MinUpDownTime;
//        public ColumnSolver(PowerSystem PS) : base(PS)
//        {
//            MinUpDownTime = PS.ConstraintConfiguration.MinUpMinDown;
//        }
//        List<GRBVar>[] GPVariables;
//        List<GenerationPlan>[] GenerationPlans;
//        public override void AddVariables()
//        {
//            base.AddVariables();

//            GPVariables = new List<GRBVar>[totalUnits];
//            GenerationPlans = new List<GenerationPlan>[totalUnits];
//            for (int u = 0; u < totalUnits; u++)
//            {
//                GPVariables[u] = new List<GRBVar>();
//                GenerationPlans[u] = new List<GenerationPlan>();
//                //hmm overbodig maar moet van Gurobi
//                var ColumnVar = model.AddVar(0.0, 1.0, 0.0, GRB.CONTINUOUS, "plan 0" + u);
//                var gp = new GenerationPlan(u, totalTime);

//                GPVariables[u].Add(ColumnVar);
//                GenerationPlans[u].Add(gp);

//            }
//        }

//        public override void AddObjective()
//        {
//            base.AddObjective();
//        }

//        public override void AddConstraints()
//        {
//            AddGenerationPlanConstraints();
//            base.AddConstraints();

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
//                    GenerationConstrMax[t, u] = model.AddConstr(dispatch <= 0, "cp" + u + "t" + t);
//                    GenerationConstrMin[t, u] = model.AddConstr(-dispatch <= 0, "cp" + u + "t" + t);
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

//                        DownwardRampingConstr[t, u] = model.AddConstr(-deltaDispatch <= 0, "r" + u + "t" + t);

//                        UpwardRampingConstr[t, u] = model.AddConstr(deltaDispatch <= 0, "r" + u + "t" + t);

//                    }
//                }
//            }
//        }
//        GRBConstr[] GenerationPlanConstr;
//        protected void AddGenerationPlanConstraints()
//        {
//            GenerationPlanConstr = new GRBConstr[totalUnits];

//            for (int u = 0; u < totalUnits; u++)
//            {
//                GenerationPlanConstr[u] = model.AddConstr(GPVariables[u][0] <= 1, "plan" + u);
//            }
//        }
//        int counter = 0;
//        private void AddColumn(GenerationPlan gp)
//        {
//            var unit = PS.Units[gp.UnitIndex];
//            GRBColumn column = new GRBColumn();

//            //Calclute and store the coeffients to generation min and max prodcution
//            for (int t = 0; t < totalTime; t++)
//            {
//                double coefficient = gp.CommitmentStatus[t] * unit.Generation.PMin;
//                column.AddTerm(coefficient, GenerationConstrMin[t, gp.UnitIndex]);


//            }
//            for (int t = 0; t < totalTime; t++)
//            {
//                double coefficient = -gp.CommitmentStatus[t] * unit.Generation.PMax;
//                column.AddTerm(coefficient, GenerationConstrMax[t, gp.UnitIndex]);

//            }

//            for (int t = 1; t < totalTime; t++)
//            {

//                double coefficient = -gp.CommitmentStatus[t - 1] * unit.Cycle.RampDown - gp.StopStatus[t] * (unit.Cycle.ShutDown - unit.Cycle.RampDown);
//                column.AddTerm(coefficient, DownwardRampingConstr[t, gp.UnitIndex]);

//            }
//            for (int t = 1; t < totalTime; t++)
//            {
//                double coefficient = -gp.CommitmentStatus[t] * unit.Cycle.RampUp - gp.StartStatus[t] * (unit.Cycle.StartUp - unit.Cycle.RampUp);
//                column.AddTerm(coefficient, UpwardRampingConstr[t, gp.UnitIndex]);

//            }


//            column.AddTerm(1.0, GenerationPlanConstr[gp.UnitIndex]);

//            //for (int i = 0; i < column.Size; i++)
//            //{
//            //    Console.WriteLine("{0} = {1}", column.GetConstr(i).ConstrName, column.GetCoeff(i));
//            //}
//            //Console.ReadLine();

//            GRBVar newVar = model.AddVar(0.0, double.MaxValue, gp.CommitmentStatus.Sum() * unit.Generation.A + gp.StartStatus.Sum() * unit.Cycle.StartCost, GRB.CONTINUOUS, column, "GPvar " + gp.UnitIndex + counter++);
//            GPVariables[gp.UnitIndex].Add(newVar);
//            GenerationPlans[gp.UnitIndex].Add(gp);
//        }


//        public void PrintReducedCost()
//        {
//            //model.Parameters.OutputFlag = 0;
//            while (true)
//            {
//                //PrintPi();
//                List<GenerationPlan> plans = new List<GenerationPlan>();
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    var unit = PS.Units[u];

//                    var newGenerationPlan = PricingProblem(u);
//                    plans.Add(newGenerationPlan);

//                }


//                var bestPlan = plans.Aggregate((a, b) => a.ReducedCost < b.ReducedCost ? a : b);

//                Console.WriteLine("unit={0} {1}", bestPlan.UnitIndex, bestPlan.CommitmentStatus.ToList().Select(x => x.ToString()).Aggregate((a, b) => a + b));

//                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$Variable added newcoloum$$$$$$$$$$$$$$$$$$$$$$$$$$");
//                // Console.ReadLine();
//                PrintUnitCommitment();

//                plans = plans.Where(plan => plan.ReducedCost < -0.01).ToList();
//                Console.WriteLine("NUMBER OF PLAN {0}",plans.Count);
//                //plans.ForEach(p => p.PrintPlan());
//                //var bestPlann = plans.Aggregate((a, b) => a.ReducedCost < b.ReducedCost ? a : b);


//                //if(bestPlann.ReducedCost > -0.01)
//                if (plans.Count == 0)
//                {
//                Console.WriteLine("Done");
//                    Console.ReadLine();
//                    GPVariables.ToList().ForEach(var => var.ForEach(grbvar => grbvar.VType = GRB.INTEGER));
//                    Solve(982032);
//                    Console.ReadLine();
//                }
//                //AddColumn(bestPlann);
//                plans.ForEach(x => AddColumn(x));// AddColumn(bestPlan);

//                //
//                model.Update();
//                Solve(982032);
//            }
//        }

//        public void PrintPlan()
//        {
//            for (int u = 0; u < totalUnits; u++)
//            {
//                Console.WriteLine("unit={0}", u);
//                var unit = PS.Units[u];
//                for (int i = 0; i < GPVariables[u].Count(); i++)
//                {
//                    var plan = GenerationPlans[u][i].CommitmentStatus.ToList().Select(x => x.ToString()).Aggregate((a, b) => a + b);
//                    Console.WriteLine("{0} \t {1} ", GPVariables[u][i].X, plan);
//                }
//                Console.WriteLine(GenerationPlanConstr[u].Slack);
//            }
//        }

//        public void PrintPi()
//        {
//            for (int u = 0; u < 1; u++)
//            {

//                for (int t = 1; t < totalTime; t++)
//                {
//                    Console.WriteLine("u{0}t{1}, minsp:{2} \t maxsp:{3} \t smin {4} \t smax {5}", u, t, GenerationConstrMin[t, u].Pi, GenerationConstrMax[t, u].Pi, GenerationConstrMin[t, u].Slack, GenerationConstrMax[t, u].Slack);
//                    Console.WriteLine("u{0}t{1}, downsp:{2} \t upsp:{3} \t sdown {4} \t sup {5}", u, t, DownwardRampingConstr[t, u].Pi, UpwardRampingConstr[t, u].Pi, DownwardRampingConstr[t, u].Slack, UpwardRampingConstr[t, u].Slack);

//                }
//                Console.WriteLine("plan pi={0}, slack={1}", GenerationPlanConstr[u].Pi, GenerationPlanConstr[u].Slack);
//                Console.WriteLine(GenerationPlanConstr[u].CBasis);
//                foreach (var varE in GPVariables[u])
//                {
//                    Console.WriteLine(varE.X);
//                }
//            }
//            Console.ReadLine();
//        }

//        public void PrintUnitCommitment()
//        {
//            for (int u = 0; u < totalUnits; u++)
//            {
//                var unit = PS.Units[u];
//                double[] summation = new double[totalTime];
//                for (int gp = 0; gp < GPVariables[u].Count; gp++)
//                {
//                    var column = GPVariables[u][gp];
//                    if (column.X > 0)
//                    {

//                        var plan = GenerationPlans[u][gp];
//                        summation = Scalar(plan.CommitmentStatus, column.X);
//                        string line = "";
//                        for (int t = 0; t < summation.Length; t++)
//                        {
//                            line += "\t" + Math.Round(summation[t], 3);
//                        }
//                        Console.WriteLine(line);
//                        //var column = GPVariables[u][gp];
//                        //var plan = GenerationPlans[u][gp];
//                        //summation = Sum(summation, Scalar(plan.CommitmentStatus, column.X));
//                    }
//                }
//                Console.WriteLine(u);
//                //string line = "";
//                //for (int t = 0; t < totalTime; t++)
//                //{
//                //    line += "\t" + Math.Round(summation[t], 3);
//                //}

//            }
//        }

//        public double[] Sum(double[] first, double[] second)
//        {
//            double[] third = new double[first.Length];
//            for (int i = 0; i < third.Length; i++)
//            {
//                third[i] = first[i] + second[i];

//            }
//            return third;
//        }
//        public double[] Scalar(int[] first, double scalar)
//        {
//            double[] third = new double[first.Length];
//            for (int i = 0; i < third.Length; i++)
//            {
//                third[i] = first[i] * scalar;

//            }
//            return third;
//        }

//        //Tuple<int[],int[]>
//        public GenerationPlan PricingProblem(int unitIndex)
//        {

//            //var g = new Graph(PS,unitIndex,GenerationConstrMax,GenerationConstrMin,UpwardRampingConstr,DownwardRampingConstr,GenerationPlanConstr);
            
//            var unit = PS.Units[unitIndex];
//            double[] CommitCost = new double[totalTime];
//            double[] StartCost = new double[totalTime];
//            double[] StopCost = new double[totalTime];

//            for (int t = 0; t < totalTime; t++)
//            {

//                CommitCost[t] = (GenerationConstrMax[t, unitIndex].Pi * unit.Generation.PMax)
//                    - (GenerationConstrMin[t, unitIndex].Pi * unit.Generation.PMin)
//                    + unit.Generation.A;
//                StartCost[t] = unit.Cycle.StartCost;
//            }

//            for (int t = 1; t < totalTime; t++)
//            {
//                CommitCost[t] += (unit.Cycle.RampUp * UpwardRampingConstr[t, unitIndex].Pi);
//                CommitCost[t - 1] += (DownwardRampingConstr[t, unitIndex].Pi * unit.Cycle.RampDown);
//                StartCost[t] += UpwardRampingConstr[t, unitIndex].Pi * (unit.Cycle.StartUp - unit.Cycle.RampUp);
//                StopCost[t] += DownwardRampingConstr[t, unitIndex].Pi * (unit.Cycle.ShutDown - unit.Cycle.RampDown);
//            }

//            // This algorithm can be improved from |T| * (MinUpTime + MinDownTime) to 2 |T| but requires some bookkeeping
//            // States <MinUpTime and <MinDownTime are redundant
//            // Only try to implement when this is the bottleneck
//            var CommitMatrix = new double[unit.Cycle.MinUpTime, totalTime];
//            var DeCommitMatrix = new double[unit.Cycle.MinDownTime, totalTime];

//            //CommitMatrix[unit.Cycle.MinUpTime - 1, 0] = double.MaxValue;
//            CommitMatrix[0, 0] = CommitCost[0] + StartCost[0] - GenerationPlanConstr[unitIndex].Pi;
//            for (int upTime = 1; upTime < unit.Cycle.MinUpTime; upTime++)
//            {
//                CommitMatrix[upTime, 0] = double.MaxValue;
//            }

//            //DeCommitMatrix[unit.Cycle.MinDownTime - 1, 0] = double.MaxValue;
//            DeCommitMatrix[0, 0] = 0 - GenerationPlanConstr[unitIndex].Pi;
//            for (int downTime = 1; downTime < unit.Cycle.MinDownTime; downTime++)
//            {
//                DeCommitMatrix[downTime, 0] = double.MaxValue;
//            }


//            for (int t = 1; t < totalTime; t++)
//            {
//                //for commitperiod >= minmumUptime
//                if (CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1] < CommitMatrix[unit.Cycle.MinUpTime - 2, t - 1])
//                {
//                    CommitMatrix[unit.Cycle.MinUpTime - 1, t] = CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1] + CommitCost[t];
//                }
//                else
//                {
//                    CommitMatrix[unit.Cycle.MinUpTime - 1, t] = CommitMatrix[unit.Cycle.MinUpTime - 2, t - 1] + CommitCost[t];
//                }

//                CommitMatrix[0, t] = DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1] + CommitCost[t] + StartCost[t];

//                for (int upTime = 1; upTime < unit.Cycle.MinUpTime - 1; upTime++)
//                {
//                    CommitMatrix[upTime, t] = CommitMatrix[upTime - 1, t - 1] + CommitCost[t];
//                }


//                if (DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1] < DeCommitMatrix[unit.Cycle.MinDownTime - 2, t - 1])
//                {
//                    DeCommitMatrix[unit.Cycle.MinDownTime - 1, t] = DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1];
//                }
//                else
//                {
//                    DeCommitMatrix[unit.Cycle.MinDownTime - 1, t] = DeCommitMatrix[unit.Cycle.MinDownTime - 2, t - 1];
//                }

//                DeCommitMatrix[0, t] = CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1] + StopCost[t];
//                for (int downTime = 1; downTime < unit.Cycle.MinDownTime - 1; downTime++)
//                {
//                    DeCommitMatrix[downTime, t] = DeCommitMatrix[downTime - 1, t - 1];
//                }
//            }


//            //Recover the plan

//            //for (int t = 0; t < totalTime; t++)
//            //{
//            //    for (int upTime = 0; upTime < unit.Cycle.MinUpTime; upTime++)
//            //    {
//            //        Console.WriteLine("Com t={0} \t time={1} \t score={2}", t, upTime, CommitMatrix[upTime, t]);
//            //    }
//            //    for (int downTime = 0; downTime < unit.Cycle.MinDownTime; downTime++)
//            //    {
//            //        Console.WriteLine("Dec t={0} \t time={1} \t score={2}", t, downTime, DeCommitMatrix[downTime, t]);
//            //    }
//            //    Console.ReadLine();
//            //}

//            var newPlan = RecoverPlan(unitIndex, CommitMatrix, DeCommitMatrix);


//            //double sum = - GenerationPlanConstr[unitIndex].Pi;
//            //for (int t = 0; t < totalTime; t++)
//            //{
//            //    sum += CommitCost[t] * newPlan.CommitmentStatus[t] + StartCost[t] * newPlan.StartStatus[t] + StopCost[t] * newPlan.StopStatus[t];
//            //}
//            //Console.WriteLine("DP={0} RC={1} delta={2}", newPlan.ReducedCost, sum, newPlan.ReducedCost - sum);

//            //{
//            //    for (int t = 0; t < totalTime; t++)
//            //    {
//            //        Console.WriteLine("t={0} \t commit={1} \t start={2} \t stop={3} \t pi={4}", t, Math.Round(CommitCost[t], 3), Math.Round(StartCost[t], 3), Math.Round(StopCost[t], 3), GenerationPlanConstr[unitIndex].Pi);

//            //        string line = "";
//            //        for (int downTime = 0; downTime < unit.Cycle.MinDownTime; downTime++)
//            //        {
//            //            line += DeCommitMatrix[downTime, t] + "\t";
//            //        }
//            //        Console.WriteLine(line);
//            //        line = t + "";
//            //        for (int upTime = 1; upTime < unit.Cycle.MinUpTime; upTime++)
//            //        {
//            //            line += CommitMatrix[upTime, t] + "\t";
//            //        }
//            //        Console.WriteLine(line);

//            //    }
//            //    if (Math.Abs(newPlan.ReducedCost - sum) > 0.01)
//            //        Console.ReadLine();
//            //}
//            //Console.WriteLine("DP={0} RC={1} delta={2}", newPlan.ReducedCost, sum, newPlan.ReducedCost - sum);
//            ////  Console.ReadLine();
//            return newPlan;

//            //for (int t = 0; t < totalTime; t++)
//            //{
//            //    Console.WriteLine("t={0} \t commit={1} \t start={2} \t stop={3}", t, Math.Round(CommitCost[t], 3), Math.Round(StartCost[t], 3), Math.Round(StopCost[t], 3));
//            //}

//            //Console.ReadLine();


//            //int[] commitVariables = new int[totalTime];
//            //int[] startVariable = new int[totalTime];

//            //return new Tuple<int[], int[]>(commitVariables, startVariable);
//        }

//        //private GenerationPlan AltPPOnGraph(int unitIndex)
//        //{

//        //    return new GenerationPlan(0, 0);
//        //    var unit = PS.Units[unitIndex];
//        //    double[] CommitCost = new double[totalTime];
//        //    double[] StartCost = new double[totalTime];
//        //    double[] StopCost = new double[totalTime];

//        //    for (int t = 0; t < totalTime; t++)
//        //    {

//        //        CommitCost[t] = (GenerationConstrMax[t, unitIndex].Pi * unit.Generation.PMax)
//        //            - (GenerationConstrMin[t, unitIndex].Pi * unit.Generation.PMin)
//        //            + unit.Generation.A;
//        //        StartCost[t] = unit.Cycle.StartCost;
//        //    }

//        //    for (int t = 1; t < totalTime; t++)
//        //    {
//        //        CommitCost[t] += (unit.Cycle.RampUp * UpwardRampingConstr[t, unitIndex].Pi);
//        //        CommitCost[t - 1] += (DownwardRampingConstr[t, unitIndex].Pi * unit.Cycle.RampDown);
//        //        StartCost[t] += UpwardRampingConstr[t, unitIndex].Pi * (unit.Cycle.StartUp - unit.Cycle.RampUp);
//        //        StopCost[t] += DownwardRampingConstr[t, unitIndex].Pi * (unit.Cycle.ShutDown - unit.Cycle.RampDown);
//        //    }

//        //}

//        private GenerationPlan RecoverPlan(int unitIndex, double[,] CommitMatrix, double[,] DeCommitMatrix)
//        {
//            string route  = "";
//            var unit = PS.Units[unitIndex];
//            var commitmentStatus = new int[totalTime];

//            int bestScoreIndex = -1;
//            double bestScore = double.MaxValue;
//            bool bestTypeIsCommited = true;
//            for (int upTime = 0; upTime < unit.Cycle.MinUpTime; upTime++)
//            {
//                if (CommitMatrix[upTime, totalTime - 1] < bestScore)
//                {
//                    bestScoreIndex = upTime;
//                    bestScore = CommitMatrix[upTime, totalTime - 1];
//                    bestTypeIsCommited = true;
//                }
//            }

//            for (int downTime = 0; downTime < unit.Cycle.MinDownTime; downTime++)
//            {
//                if (DeCommitMatrix[downTime, totalTime - 1] < bestScore)
//                {
//                    bestScoreIndex = downTime;
//                    bestScore = DeCommitMatrix[downTime, totalTime - 1];
//                    bestTypeIsCommited = false;
//                }
//            }
            

//            for (int t = totalTime - 1; t >= 0; t--)
//            {
//                route = bestScoreIndex +" " + route;
//                if (bestTypeIsCommited)
//                {
//                    commitmentStatus[t] = 1;
//                    if (t == 0) break;
//                    if (bestScoreIndex == unit.Cycle.MinUpTime - 1)
//                    {
//                        if (CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1] >= CommitMatrix[unit.Cycle.MinUpTime - 2, t - 1])
//                        {
//                            bestScoreIndex--;
//                        }
//                    }
//                    else if (bestScoreIndex == 0)
//                    {
//                        bestScoreIndex = unit.Cycle.MinDownTime - 1;
//                        bestTypeIsCommited = !bestTypeIsCommited;
//                    }
//                    else
//                    {
//                        bestScoreIndex--;
//                    }
//                }
//                else
//                {
//                    commitmentStatus[t] = 0;
//                    if (t == 0) break;
//                    if (bestScoreIndex == unit.Cycle.MinDownTime - 1)
//                    {
//                        if (DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1] >= DeCommitMatrix[unit.Cycle.MinDownTime - 2, t - 1])
//                        {
//                            bestScoreIndex--;
//                        }
//                    }
//                    else if (bestScoreIndex == 0)
//                    {
//                        bestScoreIndex = unit.Cycle.MinUpTime - 1;
//                        bestTypeIsCommited = !bestTypeIsCommited;
//                    }
//                    else
//                    {
//                        bestScoreIndex--;
//                    }
//                }
//            }
//            Console.WriteLine("route = {0}", route);
//            //for (int t = 1; t < totalTime; t++)
//            //{
//            //    //for commitperiod >= minmumUptime
//            //    if (CommitMatrix[0, t - 1] < CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1])
//            //    {
//            //        CommitMatrix[0, t] = CommitMatrix[0, t - 1] + CommitCost[t];
//            //    }
//            //    else
//            //    {
//            //        CommitMatrix[0, t] = CommitMatrix[unit.Cycle.MinUpTime - 1, t - 1] + CommitCost[t];
//            //    }

//            //    CommitMatrix[1, 0] = DeCommitMatrix[0, t - 1] + CommitCost[t] + StartCost[t];
//            //    for (int upTime = 2; upTime < unit.Cycle.MinUpTime; upTime++)
//            //    {
//            //        CommitMatrix[upTime, t] = CommitMatrix[upTime - 1, t - 1] + CommitCost[t];
//            //    }


//            //    if (DeCommitMatrix[0, t - 1] < DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1])
//            //    {
//            //        DeCommitMatrix[0, t] = DeCommitMatrix[0, t - 1];
//            //    }
//            //    else
//            //    {
//            //        DeCommitMatrix[0, t] = DeCommitMatrix[unit.Cycle.MinDownTime - 1, t - 1];
//            //    }

//            //    DeCommitMatrix[1, 0] = CommitMatrix[0, t - 1] + StopCost[t];
//            //    for (int downTime = 2; downTime < unit.Cycle.MinDownTime; downTime++)
//            //    {
//            //        DeCommitMatrix[downTime, t] = DeCommitMatrix[downTime - 1, t - 1];
//            //    }
//            //}

//            var generationPlan = new GenerationPlan(unitIndex, commitmentStatus, bestScore);
//            generationPlan.PrintPlan(); 
//            //Console.WriteLine(bestScore);
//            //generationPlan


//            return generationPlan;

//        }

//    }
//}

//class GenerationPlan
//{
//    public int[] CommitmentStatus;
//    public int[] StartStatus;
//    public int[] StopStatus;
//    public double ReducedCost;

//    public int UnitIndex;


//    //Create an Empty Generationplan
//    public GenerationPlan(int unitIndex, int totalTime)
//    {
//        UnitIndex = unitIndex;
//        CommitmentStatus = new int[totalTime];
//        StartStatus = new int[totalTime];
//        StopStatus = new int[totalTime];
//        ReducedCost = 0;
//    }
//    public GenerationPlan(int unitIndex, int[] commitmentStatus, double reducedCost)
//    {
//        ReducedCost = reducedCost;
//        UnitIndex = unitIndex;
//        CommitmentStatus = commitmentStatus;
//        StartStatus = new int[CommitmentStatus.Count()];
//        StopStatus = new int[CommitmentStatus.Count()];
//        StartStatus[0] = commitmentStatus[0] == 1 ? 1 : 0;


//        for (int t = 1; t < CommitmentStatus.Count(); t++)
//        {
//            if (commitmentStatus[t - 1] == 0 && commitmentStatus[t] == 1)
//            {
//                StartStatus[t] = 1;
//            }
//            if (commitmentStatus[t - 1] == 1 && commitmentStatus[t] == 0)
//            {
//                StopStatus[t] = 1;
//            }
//        }

//    }

//    public void PrintPlan()
//    {
//        string line = ReducedCost+"\t";
//        for (int t = 0; t < CommitmentStatus.Count(); t++)
//        {
//            line += CommitmentStatus[t];
//        }
//        Console.WriteLine("Commit\t{0}", line);
//        line = "";
//        for (int t = 0; t < CommitmentStatus.Count(); t++)
//        {
//            line += StartStatus[t];
//        }
//        Console.WriteLine("Start\t{0}", line);
//        line = "";
//        for (int t = 0; t < CommitmentStatus.Count(); t++)
//        {
//            line += StopStatus[t];
//        }
//        Console.WriteLine("Stop\t{0}", line);
//    }
//}