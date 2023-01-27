using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.MIP;
using CleanCommit.Instance;
using System.Diagnostics;
using System.IO;
using Gurobi;
namespace CleanCommit
{
    class CFMaximization
    {
        public static string[] TYDNPInstances = new string[] { "DE_2040", "GA_2040", "NT_2040", "GA_2030", "NT_2030", "DE_2030" };
        public static string[] TYDNPInstances2040 = new string[] { "DE_2040", "GA_2040", "NT_2040" };
        public void AllTests(string Gtype, double fraction,int timeHorizon, int offSet)
        {

            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
                //Adequacy = true
            };
            CC.SetLimits(offSet, timeHorizon);
            for (int year = 1950; year < 2019; year = year +10)
            {
                foreach (var instance in TYDNPInstances)
                {
                    string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC_WON\" + instance + "_" + year + ".uc";
                    PowerSystem PS = IOUtils.GetPowerSystem(filename);
                    Run();
                    void Run()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        var output = TS.CFOptimzation(36000, fraction, Gtype);
                        TS.Kill();
                    }
                }
            }
        }

        public void ALLCO2(double fraction, int timeHorizon, int offSet)
        {

            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
                //Adequacy = true
            };
            CC.SetLimits(offSet, timeHorizon);
            for (int year = 1950; year < 2019; year = year + 10)
            {
                foreach (var instance in TYDNPInstances)
                {
                    string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC_WON\" + instance + "_" + year + ".uc";
                    PowerSystem PS = IOUtils.GetPowerSystem(filename);
                    Run();
                    void Run()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        var output = TS.CO2Optimzation(36000, fraction);
                        TS.Kill();
                    }
                }
            }
        }

        public void AllTestsLOL(ConstraintConfiguration CC, double fraction, int timeHorizon, int offSet, string name )
        {


            CC.SetLimits(offSet, timeHorizon);
            for (int year = 1979; year <= 2019 -30; year++)
            {
                foreach (var instance in TYDNPInstances2040)
                {
                    string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC_WON\" + instance + "_" + year + ".uc";
                    PowerSystem PS = IOUtils.GetPowerSystem(filename);
                    Run(); Run2();
                    void Run()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        Action<Objective> test = ob => ob.LOLMaxQuadatric();
                        var output = TS.LOLHOptimzation(600, fraction,"LOLMax_" + name, test);
                        TS.Kill();
                    }
                    void Run2()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        Action<Objective> test = ob => ob.LOLObjective();
                        var output = TS.LOLHOptimzation(600, fraction, "LOLMin_" + name, test);
                        TS.Kill();
                    }
                }
            }
        }
    }
}
