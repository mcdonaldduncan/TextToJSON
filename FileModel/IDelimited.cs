using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToJSON
{
    internal interface IDelimited
    {
        public string Delimiter { get; set; }
        public string FilePath { get; set; }
        public string Extension { get; set; }
        public string FileName { get; set; }
        public string WritePath { get; set; }
    }
}
