//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.IO;
//using Gurobi;
//using System.Diagnostics;

//namespace UtrechtCommitment
//{
//    abstract class Solver
//    {

//        public PowerSystem PS;
//        //Solution Solution;
//        protected GRBEnv env;
//        public GRBModel model;

//        public GRBVar[,] DispatchsVars;
//        public GRBVar[,] Transmission;
//        public GRBVar[,] Charge;
//        public GRBVar[,] Discharge;
//        public GRBVar[,] Storage;

//        protected int totalNodes;
//        protected int totalTime;
//        protected int totalUnits;
//        protected int totalLines;
//        protected int totalStorageUnits;
//        protected int totatRESTypes;

//        protected double[,] PDTF;


//        protected bool Ramp;
//        protected bool StorageMode;
//        protected bool PTDFMode;

//        protected double VOLL;
//        public Solver(PowerSystem ps)
//        {
//            PS = ps;
//            env = new GRBEnv();
//            model = new GRBModel(env);
//            VOLL = PS.VOLL;

//            totalTime = PS.Nodes[0].Demand.Count;
//            totalUnits = PS.Units.Count;
//            totalNodes = PS.Nodes.Count;
//            totalLines = PS.Lines.Count;
//            totalStorageUnits = PS.StorageUnits.Count;
//            totatRESTypes = PS.ResGenerations.Count;
//            PDTF = PS.PDTF;

//            Ramp = PS.ConstraintConfiguration.RampingLimits;
//            StorageMode = PS.ConstraintConfiguration.Storage;
//            PTDFMode = true; //PS.ConstraintConfiguration.PTDF;
//            throw new Exception("error ptdfmode broken");
//        }


//        public void SwitchModelOutputOn()
//        {
//            model.Parameters.OutputFlag = 1 - model.Parameters.OutputFlag;
//        }


//        public virtual void ConfigureModel()
//        {

//            AddVariables();
//            if (StorageMode)
//            {
//                AddStorageVariables();
//            }
//            AddConstraints();
//            AddObjective();
//            model.Write(@"C:\Users\Rogier\Desktop\tempa\test2.lp");
//        }


//        public GRBVar[,] NodalLossOfLoad;
//        public GRBVar[,] NodalInjection;
//        public GRBVar[,,] NodalRESGeneration;
//        public virtual void AddVariables()
//        {

//            //decision variables
//            DispatchsVars = new GRBVar[totalTime, PS.Units.Count];
//            NodalLossOfLoad = new GRBVar[totalNodes, totalTime];
//            NodalRESGeneration = new GRBVar[totalNodes, totatRESTypes, totalTime];
//            NodalInjection = new GRBVar[totalNodes, totalTime];


//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];
//                    DispatchsVars[t, u] = model.AddVar(0, double.MaxValue, 0.0, GRB.CONTINUOUS, "u" + u + "t" + t);
//                }


//                for (int n = 0; n < totalNodes; n++)
//                {
//                    var node = PS.Nodes[n];
//                    double totalDemand = node.Demand[t];
//                    //double totalCapacity = PS.Units.Where(unit => node.UnitsIndex.Contains(unit.ID)).Sum(unit => unit.PMax * unit.Count);// node.Units.Sum(unit => unit.PMax);

//                    for (int r = 0; r < totatRESTypes; r++)
//                    {
//                        NodalRESGeneration[n, r, t] = model.AddVar(0, node.ResGenerations[r].ResValues[t], 0.0, GRB.CONTINUOUS, "t" + t + "r" + r + "n" + n);
//                    }
//                    NodalLossOfLoad[n, t] = model.AddVar(0, totalDemand, 0.0, GRB.CONTINUOUS, "t" + t);
//                    NodalInjection[n, t] = model.AddVar(double.MinValue, totalDemand, 0.0, GRB.CONTINUOUS, "t" + t);
//                }
//            }

//            Transmission = new GRBVar[totalLines, totalTime];
//            for (int l = 0; l < totalLines; l++)
//            {
//                var line = PS.Lines[l];
//                for (int t = 0; t < totalTime; t++)
//                {
//                    Transmission[l, t] = model.AddVar(line.MinCapacity, line.MaxCapacity, 0.0, GRB.CONTINUOUS, "l" + l + "t" + t);
//                }
//            }


//        }
//        public void AddStorageVariables()
//        {
//            Storage = new GRBVar[totalTime, totalStorageUnits];
//            Charge = new GRBVar[totalTime, totalStorageUnits];
//            Discharge = new GRBVar[totalTime, totalStorageUnits];

//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int s = 0; s < totalStorageUnits; s++)
//                {
//                    var StorageUnit = PS.StorageUnits[s];
//                    Storage[t, s] = model.AddVar(0, StorageUnit.MaxEnergy, 0.0, GRB.CONTINUOUS, "t" + t + "s" + s);
//                    Charge[t, s] = model.AddVar(0, StorageUnit.MaxCharge, 0.0, GRB.CONTINUOUS, "Charge" + t + "s" + s);
//                    Discharge[t, s] = model.AddVar(0, StorageUnit.MaxDischarge, 0.0, GRB.CONTINUOUS, "Discharge" + t + "s" + s);
//                }
//            }
//        }

//        public void AddStorageConstraints()
//        {

//            for (int s = 0; s < totalStorageUnits; s++)
//            {
//                var StorageUnit = PS.StorageUnits[s];
//                model.AddConstr(Storage[0, s] == Charge[0, s] * StorageUnit.ChargeEffiency - Discharge[0, s] * StorageUnit.DischargeEffiencyInverse, "InitalStorageLevel" + s);
//            }


//            for (int t = 1; t < totalTime; t++)
//            {
//                for (int s = 0; s < totalStorageUnits; s++)
//                {
//                    var StorageUnit = PS.StorageUnits[s];
//                    model.AddConstr(Storage[t, s] == Storage[t - 1, s] + Charge[t, s] * StorageUnit.ChargeEffiency - Discharge[t, s] * StorageUnit.DischargeEffiencyInverse, "InitalStorageLevel" + t + "s" + s);
//                }
//            }
//        }


//        public GRBLinExpr Objective = new GRBLinExpr();

//        public virtual void AddObjective()
//        {

//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    Unit unit = PS.Units[u];
//                    GRBLinExpr variableGeneration = DispatchsVars[t, u] * unit.PiecewiseCost.Last();
//                    Objective += variableGeneration;

//                }
//                for (int n = 0; n < totalNodes; n++)
//                {
//                    Objective += VOLL * NodalLossOfLoad[n, t];
//                }
//            }
//            Console.WriteLine("VOLL added {0}", VOLL);
//            //Console.ReadLine();
//            model.SetObjective(Objective, GRB.MINIMIZE);
//        }



//        public virtual void AddConstraints()
//        {
//            AddGenerationConstraints();
//            if (Ramp)
//            {
//                AddRampingConstraints();
//            }

//            if (StorageMode)
//            {
//                AddStorageConstraints();
//            }
//            AddNodalPowerConstraints();
//            AddZeroSumInjectionConstraints();
//            if (!PTDFMode)
//            {
//                AddTransmissionConstraints();
//            }
//            else
//            {
//                AddPDTFConstraints();
//            }
//        }


//        private void AddPDTFConstraints()
//        {


//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int l = 0; l < totalLines; l++)
//                {

//                    //Skip the reference node, it is assumed the first node is the reference node
//                    GRBLinExpr trans = new GRBLinExpr();
//                    for (int n = 1; n < totalNodes; n++)
//                    {
//                        trans += PDTF[l, n] * NodalInjection[n, t];
//                    }
//                    model.AddConstr(Transmission[l, t] == trans, "PDTF" + t + "l" + l);
//                }
//            }
//        }


//        GRBConstr[,] KirchoffLaw;
//        private void AddTransmissionConstraints()
//        {
//            //add variables

//            KirchoffLaw = new GRBConstr[totalNodes, totalTime];


//            for (int t = 0; t < totalTime; t++)
//            {

//                for (int n = 0; n < totalNodes; n++)
//                {
//                    GRBLinExpr TotalInjections = new GRBLinExpr();
//                    var node = PS.Nodes[n];
//                    for (int l = 0; l < totalLines; l++)
//                    {
//                        var line = PS.Lines[l];
//                        if (node == line.From)
//                        {
//                            TotalInjections -= Transmission[l, t];
//                        }
//                        if (node == line.To)
//                        {
//                            TotalInjections += Transmission[l, t];
//                        }
//                    }

//                    KirchoffLaw[n, t] = model.AddConstr(TotalInjections == NodalInjection[n, t], "KirchoffLaw" + n + "t" + t);
//                }
//            }

//        }

//        GRBConstr[] ZeroSumInjection;
//        private void AddZeroSumInjectionConstraints()
//        {
//            ZeroSumInjection = new GRBConstr[totalTime];
//            for (int t = 0; t < totalTime; t++)
//            {
//                GRBLinExpr TotalInjections = new GRBLinExpr();
//                for (int n = 0; n < totalNodes; n++)
//                {
//                    TotalInjections += NodalInjection[n, t];
//                }
//                ZeroSumInjection[t] = model.AddConstr(TotalInjections == 0, "Zero Sum Injection" + t);
//            }
//        }





//        //private GRBLinExpr GetTotalDispatch(int time)
//        //{
//        //    GRBLinExpr totalDispatch = new GRBLinExpr();
//        //    for (int u = 0; u < totalUnits; u++)
//        //    {
//        //        totalDispatch += DispatchsVars[time, u];
//        //    }
//        //    return totalDispatch;
//        //}



//        GRBConstr[,] NodalPowerBalance;
//        private void AddNodalPowerConstraints()
//        {
//            NodalPowerBalance = new GRBConstr[totalNodes, totalTime];
//            for (int n = 0; n < totalNodes; n++)
//            {
//                Node node = PS.Nodes[n];

//                for (int t = 0; t < totalTime; t++)
//                {
//                    GRBLinExpr generation = new GRBLinExpr();
//                    generation += GetNodalTotalDispatch(n, t);

//                    if (StorageMode)
//                    {
//                        generation += GetNodalTotalDischarge(n, t);

//                    }


//                    for (int r = 0; r < totatRESTypes; r++)
//                    {
//                        generation += NodalRESGeneration[n, r, t];
//                    }

//                    generation += NodalInjection[n, t];


//                    GRBLinExpr consumption = new GRBLinExpr();

//                    consumption += node.Demand[t];

//                    if (StorageMode)
//                    {
//                        consumption += GetNodalTotalCharge(n, t);
//                    }
//                    consumption += -NodalLossOfLoad[n, t];

//                    NodalPowerBalance[n, t] = model.AddConstr(generation == consumption, "NodalPowerBalance" + t);
//                }
//            }
//        }

//        private GRBLinExpr GetNodalTotalDischarge(int n, int t)
//        {
//            GRBLinExpr totalDischarge = new GRBLinExpr();
//            var node = PS.Nodes[n];
//            foreach (int s in node.StorageUnitsIndex)
//            {
//                totalDischarge += Discharge[t, s];
//            }
//            return totalDischarge;
//        }

//        private GRBLinExpr GetNodalTotalCharge(int n, int t)
//        {
//            GRBLinExpr totalCharge = new GRBLinExpr();
//            var node = PS.Nodes[n];
//            foreach (int s in node.StorageUnitsIndex)
//            {
//                totalCharge += Charge[t, s];
//            }
//            return totalCharge;
//        }

//        private GRBLinExpr GetNodalTotalDispatch(int nodeIndex, int time)
//        {
//            GRBLinExpr totalDispatch = new GRBLinExpr();
//            var node = PS.Nodes[nodeIndex];
//            foreach (int u in node.UnitsIndex)
//            {
//                totalDispatch += DispatchsVars[time, u];
//            }
//            return totalDispatch;
//        }

//        protected GRBConstr[,] GenerationConstrMin;
//        protected GRBConstr[,] GenerationConstrMax;

//        protected abstract void AddGenerationConstraints();

//        protected GRBConstr[,] UpwardRampingConstr;
//        protected GRBConstr[,] DownwardRampingConstr;
//        protected abstract void AddRampingConstraints();

//        public double Solve(int TimeLimit)
//        {
//            model.Parameters.TimeLimit = TimeLimit;
//            model.Optimize();
//            //NodalConsoleOutput();
//            return Objective.Value;
//        }

//        public void PrintVariables()
//        {
//            for (int s = 0; s < totalStorageUnits; s++)
//            {
//                string line = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    line += Storage[t, s].X + "\t";
//                }
//                Console.WriteLine(line);
//            }
//        }




//        public void Output()
//        {

//            // STOPWATCh doet het nu niet
//            var stopWatch = new Stopwatch();

//            stopWatch.Start();

//            var print = new Print();
//            double[,] dispatch = new double[totalTime, totalUnits];

//            for (int t = 0; t < totalTime; t++)
//            {
//                for (int u = 0; u < totalUnits; u++)
//                {
//                    dispatch[t, u] = DispatchsVars[t, u].X;
//                }
//            }
//            //string UniqueName = totalTime + "t" + totalUnits + "u" + (Ramp ? 1 : 0) + (MinUpDown ? 1 : 0);
//            //string outputFile = @"C:\Users\4001184\Desktop\Output\log.txt";

//            stopWatch.Stop();
//            long timeSecond = stopWatch.ElapsedMilliseconds / 1000;
//            //print.PrintUCED(PS, dispatch, UniqueName);

//            List<string> cells = new List<string>();
//            cells.Add(totalUnits.ToString());
//            cells.Add(totalTime.ToString());
//            cells.Add(Ramp ? "yes" : "no");
//            //cells.Add(MinUpDown ? "yes" : "no");
//            cells.Add(timeSecond.ToString());
//            cells.Add(model.Status.ToString());
//            //cells.Add(model.MIPGap.ToString());
//            Console.WriteLine(String.Join("\t;", cells));
//            U.Write(IOUtils.OutputFolder + "log.csv", String.Join("\t;", cells));
//        }

//        public virtual Output GetAnswer()
//        {
//            return new Output(this);
//            // output.GetGenerationMixPerTime();
//        }
//    }
//}
