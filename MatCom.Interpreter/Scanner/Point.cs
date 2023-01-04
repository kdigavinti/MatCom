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

    public class ZeroCrossingRange
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public ZeroCrossingRange()
        {

        }
        public ZeroCrossingRange(double x1, double x2)
        {
            X1 = x1;
            X2 = x2;
        }
    }
}
