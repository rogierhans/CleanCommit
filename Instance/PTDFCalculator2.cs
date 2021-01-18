using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
namespace CleanCommit.Instance
{
    class NewPTDF
    {
        List<TransmissionLineAC> Lines;
        List<Node> Nodes;

        readonly int totalLines;
        readonly int totalNodes;

        public NewPTDF(List<TransmissionLineAC> lines, List<Node> nodes)
        {
            Lines = lines;
            Nodes = nodes;
            totalLines = Lines.Count;
            totalNodes = Nodes.Count;

        }
        public double[,] GetPTDF()
        {
            var A = IncidenceMatrix();
            var B = DiagonalLines();
            var BA = (B * A).RemoveColumn(0);
            var AtBA = A.Transpose() * B * A;
            var AtBAminus1 = AtBA.RemoveColumn(0).RemoveRow(0);
            var Inverse_AtBA = AtBAminus1.Inverse();

            // PrintMatrix(Multiplication(Inverse_AtBA, RemoveColumnAndRow(AtBA, 0)));
            //Console.ReadLine();

            var PTDF = BA * Inverse_AtBA;
            var PTDFarray = AddColumn(PTDF.ToArray());
            //Write2File(PTDFarray);
            return PTDFarray;
        }
        private void Write2File(double[,] PTDFarray)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < PTDFarray.GetLength(0); i++)
            {
                List<double> cells = new List<double>();
                for (int j = 0; j < PTDFarray.GetLength(1); j++)
                {
                    cells.Add(Math.Round(PTDFarray[i, j], 4));
                }
                lines.Add(string.Join("\t",cells));
            }
            File.WriteAllLines(@"C:\Users\Rogier\Desktop\DesktopOutput\ptdf.txt", lines);
        }

        private double[,] AddColumn(double[,] matrix)
        {
            var newMatrix = new double[matrix.GetLength(0), matrix.GetLength(1) + 1];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                newMatrix[i, 0] = 0;
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    newMatrix[i, j + 1] = Math.Round(matrix[i, j], 4);
                }
            }
            return newMatrix;
        }

        private Matrix<double> IncidenceMatrix()
        {
            var matrixArray = new double[totalLines, totalNodes];
            for (int l = 0; l < totalLines; l++)
            {
                var Line = Lines[l];
                var NodeFrom = Line.From;
                var NodeTo = Line.To;
                for (int n = 0; n < totalNodes; n++)
                {
                    var Node = Nodes[n];
                    if (Node == NodeFrom)
                    {
                        matrixArray[l, n] = 1;
                    }
                    if (Node == NodeTo)
                    {
                        matrixArray[l, n] = -1;
                    }
                }
            }
            return DenseMatrix.OfArray(matrixArray);
        }
        private Matrix<double> DiagonalLines()
        {
            var matrixArray = new double[totalLines, totalLines];
            for (int i = 0; i < totalLines; i++)
            {
                matrixArray[i, i] = Lines[i].Susceptance;
            }
            return DenseMatrix.OfArray(matrixArray);
        }
    }
}
