namespace CleanCommit.Instance
{
    public class ConstraintConfiguration
    {
        public enum TransmissionType { Copperplate, TradeBased, VoltAngles, PTDF };

        public bool RampingLimits;
        public bool MinUpMinDown;
        public TransmissionType TransmissionMode;

        // public bool Storage;
        public bool TimeDependantStartUpCost;
        public bool Relax;
        public bool Tight;

        public int TotalTime;
        public int SkipTime;
        public int PiecewiseSegments;
        public bool Adequacy = false;
        public double DemandMultiplier = 1;
        public double FistNodeM = 1;

        public bool SuperTight = false;
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

        public void SetLimits(int skipTime, int totalTime)
        {
            this.SkipTime = skipTime;
            this.TotalTime = totalTime;
        }

        public void AdecuacyTest()
        {
            Adequacy = true;
        }

        public override string ToString()
        {
            return Str(Relax) + Str(RampingLimits) + Str(MinUpMinDown) + Str(TimeDependantStartUpCost) + TransmissionMode + Str(Tight) + PiecewiseSegments + TotalTime;
        }


        private string Str(bool b)
        {
            if (b) return "1"; else return "0";
        }

        public ConstraintConfiguration Copy()
        {
            var newCC = new ConstraintConfiguration(RampingLimits, MinUpMinDown, TransmissionMode, TimeDependantStartUpCost, Relax, PiecewiseSegments, Tight);
            newCC.SetLimits(SkipTime, TotalTime);
            return newCC;
        }
    }
}
