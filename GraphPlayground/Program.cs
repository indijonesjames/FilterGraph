using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphPlayground
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create a default template library
            FilterTypeLibrary builtInFilterTypes = new FilterTypeLibrary();
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "result", inputPinNames = new List<string>() { "value" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "const", outputPinNames = new List<string>() { "value" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "get", outputPinNames = new List<string>() { "value" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "set", inputPinNames = new List<string>() { "value" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "add", inputPinNames = new List<string>() { "A", "B" }, outputPinNames = new List<string>() { "output" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "subtract", inputPinNames = new List<string>() { "pos", "neg" }, outputPinNames = new List<string>() { "output" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "multiply", inputPinNames = new List<string>() { "A", "B" }, outputPinNames = new List<string>() { "output" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "divide", inputPinNames = new List<string>() { "num", "den" }, outputPinNames = new List<string>() { "output" } });
            builtInFilterTypes.filterTypes.Add(new FilterType() { Name = "negate", inputPinNames = new List<string>() { "input" }, outputPinNames = new List<string>() { "output" } });
            Application.Run(new MainForm() { FilterTypeLibrary = builtInFilterTypes });
        }
    }
}
