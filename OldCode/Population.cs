//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UtrechtCommitment
//{
//    class Population
//    {
//        PowerSystem PS;
//        List<Solution> Solutions;
//        Random RNG = new Random();
//        int PopSize;
//        public Population(PowerSystem ps, int popSize)
//        {
//            PS = ps;
//            PopSize = popSize;
//            Solutions = new List<Solution>();
//            for (int i = 0; i < popSize; i++)
//            {

//                Solutions.Add(new Solution(PS));
//            }
//            Solutions.ForEach(sol => sol.StoreUnitFitness());
//        }
//        public void NextGen()
//        {
//            var partent1 = Solutions[RNG.Next(PopSize)];
//            var partent2 = Solutions[RNG.Next(PopSize)];
//            var newSolution = partent1.RandomCrossover(partent2);
//            newSolution.StoreUnitFitness();
//            Solutions.Add(newSolution);
//            //Solutions = Solutions.OrderBy(sol => sol.StoredUnitFitness).ToList();
//            //Solutions.ForEach(sol =>
//            //{
//            //    Console.WriteLine("BEGIN LOCAL SEARCH");
//            //    //PS.Solver.SwitchModelOutputOn();
//            //    sol.LocalOptimum();
//            //});
//            Solutions = Solutions.Take(PopSize).ToList();
//            //Solutions[0].PrintUnitCommit();

//        }
//    }
//}
