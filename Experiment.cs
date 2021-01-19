using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.MIP;
using CleanCommit.Instance;
using System.IO;
namespace CleanCommit
{
    class Experiment
    {
        public static string[] AllTDSUCInstances = new string[] { "GA10.uc", "A110.uc", "KOR140.uc", "OSTRO182.uc", "HUB223.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };

        public static string[] AllInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS26.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };
        public static string[] RedoInstances = new string[] { "RCUC200.uc", "RTS26.uc", "FERC934.uc", "CA610.uc" };

        public static string[] FastInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "DispaSET.uc", "RTS26.uc" };
        public void Test3Node()
        {
            string filename = @"C:\Users\Rogier\Dropbox\Data\NewInstances\Test\RTS26.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, false);
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);

            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
        }


        public void TestGA10()
        {
            //string filename = @"C:\Users\4001184\Google Drive\Data\Github\ACDCESM.uc";
            string filename = @"C:\Users\Rogier\Google Drive\Data\Github\ACDCESM.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
            //CC.Adecuacy = true;
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PS = IOUtils.GetPowerSystem(filename);
            var newPS = PS.GetPowerSystemAtNode(PS.Nodes.First(), new double[24].ToList(), new double[24].ToList());
               

            //PowerSystem.Units.ForEach(unit => unit.ReduceLimitHACK());
            //double multiplier = LossOfLoadDemandIncrement(PowerSystem, CC);
            //CC.DemandMultiplier = multiplier;
            //CC.Adequacy =true;
            // Transmission();
            Run();
            void Run()
            {
                TightSolver TS = new TightSolver(newPS, CC);
                TS.ConfigureModel();
                var output = TS.Solve2(36000);
                //Console.ReadLine();
                output.WriteToCSV(@"C:\Users\Rogier\Desktop\DesktopOutput\","");
            }

            //void Transmission() {
            //    TightSolver TS = new TightSolver(PowerSystem, CC);
            //    TS.ConfigureModel();
            //    var injections= TS.GetTransmissionOutput(600);
            //    List<string> Lines = new List<string>();
            //    for (int n = 0; n < injections.GetLength(0); n++)
            //    {
            //        string line = "";
            //        for (int t = 0; t < injections.GetLength(1); t++)
            //        {
            //            line += Math.Round(injections[n, t]) + "\t";
            //        }
            //        Console.WriteLine(line);
            //        Lines.Add(line);
            //    }
            //    File.WriteAllLines(@"C:\Users\Rogier\Desktop\DesktopOutput\" + "transsolution.csv", Lines);
            //}

        }
        public void TestRCUC200()
        {
            string filename = @"C:\Users\Rogier\Dropbox\Data\NewInstances\RCUC200.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, false, 1, false);
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
            TS.ConfigureModel();
            var output = TS.Solve2(600);
        }
        public void TestTransmission()
        {
            string filename = @"C:\Users\Rogier\Dropbox\Data\NewInstances\Bas.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, true);
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
        }

        public void BaseTest()
        {
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, true);
            string filename = U.InstanceFolder + "CA610.uc";
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
            TS.ConfigureModel();
            var output = TS.Solve2(600);
            output.WriteToCSV(@"C:\Users\Rogier\Desktop\TEMPoutput\IP.csv", "");
            Console.ReadLine();
        }

        public void BaseTestAll()
        {
            foreach (var instance in AllInstances)
            {
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, true);
                string filename = U.InstanceFolder + instance;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                TightSolver TS = new TightSolver(PowerSystem, CC, 0.00001);
                TS.ConfigureModel();
                var output = TS.Solve2(600);
                string line = instance + " " + output.GurobiCost + "\n";
                File.AppendAllText(@"C:\Users\Rogier\Desktop\outputLP.txt", line);
                //Console.ReadLine();
            }
        }

        public static double LossOfLoadDemandIncrement(PowerSystem PS, ConstraintConfiguration CC)
        {
            var copyCC = CC.Copy();
            Console.WriteLine(CC.ToString());
            Console.WriteLine(copyCC.ToString());

            copyCC.Relax = true;
            copyCC.AdecuacyTest();
            double demandMultiplier = 1;
            double delta = 1;
            for (int i = 0; i < 20; i++)
            {
                copyCC.DemandMultiplier = demandMultiplier;
                TightSolver TS = new TightSolver(PS, copyCC);
                TS.ConfigureModel();
                var output = TS.Solve2(600);
                var LOL = output.TotalLossOfLoad;
                var totalDemand = PS.TotalDemand();
                Console.WriteLine(totalDemand);
                if ((LOL / totalDemand) < 0.001)
                {
                    demandMultiplier += delta;
                    delta = delta / 2;
                }
                else
                {
                    demandMultiplier -= delta;
                    delta = delta / 2;
                }
                Console.WriteLine((LOL / totalDemand));
                Console.WriteLine(demandMultiplier);
                TS.Kill();
            }
            return demandMultiplier;
        }
    }
}
