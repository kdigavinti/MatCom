using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    /*
     * statement = identifier ":=" expression;
     * booleanexpression = expression {("=="|"!=") expression};
     * expression = ["+"|"-"] term {("+"|"-") term};
     * term = factor {("*"|"/") factor};
     *  factor = identifier | number | "(" expression ")" | - (Exp);
     */
    public class Parser
    {
        Token _currToken;
        List<Token> Tokens;
        int _currentPosition = 0;
        public Environment _environment;
        private string _expression { get; set; }
        //private string expression { get; set; }

        public Parser()
        {
            if (_environment == null)
                _environment = new Environment();

            if(_currToken == null)
                _currToken = new Token(TokenType.Unknown, "", -1);
        }

        public string Parse(string text)
        {
            _expression = text;
           Lexer lexer = new Lexer(text);
            _currentPosition = 0;
            Tokens = lexer.Tokenize();
            //foreach (Token token in Tokens)
            //{
            //    System.Diagnostics.Debug.WriteLine("{0, -25} {1, -18}", token.type, token.value);
            //}
            NextToken();
            AST? statement = Statement();
            ExpectToken(TokenType.EOF);
            return statement.Eval().ToString();
        }

        void NextToken()
        {
            if(this._currentPosition < this.Tokens.Count)
            {
                _currToken = this.Tokens[_currentPosition];
                _currentPosition++;
            }
        }

        bool AcceptToken(TokenType type)
        {
            if(_currToken.type == type)
            {
                NextToken();
                return true;
            }
            return false;
        }

        bool ExpectToken(TokenType type)
        {
            if (AcceptToken(type))
                return true;
            throw new Exception($"Unexpected Token {_currToken} at position {_currentPosition}");
        }

        AST? Statement()
        {
            AST? expression;
            if (this._currToken.type == TokenType.Identifier && Tokens[_currentPosition].value == "=")
            {
                //Assign the environment values. 
                string[] split = _expression.Split("=");
                if (split.Length > 1)
                { 
                    string left = split[0].Trim();
                    string right = split[1].Trim();
                    _environment.assignValueByRef(left, right);
                }
               return Assignment();
            }
            else 
            { 
                expression = Expression();
                return expression;
            }
            return null;
        }

        AST? Assignment()
        {
            AST? variable = new ASTStringLeaf(_currToken.value);
            NextToken();
            while(_currToken.type != TokenType.EOF && _currToken.value == "=")
            {                
                NextToken();
                AST? rightNode = Expression();
                _environment.assign(variable.ToString(), rightNode.Eval());
                variable = new ASTStringLeaf(variable.ToString() + "=" + rightNode.Eval().ToString());
              //  NextToken();
            }
            return variable;
        }

        AST? Expression()
        {
            AST? expression = Term();
            string _operator = string.Empty;
            while(_currToken.type != TokenType.EOF && expression != null && _currToken.value is "+" or "-")
            {
                _operator = _currToken.value;
                NextToken();
                AST? rightNode = Term();
                expression = new ASTBinaryOp(expression, rightNode, _operator);
            }
            return expression;
        }


        AST? Term()
        {
            /* AST? term = Factor();
             string _operator = string.Empty;
             while (_currToken.type != TokenType.EOF && term != null && _currToken.value is "^" or "*" or "/" or "|" or "&" or "<" or ">" or "<=" or ">=" or "==" or "!=" or "&&" or "||")
             {
                 _operator = _currToken.value;
                 NextToken();
                 AST? rightNode = Factor();
                 term = new ASTBinaryOp(term, rightNode, _operator);
             }
             return term;*/
            AST? term = Power();
            string _operator = string.Empty;
            while (_currToken.type != TokenType.EOF && term != null && _currToken.value is "*" or "/" or "|" or "&" or "<" or ">" or "<=" or ">=" or "==" or "!=" or "&&" or "||")
            {
                _operator = _currToken.value;
                NextToken();
                AST? rightNode = Power();
                term = new ASTBinaryOp(term, rightNode, _operator);
            }
            return term;
        }

        AST? Power()
        {
            AST? power = Factor();
            string _operator = string.Empty;
            while(_currToken.type != TokenType.EOF && power != null && _currToken.value == "^")
            {
                _operator = _currToken.value;
                NextToken();
                AST? rightNode = Factor();
                power = new ASTBinaryOp(power, rightNode, _operator);
            }
            return power;
        }

        AST? Factor()
        {
            AST? factor = null;
            switch (_currToken.type)
            {
                case TokenType.LeftParantheses:
                    NextToken();
                    factor = Expression();
                    Match(")");
                    break;
                case TokenType.Unary:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    NextToken();
                    break;
                case TokenType.Operator:
                    NextToken();
                    factor = Expression();
                    break;
                case TokenType.Identifier:
                    object identifierValue = _environment.getValue(_currToken.value);
                    if (identifierValue != null)
                    {
                        factor = new ASTNumericLeaf(Convert.ToDouble(identifierValue.ToString()));
                        NextToken();
                    }
                    else
                        throw new Exception($"unknown variable {_currToken.value} ");
                    break;
                case TokenType.Constants:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    NextToken();
                    break;
                case TokenType.Number:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    NextToken();
                    break;
                case TokenType.Functions:
                    string functionName = _currToken.value;
                    NextToken();
                    if (_currToken.value == "(")
                    {
                        NextToken(); //Skip left parantheses
                        factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), _currToken.value));
                        NextToken(); //eat Token )
                        if(_currToken.value != ")")
                        {
                            throw new Exception($"Closing Parantheses not found at position {_currToken.position}");
                        }
                        NextToken();
                    }
                    else
                        throw new Exception($"Invalid Function {_currToken.value} at position {_currToken.position}");
                    break;
                default: throw new Exception($"Unexpected token {_currToken.value} at position {_currToken.position}");
            }
            return factor;
        }

        void Match(string expected)
        {
            if (_currentPosition > 1 && Tokens[_currentPosition-1].value == expected)
            {
                NextToken();
            }
            else
            {
                throw new Exception($"Expected token {expected} at position {_currentPosition}");
            }
        }

      /*  public Queue<Token> Parse1(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("Expression is empty");
            }
            Lexer lexer = new(expression);
            List<Token> tokens = lexer.Tokenize();

            foreach (Token token in tokens)
            {
                System.Diagnostics.Debug.WriteLine("{0, -25} {1, -18}", token.Type, token.Value);
            }
            var stack = new Stack<Token>();
              var queue = new Queue<Token>();
              foreach (Token token in tokens)
              {
                  if (token != null)
                  {
                    switch (token.Type)
                    {
                        case TokenType.Number:
                        case TokenType.Unary:
                        case TokenType.Identifier:
                        case TokenType.Constants:
                            queue.Enqueue(token);
                            Print(token.Value);
                            break;
                        case TokenType.Operator:
                            while(stack.Count > 0 && token.Priority <= stack.Peek().Priority)
                            {
                                queue.Enqueue(stack.Pop());
                                Print(token.Value);
                            }
                            stack.Push(token);
                            Print(token.Value);
                            break;
                        case TokenType.LeftParantheses:
                        case TokenType.Functions:
                            stack.Push(token);
                            Print(token.Value);
                            break;
                        case TokenType.RightParantheses:
                            Token stackTopToken;
                            while((stackTopToken = stack.Pop()).Type != TokenType.LeftParantheses )
                            {
                                queue.Enqueue(stackTopToken);
                                Print(token.Value);
                            }
                            break;
                        default:break;
                    }
                  }
              }
              Token elem;
              while(stack.Count > 0 && (elem = stack.Pop()).Type != TokenType.LeftParantheses )
              {
                queue.Enqueue(elem);
                Print(elem.Value);
              }
            void Print(string action) => System.Diagnostics.Debug.WriteLine("{0,-4} {1,-18} {2}", action + ":", $"stack[ {string.Join(" ", stack.Reverse())} ]", $"out[ {string.Join(" ", queue)} ]");
            return queue;
              
        }*/

    }
}
