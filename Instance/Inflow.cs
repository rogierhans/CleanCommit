using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCommit.Instance
{
    class Inflow
    {
        public int ID;
        public string StorageID;
        public List<double> Inflows;

        public Inflow(int iD, string storageID, List<double> inflows)
        {
            ID = iD;
            StorageID = storageID;
            Inflows = inflows;
        }
    }
}
