using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyMNGR.Utils
{
    public class ConsoleManager
    {
        private RichTextBox _target;

        public ConsoleManager(RichTextBox target)
        {
            _target = target;
        }

        public void LogMessage(string message)
        {
            _target.AppendText($"[{DateTime.Now.ToString("yyyy/dd/MM HH:mm")}] {message}");
        }
    }
}
