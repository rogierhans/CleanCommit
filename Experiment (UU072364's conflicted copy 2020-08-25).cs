using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtrechtCommitment.Constraints;
using UtrechtCommitment.Instance;
using System.IO;
namespace UtrechtCommitment
{
    class Experiment
    {
        public static string[] AllTDSUCInstances = new string[] { "GA10.uc", "A110.uc", "KOR140.uc", "OSTRO182.uc", "HUB223.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };

        public static string[] AllInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS26.uc", "RTS54.uc", "RTS96.uc", "FERC934.uc", "CA610.uc", "GMLC73.uc" };
        public static string[] RedoInstances = new string[] { "RCUC200.uc", "RTS26.uc", "FERC934.uc", "CA610.uc" };

        public static string[] FastInstances = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "DispaSET.uc", "RTS26.uc" };
        public void Test3Node()
        {
            string filename = @"C:\Users\Rogier\Dropbox\Data\NewInstances\Test\GA10.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, true);
            Console.WriteLine(filename);
            CC.SetLimits(0, 24);
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
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
            var output = TS.Solve(600);
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
                var output = TS.Solve(600);
                string line = instance + " " + output.GurobiCost + "\n";
                File.AppendAllText(@"C:\Users\Rogier\Desktop\outputLP.txt", line);
                //Console.ReadLine();
            }
        }
        public void ReserveTest(string instanceName, int start, int n, string outputname)
        {
            for (int i = start; i < n; i++)
            {

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                string filename = U.InstanceFolder + instanceName;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                PowerSystem.PeturbDemand(100);
                var output1 = CSW(PowerSystem, CC, i);
                PowerSystem.PercentageDemandForReserve = 0.05;
                var output2 = CSW(PowerSystem, CC, i);
                PowerSystem.PercentageDemandForReserve = 0.10;
                var output3 = CSW(PowerSystem, CC, i);
                PowerSystem.PercentageDemandForReserve = 0.20;
                var output4 = CSW(PowerSystem, CC, i);
                List<string> cells = new List<string>();
                cells.Add(output1.time.ToString());
                cells.Add(output2.time.ToString());
                cells.Add(output3.time.ToString());
                cells.Add(output4.time.ToString());
                var line = PowerSystem.Name + " ; " + i + " ; " + String.Join(" ; ", cells);
                U.Write(U.LogFolder + outputname + @".txt", line);
            }
            Output CSW(PowerSystem PS, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PS, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\" + outputname + @"\", i.ToString());
                TS.Kill();
                return output;
            }
        }

        public void TDSUC(string instanceName, int start, int n, string outputname)
        {
            for (int i = start; i < n; i++)
            {

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                string filename = U.InstanceFolder + instanceName;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                PowerSystem.PeturbDemand(100);
                var output1 = CSW(PowerSystem, CC, i);
                CC.TimeDependantStartUpCost = false;
                var output2 = CSW(PowerSystem, CC, i);
                List<string> cells = new List<string>();
                cells.Add(output1.time.ToString());
                cells.Add(output2.time.ToString());
                var line = PowerSystem.Name + " ; " + i + " ; " + String.Join(" ; ", cells);
                U.Write(U.LogFolder + outputname + @".txt", line);
            }
            Output CSW(PowerSystem PS, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PS, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\" + outputname + @"\", i.ToString());
                return output;
            }
        }



        public static void ProgramTest1()
        {
            string filename = U.InstanceFolder + "GA10.uc";
            SolverOutput o1 = null;
            SolverOutput o2 = null;
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, false, 1, true);
            CC.SetLimits(0, 24);
            {

                ////CC.AdecuacyTest();
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                //PowerSystem.PeturbDemand(100);
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve2(600);
                o1 = output;
                output.WriteToCSV(U.OutputFolder + @"Test\", "base");
                List<object> os = new List<object>() { Math.Round(output.time), CC.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), output.GurobiCost };
                string line = String.Join("\t", os.Select(o => o.ToString()));
                U.Write(U.OutputFolder + @"PROGRAMTEST.txt", line);
            }
            return;
            {
                CC.Tight = false;
                ////CC.AdecuacyTest();
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                //PowerSystem.PeturbDemand(100);
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve2(600);
                o2 = output;
                output.WriteToCSV(U.OutputFolder + @"Test\", "false");
                List<object> os = new List<object>() { Math.Round(output.time), CC.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), output.GurobiCost };
                string line = String.Join("\t", os.Select(o => o.ToString()));
                U.Write(U.OutputFolder + @"PROGRAMTEST.txt", line);
            }
            Console.WriteLine(CalculateGap(o1.GurobiCost, o2.GurobiCost).ToString("0.##E+0"));
            Console.WriteLine(CalculateGap(o1.GurobiCost, o2.GurobiCost));
            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void ProgramTest3()
        {
            var instanceNames = new string[] { "bas.uc", "COSTRO182.uc", "DispaSet2.uc" };
            var ccc = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, false, 1, false);
            ccc.SetLimits(0, 24);

            foreach (var instanceName in instanceNames)
                for (int i = 0; i < 30; i++)
                {
                    List<object> cells = new List<object>();
                    string filename = U.InstanceFolder + instanceName;
                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                    PowerSystem.PeturbDemand(100);
                    ccc.Clustered = true;
                    var output1 = CSW(PowerSystem, ccc, i);
                    ccc.Clustered = false;
                    PowerSystem.UnCluster();
                    var output2 = CSW(PowerSystem, ccc, i);

                    cells.Add(output1.time);
                    cells.Add(output2.time);
                    cells.Add(CalculateGap(output1.GurobiCost, output2.GurobiCost));
                    var line = PowerSystem.Name + " ; " + i + " ; " + String.Join(";", cells);
                    //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                    //string line = String.Join("\t", os.Select(o => o.ToString()));
                    U.Write(U.LogFolder + @"cluster.txt", line);
                }

            Output CSW(PowerSystem PS, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PS, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\cluster\", i.ToString());
                return output;
            }
            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void ProgramTest4()
        {
            //var fileNames = new List<string> { "RTS26.uc" };
            //// var fileNames = new List<string> { "FERC934.uc", "CA610.uc", "GMLC73.uc", "RTS96.uc", "RTS26.uc" };
            //var ccc = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, true, 1, false);
            //ccc.SetLimits(0, 24);

            //foreach (var instanceName in fileNames)
            //{
            //    List<object> cells = new List<object>();
            //    string filename = U.InstanceFolder + instanceName;
            //    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            //    PowerSystem.Test(ccc);
            //    Console.ReadLine();
            //    PowerSystem.PeturbDemand(100);
            //    TightSolver TS = new TightSolver(PowerSystem, ccc);
            //    TS.ConfigureModel();
            //    var output = TS.Solve(600);
            //}
        }
        public void ProgramTest2()
        {
            string filename = U.InstanceFolder + "RTS26.uc";
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, true, true, 1, false);
            CC.SetLimits(0, 24);
            CC.DemandMultiplier = 2;
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            TightSolver TS = new TightSolver(PowerSystem, CC);
            TS.ConfigureModel();
            var output = TS.Solve(600);
            List<object> os = new List<object>() { Math.Round(output.time), CC.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), output.GurobiCost, output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
            string line = String.Join("\t", os.Select(o => o.ToString()));
            U.Write(U.LogFolder + @"PROGRAMTEST.txt", line);
            //LossOfLoadDemandIncrement(PowerSystem, CC);
        }
        public void AdditionalTests()
        {
            var fileNames = new List<string> { "FERC934.uc", "HUB223.uc", "CA610.uc", "GMLC73.uc" };
            foreach (var name in fileNames)
            {
                string filename = U.InstanceFolder + name;
                //var instanceNames = new List<string>();
                //instanceFolderName.ForEach(folderName => new DirectoryInfo(U.InstanceFolder + folderName).GetFiles().ToList().ForEach(file => instanceNames.Add(file.FullName)));
                //Relax
                RelaxTest(filename, 30, "relax4");
                //MinUpDown
                MinUpDownTimeTest(filename, 30, "mumd4");
                //Ramp   
                RampTest(filename, 0, 30, "ramp4");
            }
        }

        private static void RelaxTest(string filename, int n, string outputFileName)
        {
            for (int i = 0; i < n; i++)
            {
                {
                    var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                    //string filename = U.InstanceFolder + instanceName;
                    Console.WriteLine(filename);
                    CC.SetLimits(0, 24);
                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                    PowerSystem.PeturbDemand(100);

                    var output1 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output2 = CSW(PowerSystem, CC, i);

                    CC.Relax = false;
                    CC.Tight = true;
                    var output3 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output4 = CSW(PowerSystem, CC, i);


                    var line = PowerSystem.Name + " ; " + i + " ; " + output1.GetRelaxTestOutput(output1, output2, output3, output4);
                    //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                    //string line = String.Join("\t", os.Select(o => o.ToString()));
                    U.Write(U.LogFolder + outputFileName + ".txt", line);
                }
                System.GC.Collect();
            }
            Output CSW(PowerSystem PowerSystem, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\" + outputFileName + @"\", i.ToString());
                TS.Kill();
                return output;
            }

        }

        public static void AGAINLEL()
        {
           // FastInstances.ToList().ForEach(name => MotherTest(name, 10, 168, "Week30"));
            AllInstances.ToList().ForEach(name => MotherTest(name, 10, 24, "Day20"));

        }

        private static void MotherTest(string instanceName, int n, int timsteps, string outputFileName)
        {
            string filename = U.InstanceFolder + instanceName;
            Dictionary<string, List<List<double>>> Summary = new Dictionary<string, List<List<double>>>();
            for (int i = 0; i < n; i++)
            {
                {
                    var BaseCC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, true, false, 1, false);
                    ConstraintConfiguration CC = null;
                    Console.WriteLine(filename);
                    BaseCC.SetLimits(0, timsteps);
                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                    PowerSystem.PeturbDemand(100);
                    PowerSystem.PercentageDemandForReserve = 0.10;

                    //Base
                    CC = BaseCC.Copy();
                    var baseOutput = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, baseOutput, PowerSystem.NameOK(), "Base", i);


                    //Tight
                    CC = BaseCC.Copy();
                    CC.Tight = true;
                    var Tight = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, Tight, PowerSystem.NameOK(), "Tight", i);


                    //LP
                    CC = BaseCC.Copy();
                    CC.Relax = true;
                    var LP = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, LP, PowerSystem.NameOK(), "LP", i);

                    //TLP
                    CC = BaseCC.Copy();
                    CC.Relax = true;
                    CC.Tight = true;
                    var TLP = CSW(PowerSystem, CC, i);
                    Handle(Tight, TLP, PowerSystem.NameOK(), "TLP", i);

                    //RLP
                    CC = BaseCC.Copy();
                    CC.RampingLimits = false;
                    var RLP = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, RLP, PowerSystem.NameOK(), "RLP", i);

                    //MUMD
                    CC = BaseCC.Copy();
                    CC.MinUpMinDown = false;
                    var MUMD = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, MUMD, PowerSystem.NameOK(), "MUMD", i);

                    //TDSUC
                    if (AllTDSUCInstances.Contains(instanceName))
                    {
                        CC = BaseCC.Copy();
                        CC.TimeDependantStartUpCost = false;
                        var TDSUC = CSW(PowerSystem, CC, i);
                        Handle(baseOutput, TDSUC, PowerSystem.NameOK(), "TDSUC", i);
                    }

                    //Reserve
                    CC = BaseCC.Copy();
                    PowerSystem.PercentageDemandForReserve = 0;
                    var Reserve = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, Reserve, PowerSystem.NameOK(), "Reserve", i);

                    //All 
                    CC = BaseCC.Copy();
                    CC.MinUpMinDown = false;
                    CC.RampingLimits = false;
                    CC.TimeDependantStartUpCost = false;
                    var All = CSW(PowerSystem, CC, i);
                    Handle(baseOutput, All, PowerSystem.NameOK(), "All", i);

                    //var line = PowerSystem.Name + " ; " + i + " ; " + baseOutput.GetRelaxTestOutput(baseOutput, output2, Tight, TLP);
                    //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                    //string line = String.Join("\t", os.Select(o => o.ToString()));
                    //  U.Write(U.LogFolder + outputFileName + ".txt", line);
                }
                System.GC.Collect();

            }
            foreach (var kvp in Summary)
            {
                List<double> cellsTotal = new List<double>();
                for (int i = 0; i < kvp.Value[0].Count; i++)
                {
                    cellsTotal.Add(0);
                }
                foreach (var list in kvp.Value)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        cellsTotal[i] += list[i];
                    }
                }

                string line = instanceName + " ; " + kvp.Key + " ; " + String.Join(" ; ", cellsTotal.Select(cell => cell / kvp.Value.Count));
                U.Write(U.LogFolder + outputFileName + "Total.csv", line);
            }
            Output CSW(PowerSystem PowerSystem, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\20\" + outputFileName + @"\", i.ToString());
                TS.Kill();
                return output;
            }
            void Handle(Output Base, Output Relax, string powersystemName, string name, int i)
            {
                var cells = Compare(Base, Relax);
                if (!Summary.ContainsKey(name)) Summary[name] = new List<List<double>>();
                Summary[name].Add(cells);
                string line = powersystemName + " ; " + name + " ; " + i + " ; " + String.Join(" ; ", cells);
                U.Write(U.LogFolder + outputFileName + ".csv", line);
            }

        }
        public static List<double> Compare(Output Base, Output Relax)
        {
            List<double> cells = new List<double>();
            cells.Add(Relax.time);
            cells.Add(Relax.Gap);
            cells.Add(Base.time / Relax.time);
            cells.Add(HammingDistanceUnitCommitment(Base, Relax));
            cells.Add(DeltaCF(Base, Relax).Average());
            cells.Add(DeltaCF(Base, Relax).Max());
            cells.Add(CalculateGap(Base.GurobiCost, Relax.GurobiCost));
            cells.Add(Relax.CountFractionalRatio());
            double rampupViolations = (Relax.totalRampUpViolations + Relax.totalStartUpViolations) / ((double)Relax.CountMoments());
            double rampDownVio = (Relax.totalRampDownViolations + Relax.totalShutDownViolations) / ((double)Relax.CountMoments());
            cells.Add(rampupViolations);
            cells.Add(rampDownVio);
            cells.Add(((double)Relax.totalUpTimeViolation / Relax.CountStops()));
            cells.Add(((double)Relax.totalDownTimeViolation / Relax.CountStarts()));
            return cells;
            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }

        public static List<double> DeltaCF(Output Base, Output Relax)
        {
            var CF = Base.CapcityFactors;
            var otherCF = Relax.CapcityFactors;
            List<double> deltas = new List<double>();
            for (int i = 0; i < CF.Count; i++)
            {
                var delta = Math.Abs(CF[i] - otherCF[i]);
                deltas.Add(delta * 100);
            }
            return deltas;
        }
        public static double HammingDistanceUnitCommitment(Output output1, Output output2)
        {
            double distance = 0;
            for (int i = 0; i < output1.totalTime; i++)
            {
                for (int u = 0; u < output1.totalUnits; u++)
                {
                    distance += Math.Abs(output2.CommitStatus[i, u] - output1.CommitStatus[i, u]);
                }
            }
            return distance / (output1.totalTime * output1.totalUnits);
        }

        public static void Test()
        {
            foreach (string instanceName in AllInstances)
            {
                string filename = U.InstanceFolder + instanceName;
                Console.WriteLine(filename);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                double multiplier = LossOfLoadDemandIncrement(PowerSystem, CC);
                CC.DemandMultiplier = multiplier;
                // U.L(multiplier);
                if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                for (int i = 0; i < 10; i++)
                {
                    PowerSystem = IOUtils.GetPowerSystem(filename);
                    PowerSystem.PeturbDemand(1000);
                    var output1 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output2 = CSW(PowerSystem, CC, i);
                    CC.Relax = false;
                    CC.Tight = true;
                    var output3 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output4 = CSW(PowerSystem, CC, i);
                    var line = instanceName + " ; " + i + " ; " + output1.GetRelaxTestOutput(output1, output2, output3, output4);
                    //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                    //string line = String.Join("\t", os.Select(o => o.ToString()));
                    U.Write(U.LogFolder + @"Relax.txt", line);
                }
            }
            Output CSW(PowerSystem PowerSystem, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"C:\Users\4001184\Google Drive\Output\", i.ToString());
                TS.Kill();
                return output;
            }
        }
        public void PieceWiseTest(string instanceName)
        {

            //var instanceNames = new string[] { "RTS96.uc" };
            // foreach (var instanceName in instanceNames)
            {
                string filename = U.InstanceFolder + instanceName;
                for (int i = 0; i < 10; i++)
                {
                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                    var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                    CC.SetLimits(0, 24);
                    PowerSystem.PeturbDemand(100);
                    var output1 = HCSW("PieceWise", PowerSystem, CC, i);
                    CC.PiecewiseSegments = 2;
                    var output2 = HCSW("PieceWise", PowerSystem, CC, i);
                    CC.PiecewiseSegments = 3;
                    var output3 = HCSW("PieceWise", PowerSystem, CC, i);
                    CC.PiecewiseSegments = 4;
                    var output4 = HCSW("PieceWise", PowerSystem, CC, i);
                    CC.PiecewiseSegments = 5;
                    var output5 = HCSW("PieceWise", PowerSystem, CC, i);
                    CC.PiecewiseSegments = 10;
                    var output6 = HCSW("PieceWise", PowerSystem, CC, i);
                    List<string> cells = new List<string>();
                    cells.Add(output1.time.ToString());
                    cells.Add(output2.time.ToString());
                    cells.Add(output3.time.ToString());
                    cells.Add(output4.time.ToString());
                    cells.Add(output5.time.ToString());
                    cells.Add(output6.time.ToString());
                    cells.Add(CalculateGap(output1.CalculateGenerationCost(), output1.RealGenerationCost).ToString());
                    cells.Add(CalculateGap(output2.CalculateGenerationCost(), output2.RealGenerationCost).ToString());
                    cells.Add(CalculateGap(output3.CalculateGenerationCost(), output3.RealGenerationCost).ToString());
                    cells.Add(CalculateGap(output4.CalculateGenerationCost(), output4.RealGenerationCost).ToString());
                    cells.Add(CalculateGap(output5.CalculateGenerationCost(), output5.RealGenerationCost).ToString());
                    cells.Add(CalculateGap(output6.CalculateGenerationCost(), output6.RealGenerationCost).ToString());
                    double CalculateGap(double baseValue, double value)
                    {
                        return (baseValue - value) / baseValue;
                    }
                    var line = instanceName + " ; " + i + " ; " + String.Join(" \t; ", cells);
                    U.Write(U.LogFolder + @"PieceLast.txt", line);
                }

            }
            Output HCSW(string folder, PowerSystem PowerSystem, ConstraintConfiguration CC, int i)
            {
                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"C:\Users\4001184\Google Drive\Output\", i.ToString());
                TS.Kill();
                return output;
            }
        }



        private static void RampTest(string filename, int start, int n, string outputname)
        {
            for (int i = start; i < n; i++)
            {

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                var maxComputationTime = 600;
                //string filename = U.InstanceFolder + instanceName;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                PowerSystem.PeturbDemand(100);

                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(maxComputationTime);
                output.WriteToCSV(U.OutputFolder + outputname + @"\", i.ToString());
                TS.Kill();
                CC.RampingLimits = false;
                TightSolver TS2 = new TightSolver(PowerSystem, CC);
                TS2.ConfigureModel();
                var output2 = TS2.Solve(maxComputationTime);
                output2.WriteToCSV(U.OutputFolder + outputname + @"\", i.ToString());
                TS2.Kill();

                var line = PowerSystem.Name + " ; " + i + " ; " + output.GetRampTestOutput(output2);
                //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                //string line = String.Join("\t", os.Select(o => o.ToString()));
                U.Write(U.LogFolder + outputname + @".txt", line);
            }
        }
        private static void MinUpDownTimeTest(string filename, int n, string outputname)
        {
            for (int i = 0; i < n; i++)
            {

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                var maxComputationTime = 600;
                //string filename = U.InstanceFolder + instanceName;
                Console.WriteLine(filename);
                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                PowerSystem.PeturbDemand(100);

                TightSolver TS = new TightSolver(PowerSystem, CC);
                TS.ConfigureModel();
                var output = TS.Solve(maxComputationTime);
                output.WriteToCSV(U.OutputFolder + outputname + @"\", i.ToString());
                TS.Kill();
                CC.MinUpMinDown = false;
                TightSolver TS2 = new TightSolver(PowerSystem, CC);
                TS2.ConfigureModel();
                var output2 = TS2.Solve(maxComputationTime);
                output2.WriteToCSV(U.OutputFolder + outputname + @"\", i.ToString());
                TS2.Kill();
                var line = PowerSystem.Name + " ; " + i + " ; " + output.GetMinTestOutput(output2);

                //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                //string line = String.Join("\t", os.Select(o => o.ToString()));
                U.Write(U.LogFolder + outputname + @".txt", line);
            }
        }

        public static void TransmissionTest()
        {
            foreach (var instanceName in new string[] { "RTS26.uc", "RTS54.uc", "RTS96.uc", "DispaSET.uc", "DispaSET2.uc", "Bas.uc" })
                for (int i = 0; i < 10; i++)
                {

                    var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, false, 1, false);
                    string folder = @"C:\Users\4001184\Google Drive\Output\";
                    var maxComputationTime = 600;
                    string filename = U.InstanceFolder + instanceName;
                    Console.WriteLine(filename);
                    CC.SetLimits(0, 24);
                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                    PowerSystem.PeturbDemand(100);
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    TightSolver TS = new TightSolver(PowerSystem, CC);
                    TS.ConfigureModel();
                    var outputCopper = TS.Solve(maxComputationTime);
                    outputCopper.WriteToCSV(folder + @"newTrans2\", i.ToString());
                    TS.Kill();

                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.TradeBased;
                    TightSolver TS2 = new TightSolver(PowerSystem, CC);
                    TS2.ConfigureModel();
                    var outputTrade = TS2.Solve(maxComputationTime);
                    outputTrade.WriteToCSV(folder + @"newTrans2\", i.ToString());
                    TS2.Kill();

                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.VoltAngles;
                    TightSolver TS3 = new TightSolver(PowerSystem, CC);
                    TS3.ConfigureModel();
                    var outputVoltAngles = TS3.Solve(maxComputationTime);
                    outputVoltAngles.WriteToCSV(folder + @"newTrans2\", i.ToString());
                    TS3.Kill();

                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.PTDF;
                    TightSolver TS4 = new TightSolver(PowerSystem, CC);
                    TS4.ConfigureModel();
                    var output4 = TS4.Solve(maxComputationTime);
                    output4.WriteToCSV(U.OutputFolder + @"newTrans2\", i.ToString());
                    TS4.Kill();
                    var line = instanceName + " ; " + i + " ; " + outputCopper.GetTransTestOutput(outputTrade, outputVoltAngles, output4);

                    //List<object> os = new List<object>() { Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                    //string line = String.Join("\t", os.Select(o => o.ToString()));
                    U.Write(U.LogFolder + @"newTrans6.txt", line);
                }
        }
        public static void ATransmissionTest(string instanceName)
        {
            string filename = U.InstanceFolder + instanceName;
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, true, false, 1, false);
            CC.SetLimits(0, 24);
            CC.AdecuacyTest();
            double multiplier = LossOfLoadDemandIncrement(PowerSystem, CC);
            CC.DemandMultiplier = multiplier;

            for (int i = 0; i < 1; i++)
            {
                if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                CC.TransmissionMode = ConstraintConfiguration.TransmissionType.Copperplate;
                var output1 = CSW(PowerSystem, CC, i);
                CC.TransmissionMode = ConstraintConfiguration.TransmissionType.TradeBased;
                var output2 = CSW(PowerSystem, CC, i);
                CC.TransmissionMode = ConstraintConfiguration.TransmissionType.VoltAngles;
                var output3 = CSW(PowerSystem, CC, i);
                CC.TransmissionMode = ConstraintConfiguration.TransmissionType.PTDF;
                var output4 = CSW(PowerSystem, CC, i);
                var line = instanceName + " ; " + i + " ; " + output1.GetTransTestOutput(output2, output3, output4);
                U.Write(U.OutputFolder + @"AnewTrans2.txt", line);
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                output.WriteToCSV(@"E:\Output\AnewTrans2\", i.ToString());
                return output;
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
                var output = TS.Solve(600);
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

        public static void CLusterInstance()
        {
            Dictionary<string, string> instanceFileName = new Dictionary<string, string>();
            foreach (string filename in U.ReadFolder(U.InstanceFolder).Select(kvp => kvp.Key))
            {
                instanceFileName[filename.Split('\\').Last().Split('.').First()] = U.InstanceFolder + filename;
            }

            List<string> clusterdInstances = new List<string>() { "COSTRO182", "DispaSET2", "Bas" };


            var TransmissionMode = new string[] { "Copperplate", "Flow", "Angles", "PDTF" };
            foreach (string instance in clusterdInstances)
            {

                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, false, true, 1, false);

                CC.SetLimits(0, 24);
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(instanceFileName[instance]);
                TightSolver TS = new TightSolver(PowerSystem, CC);
                PowerSystem.UnCluster();
                TS.ConfigureModel();
                var output = TS.Solve(600);
                List<object> os = new List<object>() { instance, 24, Math.Round(output.time), CC.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                string line = String.Join("\t", os.Select(o => o.ToString()));
                U.Write(@"C:\Users\4001184\Dropbox\Output\log11.txt", line);
                //PowerSystem.PrintUnits();
                //Console.ReadLine();
                //
                //PowerSystem.PrintUnits();
                //Console.ReadLine();
                continue;

                //    var lines = U.ReadFile(instanceFileName[instance]);
                //    bool hasQdata = lines[1].Split('=')[1] == "True";
                //    bool hasTimeDep = lines[2].Split('=')[1] == "True";
                //    bool hasTransmission = lines[3].Split('=')[1] == "True";
                //    int totalTime = int.Parse(lines[4].Split('=')[1]);
                //    foreach (var relax in new bool[] { false, true })
                //        foreach (var ramp in new bool[] { false, true })
                //            foreach (var minUpDown in new bool[] { false, true })
                //            {
                //                var transmissionDomian = hasTransmission ? TransmissionMode : new string[] { "Copperplate" };
                //                foreach (var transmission in transmissionDomian)
                //                {
                //                    var ConstraintConfiguration = new ConstraintConfiguration(ramp, minUpDown, transmission, true, false, relax, 1, false);
                //                    ConstraintConfiguration.SetLimits(0, 24, -1, -1);
                //                    ConstraintConfiguration.SetClusterd();
                //                    PowerSystem PowerSystem = IOUtils.GetPowerSystem(instanceFileName[instance], ConstraintConfiguration);

                //                    //PowerSystem.PrintUnits();
                //                    //PowerSystem.Test();
                //                    //Console.ReadLine();
                //                    TightSolver TS = new TightSolver(PowerSystem);
                //                    TS.ConfigureModel();
                //                    TS.Solve(600);
                //                    var output = TS.GetAnswer();
                //                    List<object> os = new List<object>() { instance, relax, ramp, minUpDown,  transmission,  24, Math.Round(output.time), PowerSystem.ConstraintConfiguration.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), Math.Round(output.GurobiCost), output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
                //                    string line = String.Join("\t", os.Select(o => o.ToString()));
                //                    U.Write(@"C:\Users\4001184\Dropbox\Output\log4.txt", line);
                //                }
                //            }
            }
            return;
        }
        public void DemandMultiplierTestTrans()
        {
            foreach (var instanceName in new string[] { "RTS54.uc", "DispaSET.uc", "DispaSET2.uc", "Bas.uc" })
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, true, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                for (int i = 0; i < 100; i++)
                {
                    CC.DemandMultiplier = 1 + (((double)i / 50));
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.Copperplate;
                    var outputCopper = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.TradeBased;
                    var outputTrade = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.PTDF;
                    var outputPTDF = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(outputCopper.TotalLossOfLoad.ToString());
                    cells.Add(outputTrade.TotalLossOfLoad.ToString());
                    cells.Add(outputPTDF.TotalLossOfLoad.ToString());
                    cells.Add((outputCopper.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputTrade.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputPTDF.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputCopper.GurobiCost).ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputTrade.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMTrans.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                //output.WriteToCSV(@"E:\Output\DMTrans\", i.ToString());
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void DemandMultiplierTestTrans2()
        {
            foreach (var instanceName in new string[] { "DispaSET.uc", "DispaSET2.uc", "Bas.uc" })
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, true, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                for (int i = 0; i < 100; i++)
                {
                    CC.FistNodeM = 1 + (((double)i));
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.Copperplate;
                    var outputCopper = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.TradeBased;
                    var outputTrade = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.PTDF;
                    var outputPTDF = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(outputCopper.TotalLossOfLoad.ToString());
                    cells.Add(outputTrade.TotalLossOfLoad.ToString());
                    cells.Add(outputPTDF.TotalLossOfLoad.ToString());
                    cells.Add((outputCopper.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputTrade.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputPTDF.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputCopper.GurobiCost).ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputTrade.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMTrans2.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                //output.WriteToCSV(@"E:\Output\DMTrans\", i.ToString());
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void DemandMultiplierTestTrans3()
        {
            foreach (var instanceName in new string[] { "RTS26.uc", "RTS54.uc", "RTS96.uc", "DispaSET.uc", "DispaSET2.uc", "Bas.uc" })
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.PTDF, false, true, 1, false);
                CC.SetLimits(0, 24);
                //CC.AdecuacyTest();
                for (int i = 0; i < 100; i = i + 1)
                {
                    CC.DemandMultiplier = 1 + (((double)(i - 50) / 50));
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.Copperplate;
                    var outputCopper = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.TradeBased;
                    var outputTrade = CSW(PowerSystem, CC, i);
                    CC.TransmissionMode = ConstraintConfiguration.TransmissionType.PTDF;
                    var outputPTDF = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(outputCopper.GurobiCost.ToString());
                    cells.Add(outputTrade.GurobiCost.ToString());
                    cells.Add(outputPTDF.GurobiCost.ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputCopper.GurobiCost).ToString());
                    cells.Add(CalculateGap(outputPTDF.GurobiCost, outputTrade.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMTrans3.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                //output.WriteToCSV(@"E:\Output\DMTrans\", i.ToString());
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void DemandMultiplierTestMD()
        {
            var instanceNames = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS54.uc" };
            foreach (var instanceName in instanceNames)
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, true, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                for (int i = 0; i < 100; i = i + 10)
                {
                    CC.DemandMultiplier = 1 + (((double)i / 50));
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    CC.MinUpMinDown = true;
                    var outputBase = CSW(PowerSystem, CC, i);
                    CC.MinUpMinDown = false;
                    var outputNB = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(outputBase.TotalLossOfLoad.ToString());
                    cells.Add(outputNB.TotalLossOfLoad.ToString());
                    cells.Add((outputBase.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputNB.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add(CalculateGap(outputBase.GurobiCost, outputNB.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMMD.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void DemandMultiplierTestRamp()
        {

            var instanceNames = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS54.uc" };
            foreach (var instanceName in instanceNames)
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, true, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                for (int i = 0; i < 100; i = i + 10)
                {
                    CC.DemandMultiplier = 1 + (((double)i / 50));
                    if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
                    CC.RampingLimits = true;
                    var outputBase = CSW(PowerSystem, CC, i);
                    CC.RampingLimits = false;
                    var outputNB = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(outputBase.TotalLossOfLoad.ToString());
                    cells.Add(outputNB.TotalLossOfLoad.ToString());
                    cells.Add((outputBase.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((outputNB.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add(CalculateGap(outputBase.GurobiCost, outputNB.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMRL.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        public void DemandMultiplierTestRelax()
        {

            var instanceNames = new string[] { "GA10.uc", "TAI38.uc", "A110.uc", "RCUC50.uc", "KOR140.uc", "OSTRO182.uc", "RCUC200.uc", "HUB223.uc", "DispaSET.uc", "RTS54.uc" };
            foreach (var instanceName in instanceNames)
            {
                string filename = U.InstanceFolder + instanceName;
                PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
                var CC = new ConstraintConfiguration(true, true, ConstraintConfiguration.TransmissionType.Copperplate, true, true, 1, false);
                CC.SetLimits(0, 24);
                CC.AdecuacyTest();
                for (int i = 0; i < 100; i = i + 10)
                {
                    CC.DemandMultiplier = 1 + (((double)i / 50));
                    var output1 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output2 = CSW(PowerSystem, CC, i);
                    CC.Relax = false;
                    CC.Tight = true;
                    var output3 = CSW(PowerSystem, CC, i);
                    CC.Relax = true;
                    var output4 = CSW(PowerSystem, CC, i);

                    var totalDemand = PowerSystem.TotalDemand() * CC.DemandMultiplier;
                    List<string> cells = new List<string>();
                    cells.Add(output1.TotalLossOfLoad.ToString());
                    cells.Add(output2.TotalLossOfLoad.ToString());
                    cells.Add(output3.TotalLossOfLoad.ToString());
                    cells.Add(output4.TotalLossOfLoad.ToString());
                    cells.Add((output1.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((output2.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((output3.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add((output4.TotalLossOfLoad / totalDemand).ToString());
                    cells.Add(CalculateGap(output1.GurobiCost, output2.GurobiCost).ToString());
                    cells.Add(CalculateGap(output3.GurobiCost, output4.GurobiCost).ToString());

                    var line = instanceName + " ; " + i + " ; " + String.Join(" ; ", cells);
                    U.Write(U.LogFolder + @"DMRE.txt", line);
                }
            }
            Output CSW(PowerSystem pss, ConstraintConfiguration ccc, int i)
            {
                TightSolver TS = new TightSolver(pss, ccc);
                TS.ConfigureModel();
                var output = TS.Solve(600);
                return output;
            }

            double CalculateGap(double baseValue, double value)
            {
                return (baseValue - value) / baseValue;
            }
        }
        private void RunInstanceTest(ConstraintConfiguration CC, string instanceName)
        {
            string filename = U.InstanceFolder + instanceName;
            PowerSystem PowerSystem = IOUtils.GetPowerSystem(filename);
            PowerSystem.UnCluster();
            TightSolver TS = new TightSolver(PowerSystem, CC);
            if (PowerSystem.Units.Select(unit => unit.Count > 1).Aggregate((a, b) => a || b)) CC.SetClusterd();
            TS.ConfigureModel();
            var output = TS.Solve(600);
            List<object> os = new List<object>() { Math.Round(output.time), CC.Relax ? 0 : Math.Round(output.Model.MIPGap, 6), output.GurobiCost, output.totalRampUpViolations, output.totalRampDownViolations, output.totalStartUpViolations, output.totalShutDownViolations, output.totalUpTimeViolation, output.totalDownTimeViolation };
            string line = String.Join("\t", os.Select(o => o.ToString()));
            U.Write(U.OutputFolder + @"PROGRAMTEST.txt", line);
        }
    }
}
