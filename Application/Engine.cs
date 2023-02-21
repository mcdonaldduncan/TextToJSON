using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextToJSON.Constant;
using TextToJSON.ErrorReporting;
using System.Diagnostics;

namespace TextToJSON
{
    internal sealed class Engine
    {
        
        string errorFile = string.Empty;

        /// <summary>
        /// ProcessFiles takes a list of IDeliminated files with Pipe(txt) or csv extension and processes each of them in parallel
        /// </summary>
        /// <param name="filesToProcess">List of Ideliminated files prepared by the parser and deliminated file constructor</param>
        /// <returns errors>List of errors while processing</returns>
        public bool ProcessFiles(List<IDeliminated> filesToProcess)
        {
            Thread[] threads = new Thread[filesToProcess.Count];

            try
            {
                for (int i = 0; i < filesToProcess.Count; i++)
                {
                    int temp = i;
                    threads[temp] = new Thread(() => ProcessFile(filesToProcess[temp], temp));

                    threads[temp].Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }
            catch (Exception e)
            {
                ErrorCollection.Instance.Errors.Add(new Error(e.Message, e.Source ?? "Unknown"));
                Console.WriteLine($"Error while processing files, check errors for more detail");
                
                return false;
            }

            return true;
        }

        /// <summary>
        /// ProcessFile takes a deliminated file and the index of the thread it runs on. The file is processed into a list of string arrays for fields and a string arr for fields
        /// processed arrays are then formatted into JSON, adding an additional index field
        /// </summary>
        /// <param name="deliminatedFile"></param>
        /// <param name="threadIndex"></param>
        void ProcessFile(IDeliminated deliminatedFile, int threadIndex)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            StringBuilder report = new StringBuilder();

            List<string[]> lines = new List<string[]>();
            string[] fields = null;

            report.AppendLine(breakLine);
            report.AppendLine(DateTime.Now.ToString());
            report.AppendLine($"Processing {deliminatedFile.FileName}");

            if (File.Exists(deliminatedFile.WritePath))
            {
                File.Delete(deliminatedFile.WritePath);
            }

            using (StreamReader sr = new StreamReader(deliminatedFile.FilePath))
            {
                bool fieldsCollected = false;
                while (!sr.EndOfStream)
                {
                    var lineItems = sr.ReadLine()?.Split(deliminatedFile.Delimiter) ?? new string[0];

                    for (int i = 0; i < lineItems.Length; i++) lineItems[i] = lineItems[i].Trim('"');

                    if (fieldsCollected) lines.Add(lineItems);
                    else
                    {
                        fields = lineItems;
                        fieldsCollected = true;
                    }
                }
                report.AppendLine("File read successful");
            }

            using (StreamWriter sw = new StreamWriter(deliminatedFile.WritePath, true))
            {
                sw.WriteLine("[");

                for (int n = 0; n < lines.Count; n++)
                {
                    sw.WriteLine("  {");
                    sw.WriteLine(@$"     ""ID"": {n + 1},");
                    for (int j = 0; j < lines[n].Length; j++)
                    {
                        string temp = string.Empty;

                        if (int.TryParse(lines[n][j], out _) && j < fields?.Length && fields[j] != "Code") temp = lines[n][j];
                        else temp = $@"""{lines[n][j]}""";

                        if (j < fields?.Length) sw.Write(@$"     ""{fields[j]}"": {temp}");
                        else sw.Write(@$"     ""Field{j + 1}"": {temp}");

                        if (!(j + 1 == lines[n].Length)) sw.Write("," + Environment.NewLine);
                        else sw.Write(Environment.NewLine);
                    }

                    sw.Write("  }");
                    if (!(n + 1 == lines.Count)) sw.Write("," + Environment.NewLine);
                    else sw.Write(Environment.NewLine);
                }

                sw.Write("]");

                report.AppendLine("File write successful");
                report.AppendLine($"Location: {deliminatedFile.WritePath}");
                report.AppendLine($"Thread {threadIndex} Time: {stopwatch.Elapsed}");
                report.AppendLine(breakLine);
            }

            Console.Write(report.ToString());
        }
    }
}
