using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextToJSON.Constant;
using TextToJSON.ErrorReporting;

namespace TextToJSON
{
    internal sealed class DelimitedFile : IDelimited
    {
        private string delimiter;
        private string filePath;
        private string extension;
        private string fileName;
        private string writePath;

        public string Delimiter { get => delimiter; set => delimiter = value; }
        public string FilePath { get => filePath; set => filePath = value; }
        public string Extension { get => extension; set => extension = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string WritePath { get => writePath; set => writePath = value; }


       


        public DelimitedFile(string _fileName)
        {
            fileName = _fileName.Substring(_fileName.LastIndexOf(@"\") + 1);

            if (_fileName.EndsWith(FileExtensions.Pipe))
            {
                delimiter = FileDelimiters.Pipe;
                extension = FileExtensions.Pipe;
                writePath = Path.Combine(writeDirectory, fileName).Replace(extension, $"_out{FileExtensions.JSON}");
                filePath = Path.Combine(directoryPath, _fileName);
            }
            else if (_fileName.EndsWith(FileExtensions.CSV))
            {
                delimiter = FileDelimiters.CSV;
                extension = FileExtensions.CSV;
                writePath = Path.Combine(writeDirectory, fileName).Replace(extension, $"_out{FileExtensions.JSON}");
                filePath = Path.Combine(directoryPath, _fileName);
            }
            else
            {
                ErrorCollection.Instance.Errors.Add(new Error($"Invalid File Extension, {_fileName.Substring(_fileName.LastIndexOf("."))} is not supported", $"{_fileName}"));
            }
        }
    }
}
