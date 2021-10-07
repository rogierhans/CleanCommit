using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CleanCommit.Instance
{
    [Serializable]
    public class Reserve
    {
        //Sub 5 min, 100% spin              1% of demand
        //10 min, 50% spin                  Maximum of 6% of demand and the largest contingency
        //1 h, 100% spin                    10% of wind generation+7.5% of solar generation
        public double OfDemand;
        public double FixedValue;
        public double RatioRamped;
        public double OfWind;
        public double OfSolar;
        public Reserve() { }
        public Reserve(double ofDemand, double fixedValue, double ratioRamped, double ofWind, double ofSolar)
        {
            OfDemand = ofDemand;
            FixedValue = fixedValue;
            RatioRamped = ratioRamped;
            OfWind = ofWind;
            OfSolar = ofSolar;
        }

        public double GetReserve(PowerSystem PS, int timestep, int timeOFFset)
        {
            var totalDemand = PS.Nodes.Sum(node => node.NodalDemand(timestep,timeOFFset));
            var totalWind = PS.ResGenerations.Where(RES => RES.Type == "WON" || RES.Type == "WOF").Sum(RES => RES.GetValue(timestep, timeOFFset));
            var totalSolar = PS.ResGenerations.Where(RES => RES.Type == "PV").Sum(RES => RES.GetValue(timestep, timeOFFset));
            return (OfDemand * totalDemand + OfSolar * totalSolar + OfWind * totalWind + FixedValue) ;
        }
    }
}
