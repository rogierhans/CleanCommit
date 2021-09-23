using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.Instance;
using CleanCommit.MIP;

namespace CleanCommit
{
    static class SolutionAnalyssis
    {
        public static double GetTotalGeneration(Solution sol, string GeneratorType)
        {
            double total = 0;
            int totalTime = sol.Dispatch.GetLength(0);
            int totalUnits = sol.Dispatch.GetLength(1);
            var PS = sol.PS;
            for (int t = 0; t < totalTime; t++)
            {
                for (int u = 0; u < totalUnits; u++)
                {

                    Unit unit = PS.Units[u];
                    if (unit.PrintType == GeneratorType)
                    {
                        total += sol.Dispatch[t,u];
                    }
                }
            }
            return total;
        }

    }
}
