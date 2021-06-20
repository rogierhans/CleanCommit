using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CleanCommit.Instance;

namespace CleanCommit
{
    class Program
    {
        static void Main(string[] args)
        {


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
            var Experiment = new Experiment();

           // new DirectoryInfo(@"C:\Users\4001184\Desktop\ZondagNacht").GetFiles().Select(x => x.FullName).ToList().ForEach(x => Experiment.Foto(x, x.Split('\\').Last().Split('.').First()));
            //Experiment.Test0();
           Experiment.TestGA10();
            //Experiment.TestGA10();
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
}
