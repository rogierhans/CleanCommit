using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using CleanCommit.Instance;

namespace CleanCommit
{
    class Print
    {
        //string OutputFolder = @"C:\Users\4001184\Desktop\Output\UCED";
        public void PrintUCED(PowerSystem PS, ConstraintConfiguration CC, double[,] dispatch, double[,] Sdispatch, double[,] Rdispatch, string name)
        {
            int hSize = 3000;
            int timeWidth = 30;
            //int wSize = 1500;
            string file = @"C:\Users\4001184\Desktop\" + name + ".png";
            double MaxDemand = 0;
            for (int t = 0; t < CC.TotalTime; t++)
            {
                MaxDemand = Math.Max(MaxDemand, PS.Nodes.Sum(node => node.NodalDemand(t)));
            }
            MaxDemand *= 1.3;
            int timeSteps = CC.TotalTime;
            float HRescale = hSize / (float) MaxDemand;

            var bmp = new Bitmap(timeSteps * timeWidth, hSize);
            var g = Graphics.FromImage(bmp);
            List<double> demand = new List<double>();
            Dictionary<string, double> key2Var = new Dictionary<string, double>();
            List<Dictionary<string, double>> store = new List<Dictionary<string, double>>();
            for (int t = 0; t < timeSteps; t++)
            {
                demand.Add(PS.Nodes.Sum(node => node.NodalDemand(t)));
                Dictionary<string, double> DispatchPerTechology = new Dictionary<string, double>();
                //var time = PS.Times[t];
                for (int u = 0; u < PS.Units.Count; u++)
                {
                    var unit = PS.Units[u];
                  //  Console.WriteLine("{0} {1}", t, u);
                    var disp = dispatch[t, u];
                    AddToDict(DispatchPerTechology, unit.PrintType, disp);

                }
                for (int s = 0; s < PS.StorageUnits.Count; s++)
                {
                    var sunit = PS.StorageUnits[s];
                    var SName = sunit.Name;
                    AddToDict(DispatchPerTechology, SName, Sdispatch[t, s]);
                }

                for (int r = 0; r < PS.ResGenerations.Count; r++)
                {
                    var runit = PS.ResGenerations[r];
                    var rName = runit.Name;
                    AddToDict(DispatchPerTechology, rName, Rdispatch[t, r]);
                }
                store.Add(DispatchPerTechology);
            }
            foreach (var key in store.First().Keys) {
                List<double> values = new List<double>();
                for (int t = 0; t < timeSteps; t++) {
                    values.Add(store[t][key]);
                }
                Console.WriteLine("{0} {1}",key, values.Sum());
                key2Var[key] = Variance(values);
            }

            for (int t = 0; t < timeSteps; t++)
            {
                demand.Add(PS.Nodes.Sum(node => node.NodalDemand(t)));
                Dictionary<string, double> DispatchPerTechology = new Dictionary<string, double>();
                //var time = PS.Times[t];
                for (int u = 0; u < PS.Units.Count; u++)
                {
                    var unit = PS.Units[u];
                    AddToDict(DispatchPerTechology, unit.PrintType, dispatch[t, u]);

                }
                for (int s = 0; s < PS.StorageUnits.Count; s++)
                {
                    var sunit = PS.StorageUnits[s];
                    var SName = sunit.Name;
                    AddToDict(DispatchPerTechology, SName, Sdispatch[t, s]);
                }

                for (int r = 0; r < PS.ResGenerations.Count; r++)
                {
                    var runit = PS.ResGenerations[r];
                    var rName = runit.Name;
                    AddToDict(DispatchPerTechology, rName, Rdispatch[t, r]);
                }

                DrawTimeStep1TechFist(g, DispatchPerTechology,(float) MaxDemand, HRescale, t, timeWidth, "COAL");
            }
            for (int t = 0; t < timeSteps-1; t++)
            {
                float y1 = GetY((float)demand[t], (float)MaxDemand, (float)HRescale);
                float y2 = GetY((float)demand[t + 1], (float)MaxDemand, (float)HRescale);
                float x1 = t * timeWidth + (timeWidth/2);
                float x2 = (t + 1) * timeWidth + (timeWidth / 2);
                g.DrawLine(new Pen(Color.Black,10),x1,y1,x2,y2);
                g.FillEllipse(new SolidBrush(Color.Black), x1-15, y1 - 15, 30, 30);
            }

            bmp.Save(file);
        }

        private double Variance(List<double> data) {
            double avg = data.Average();
            double sum = 0;
            foreach (var value in data) {
                sum += (value - avg) * (value - avg);
            }
            return Math.Sqrt(sum / data.Count()) / avg;
        }
        

        private void DrawTimeStep1TechFist(Graphics g, Dictionary<string, double> dispatchPerTechology, float maxDemand, float hRescale, int t, int timeWidth, string first)
        {
            float position = maxDemand;
            foreach (var kvp in dispatchPerTechology.OrderBy(kvp => kvp.Key== first ? 0 : GetNumber(kvp.Key)))
            {
                var dispatch = (float)kvp.Value;
                var newPosition = position - dispatch;
                if (newPosition < 0) return;


                float scaledPosistion = position * hRescale;
                float scaledNewPosistion = newPosition * hRescale;
                var Brush = GetPen(kvp.Key);
                g.FillRectangle(Brush, t * timeWidth, scaledNewPosistion, timeWidth, (scaledPosistion - scaledNewPosistion));
                //g.DrawRectangle(Pen, t , scaledPosistion, t, scaledNewPosistion);
                position = newPosition;
            }
        }


        private void DrawTimeStepVariance(Graphics g, Dictionary<string, double> dispatchPerTechology, float maxDemand, float hRescale, int t, int timeWidth, Dictionary<string, double> key2Var)
        {
            float position = maxDemand;
            foreach (var kvp in dispatchPerTechology.OrderBy(kvp => kvp.Key == "BIO" ? 0 : key2Var[kvp.Key]))
            {
                var dispatch = (float)kvp.Value;
                var newPosition = position - dispatch;
                if (newPosition < 0) return;


                float scaledPosistion = position * hRescale;
                float scaledNewPosistion = newPosition * hRescale;
                var Brush = GetPen(kvp.Key);
                g.FillRectangle(Brush, t * timeWidth, scaledNewPosistion, timeWidth, (scaledPosistion - scaledNewPosistion));
                //g.DrawRectangle(Pen, t , scaledPosistion, t, scaledNewPosistion);
                position = newPosition;
            }
        }

        public float GetY(float dispatch, float maxDemand, float hRescale) {
            var newPosition = maxDemand - dispatch;
            if (newPosition < 0) throw new Exception();

            float scaledPosistion = maxDemand * hRescale;
            float scaledNewPosistion = newPosition * hRescale;
            return scaledNewPosistion;
        }

        public void AddToDict(Dictionary<string, double> dict, string name, double value)
        {
            if (!dict.ContainsKey(name))
            {
                dict[name] = 0;
            }
            dict[name] += value;
        }

        public SolidBrush GetPen(string tech)
        {

            if (tech == "COAL CCS") return new SolidBrush(Color.Purple);
            if (tech == "GAS CCS") return new SolidBrush(Color.Purple);
            if (tech == "LIGNITE CCS") return new SolidBrush(Color.Purple);
            if (tech == "COAL") return new SolidBrush(Color.Black);
            if (tech == "GAS") return new SolidBrush(Color.DarkGreen);
            if (tech == "OIL") return new SolidBrush(Color.Brown);
            if (tech == "BIO") return new SolidBrush(Color.Green);
            if (tech == "LIGNITE") return new SolidBrush(Color.SandyBrown);
            if (tech == "NUC") return new SolidBrush(Color.Red);
            if (tech == "Battery") return new SolidBrush(Color.Pink); 
            if (tech == "Reservoir") return new SolidBrush(Color.LightBlue); 
            if (tech == "Onshore Wind") return new SolidBrush(Color.LightGreen);
            if (tech == "Offshore Wind") return new SolidBrush(Color.GreenYellow);
            if (tech == "Solar PV") return new SolidBrush(Color.Yellow);
            if (tech == "Run-of-River") return new SolidBrush(Color.Blue);
            if (tech == "Bio") return new SolidBrush(Color.ForestGreen);
            return new SolidBrush(Color.OrangeRed);
        }

        public double GetNumber(string tech)
        {

            if (tech == "COAL") return 2;
            if (tech == "COAL CCS") return  7.1;
            if (tech == "GAS CCS") return 7.2;
            if (tech == "LIGNITE CCS") return 7.3;
            if (tech == "GAS") return 7;
            if (tech == "OIL") return 4;
            if (tech == "BIO") return 1;
            if (tech == "LIGNITE") return 3;
            if (tech == "NUC") return 2;
            if (tech == "Battery") return 8;
            if (tech == "Reservoir") return 9;
            if (tech == "Onshore Wind") return 10;
            if (tech == "Offshore Wind") return  11;
            if (tech == "Solar PV") return 12;
            if (tech == "Run-of-River") return 9;
            //if (tech == "Bio") return 6;
            return 20;
        }

    }
}
