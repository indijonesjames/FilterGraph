using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPlayground
{
    public class FilterType
    {
        public string Name;

        public List<string> inputPinNames = new List<string>();
        public List<string> outputPinNames = new List<string>();

        public override string ToString()
        {
            return Name;
        }
    }
}
