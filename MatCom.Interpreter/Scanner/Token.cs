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
        public TokenType type { get; set; } //TYPE OF THE TOKEN
        public string value { get; set; } //ACTUAL VALUE OF THE TOKEN
        public int position { get; set; } //POSIITON OF THE TOKEN WITHIN THE STRING

        public Token(TokenType type, string value, int position)
        {
            
            this.type = type;
            this.value = value;
            this.position = position;
        }

        public override string ToString()
        {
            return this.value;
        }
    }
}
