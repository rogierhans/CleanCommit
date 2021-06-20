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

        public static string[] TYDNPInstances = new string[] { "DE_2030", "GA_2030", "NT_2030", "DE_2040", "GA_2040", "NT_2040" };

        public void Foto(string filename, string nameOutput)
        {

            var outputLines = File.ReadAllLines(filename).ToList();
            var outputDispatch = GetLineInterval("Dispatch", outputLines);
            int totalTime = 72;
            int totalUnit = outputDispatch[0].Split(';').Length;
            double[,] dispatch = new double[totalTime, totalUnit];
            for (int i = 0; i < totalTime; i++)
            {
                var input = outputDispatch[i].Split(';');
                for (int g = 0; g < totalUnit; g++)
                {
                    dispatch[i, g] = double.Parse(input[g]);
                }
            }

            var resDipstach = GetLineInterval("ResDispatch", outputLines);
            int totalRUnit = resDipstach[0].Split(';').Length;
            double[,] Rdispatch = new double[totalTime, totalRUnit];
            for (int i = 0; i < totalTime; i++)
            {
                var input = resDipstach[i].Split(';');
                for (int g = 0; g < totalRUnit; g++)
                {
                    Rdispatch[i, g] = double.Parse(input[g]);
                }
            }
            var storageDispatch = GetLineInterval("StorageDischarge", outputLines);
            int totalSUnit = storageDispatch[0].Split(';').Length;
            double[,] Sdispatch = new double[totalTime, totalSUnit];
            for (int i = 0; i < totalTime; i++)
            {
                var input = storageDispatch[i].Split(';');
                for (int g = 0; g < totalSUnit; g++)
                {
                    Sdispatch[i, g] = double.Parse(input[g]);
                }
            }
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, false, 1, false);
            CC.SetLimits(0, totalTime);
            PowerSystem PS = IOUtils.GetPowerSystem(@"C:\Users\4001184\Google Drive\Data\Github\ACDCESMSmall.uc");

            var test = new Print();
            test.PrintUCED(PS, CC, dispatch, Sdispatch, Rdispatch, nameOutput);
        }

        static public List<string> GetLineInterval(string identifier, List<string> lines)
        {
            string begin = "<" + identifier + ">"; string end = "</" + identifier + ">";
            bool skip = true;
            List<string> newLines = new List<string>();
            foreach (var line in lines)
            {
                if (end == line)
                    skip = true;
                if (!skip)
                    newLines.Add(line);
                if (begin == line)
                    skip = false;
            }
            return newLines;
        }
        public void TestGA10()
        {
            foreach (var instance in TYDNPInstances)
            {
                string filename = @"C:\Users\4001184\Google Drive\Data\Github\"+instance +".uc";
                //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
                //  CC.Adequacy = true;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PS = IOUtils.GetPowerSystem(filename);
                Run();
                void Run()
                {
                    TightSolver TS = new TightSolver(PS, CC);
                    TS.ConfigureModel();
                    var output = TS.Solve(36000, 1);
                    //Console.ReadLine();
                    output.WriteToCSV(@"C:\Users\4001184\Desktop\DesktopOutput\", instance);
                }

            }


        }

        public void Test0()
        {
            string filename = @"C:\Users\Rogier\Desktop\TransTemp\0.uc";
            //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, true);
            //CC.Adecuacy = true;
            Console.WriteLine(filename);
            CC.SetLimits(0, 72);
            PowerSystem PS = IOUtils.GetPowerSystem(filename);
            Run();
            void Run()
            {
                TightSolver TS = new TightSolver(PS, CC);
                TS.ConfigureModel();
                var output = TS.Solve(36000, -418942184);
                //Console.ReadLine();
                output.WriteToCSV(@"C:\Users\Rogier\Desktop\DesktopOutput\", "0");
                //Process myProcess = new Process();
                //Process.Start("notepad++.exe", filename);
                //Console.ReadLine();
            }
        }

        public void Dump()
        {

            string filename = @"C:\Users\Rogier\Google Drive\Data\Github\ACDCESMSmall.uc";
            //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, true);
            //CC.Adecuacy = true;
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PS = IOUtils.GetPowerSystem(filename);
            var dict = new Dictionary<string, HashSet<double>>();
            foreach (var unit in PS.Units)
            {
                if (!dict.ContainsKey(unit.PrintType))
                {
                    dict[unit.PrintType] = new HashSet<double>();
                }
                dict[unit.PrintType].Add(unit.B);
            }
            File.WriteAllLines(@"C:\Users\Rogier\Desktop\grkn.txt", dict.Select(kvp => kvp.Key + "={" + string.Join(",", kvp.Value.OrderBy(x => x).Select(x => Math.Round(x, 1))) + "}"));

            PS.Dump();

        }
        public void TestGA102()
        {
            foreach (double fraction in new List<double> { 1.01, 1.005, 1.001 })
            {
                string filename = @"C:\Users\4001184\Google Drive\Data\Github\ACDCESMSmall.uc";
                //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, true);
                //CC.Adecuacy = true;
                Console.WriteLine(filename);
                CC.SetLimits(0, 72);
                PowerSystem PS = IOUtils.GetPowerSystem(filename);
                //  var newPS = PS.GetPowerSystemAtNode(PS.Nodes[1], new double[1000].ToList(), new double[1000].ToList());


                //PowerSystem.Units.ForEach(unit => unit.ReduceLimitHACK());
                //double multiplier = LossOfLoadDemandIncrement(PowerSystem, CC);
                //CC.DemandMultiplier = multiplier;
                //CC.Adequacy =true;
                // Transmission();
                Run();

                void Run()
                {
                    TightSolver TS = new TightSolver(PS, CC);
                    //TS.ConfigureModel();
                    // TS.SolveMin(36000, fraction);
                    //TS.Kill();

                    //TS = new TightSolver(PS, CC);
                    TS.ConfigureModel();
                    TS.SolveMax(36000, fraction);
                    TS.Kill();
                }
            }
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
            var output = TS.SolveMin(600, 0);
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
            var output = TS.SolveMin(600, 0);
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
                var output = TS.SolveMin(600, 0);
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
                var output = TS.SolveMin(600, 0);
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
