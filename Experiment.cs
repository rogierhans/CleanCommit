using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCommit.MIP;
using CleanCommit.Instance;
using System.Diagnostics;
using System.IO;
namespace CleanCommit
{
    class Experiment
    {
        public static string[] AllTDSUCInstances = new string[] { "GA10.uc", "A110.uc", "KOR140.uc", "OSTRO182.uc", "HUB223.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };
        public static string[] AllInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS26.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };
        public static string[] RedoInstances = new string[] { "RCUC200.uc", "RTS26.uc", "FERC934.uc", "CA610.uc" };
        public static string[] FastInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "DispaSET.uc", "RTS26.uc" };

        public static string[] TYDNPInstances = new string[] { "DE_2040", "GA_2040", "NT_2040", "GA_2030", "NT_2030", "DE_2030" };
         //public static string[] TYDNPInstances = new string[] { "DE_2030"};
       // public static string[] TYDNPInstances = new string[] { "GA_2030", "NT_2030", "DE_2040", "GA_2040", "NT_2040" };
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
        public void YearTests()
        {
            foreach (var instance in TYDNPInstances)
            {
                string filename = @"C:\Users\" + Environment.UserName + @"\Google Drive\Data\Github\" + instance + ".uc";
                //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";
                var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
                CC.Adequacy = true;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24 * 30);

                //CC.Reserves.Add(new Reserve(0.01, 0, 1.0 / 12, 0, 0));
                //CC.Reserves.Add(new Reserve(0, 3000, 1.0 / 6, 0, 0));
                //CC.Reserves.Add(new Reserve(0, 0, 1, 0.12, 0.10));
                PowerSystem PS = IOUtils.GetPowerSystem(filename);


                //string xmlFilename = @"C:\Users\" + Environment.UserName + @"\Desktop\" + instance + ".bin";
                //BinarySerialization.WriteToBinaryFile<PowerSystem>(xmlFilename, PS);
                //return;

                Run();
                void Run()
                {
                    TightSolver TS = new TightSolver(PS, CC);
                    TS.ConfigureModel();
                    var output = TS.NewSolve(36000, 1);
                    //Console.ReadLine();
                    List<object> cells = new List<object>() { output.GurobiCost, output.GurobiCostLOL, output.GurobiCostLOR, output.GurobiCostGeneration, output.GurobiCostCycle };
                    var line = string.Join("\t", cells);
                    File.AppendAllText(@"C:\Users\" + Environment.UserName + @"\Desktop\log.txt", line + "\n");
                    //output.WriteToCSV(@"C:\Users\" + Environment.UserName + @"\Desktop\DesktopOutput\", instance);
                    string binFilename = @"C:\Users\" + Environment.UserName + @"\Desktop\" + instance + ".bin";
                    BinarySerialization.WriteToBinaryFile<Solution>(binFilename, output);

                    // Then in some other function.
                    var otherSolverOutput = BinarySerialization.ReadFromBinaryFile<Solution>(binFilename);
                    TS.Kill();
                }

            }
        }

 

        public void Wrapper()
        {

            int timehorizon = 8760;
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
            // CC.Adequacy = true;
            CC.SetLimits(0, timehorizon);
            CC.Reserves.Add(new Reserve(0.01, 0, 1.0 / 12, 0, 0));
            CC.Reserves.Add(new Reserve(0, 3000, 1.0 / 6, 0, 0));
            CC.Reserves.Add(new Reserve(0, 0, 1, 0.12, 0.10));
            Expriment8(CC,"ComplexYear");


            //CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.TradeBased, false, false, 1, false);
            //CC.Adequacy = true;
            //CC.SetLimits(0, 200);
            //CC.Reserves.Add(new Reserve(0.01, 0, 1.0 / 12, 0, 0));
            //CC.Reserves.Add(new Reserve(0, 3000, 1.0 / 6, 0, 0));
            //CC.Reserves.Add(new Reserve(0, 0, 1, 0.12, 0.10));
            //Expriment8(CC);

            //CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
            //CC.Adequacy = true;
            ////CC.Reserves.Add(new Reserve(0.01, 0, 1.0 / 12, 0, 0));
            ////CC.Reserves.Add(new Reserve(0, 3000, 1.0 / 6, 0, 0));
            ////CC.Reserves.Add(new Reserve(0, 0, 1, 0.12, 0.10));
            //CC.SetLimits(0, timehorizon);
            //Expriment8(CC,"Simple");
        }
        public void Expriment8(ConstraintConfiguration CC, string extra)
        {
            foreach (var instance in TYDNPInstances)
            {
                string filename = @"C:\Users\4001184\Google Drive\Data\Github\" + instance + ".uc";
                //string filename = @"C:\Users\Rogier\Google Drive\Data\Github\RCUC200.uc";

                PowerSystem PS = IOUtils.GetPowerSystem(filename);
                Run();
                void Run()
                {
                    TightSolver TS = new TightSolver(PS, CC);
                    TS.ConfigureModel();
                    var output = TS.NewSolve(36000, 1);
                    //Console.ReadLine();
                    List<object> cells = new List<object>() {
                        PS.ToString(),
                        CC.ToString(),
                        output.GurobiCost,
                        output.GurobiCostLOL,
                        output.GurobiCostLOR,
                        output.GurobiCostGeneration,
                        output.GurobiCostCycle,
                        output.ComputationTime };
                    var line = string.Join("\t", cells);
                    File.AppendAllText(@"C:\Users\4001184\Desktop\log.txt", line + "\n");
                    output.ToCSV(@"C:\Users\4001184\Desktop\UC\" + PS.Name.Split('.').First() +"_" + extra+ ".csv");
                    output.ToBin(@"C:\Users\4001184\Desktop\UC\" + PS.Name.Split('.').First() + "_" + extra + ".bin");
                    TS.Kill();
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
            CC.SetLimits(0, 672);
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
        public void Test()
        {

            //  string filename = @"C:\Users\4001184\Google Drive\Data\Github\ACDCESMSmall.uc";
            string filename = @"C:\Users\Rogier\Google Drive\Data\Github\FERC923.uc";
            var CC = new ConstraintConfiguration(false, false, ConstraintConfiguration.TransmissionType.TradeBased, false, true, 1, false);
            CC.Adequacy = true;
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PS = IOUtils.GetPowerSystem(filename);
            Run();

            void Run()
            {
                TightSolver TS = new TightSolver(PS, CC);
                TS.ConfigureModel();
                TS.Solve(36000, 1);
                TS.Kill();
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
    }
}
