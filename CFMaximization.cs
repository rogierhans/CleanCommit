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
        public void AllTests(string Gtype, double fraction)
        {

            int timehorizon = 24;
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
                //Adequacy = true
            };
            CC.SetLimits(0, timehorizon);
            for (int year = 1979; year <= 1979; year++)
            {
                foreach (var instance in TYDNPInstances)
                {
                    string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC\" + instance + "_" + year + ".uc";
                    PowerSystem PS = IOUtils.GetPowerSystem(filename);
                    Run(); Run2();
                    void Run()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        var output = TS.CFOptimzation(36000, fraction, Gtype,GRB.MINIMIZE);
                        //Console.ReadLine();
                        //List<object> cells = new List<object>() {
                        //year,
                        //PS.ToString(),
                        //CC.ToString(),
                        //output.LOLCounter,
                        //output.DRCounter,
                        //output.GurobiCost,
                        //output.GurobiCostLOL,
                        //output.GurobiCostLOR,
                        //output.GurobiCostDR,
                        //output.GurobiCostGeneration,
                        //output.GurobiCostCycle,
                        //output.ComputationTime };
                        // var line = string.Join("\t", cells);
                        //  File.AppendAllText(@"C:\Users\" + Environment.UserName + @"\Desktop\FullExperiment.txt", line + "\n");
                        //output.ToCSV(@"E:\UCCsv\" + PS.Name.Split('.').First() + "_" + "CF" + ".csv");
                        //output.ToBin(@"E:\UCBin\" + PS.Name.Split('.').First() + "_" + "CF"+ ".bin");
                        TS.Kill();
                    }
                    void Run2()
                    {
                        TightSolver TS = new TightSolver(PS, CC);
                        TS.ConfigureModel();
                        var output = TS.CFOptimzation(36000, fraction, Gtype, GRB.MAXIMIZE);
                        //Console.ReadLine();
                        //List<object> cells = new List<object>() {
                        //year,
                        //PS.ToString(),
                        //CC.ToString(),
                        //output.LOLCounter,
                        //output.DRCounter,
                        //output.GurobiCost,
                        //output.GurobiCostLOL,
                        //output.GurobiCostLOR,
                        //output.GurobiCostDR,
                        //output.GurobiCostGeneration,
                        //output.GurobiCostCycle,
                        //output.ComputationTime };
                        // var line = string.Join("\t", cells);
                        //  File.AppendAllText(@"C:\Users\" + Environment.UserName + @"\Desktop\FullExperiment.txt", line + "\n");
                        //output.ToCSV(@"E:\UCCsv\" + PS.Name.Split('.').First() + "_" + "CF" + ".csv");
                        //output.ToBin(@"E:\UCBin\" + PS.Name.Split('.').First() + "_" + "CF"+ ".bin");
                        TS.Kill();
                    }
                }
            }
        }
    }
}