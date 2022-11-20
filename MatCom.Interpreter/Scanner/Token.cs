using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    public enum TokenType
    {
        Identifier,
        Keyword,
        Separator,
        Operator,
        Comment,
        Number,
        String,
        WhiteSpace,
        LeftParantheses,
        RightParantheses,
        Reserved,
        Functions,
        Unary,
        Constants,
        Unknown,
        EOF
    }
  
    public class Token
    {
        public TokenType type { get; set; }
        public string value { get; set; }
     //   public int priority { get; set; }
        public int position { get; set; }

        public Token(TokenType type, string value, int position)
        {
            
            this.type = type;
            this.value = value;
    //        priority = this.TokenPriority(type, value);
            this.position = position;
        }

        public override string ToString()
        {
            return this.value;
        }
     /*   private  int TokenPriority(TokenType type, string value)
        {
            switch (type)
            {
                case TokenType.LeftParantheses:
                    return 0;
                case TokenType.RightParantheses:
                    return -1;
                case TokenType.Operator:
                    switch (value)
                    {
                        case "+":
                        case "-":
                            return 1;
                        case "/":
                        case "*":
                            return 2;
                        case "^":
                            return 4;
                        default:break;
                    }
                    return -1;
                case TokenType.Unary:
                    return 6;
                case TokenType.Functions:
                    switch (value.ToLower())
                    {
                        case "sqrt":
                        case "log":
                        case "logn":
                        case "exp":
                        case "abs":
                        case "sin":
                        case "cos":
                        case "tan":
                        case "sec":
                        case "csc":
                            return 8;
                        default:break;
                    }
                    return -1;
            }
            return -1;
        }*/
    }
}
