using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextToJSON.Constant;
using TextToJSON.ErrorReporting;

namespace TextToJSON
{
    internal class DeliminatedFile : IDeliminated
    {
        private string delimiter;
        private string filePath;
        private string extension;
        private string fileName;

        public string Delimiter { get => delimiter; set => delimiter = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public string Extension { get => extension; set => extension = value; }
        public string FileName { get => fileName; set => fileName = value; }


        public DeliminatedFile(string _fileName)
        {
            fileName = _fileName.Substring(_fileName.LastIndexOf(@"\") + 1);

            if (_fileName.EndsWith(FileExtensions.Pipe))
            {
                delimiter = FileDelimiters.Pipe;
                extension = FileExtensions.Pipe;
            }
            else if (_fileName.EndsWith(FileExtensions.CSV))
            {
                delimiter = FileDelimiters.CSV;
                extension = FileExtensions.CSV;
            }
            else
            {
                ErrorCollection.Instance.Errors.Add(new Error($"Invalid File Extension, {_fileName.Substring(_fileName.LastIndexOf("."))} is not supported", $"{_fileName}"));
            }
            filePath = Path.Combine(directoryPath, _fileName);
        }

    }
}
