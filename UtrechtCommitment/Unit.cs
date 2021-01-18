using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtrechtCommitment
{



    class Unit
    {
        public string ID;
        public double PMin, PMax, UT, UD;
        public double A, B;
        public double HC;// CC, tCold;
        public double RampUp,RampDown;
        public double StartUp, ShutDown;

        public string FuelType;

        Dictionary<string, double> Fuel = new Dictionary<string, double> { { "Gas", 22.00 }, { "Coal", 14.41 }, { "Lignite", 9.33 } };
        //Dictionary<string, double> RES = new Dictionary<string, double> { {"Windon",40000},{"Windoff",10000},{"PV",50000},{"Hydro",4500},{"Bio",5500} };
        //public Unit(string line)
        //{
        //    string[] cells = line.Split(';');
        //    int i = 0;
        //    ID = cells[i++];
        //    PMax = Int32.Parse(cells[i++]);
        //    Ramp = PMax / 20;
        //    PMin = Int32.Parse(cells[i++]);
        //    UT = Int32.Parse(cells[i++]);
        //    UD = Int32.Parse(cells[i++]);
        //    on = (Int32.Parse(cells[i++]) > 0);
        //    A = Double.Parse(cells[i++]);
        //    B = Double.Parse(cells[i++]);
        //    C = Double.Parse(cells[i++]);
        //    HC = Int32.Parse(cells[i++]);
        //    CC = Int32.Parse(cells[i++]);
        //    tCold = Int32.Parse(cells[i++]);
        //}

        public Unit(string line, bool p)
        {

            string[] cells = line.Split(',');
            int i = 0;
            ID = cells[i++];
            FuelType = cells[i++];
            PMin =  Double.Parse(cells[i++]);
            //Console.WriteLine("PMIN DOET NIET MEE!!!");
//PMin = 0;
            PMax = Double.Parse(cells[i++]);
            double effMax = Double.Parse(cells[i++]);
            double effMin = Double.Parse(cells[i++]);

            RampUp = PMax * Double.Parse(cells[i++]);
            RampDown = PMax * Double.Parse(cells[i++]);
            StartUp = PMax * Double.Parse(cells[i++]);
            ShutDown = PMax * Double.Parse(cells[i++]);
            UT = Double.Parse(cells[i++]);
            UD = Double.Parse(cells[i++]);
            HC = Double.Parse(cells[i++]) + Double.Parse(cells[i++]);
            //on = (Int32.Parse(cells[i++]) > 0);
            //A = Double.Parse(cells[i++]);
            var astar = PMin / effMin;
            B = ((PMax / effMax) - (PMin / effMin)) / (PMax - PMin);

            A = astar - PMin * B;
            B *= Fuel[FuelType];
            A *= Fuel[FuelType];
            //C = Double.Parse(cells[i++]);

            //tCold = Int32.Parse(cells[i++]);
            Console.WriteLine("{0},{1}  a={2}  b={3}  min={4}  max{5} ram={6}", PMin, PMax, A, B, effMin, effMax, RampUp);
        }

        public double GetGenerationCost(double distpach) {

            return A + B * distpach;
        }

        public double GetStartCost() {
            return HC;
        }
    }
}
