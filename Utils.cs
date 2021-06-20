using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

using System.Xml.Serialization;
namespace CleanCommit
{
    static class U
    {
        public static string DropBoxPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public static string InstanceFolder = DropBoxPath + @"\Data\NewInstances\";
        public static string LogFolder =  DropBoxPath + @"\Output\";
        public static string OutputFolder = @"E:\Output\";// DropBoxPath + @"\Output\";// Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Output\";
        //public static string OutputFolder = @"C:\Users\Rogier\Desktop\output\";// Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Output\";
        public static Random RNG = new Random();
        public static Dictionary<string, List<string>> ReadFolder(string folder)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            (new DirectoryInfo(folder).GetFiles()).ToList().ForEach(x =>
            {
                dict[x.Name] = ReadFile(x.FullName);
            });
            return dict;
        }

        public static Dictionary<string, List<List<double>>> ReadFolderMatrix(string folder, string type)
        {

            Dictionary<string, List<List<double>>> dict = new Dictionary<string, List<List<double>>>();
            (new DirectoryInfo(folder).GetFiles(type)).ToList().ForEach(x => dict[x.Name.Split('.')[0]] = ReadMartix(x.FullName));
            return dict;
        }

        public static List<List<double>> ReadMartix(string filename)
        {
            List<List<double>> lines = new List<List<double>>();
            using (var sw = File.OpenText(filename))
            {

                string line = sw.ReadLine();
                while (line != null)
                {
                    lines.Add(line.Split(';').ToList().Select(x => Double.Parse(x)).ToList());
                    line = sw.ReadLine();
                }
            }
            return lines;

        }
        public static List<List<double>> ReverseMatrix(List<List<double>> matrix)
        {
            List<List<double>> rmatrix = new List<List<double>>();
            for (int column = 0; column < matrix[0].Count; column++)
            {
                var newRow = new List<double>();
                for (int row = 0; row < matrix.Count; row++)
                {
                    newRow.Add(matrix[row][column]);
                }
                rmatrix.Add(newRow);
            }

            return rmatrix;
        }
        public static List<string> ReadFile(string filename)
        {
            List<string> lines = new List<string>();
            using (var sw = File.OpenText(filename))
            {

                string line = sw.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = sw.ReadLine();
                }
            }
            return lines;
        }

        static readonly object @lock = new object();
        public static void Write(string filename, string line)
        {
            lock (@lock)
            {
                if (!File.Exists(filename))
                {
                    using (StreamWriter sw = File.CreateText(filename))
                    {
                        sw.WriteLine(line);

                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filename))
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }
        public static void Write(string filename, List<string> lines)
        {
            lock (@lock)
            {
                if (!File.Exists(filename))
                {
                    using (StreamWriter sw = File.CreateText(filename))
                    {
                        lines.ForEach(line => sw.WriteLine(line));

                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filename))
                    {
                        lines.ForEach(line => sw.WriteLine(line));
                    }
                }
            }
        }


        public static void E(bool condition, string line, params object[] variables)
        {
            if (condition)
            {
                throw new Exception(S(line, variables));
            }
        }

        public static string S(string line, params object[] variables)
        {
            List<String> map = new List<string>();
            for (int i = 0; i < variables.Length; i++)
            {
                if (!line.Contains(i.ToString())) throw new Exception(line + " does not contain " + i);
                string mapString = "{" + i.ToString() + "}";
                map.Add(mapString);
                line = line.Replace(i.ToString(), map[i]);
            }
            for (int i = 0; i < variables.Length; i++)
            {
                line = line.Replace(map[i], variables[i].ToString());
            }
            return line;
        }

        public static void PrintArray(double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                string line = "";
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    line += "\t" + array[i, j];
                }
                Console.WriteLine(line);
            }

        }

        public static void L(object o) {
            Console.WriteLine(o.ToString());
            Console.ReadLine();
        }
        public static List<int> GetNumbers(int i)
        {
            return Enumerable.Range(0, i).ToList();
        }

        public static int MaxLookback(int lb, int t)
        {
            return Math.Min(lb, t);
        }

        public static int ZeroOrGreater(int i)
        {
            return Math.Max(i, 0);

        }

        public static double ZeroOrGreater(double i)
        {
            return Math.Max(i, 0);

        }

        public static int Min(params int[] values)
        {
            int min = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                min = Math.Min(min, values[i]);

            }
            return min;
        }
        public static List<string> MArrayToString(string identifier, double[,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            for (int i = 0; i < input.GetLength(0); i++)
            {
                List<double> values = new List<double>();
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    values.Add(input[i, j]);
                }
                lines.Add(String.Join(";", values.Select(x => x.ToString())));
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public static List<string> MArrayToString(string identifier, int[,] input)
        {
            List<string> lines = new List<string>
            {
                "<" + identifier + ">"
            };
            for (int i = 0; i < input.GetLength(0); i++)
            {
                List<double> values = new List<double>();
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    values.Add(input[i, j]);
                }
                lines.Add(String.Join(";", values.Select(x => x.ToString())));
            }
            lines.Add("</" + identifier + ">");
            return lines;
        }
        public static void PrintArray(object[,] array) {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                string line = "";
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    line += array[i, j].ToString();
                }
                Console.WriteLine(line);
            }
        }
    }
}
