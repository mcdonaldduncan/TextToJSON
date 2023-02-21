using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToJSON
{
    public sealed class Constant
    {
        private const string folderName = "temp";

        public const string breakLine = "---------------------------------------------------------------------------------------------------------";

        public static string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        public sealed class FileExtensions
        {
            public static string JSON => ".json";
            public static string XML => ".xml";
            public static string CSV => ".csv";
            public static string Pipe => ".txt";
            public static string Text => ".txt";
        }

        public sealed class FileDelimiters
        {
            public static string JSON => "";
            public static string XML => "";
            public static string CSV => ",";
            public static string Pipe => "|";
            public static string Text => " ";
        }
    }
}
