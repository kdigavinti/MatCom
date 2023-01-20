using System.Text.RegularExpressions;

namespace MatCom.Interpreter.Scanner
{
    public class Lexer
    {

        private string _text { get; set; }
        private int _currentPosition { get; set; }
        private int _textLength { get; set; }


        public Lexer(string text)
        {
            this._text = text;
            _currentPosition = 0;
            _textLength = text.Length;
        }

        public List<Token> Tokenize()
        {            
            List<Token> TokenList = new List<Token>();
            
            string match = string.Empty;
            while (_currentPosition < _textLength)
            {
                match = string.Empty;
                bool unary = false;
                //CHECK UNARY
                if (_currentPosition == 0 || (_currentPosition > 0 && _text[_currentPosition-1] is ' ' or '(' or '*' or '/' or '+' or '-' or '^' or '=' or '|'))
                {
                    if ((match = CheckToken(@"^[-+?](\d*\.{0,1}\d+)")).Length > 0)
                    {
                        TokenList.Add(new Token(TokenType.Unary, match, _currentPosition));
                        unary = true;
                    }
                }
                if (unary == false)
                {
                    if (CheckToken(@"^\s").Length > 0) //CHECK WHITESPACES
                    {
                        ;
                    }
                    else if ((match = CheckToken(@"^((\d+(\.\d*)?)|(\.\d+))")).Length > 0) //CHECK DECIMALS
                    {
                        TokenList.Add(new Token(TokenType.Number, match, _currentPosition));
                    }
                    else if((match = CheckToken(@"^(&&|==|\|\||<=|>=|!=)")).Length > 0) //Check Boolean Operators
                    {
                        TokenList.Add(new Token(TokenType.Operator, match, _currentPosition));
                    }
                    else if ((match = CheckToken(@"^[-+*%=\<\>\/\^\|]")).Length > 0) //CHECK OPERATORS
                    {
                        TokenList.Add(new Token(TokenType.Operator, match, _currentPosition));
                    }
                    else if ((match = CheckToken(@"^(sqrt|cbrt|log|ln|exp|abs|sin|cos|tan|sec|csc|cot|diffvalue|diff)")).Length > 0) //CHECK Functions
                    {
                        TokenList.Add(new Token(TokenType.Functions, match, _currentPosition));
                    }
                    else if ((match = CheckToken(@"^[a-zA-Z0-9_]*")).Length > 0) //CHECK IDENTIFIERS AND MAKE SURE IT IS NOT A RESSERVED WORD. 
                    {
                        var constant = "";
                        if (Constants.AllowedConstants.ContainsKey(match.ToLower()) && (constant = Constants.AllowedConstants[match.ToLower()]) != "")
                        {
                            TokenList.Add(new Token(TokenType.Constants, constant, _currentPosition));
                        }
                        else if (Constants.Keywords.ContainsKey(match.ToLower()))
                        {
                            throw new Exception($"{match} is a reserved word. Variables cannot use reserved words");
                        }
                        else
                        {
                            TokenList.Add(new Token(TokenType.Identifier, match, _currentPosition));
                        }
                    }
                    else if ((match = CheckToken(@"^\(")).Length > 0) //CHECK Left Parantheses
                    {
                        TokenList.Add(new Token(TokenType.LeftParantheses, match, _currentPosition));
                    }
                    else if ((match = CheckToken(@"^\)")).Length > 0) //CHECK Right Parantheses
                    {
                        TokenList.Add(new Token(TokenType.Functions, match, _currentPosition));
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid token {_text[_currentPosition]} at position {_currentPosition + 1}");
                    }
                }
            }
            TokenList.Add(new Token(TokenType.EOF, "", _currentPosition));
            return TokenList;
        }

        /// <summary>
        /// CHECK THE TOKENS FOR THE MATCHING TOKEN TYPES.
        /// </summary>
        /// <param name="regExp"></param>
        /// <returns></returns>
        string CheckToken(string regExp)
        {
            var regExpression = new Regex(regExp);
            Match match = regExpression.Match(this._text.Substring(_currentPosition));
            if (match.Success && match.Length > 0)
            {
                _currentPosition += match.Length;
                return match.Value;
            }
            return String.Empty;
        }
    }
}