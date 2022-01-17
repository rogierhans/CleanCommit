using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CleanCommit.Instance;
using CleanCommit.MIP;
namespace CleanCommit
{
    class Program
    {
        static void Main(string[] args)
        { var Experiment = new CFMaximization();

            for (int dayOffset = 0; dayOffset < 365; dayOffset += 30)
            {
                int hourOffset = dayOffset * 24;
                Experiment.AllTestsLOL(1, 72, hourOffset);
            }
            // RunOldRoller();

            //string filename = @"C:\Users\" + Environment.UserName + @"\OneDrive - Universiteit Utrecht\ACDC_WON\" + "DE_2040" + "_" + 1979 + ".uc";
            //foreach (var filename in new DirectoryInfo(@"C:\Users\4001184\OneDrive - Universiteit Utrecht\ACDC_WON").GetFiles().ToList().OrderByDescending(x => int.Parse(x.Name.Split('_')[1])).Select(x => x.FullName))
            //{
            //    string name = filename.Split('\\').Last().Split('.').First();
            //    Console.WriteLine(name);
            //    var doneNames = new DirectoryInfo(@"E:\Temp2").GetFiles().ToList().OrderByDescending(x => int.Parse(x.Name.Split('_')[1])).Select(x => x.Name.Split('\\').Last().Split('.').First());

            //    if (!doneNames.Contains(name))
            //  RunnerRolling(filename);
            //}

            return;








            //CFMaximazation();
            var exp = new Experiment();
            exp.AllTests();

        }

        private static void RunOldRoller() {
            var binfile = @"E:\Temp2\DE_2040_1979.bin";
            var sol = Solution.GetFromBin(binfile);
            var name = sol.PS.Name;
            int timehorizon = 24 * 7;
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
                Adequacy = true
            };
            CC.SetLimits(0, timehorizon);
            //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\GA10.uc";
            PowerSystem PS = sol.PS;
            var newCC = CC.Copy();
            //newCC.Adequacy = true;
            newCC.MinUpMinDown = true;
            newCC.RampingLimits = true;
            newCC.Relax = true;

            var rollingSolver = new RollingSolver(sol, newCC);

            var rollingSolution = rollingSolver.Roll(36, 12,12, name);
            rollingSolver.Kill();
        }

        private static void RunnerRolling(string filename)
        {
            string name = filename.Split('\\').Last().Split('.').First();
            Console.WriteLine(name);
            int timehorizon = 8760;
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false)
            {
                Adequacy = true
            };
            CC.SetLimits(0, timehorizon);
            //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\GA10.uc";
            PowerSystem PS = IOUtils.GetPowerSystem(filename);
            var newCC = CC.Copy();
            newCC.Adequacy = false;
            //newCC.MinUpMinDown = true;
            //newCC.RampingLimits = true;
            //newCC.Relax = true;
            TightSolver TS = new TightSolver(PS, CC);

            TS.ConfigureModel();
            var sol = TS.NewSolve(36000, 1);
            TS.Kill();
            sol.ToCSV(@"E:\Temp2\" + name + ".csv");
            sol.ToBin(@"E:\Temp2\" + name + ".bin");
            // var recalc = new RecalcSolution(sol);
            //Console.WriteLine("total Cost:{0} {1} {2} {3} {4} {5}", recalc.TotalCost, recalc.GenerationCost,recalc.CycleCost, recalc.DRCost , recalc.LOLCost, recalc.LORCost );
            //Console.WriteLine("total Cost:{0} {1} {2} {3} {4} {5}", sol.GurobiCost, sol.GurobiCostGeneration,sol.GurobiCostCycle, sol.GurobiCostDR , sol.GurobiCostLOL, sol.GurobiCostLOR);
            //  Console.ReadLine();
            var rollingSolver = new RollingSolver(sol, newCC);

            var rollingSolution = rollingSolver.Roll(240, 120,120, name);
            rollingSolver.Kill();
        }

        private static void CFMaximazation()
        {
            var Experiment = new CFMaximization();
            for (double fraction = 1.001; fraction <= 1.01; fraction += 0.001)
            {
                for (int dayOffset = 0; dayOffset < 365; dayOffset += 30)
                {
                    int hourOffset = dayOffset * 24;
                    Experiment.AllTests("GAS", fraction, 72, hourOffset);
                    Experiment.AllTests("COAL", fraction, 72, hourOffset);
                    Experiment.AllTests("NUC", fraction, 72, hourOffset);
                }
            }
        }





        // Console.WriteLine("UserName: {0}", Environment.UserName);return;

        //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\GA10.uc";
        ////string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
        //var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, false, 1, false);
        ////CC.Adequacy = true;
        //Console.WriteLine(filename);
        //CC.SetLimits(0, 24);
        //PowerSystem PS = CleanCommit.IOUtils.GetPowerSystem(filename);
        //Run();

        //void Run()
        //{
        //    CleanCommit.TightSolver TS = new CleanCommit.TightSolver(PS, CC);
        //    TS.ConfigureModel();
        //    var output = TS.Solve(36000);
        //}
        //return;



        // var instanceNames = new string[] { , };


        // new DirectoryInfo(@"C:\Users\4001184\Desktop\ZondagNacht").GetFiles().Select(x => x.FullName).ToList().ForEach(x => Experiment.Foto(x, x.Split('\\').Last().Split('.').First()));
        //Experiment.Test0();
        //  Experiment.Expriment9();


        // Experiment.Test();
        //  Experiment.TestGA10();
        //Experiment.TestGA102();
        // Experiment.Dump();
        // Experiment.TestGA10();

        //
        //return;
        //Experiment.Test3Node();
        //
        // Experiment.Foto("LPRamp");
        //Experiment.Foto("11.01");
        //Experiment.Foto("21.01");
        //Experiment.Foto("31.01");
        //Experiment.Foto("11.001");
        //Experiment.Foto("21.001");
        //Experiment.Foto("31.001");
        //Experiment.Foto("11.001");
        //Experiment.Foto("21.001");
        //Experiment.Foto("31.001");
        // Console.ReadLine();
        //Experiment.TestDispaSET();

        // Experiment.TestRCUC200() ;
        // Experiment.AGAINLEL();
        //Experiment.ProgramTest4();
        //december experimenten
        // Experiment.AllTDSUCInstances.ToList().ForEach(i => new Experiment().TDSUC(i, 0, 10, "TDSUC"));
        // Experiment.AllInstances.ToList().ForEach(i => new Experiment().ReserveTest(i, 0, 10, "Reserve2"));
        //new Experiment().AdditionalTests();
        //Experiment.TestTransmission();
        //Experiment.BaseTestAll();
        // new List<string> { "RCUC200.uc" }.ForEach(i => new Experiment().PieceWiseTest(i));
        // Experiment.TransmissionTest();



        // new Experiment().AdditionalTests();
        // Experiment.DemandMultiplierTestTrans();
        // Experiment.DemandMultiplierTestTrans3();
        // Experiment.ProgramTest3();
        //Experiment.DemandMultiplierTestRelax();
        // Experiment.AdditionalTests();
        //Experiment.DemandMultiplierTestRamp();
        //Experiment.DemandMultiplierTestMD();
        //instanceNames.ToList().ForEach(name => Experiment.DemandMultiplierTest(name));

        //Experiment.TransmissionTest("RTS26.uc");
        //var instanceNames = new string[] { "RTS26.uc","RTS54.uc","RTS96.uc", "DispaSET.uc", "DispaSET2.uc", "Bas.uc" };
        // var instanceNames = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS54.uc" };
        //instanceNames.ToList().ForEach(name => RampTest(name));
        //instanceNames.ToList().ForEach(name => MinUpDownTimeTest(name));
        //instanceNames.ToList().ForEach(name => Experiment.ATransmissionTest(name));
        //instanceNames.ToList().ForEach(name => RelaxTest(name));
        //instanceNames.ToList().ForEach(name => Experiment.ARelaxTest(name));
    }
}
