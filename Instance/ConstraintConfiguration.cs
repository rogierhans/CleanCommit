using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using CleanCommit.MIP;
using CleanCommit.Instance;
using System.IO;
using System.Diagnostics;

namespace CleanCommit.Instance
{

    [Serializable]
    public class ConstraintConfiguration
    {
        public enum TransmissionType { Copperplate, TradeBased, VoltAngles, PTDF };



        public bool RampingLimits;
        public bool MinUpMinDown;
        public TransmissionType TransmissionMode;

        public bool TimeDependantStartUpCost;
        public bool Relax;
        public bool Tight;

        public int TotalTime;
        public int TimeOffSet;
        public int PiecewiseSegments;
        public List<Reserve> Reserves = new List<Reserve>();
        public bool Adequacy = false;

        public bool IngorneP2GRes = true;
        public ConstraintConfiguration() { }
        public ConstraintConfiguration(bool rampingLimits, bool minUpMinDown, TransmissionType transmissionMode, bool timeDependantStartUpCost, bool relax, int pwsegments, bool tight)
        {
            RampingLimits = rampingLimits;
            MinUpMinDown = minUpMinDown;
            TransmissionMode = transmissionMode;
            TimeDependantStartUpCost = timeDependantStartUpCost;
            Relax = relax;
            PiecewiseSegments = pwsegments;
            Tight = tight;
        }

        public void SetLimits(int timeOffset, int totalTime)
        {
            this.TimeOffSet = timeOffset;
            this.TotalTime = totalTime;
        }




        public void AdecuacyTest()
        {
            Adequacy = true;
        }

        public override string ToString()
        {
            return Str(Relax) + Str(RampingLimits) + Str(MinUpMinDown) + Str(TimeDependantStartUpCost) + TransmissionMode + Str(Tight) +  TotalTime + "_" + Reserves.Count();
        }


        private string Str(bool b)
        {
            if (b) return "1"; else return "0";
        }

        public ConstraintConfiguration Copy()
        {
            var newCC = new ConstraintConfiguration(RampingLimits, MinUpMinDown, TransmissionMode, TimeDependantStartUpCost, Relax, PiecewiseSegments, Tight);
            newCC.SetLimits(TimeOffSet, TotalTime);
            return newCC;
        }
    }
}
