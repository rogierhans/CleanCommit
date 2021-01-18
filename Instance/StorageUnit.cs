using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{


    class StorageUnit
    {
        public int ID;
        public string Name;
        public double MaxCharge;
        public double MaxDischarge;
        public double MaxEnergy;
        public double ChargeEffiency;
        public double DischargeEffiencyInverse;
        private double DischargeEffiency;


        public StorageUnit(int iD, string name, double maxCharge, double maxDischarge, double maxEnergy, double chargeEffiency, double dischargeEffiency)
        {
            ID = iD;
            Name = name;
            MaxCharge = maxCharge;
            MaxDischarge = maxDischarge;
            MaxEnergy = maxEnergy;
            ChargeEffiency = chargeEffiency;
            DischargeEffiency = dischargeEffiency;
            DischargeEffiencyInverse = 1 / dischargeEffiency;
        }


    }
}
