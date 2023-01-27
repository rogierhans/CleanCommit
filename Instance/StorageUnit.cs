using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{

    [Serializable]
    public class StorageUnit
    {
        readonly public string ID;
        readonly public string Name;
        readonly public double MaxCharge;
        readonly public double MaxDischarge;
        readonly public double MaxEnergy;
        readonly public double ChargeEffiency;
        readonly public double DischargeEffiencyInverse;
        readonly public double DischargeEffiency;
        readonly public List<double> Inflow;

        public StorageUnit() { }
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

        public double GetInflow(int t, int timeOffset)
        {
            if (Inflow.Count == 0) return 0;
            return Inflow[(t+ timeOffset) % Inflow.Count];
        }
        public override string ToString()
        {

            List<string> Properties = new List<string>
            {
                ID.ToString(),
                Name,
                MaxCharge.ToString(),
                MaxDischarge.ToString(),
                MaxEnergy.ToString(),
                ChargeEffiency.ToString(),
                DischargeEffiency.ToString()
            };

            return String.Join(";", Properties);

        }

    }
}
