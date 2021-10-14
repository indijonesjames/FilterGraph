using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorMath;

namespace GraphLayoutLib
{
    public static class DrawingExtensions
    {
        public static Point ToPoint(this int2 me)
        {
            return new Point(me.x, me.y);
        }

        public static Size ToSize(this int2 me)
        {
            return new Size(me.x, me.y);
        }

        public static PointF ToPointF(this int2 me)
        {
            return new PointF((float)me.x, (float)me.y);
        }

        public static RectangleF ToRectangleF(this Rectangle me)
        {
            return new RectangleF((float)me.X, (float)me.Y, (float)me.Width, (float)me.Height);
        }

        public static int2 ToInt2(this Point me)
        {
            return new int2(me.X, me.Y);
        }
    }
}
