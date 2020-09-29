using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Data
{
    public class ProcessResult
    {
        public string Output;

        public int ExitCode;

        public bool Success { get { return ExitCode == 0; } }

        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(Output); } }
    }
}
