using System.Collections.Generic;
using VectorMath;

namespace GraphLayoutLib
{
    public class NodeDesign
    {
        public string name;
        public int2 position;
        public int2 size;
        public List<PinDesign> inputPinDesigns = new List<PinDesign>();
        public List<PinDesign> outputPinDesigns = new List<PinDesign>();
        public bool flag = false;
        public string @params;
    }

}

