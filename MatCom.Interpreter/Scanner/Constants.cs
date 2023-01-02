using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    internal class Constants
    {
        public static Dictionary<string, string> AllowedOperators =
            new()
            {
                {"*", "Multiply"},
                {"/", "Divide"},
                {"+", "Add" },
                {"-", "Subtract" },
                {"^", "Power" },
                {"%", "Modulus" },
                {"(", "LeftParantheses" },
                {")", "RightParantheses" }
            };

        public static Dictionary<string, string> AllowedConstants =
            new()
            {
                {"e", Math.E.ToString() },
                {"pi", Math.PI.ToString() },
                {"π", Math.PI.ToString() }                
            };

        public static Dictionary<string, string> AllowedFunctions =
            new()
            {
                {"abs", "Abs" },
                {"exp", "Exp" },
                {"log", "Log" },
                {"ln", "ln" },
                {"sqrt", "Sqrt" },
                {"√", "Sqrt" },
                {"sin", "Sin"},
                {"cos", "Cos" },
                {"tan","Tan" },
                {"csc", "Csc" },
                {"sec", "Sec" },
                {"cot", "Cot" }
            };

        public static Dictionary<string, string> Keywords =
            new()
            {
                {"if", "if" },
                {"else", "else" },
                {"then", "then" },
                {"switch", "switch" },
                {"while", "while" },
                {"var", "var" },
                {"for", "for" },
                {"case", "case" },
                {"int", "int" },
                {"float", "float" },
                {"double", "double" },
                {"bool", "bool" },
                {"string", "string" }
            };

        public static double FunctionValue(string functionName, string value)
        {
            if (String.Compare(value, "pi", StringComparison.OrdinalIgnoreCase) == 0)
                value = Math.PI.ToString();
            Parser parser = new Parser();
            value = parser.Parse(value);
            double _value = 0;
            double result = 0;
            if (double.TryParse(value, out _value))
            {                
                switch (functionName.ToLower())
                {
                    case "abs": result = Math.Abs(_value); break;
                    case "log": result = Math.Log10(_value); break;
                    case "ln": result = Math.Log(_value); break;
                    case "exp": result = Math.Exp(_value); break;
                    case "sqrt": result = Math.Sqrt(_value); break;
                    case "cbrt": result = Math.Cbrt(_value); break;
                    case "sin": result = Math.Sin(_value); break;
                    case "cos": result = Math.Cos(_value); break;
                    case "tan": result = Math.Tan(_value); break;
                    case "csc": result = 1 / Math.Sin(_value); break;
                    case "sec": result = 1 / Math.Cos(_value); break;
                    case "cot": result = 1 / Math.Tan(_value); break;
                    default: break;
                }
            }
            else
                throw new Exception("Invalid values for the function");
            return result;
        }

    }
}
