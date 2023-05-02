using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MiniPaint.Data
{
    abstract class Diagram
    {
        public Point Location { get; set; } = new Point(0, 0);
        public int type { get; set; }


    }
}
