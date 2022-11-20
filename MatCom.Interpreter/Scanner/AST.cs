using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    public abstract class AST
    {
        public abstract object Eval();
    }

    public class ASTNumericLeaf: AST
    {
        public readonly double _value;
        
        public ASTNumericLeaf(double value)
        {
            this._value = value;
        }

        public override object Eval()
        {
            return this._value;
        }

        public override string ToString()
        {
            return this._value.ToString();
        }
    }

    public class ASTStringLeaf : AST
    {
        public readonly string _value;

        public ASTStringLeaf(string value)
        {
            this._value = value;
        }

        public override string Eval()
        {
            return this._value;
        }

        public override string ToString()
        {
            return this._value.ToString();
        }
    }

    //public class ASTAssignOp: AST
    //{
    //    readonly AST _leftNode;
    //    readonly AST _rightNode;    
        
    //}

    public class ASTBinaryOp : AST
    {
        public readonly AST _leftNode;
        public readonly AST _rightNode;
        public string _operator;

        public ASTBinaryOp(AST leftNode, AST rightNode, string @operator)
        {
            this._leftNode = leftNode;
            this._rightNode = rightNode;
            _operator = @operator;
        }

        public override object Eval()
        {
            // Perform the evaluation
            switch(_operator){
                case "+": return (double)this._leftNode.Eval() + (double)this._rightNode.Eval();
                case "-": return (double)this._leftNode.Eval() - (double)this._rightNode.Eval();
                case "*": return (double)this._leftNode.Eval() * (double)this._rightNode.Eval();
                case "/":
                    if ((double)this._rightNode.Eval() != 0)
                        return (double)this._leftNode.Eval() / (double)this._rightNode.Eval();
                    else
                        throw new DivideByZeroException("Divide by Zero error!!!");
                case "==": return (double)this._leftNode.Eval() == (double)this._rightNode.Eval();
                case ">=": return (double)this._leftNode.Eval() >= (double)this._rightNode.Eval();
                case "<=": return (double)this._leftNode.Eval() <= (double)this._rightNode.Eval();
                case "<":
                    return (bool)((double)this._leftNode.Eval() < (double)this._rightNode.Eval());
                case ">": return (bool)((double)this._leftNode.Eval() < (double)this._rightNode.Eval());
                case "|":
                    bool _nodeValue = false;
                    if(!Boolean.TryParse(this._leftNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator | can only be applied to boolean types");
                    }
                    if (!Boolean.TryParse(this._rightNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator | can only be applied to boolean types");
                    }
                    return (Boolean)this._leftNode.Eval() | (Boolean)this._rightNode.Eval();
                case "&":
                    if (!Boolean.TryParse(this._leftNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    if (!Boolean.TryParse(this._rightNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    return (Boolean)this._leftNode.Eval() | (Boolean)this._rightNode.Eval();
                case "||":
                    if (!Boolean.TryParse(this._leftNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    if (!Boolean.TryParse(this._rightNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    return (Boolean)this._leftNode.Eval() || (Boolean)this._rightNode.Eval();
                case "&&":
                    if (!Boolean.TryParse(this._leftNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    if (!Boolean.TryParse(this._rightNode.Eval().ToString(), out _nodeValue))
                    {
                        throw new ArgumentException("Operator & can only be applied to boolean types");
                    }
                    return (Boolean)this._leftNode.Eval() && (Boolean)this._rightNode.Eval();
                default: break;
            }
            return 0;
        }

        public override string ToString()
        {
            return String.Format($"{this._leftNode.ToString()} {_operator} {this._rightNode.ToString()}");
        }
    }


}
