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
    /// <summary>
    /// Functions for performing common binary Serialization operations.
    /// <para>All properties and variables will be serialized.</para>
    /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
    /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
    /// </summary>
    public static class BinarySerialization
    {
        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
    // <summary>
    /// Functions for performing common XML Serialization operations.
    /// <para>Only public properties and variables will be serialized.</para>
    /// <para>Use the [XmlIgnore] attribute to prevent a property/variable from being serialized.</para>
    /// <para>Object to be serialized must have a parameterless constructor.</para>
    /// </summary>
    public static class XmlSerialization
    {
        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        
    }
    /// <summary>
    /// Functions for performing common Json Serialization operations.
    /// <para>Requires the Newtonsoft.Json assembly (Json.Net package in NuGet Gallery) to be referenced in your project.</para>
    /// <para>Only public properties and variables will be serialized.</para>
    /// <para>Use the [JsonIgnore] attribute to ignore specific public properties or variables.</para>
    /// <para>Object to be serialized must have a parameterless constructor.</para>
    /// </summary>
    public static class JsonSerialization
    {
        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = Newtonsoft.Json.JsonConvert.SerializeObject(objectToWrite);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
