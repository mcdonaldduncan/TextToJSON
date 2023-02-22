﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextToJSON.Constant;

namespace TextToJSON
{
    internal sealed class Parser
    {
        List<IDelimited> filesToProcess = new List<IDelimited>();

        bool hasErrors => ErrorCollection.Instance?.Errors.Any() ?? false;

        public Parser()
        {
            Console.WriteLine("Process Started!");

            List<string> fileNames = GetAllFiles();

            foreach (var name in fileNames)
            {
                filesToProcess.Add(new DelimitedFile(name));
            }

            if (hasErrors)
            {
                ErrorCollection.Instance.ReportErrors();
                return;
            }

            Engine engine = new Engine();

            if (engine.ProcessFiles(filesToProcess))
            {
                Console.WriteLine($"Processed files can be found in {writeDirectory}");
            }
            else
            {
                ErrorCollection.Instance.ReportErrors();
            }


            if (hasErrors) ErrorCollection.Instance.ReportErrors();
        }

        List<string> GetAllFiles()
        {
            return Directory.GetFiles(directoryPath).Where(x => !x.EndsWith("_out.json")).ToList();
        }
    }
}
