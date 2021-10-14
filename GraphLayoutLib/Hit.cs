using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayoutLib
{
    public class Hit
    {
        public Rectangle bounds;
        public Action<Point> MouseDown;
        public Action<Point> MouseDoubleClick;
        public Action<Point> Drag;
        public Func<Point, bool> PreviewDrop;
        public Action Drop;
        public Action EndDrag;
        public Action HoverFocus;
        public Action HoverBlur;
    }
}
