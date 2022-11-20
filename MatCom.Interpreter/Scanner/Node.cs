using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Analysis.Scanner
{
    public abstract class TreeNode
    {
        public abstract double Eval();

    }

    public class Add: TreeNode
    {
        public override double Eval()
        {
            throw new NotImplementedException();
        }
    }


}
