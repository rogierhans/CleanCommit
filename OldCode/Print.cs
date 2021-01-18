//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.IO;

//namespace UtrechtCommitment
//{
//    class Print
//    {
//        //string OutputFolder = @"C:\Users\4001184\Desktop\Output\UCED";
//        public void PrintUCED(PowerSystem PS, double[,] dispatch,string name)
//        {
//            //int hSize = 1500;
//            //int timeWidth = 10;
//            ////int wSize = 1500;
//            //string file = OutputFolder + name + ".bmp";

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
//            //        AddToDict(DispatchPerTechology, unit.Generation.Fuel.Type, dispatch[t, u]);

//            //    }
//            //    foreach
//            //    ddToDict(DispatchPerTechology, unit.Generation.Fuel.Type, dispatch[t, u]);

//            //    DrawTimeStep(g, DispatchPerTechology, maxDemand, HRescale, t, timeWidth);
//            //}

//            //double RescaleH = maxDemand
//            //var bmp = new Bitmap(size, size);
//            //var g = Graphics.FromImage(bmp);
//            //bmp.Save(file);
//        }



//        private void DrawTimeStep(Graphics g,Dictionary<string, double> dispatchPerTechology, float maxDemand, float hRescale,int t, int timeWidth)
//        {
//            float position = maxDemand;
//            foreach (var kvp in dispatchPerTechology) {
//                var dispatch = (float) kvp.Value;
//                var newPosition = position - dispatch;
//                if (newPosition < 0) return;

//                float scaledPosistion = position * hRescale;
//                float scaledNewPosistion = newPosition * hRescale;
//                var Brush = GetPen(kvp.Key);
//                g.FillRectangle(Brush, t * timeWidth, scaledNewPosistion, timeWidth,(scaledPosistion - scaledNewPosistion));
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
//            return new SolidBrush(Color.OrangeRed);
//        }

//    }
//}
