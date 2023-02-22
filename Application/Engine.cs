using System.Text;
using TextToJSON.ErrorReporting;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using static TextToJSON.Constant;
using static TextToJSON.Application.Utility;

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
        public bool ProcessFiles(List<IDelimited> filesToProcess)
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
        /// <param name="delimitedFile"></param>
        /// <param name="threadIndex"></param>
        void ProcessFile(IDelimited delimitedFile, int threadIndex)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                StringBuilder report = new StringBuilder();

                report.AppendLine(breakLine);
                report.AppendLine(DateTime.Now.ToString());
                report.AppendLine($"Processing {delimitedFile.FileName}");

                if (File.Exists(delimitedFile.WritePath))
                {
                    File.Delete(delimitedFile.WritePath);
                }

                if (ReadFile(delimitedFile, out List<string[]> lines, out string[] fields))
                {
                    report.AppendLine("File Read Successful");
                }
                else
                {
                    report.AppendLine(breakLine);
                    Console.Write(report.ToString());
                    return;
                }

                if (WriteFile(delimitedFile, lines, fields))
                {
                    report.AppendLine($"File write successful");
                    report.AppendLine($"Size: {BytesToString(delimitedFile.writeInfo.Length)}");
                    report.AppendLine($"Location: {delimitedFile.WritePath}");
                    report.AppendLine($"Thread {threadIndex} Time: {stopwatch.Elapsed}");
                    report.AppendLine(breakLine);
                }
                else
                {
                    report.AppendLine(breakLine);
                    Console.Write(report.ToString());
                    return;
                }

                Console.Write(report.ToString());
            }
            catch (Exception e)
            {
                ErrorCollection.Instance.Errors.Add(new Error(e.Message, e.Source ?? "Unknown"));
                Console.WriteLine($"Error while processing files, check errors for more detail");
                return;
            }
        }

        bool ReadFile(IDelimited delimitedFile, out List<string[]> lines, out string[] fields, bool containsCommas = false)
        {
            lines = new List<string[]>();
            fields = new string[0];

            try
            {
                if (containsCommas)
                {
                    using (TextFieldParser parser = new TextFieldParser(delimitedFile.FilePath))
                    {
                        bool fieldsCollected = false;

                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(delimitedFile.Delimiter);

                        while (!parser.EndOfData)
                        {
                            var lineItems = parser.ReadFields();

                            if (lineItems == null) continue;

                            for (int i = 0; i < lineItems.Length; i++) lineItems[i] = lineItems[i].Trim('"');

                            if (fieldsCollected) lines.Add(lineItems);
                            else
                            {
                                fields = lineItems;
                                fieldsCollected = true;
                            }
                        }
                    }
                }
                else
                {
                    using (StreamReader sr = new StreamReader(delimitedFile.FilePath))
                    {
                        bool fieldsCollected = false;
                        while (!sr.EndOfStream)
                        {
                            

                            var lineItems = sr.ReadLine()?.Split(delimitedFile.Delimiter) ?? new string[0];

                            for (int i = 0; i < lineItems.Length; i++) lineItems[i] = lineItems[i].Trim('"');

                            if (fieldsCollected) lines.Add(lineItems);
                            else
                            {
                                fields = lineItems;
                                fieldsCollected = true;
                            }


                            if (DataContainsCommas(fields.Length, lineItems.Length)) 
                                return ReadFile(delimitedFile, out lines, out fields, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorCollection.Instance.Errors.Add(new Error(e.Message, e.Source ?? "Unknown"));
                Console.WriteLine($"Error while reading {delimitedFile.FileName}, check errors for more detail");
                return false;
            }

            return true;
        }

        bool WriteFile(IDelimited delimitedFile, List<string[]> lines, string[] fields)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(delimitedFile.WritePath, true))
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
                }
            }
            catch (Exception e)
            {
                ErrorCollection.Instance.Errors.Add(new Error(e.Message, e.Source ?? "Unknown"));
                Console.WriteLine($"Error while writing JSON file for {delimitedFile.FileName}, check errors for more detail");
                return false;
            }

            return true;
        }

        bool DataContainsCommas(int fieldsLength, int dataLength)
        {
            if (fieldsLength == 0) return false;
            if (dataLength == 0) return false;

            return !(fieldsLength == dataLength);
        }
    }
}
