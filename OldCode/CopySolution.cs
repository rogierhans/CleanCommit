//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Drawing;
//using System.Drawing.Drawing2D;

//namespace UtrechtCommitment
//{
//    class CopySolution
//    {
//        public bool[,] UnitCommit;
//        public double[,] EconomicDispatch;
//        public double[,] Storage;
//        public double[] TotalRES;


//        string OutputFolder;// = @"C:\Users\Rogier\Desktop\TempOut\";

//        string Substring;

//        public CopySolution(LPSolver LPS, bool storage, string substring)
//        {
//            OutputFolder = @"C:\Users\Rogier\Desktop\AgainTemp\";
//            Substring = substring;
//            System.IO.Directory.CreateDirectory(OutputFolder + @"d\" );
//            System.IO.Directory.CreateDirectory(OutputFolder + @"s\");
//            System.IO.Directory.CreateDirectory(OutputFolder + @"c\");
//            UnitCommit = new bool[LPS.DefinedUnitCommitment.GetLength(0), LPS.DefinedUnitCommitment.GetLength(1)];
//            for (int t = 0; t < LPS.DefinedUnitCommitment.GetLength(0); t++)
//            {
//                for (int u = 0; u < LPS.DefinedUnitCommitment.GetLength(1); u++)
//                {
//                    UnitCommit[t, u] = LPS.DefinedUnitCommitment[t, u];
//                }
//            }
//            EconomicDispatch = new double[LPS.DispatchsVars.GetLength(0), LPS.DispatchsVars.GetLength(1)];
//            for (int t = 0; t < EconomicDispatch.GetLength(0); t++)
//            {
//                for (int u = 0; u < EconomicDispatch.GetLength(1); u++)
//                {
//                    EconomicDispatch[t, u] = LPS.DispatchsVars[t, u].X;
//                }
//            }

//            if (storage)
//            {
//                Storage = new double[LPS.Storage.GetLength(0), LPS.Storage.GetLength(1)];
//                for (int t = 0; t < LPS.Storage.GetLength(0); t++)
//                {
//                    for (int s = 0; s < LPS.Storage.GetLength(1); s++)
//                    {
//                        Storage[t, s] = LPS.Storage[t, s].X;
//                    }
//                }
//            }
//            var Nodes = LPS.PS.Nodes;
//            TotalRES = new double[Nodes.First().Demand.Count];
//            for (int n = 0; n < Nodes.Count; n++)
//            {
//                var node = Nodes[n];
//                for (int t = 0; t < Nodes.First().Demand.Count; t++)
//                {
//                    TotalRES[t] += node.ResGeneration[t].TotalGeneration - LPS.NodalREScurtailment[n, t].X;
//                }
//            }
//        }
//        //System.IO.Directory.CreateDirectory(myDir);
//        public void PrintDispatch(CopySolution OtherCopy, int tUnitCommitment, int uUnitCommitment)
//        {
//            //int hSize = 1500;
//            //int timeWidth = 10;
//            ////int wSize = 1500;
//            string file = OutputFolder + @"c\"  + Substring + ".bmp";
//            var bmp = new Bitmap(EconomicDispatch.GetLength(0) * 100, EconomicDispatch.GetLength(1) * 100);
//            var g = Graphics.FromImage(bmp);

//            for (int t = 0; t < EconomicDispatch.GetLength(0); t++)
//            {
//                for (int u = 0; u < EconomicDispatch.GetLength(1); u++)
//                {
//                    bool difference = DifferenceDispatch(OtherCopy, t, u);
//                    SolidBrush pen;

//                    if (t == tUnitCommitment && u == uUnitCommitment)
//                    {
//                        pen = new SolidBrush(Color.Yellow);
//                    }
//                    else if (difference)
//                    {
//                        pen = new SolidBrush(Color.Red);
//                    }
//                    else
//                    {
//                        pen = new SolidBrush(Color.Black);
//                    }
//                    g.FillRectangle(pen, t * 100, u * 100, 100, 100);
//                }
//            }

//            bmp.Save(file);

//        }

//        public bool DifferenceDispatch(CopySolution OtherCopy, int t, int u)
//        {
//            bool diff = Math.Abs(EconomicDispatch[t, u] - OtherCopy.EconomicDispatch[t, u]) > 0.001;
//            if (diff)
//            {
//                Console.WriteLine(EconomicDispatch[t, u] - OtherCopy.EconomicDispatch[t, u]);
//            }
//            return diff;
//        }
        

//        public void PrintUCED(PowerSystem PS)
//        {
//            //int hSize = 1500;
//            //int timeWidth = 10;
//            ////int wSize = 1500;
//            //string file = OutputFolder + @"d\" + Substring + ".bmp";

//            //float maxDemand = PS.Times.Max(time => (float)time.Demand);
//            //int timeSteps = PS.Times.Count;
//            //float HRescale = hSize / maxDemand;

//            //var bmp = new Bitmap(timeSteps * timeWidth, hSize);
//            //var g = Graphics.FromImage(bmp);
//            //for (int t = 0; t < PS.Times.Count; t++)
//            //{
//            //    Dictionary<string, double> DispatchPerTechology = new Dictionary<string, double>();
//            //    var time = PS.Times[t];
//            //    for (int u = 0; u < PS.Units.Count; u++)
//            //    {
//            //        var unit = PS.Units[u];
//            //        AddToDict(DispatchPerTechology, unit.Generation.Fuel.Type, EconomicDispatch[t, u]);
//            //    }
//            //    AddToDict(DispatchPerTechology, "RES", TotalRES[t]);

//            //    DrawTimeStep(g, DispatchPerTechology, maxDemand, HRescale, t, timeWidth);
//            //}
//            //bmp.Save(file);
//        }



//        private void DrawTimeStep(Graphics g, Dictionary<string, double> dispatchPerTechology, float maxDemand, float hRescale, int t, int timeWidth)
//        {
//            float position = maxDemand;
//            foreach (var kvp in dispatchPerTechology)
//            {
//                var dispatch = (float)kvp.Value;
//                var newPosition = position - dispatch;
//                if (newPosition < 0) return;

//                float scaledPosistion = position * hRescale;
//                float scaledNewPosistion = newPosition * hRescale;
//                var Brush = GetPen(kvp.Key);
//                g.FillRectangle(Brush, t * timeWidth, scaledNewPosistion, timeWidth, (scaledPosistion - scaledNewPosistion));
//                //g.DrawRectangle(Pen, t , scaledPosistion, t, scaledNewPosistion);
//                position = newPosition;
//            }
//        }

//        public void AddToDict(Dictionary<string, double> dict, string name, double value)
//        {
//            if (!dict.ContainsKey(name))
//            {
//                dict[name] = 0;
//            }
//            dict[name] += value;
//        }

//        public SolidBrush GetPen(string tech)
//        {
//            if (tech == "Coal") return new SolidBrush(Color.Black);
//            if (tech == "Gas") return new SolidBrush(Color.DarkGreen);
//            if (tech == "Lignite") return new SolidBrush(Color.Brown);
//            if (tech == "Windon") return new SolidBrush(Color.AliceBlue);
//            if (tech == "Windoff") return new SolidBrush(Color.LightBlue);
//            if (tech == "PV") return new SolidBrush(Color.Yellow);
//            if (tech == "Hydro") return new SolidBrush(Color.Blue);
//            if (tech == "Bio") return new SolidBrush(Color.ForestGreen);
//            if (tech == "RES") return new SolidBrush(Color.YellowGreen);
//            return new SolidBrush(Color.OrangeRed);
//        }
//    }
//}
