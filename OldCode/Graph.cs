//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Gurobi;
//using System.IO;

//namespace UtrechtCommitment
//{
//    class Graph
//    {
//        private double[,] Value;
//        private PowerSystem PS;
//        private int totalTime;
//        private int unitIndex;

//        private double[] CommitCost;
//        private double[] StartCost;
//        private double[] StopCost;

//        private double[,] CommitMatrix;
//        private double[,] DeCommitMatrix;

//        string[] anwsers;

//        private int minUpTime;
//        private int minDownTime;
//        public Graph(PowerSystem ps, int index, GRBConstr[,] GenerationConstrMax, GRBConstr[,] GenerationConstrMin, GRBConstr[,] UpwardRampingConstr, GRBConstr[,] DownwardRampingConstr, GRBConstr[] GenerationPlanConstr)
//        {
//            PS = ps;
//            totalTime = ps.Times.Count;
//            unitIndex = index;
//            var unit = PS.Units[unitIndex];
//            CalculateCosts(GenerationConstrMax, GenerationConstrMin, UpwardRampingConstr, DownwardRampingConstr);

//            minUpTime = unit.Cycle.MinUpTime;
//            minDownTime = unit.Cycle.MinDownTime;
//            CommitMatrix = new double[minUpTime, totalTime];
//            DeCommitMatrix = new double[minDownTime, totalTime];

//            anwsers = new string[minDownTime + minUpTime];
//            Init(GenerationPlanConstr);
//            FillTable();
//            PrintTable();


//        }

//        public void Init(GRBConstr[] GenerationPlanConstr)
//        {

//            CommitMatrix[0, 0] = CommitCost[0] + StartCost[0] - GenerationPlanConstr[unitIndex].Pi;
//            for (int upTime = 1; upTime < minUpTime; upTime++)
//            {
//                CommitMatrix[upTime, 0] = double.MaxValue;
//            }


//            DeCommitMatrix[0, 0] = 0 - GenerationPlanConstr[unitIndex].Pi;
//            for (int downTime = 1; downTime < minDownTime; downTime++)
//            {
//                DeCommitMatrix[downTime, 0] = double.MaxValue;
//            }

//        }

//        public void FillTable()
//        {
            
//            for (int t = 1; t < totalTime; t++)
//            {
//                int successorMinUpTime = Math.Max(0, minUpTime - 2); // in case of minUptime =1
//                if (CommitMatrix[minUpTime - 1, t - 1] < CommitMatrix[successorMinUpTime, t - 1])
//                {
//                    CommitMatrix[minUpTime - 1, t] = CommitMatrix[minUpTime - 1, t - 1] + CommitCost[t];
//                }
//                else
//                {
//                    CommitMatrix[minUpTime - 1, t] = CommitMatrix[successorMinUpTime, t - 1] + CommitCost[t];
//                }

//                //unit can only transission from mindownTime to first commited status
//                CommitMatrix[0, t] = DeCommitMatrix[minDownTime - 1, t - 1] + CommitCost[t] + StartCost[t];

//                for (int upTime = 1; upTime < minUpTime - 1; upTime++)
//                {
//                    CommitMatrix[upTime, t] = CommitMatrix[upTime - 1, t - 1] + CommitCost[t];
//                }

//                int successorMinDownTime = Math.Max(0, minDownTime - 2); // in case of minUptime =1
//                if (DeCommitMatrix[minDownTime - 1, t - 1] < DeCommitMatrix[successorMinDownTime, t - 1])
//                {
//                    DeCommitMatrix[minDownTime - 1, t] = DeCommitMatrix[minDownTime - 1, t - 1];
//                }
//                else
//                {
//                    DeCommitMatrix[minDownTime - 1, t] = DeCommitMatrix[successorMinDownTime, t - 1];
//                }

//                DeCommitMatrix[0, t] = CommitMatrix[minDownTime - 1, t - 1] + StopCost[t];
//                for (int downTime = 1; downTime < minDownTime - 1; downTime++)
//                {
//                    DeCommitMatrix[downTime, t] = DeCommitMatrix[downTime - 1, t - 1];
//                }
//            }
//        }


//        string fileName = @"C:\Users\Rogier\Desktop\tempLOG\tempLog.csv";
//        public void PrintTable()
//        {
//            List<string> lines = new List<string>();
//            lines.Add("new");
//            for (int upTime = 0; upTime < minUpTime; upTime++)
//            {
//                string line = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    line += "\t"+CommitMatrix[upTime, t];
//                }
//                lines.Add(line);

//            }
//            for (int downTime = 0; downTime < minDownTime; downTime++)
//            {
//                string line = "";
//                for (int t = 0; t < totalTime; t++)
//                {
//                    line += "\t" + DeCommitMatrix[downTime, t];
//                }
//                lines.Add(line);
//            }
//            lines.Add("end");

//            File.WriteAllLines(fileName, lines);

//        }

//        public double Cost(bool up, int time)
//        {
//            return up ? CommitCost[time] : 0;
//        }

//        public void CalculateCosts(GRBConstr[,] GenerationConstrMax, GRBConstr[,] GenerationConstrMin, GRBConstr[,] UpwardRampingConstr, GRBConstr[,] DownwardRampingConstr)
//        {

//            var unit = PS.Units[unitIndex];
//            CommitCost = new double[totalTime];
//            StartCost = new double[totalTime];
//            StopCost = new double[totalTime];


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
//        }


//    }
//}
