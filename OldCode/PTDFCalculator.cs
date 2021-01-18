//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Gurobi;

//namespace CleanCommit.Instance
//{
//    class PTDFCalculator
//    {

//        List<TransmissionLineAC> Lines;
//        List<Node> Nodes;

//        int totalLines;
//        int totalNodes;

//        public PTDFCalculator(List<TransmissionLineAC> lines, List<Node> nodes)
//        {
//            //Test();
//            Lines = lines;
//            Nodes = nodes;
//            totalLines = Lines.Count;
//            totalNodes = Nodes.Count;

//        }

//        private void Test()
//        {
//            var A = TestIncidenceMatrix();


//            //later anders
//            var B = TestDiagonalLines();

//            var BA = RemoveColumn(Multiplication(B, A), 2);

//            var AtBA = Multiplication(Multiplication(Transpose(A), B), A);
//            var Inverse_AtBA = Inverse(RemoveColumnAndRow(AtBA, 2));

//            PrintMatrix(Multiplication(Inverse_AtBA, RemoveColumnAndRow(AtBA, 2)));

//            var PTDF = Multiplication(BA, Inverse_AtBA);

//            PrintMatrix(A);
//            PrintMatrix(B);
//            PrintMatrix(BA);
//            PrintMatrix(Inverse_AtBA);
//            PrintMatrix(PTDF);


//            //test PST

//            var PSDF = Substraction(B, Multiplication(Multiplication(BA, Inverse_AtBA), Transpose(BA)));
//            PrintMatrix(PSDF);
//            Console.ReadLine();
//        }

//        int testN = 5;
//        int testL = 6;
//        private double[,] TestDiagonalLines()
//        {
//            var matrix = new double[testL, testL];
//            for (int l = 0; l < testL; l++)
//            {
//                matrix[l, l] = 0.5;
//            }
//            return matrix;
//        }

//        private double[,] TestIncidenceMatrix()
//        {
//            var matrix = new double[testL, testN];

//            matrix[0, 0] = 1;
//            matrix[1, 0] = 1;
//            matrix[2, 1] = 1;
//            matrix[3, 2] = 1;
//            matrix[4, 2] = 1;
//            matrix[5, 3] = 1;

//            matrix[0, 1] = -1;
//            matrix[1, 2] = -1;
//            matrix[2, 3] = -1;
//            matrix[3, 3] = -1;
//            matrix[4, 4] = -1;
//            matrix[5, 4] = -1;

//            return matrix;
//        }

//        public double[,] GetPTDF()
//        {
//            var A = IncidenceMatrix();

//            //PrintMatrix(A);

//            Console.WriteLine("ok");
//            //Console.ReadLine();
//            //later anders
//            var B = DiagonalLines();


//            var BA = RemoveColumn(Multiplication(B, A), 0);

//            var AtBA = Multiplication(Multiplication(Transpose(A), B), A);
//            var Inverse_AtBA = Inverse(RemoveColumnAndRow(AtBA, 0));

//            // PrintMatrix(Multiplication(Inverse_AtBA, RemoveColumnAndRow(AtBA, 0)));
//            //Console.ReadLine();

//            var PTDF = Multiplication(BA, Inverse_AtBA);

//            return AddColumn(PTDF);
//        }

//        private double[,] AddColumn(double[,] matrix)
//        {
//            var newMatrix = new double[matrix.GetLength(0), matrix.GetLength(1) + 1];

//            for (int i = 0; i < matrix.GetLength(0); i++)
//            {
//                newMatrix[i, 0] = 0;
//                for (int j = 0; j < matrix.GetLength(1); j++)
//                {
//                    newMatrix[i, j + 1] = Math.Round(matrix[i, j], 4);
//                }
//            }
//            return newMatrix;
//        }

//        private double[,] RemoveColumn(double[,] matrix, int index)
//        {
//            var newMatrix = new double[matrix.GetLength(0), matrix.GetLength(1) - 1];

//            for (int i = 0; i < matrix.GetLength(0); i++)
//            {
//                for (int j = 0; j < index; j++)
//                {
//                    newMatrix[i, j] = matrix[i, j];
//                }
//                for (int j = index + 1; j < matrix.GetLength(1); j++)
//                {
//                    newMatrix[i, j - 1] = matrix[i, j];
//                }
//            }
//            return newMatrix;
//        }

//        private double[,] RemoveColumnAndRow(double[,] matrix, int index)
//        {
//            var newMatrix = new double[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];

//            for (int i = 0; i < index; i++)
//            {
//                for (int j = 0; j < index; j++)
//                {
//                    newMatrix[i, j] = matrix[i, j];
//                }
//                for (int j = index + 1; j < matrix.GetLength(1); j++)
//                {
//                    newMatrix[i, j - 1] = matrix[i, j];
//                }
//            }
//            for (int i = index + 1; i < matrix.GetLength(0); i++)
//            {
//                for (int j = 0; j < index; j++)
//                {
//                    newMatrix[i - 1, j] = matrix[i, j];
//                }
//                for (int j = index + 1; j < matrix.GetLength(1); j++)
//                {
//                    newMatrix[i - 1, j - 1] = matrix[i, j];
//                }
//            }
//            return newMatrix;
//        }

//        private void PrintMatrix(double[,] matrix)
//        {
//            Console.WriteLine("Printmatrix");
//            for (int i = 0; i < matrix.GetLength(0); i++)
//            {
//                string line = "";
//                for (int j = 0; j < matrix.GetLength(1); j++)
//                {
//                    line += Math.Round(matrix[i, j], 8).ToString() + " ";
//                }
//                Console.WriteLine(line);
//            }
//        }


//        private double[,] DiagonalLines()
//        {
//            var matrix = new double[totalLines, totalLines];
//            for (int i = 0; i < totalLines; i++)
//            {
//                matrix[i, i] = Lines[i].Susceptance;
//            }
//            return matrix;
//        }

//        private double[,] IncidenceMatrix()
//        {
//            var matrix = new double[totalLines, totalNodes];
//            for (int l = 0; l < totalLines; l++)
//            {
//                var Line = Lines[l];
//                var NodeFrom = Line.From;
//                var NodeTo = Line.To;
//                for (int n = 0; n < totalNodes; n++)
//                {
//                    var Node = Nodes[n];
//                    if (Node == NodeFrom)
//                    {
//                        matrix[l, n] = 1;
//                    }
//                    if (Node == NodeTo)
//                    {
//                        matrix[l, n] = -1;
//                    }
//                }
//            }

//            return matrix;
//        }

//        private double[,] Inverse(double[,] matrixInput)
//        {
//            //sanitycheck
//            if (matrixInput.GetLength(0) != matrixInput.GetLength(1))
//            {
//                Console.WriteLine("error matrix not a squire");
//            }
//            int n = matrixInput.GetLength(0);

//            GRBVar[,] GurobiInverse = new GRBVar[n, n];
//            var env = new GRBEnv();
//            var model = new GRBModel(env);

//            for (int i = 0; i < n; i++)
//            {
//                for (int j = 0; j < n; j++)
//                {
//                    GurobiInverse[i, j] = model.AddVar(double.MinValue, double.MaxValue, 0.0, GRB.CONTINUOUS, i + " " + j);
//                }
//            }
//            for (int i = 0; i < n; i++)
//            {

//                for (int j = 0; j < n; j++)
//                {
//                    var linearExpersion = new GRBLinExpr();
//                    for (int x = 0; x < n; x++)
//                    {
//                        linearExpersion += GurobiInverse[i, x] * matrixInput[x, j];
//                    }
//                    if (i == j)
//                    {
//                        model.AddConstr(linearExpersion == 1, i + " " + j);
//                    }
//                    else
//                    {
//                        model.AddConstr(linearExpersion == 0, i + " " + j);
//                    }
//                }
//            }

//            model.Optimize();

//            var inverse = new double[n, n];

//            for (int j = 0; j < n; j++)
//            {
//                string line = "";
//                for (int i = 0; i < n; i++)
//                {

//                    inverse[i, j] = GurobiInverse[i, j].X;
//                    line += " " + inverse[i, j];
//                }
//                Console.WriteLine(line);
//            }

//            return inverse;
//        }

//        private double[,] Multiplication(double[,] matrix1, double[,] matrix2)
//        {
//            int rows1 = matrix1.GetLength(0);
//            int columns1 = matrix1.GetLength(1);
//            int rows2 = matrix2.GetLength(0);
//            int columns2 = matrix2.GetLength(1);

//            if (columns1 != rows2)
//            {
//                throw new Exception(string.Format("Matix multiplication Error ({0}x{1}) and ({2}x{3})", rows1, columns1, rows2, columns2));
//            }

//            var matrix = new double[rows1, columns2];
//            for (int i = 0; i < rows1; i++)
//            {
//                for (int j = 0; j < columns2; j++)
//                {
//                    double sum = 0;
//                    for (int x = 0; x < columns1; x++)
//                    {
//                        sum += matrix1[i, x] * matrix2[x, j];
//                    }
//                    matrix[i, j] = sum;
//                }
//            }
//            return matrix;
//        }

//        private double[,] Substraction(double[,] matrix1, double[,] matrix2)
//        {
//            int rows1 = matrix1.GetLength(0);
//            int columns1 = matrix1.GetLength(1);
//            int rows2 = matrix2.GetLength(0);
//            int columns2 = matrix2.GetLength(1);

//            if (rows1 != rows2 || columns1 != columns2)
//            {
//                throw new Exception(string.Format("Matix Substraction Error ({0}x{1}) and ({2}x{3})", rows1, columns1, rows2, columns2));
//            }

//            var matrix = new double[rows1, columns2];
//            for (int i = 0; i < rows1; i++)
//            {
//                for (int j = 0; j < columns1; j++)
//                {
//                    matrix[i, j] += matrix1[i, j] - matrix2[i, j];
//                }
//            }
//            return matrix;
//        }

//        private double[,] Transpose(double[,] matrixInput)
//        {
//            var matrixOutput = new double[matrixInput.GetLength(1), matrixInput.GetLength(0)];

//            for (int i = 0; i < matrixInput.GetLength(0); i++)
//            {
//                for (int j = 0; j < matrixInput.GetLength(1); j++)
//                {
//                    matrixOutput[j, i] = matrixInput[i, j];
//                }
//            }
//            return matrixOutput;
//        }
//        //private double[,] Duplicate(double[,] matrix)
//        //{
//        //    var copy = new double[matrix.GetLength(0), matrix.GetLength(1)];
//        //    for (int i = 0; i < matrix.GetLength(0); i++)
//        //    {
//        //        for (int j = 0; j < matrix.GetLength(1); j++)
//        //        {
//        //            copy[i, j] = matrix[i, j];
//        //        }
//        //    }
//        //    return copy;
//        //}
//    }
//}
