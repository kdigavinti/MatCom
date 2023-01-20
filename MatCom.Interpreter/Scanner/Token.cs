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
  
    /// <summary>
    /// CLASS THAT MAINTAINS THE TOKENS.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; set; } //TYPE OF THE TOKEN
        public string Value { get; set; } //ACTUAL VALUE OF THE TOKEN
        public int Position { get; set; } //POSIITON OF THE TOKEN WITHIN THE STRING

        public Token(TokenType type, string value, int position)
        {
            
            this.Type = type;
            this.Value = value;
            this.Position = position;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}
