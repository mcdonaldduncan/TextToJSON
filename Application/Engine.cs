using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextToJSON.Constant;
using TextToJSON.ErrorReporting;

namespace TextToJSON
{
    sealed class Engine
    {
        
        string errorFile = string.Empty;

        /// <summary>
        /// ProcessFiles takes a list of IDeliminated files with Pipe(txt) or csv extension and processes each of them sequentially
        /// </summary>
        /// <param name="filesToProcess">List of Ideliminated files prepared by the parser and MyFile constructor</param>
        /// <returns errors>List of errors while processing</returns>
        public bool ProcessFiles(List<IDeliminated> filesToProcess)
        {
            Thread[] threads = new Thread[filesToProcess.Count];

            try
            {
                for (int i = 0; i < filesToProcess.Count; i++)
                {
                    int temp = i;
                    threads[temp] = new Thread(() => ProcessFile(filesToProcess[temp]));

                    threads[temp].Start();
                    Console.WriteLine($"Thread{i} Started");
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

        void ProcessFile(IDeliminated deliminatedFile)
        {
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
                    sw.WriteLine(@$"     ""ID"": ""{n + 1}"",");
                    for (int j = 0; j < lines[n].Length; j++)
                    {
                        if (j < fields?.Length) sw.Write(@$"     ""{fields[j]}"": ""{lines[n][j]}""");
                        else sw.Write(@$"     ""Field{j + 1}"": ""{lines[n][j]}""");

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
                report.AppendLine(breakLine);
            }

            Console.WriteLine(report.ToString());
        }
    }
}
