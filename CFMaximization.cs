﻿using System;
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
            for (int year = 1979; year <= 1979; year++)
            {
                foreach (var instance in TYDNPInstances)
                {
                    string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC\" + instance + "_" + year + ".uc";
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
        public void AllTestsLOL(double fraction, int timeHorizon, int offSet)
        {

            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
               // Adequacy = true
            };
            CC.SetLimits(offSet, timeHorizon);
            for (int year = 1979; year <= 2019; year++)
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
                        var output = TS.LOLOptimzation(600, fraction,"LOLMax", test);
                        TS.Kill();
                    }
                    void Run2()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        Action<Objective> test = ob => ob.LOLObjective();
                        var output = TS.LOLOptimzation(600, fraction, "LOLMin", test);
                        TS.Kill();
                    }
                }
            }
        }
    }
}
