using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace UtrechtCommitment
{
    static class Utils
    {

        static int TimeID = 0;
        static public int GetTimeID() {
            return TimeID++;
        }

        public static Dictionary<string, List<string>> ReadFolder(string folder, string type)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            (new DirectoryInfo(folder).GetFiles(type)).ToList().ForEach(x => dict[x.FullName] = ReadFile(x.FullName));
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

        static object SteveJobs = new object();
        public static void Write(string filename, string line)
        {
            lock (SteveJobs)
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
            lock (SteveJobs)
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
    }
}
