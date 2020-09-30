using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Data
{
    public class ProcessResult
    {
        public string Error;

        public int ExitCode;

        public string Output;

        public bool Success { get { return ExitCode == 0 && IsErrorEmpty; } }

        public bool IsOutputEmpty { get { return string.IsNullOrWhiteSpace(Output); } }

        public bool IsErrorEmpty { get { return string.IsNullOrWhiteSpace(Error); } }
    }
}
