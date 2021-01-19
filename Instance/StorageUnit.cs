using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{


    class StorageUnit
    {
        readonly public string ID;
        readonly public string Name;
        readonly public double MaxCharge;
        readonly public double MaxDischarge;
        readonly public double MaxEnergy;
        readonly public double ChargeEffiency;
        readonly public double DischargeEffiencyInverse;
        readonly private double DischargeEffiency;
        readonly private List<double> Inflow;

        public StorageUnit(string iD, string name, double maxCharge, double maxDischarge, double maxEnergy, double chargeEffiency, double dischargeEffiency, List<double> inflow)
        {
            ID = iD;
            Name = name;
            MaxCharge = maxCharge;
            MaxDischarge = maxDischarge;
            MaxEnergy = maxEnergy;
            ChargeEffiency = chargeEffiency;
            DischargeEffiency = dischargeEffiency;
            DischargeEffiencyInverse = 1 / dischargeEffiency;
            Inflow = inflow;
        }

        public double GetInflow(int t)
        {
            if (t < Inflow.Count)
                return Inflow[t];
            else
                return 0;
        }


    }
}
