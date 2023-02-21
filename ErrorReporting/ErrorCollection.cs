using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextToJSON.ErrorReporting;

namespace TextToJSON
{
    internal sealed class ErrorCollection
    {
        public static readonly Lazy<ErrorCollection> lazy = new Lazy<ErrorCollection>(() => new ErrorCollection());

        public static ErrorCollection Instance { get { return lazy.Value; } }

        public List<Error> Errors = new List<Error>();

        public void ReportErrors()
        {
            Console.WriteLine("Process exited with errors!");
            foreach (var error in Errors)
            {
                Console.WriteLine(Constant.breakLine);
                Console.WriteLine(error);
                Console.WriteLine(Constant.breakLine);
            }
        }

        private ErrorCollection()
        {

        }
    }
}
