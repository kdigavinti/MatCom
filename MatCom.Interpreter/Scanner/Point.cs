using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    public class GraphPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        //double x, y;
        public GraphPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
