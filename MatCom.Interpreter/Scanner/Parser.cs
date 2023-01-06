using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    /*
     * statement = identifier ":=" boolExpression;
     * boolExpression = expression [('==' | '!=' | '<' | '<=' | '>' | '>=') expression];
     * expression = ["+"|"-"] term {("+"|"-"|'||') term};
     * term = factor {("*"|"/"|"%"|'&&') factor};
     *  factor = identifier | number | "(" expression ")" | - (Exp);
     */
    public class Parser
    {
        Token _currToken;
        public List<Token> Tokens;
        int _currentPosition = 0;
        public Environment EnvVariables;
        private string _expression { get; set; }
        //private string expression { get; set; }

        public Parser()
        {
            if (EnvVariables == null)
                EnvVariables = new Environment();

            if(_currToken == null)
                _currToken = new Token(TokenType.Unknown, "", -1);
        }

        public string Parse(string text)
        {
            _expression = text;
          /*(  if (text.StartsWith("diff("))
            {
                _expression = Differentiate(_expression);
            }*/
            Lexer lexer = new Lexer(_expression);
            _currentPosition = 0;
            Tokens = lexer.Tokenize();
            //foreach (Token token in Tokens)
            //{
            //    System.Diagnostics.Debug.WriteLine("{0, -25} {1, -18}", token.type, token.value);
            //}
            GetToken();
            AST? statement = Statement();
            ExpectToken(TokenType.EOF);
            return statement.Eval().ToString();
            
        }

        bool Validate(string expression)
        {
            expression = expression.Replace("x", "1", StringComparison.OrdinalIgnoreCase);
            if (Parse(expression) != "")
            {
                return true;
            }
            return false;
        }

        void GetToken()
        {
            if(this._currentPosition < this.Tokens.Count)
            {
                _currToken = this.Tokens[_currentPosition];
                _currentPosition++;
            }
        }

        Token NextToken()
        {
            if(this._currentPosition + 1 < this.Tokens.Count)
            {
                Token token = this.Tokens[_currentPosition+1];
                return token;
            }
            return null;
        }

        bool AcceptToken(TokenType type)
        {
            if(_currToken.type == type)
            {
                GetToken();
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
                    EnvVariables.assignValueByRef(left, right);
                }
               return Assignment();
            }
            else 
            { 
                expression = BooleanExpression();
                return expression;
            }
            return null;
        }

        AST? Assignment()
        {
            AST? variable = new ASTStringLeaf(_currToken.value);
            GetToken();
            while(_currToken.type != TokenType.EOF && _currToken.value == "=")
            {                
                GetToken();
                AST? rightNode = BooleanExpression();
                EnvVariables.assign(variable.ToString(), rightNode.Eval());
                variable = new ASTStringLeaf(variable.ToString() + "=" + rightNode.Eval().ToString());
              //  NextToken();
            }
            return variable;
        }

        AST? BooleanExpression()
        {
            AST? boolExpression = Expression();
            string _operator = string.Empty;
            while(_currToken.type != TokenType.EOF && boolExpression != null && _currToken.value is "==" or "!=" or "<" or "<=" or ">" or ">=")
            {
                _operator = _currToken.value.ToString();
                GetToken();
                AST? rightNode = Expression();
                boolExpression = new ASTBinaryOp(boolExpression, rightNode, _operator);
            }
            return boolExpression;
        }



        AST? Expression()
        {
            AST? expression = Term();
            string _operator = string.Empty;
            while(_currToken.type != TokenType.EOF && expression != null && _currToken.value is "+" or "-" or "||")
            {
                _operator = _currToken.value;
                GetToken();
                if (_currToken.type == TokenType.Operator)
                    throw new Exception($"Invalid Operator {_currToken.value} at position {_currToken.position}");
                AST? rightNode = Term();
                expression = new ASTBinaryOp(expression, rightNode, _operator);
            }
            return expression;
        }


        AST? Term()
        {
            AST? term = Power();
            string _operator = string.Empty;
            while (_currToken.type != TokenType.EOF && term != null && _currToken.value is "*" or "/" or "%" or "&&")
            {
                _operator = _currToken.value;
                GetToken();
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
                GetToken();
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
                    GetToken();
                    factor = BooleanExpression();
                    Match(")");
                    break;
                case TokenType.Unary:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    GetToken();
                    break;
                case TokenType.Operator:
                    GetToken();
                    factor = BooleanExpression();
                    break;
                case TokenType.Identifier:
                    object identifierValue = EnvVariables.getValue(_currToken.value);
                    if (identifierValue != null)
                    {
                        factor = new ASTNumericLeaf(Convert.ToDouble(identifierValue.ToString()));
                        GetToken();
                    }
                    else
                        throw new Exception($"unknown variable {_currToken.value} ");
                    break;
                case TokenType.Constants:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    GetToken();
                    break;
                case TokenType.Number:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.value));
                    GetToken();
                    break;
                case TokenType.Functions:
                    string functionName = _currToken.value;                    
                    GetToken();
                    if (_currToken.value == "(")
                    {
                        GetToken();
                        if(functionName.ToLower() == "diff")
                        {
                            _expression = Differentiate();
                            _currentPosition = 0;
                            GetToken();
                            factor = BooleanExpression();
                            factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), factor.Eval().ToString()));
                        }
                        else if ((_currToken.type != TokenType.Number && _currToken.type != TokenType.Identifier) || (NextToken != null && NextToken().value != ")"))
                        {
                            factor = BooleanExpression();
                            factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), factor.Eval().ToString()));
                            Match(")");
                        }
                        else
                        {
                            if (_currToken.type == TokenType.Identifier)
                                factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), EnvVariables.getValue(_currToken.value).ToString()));
                            else
                                factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), _currToken.value));
                            GetToken();
                            Match(")");
                        }
                       
                    }
                    else
                        throw new Exception($"Invalid Function {_currToken.value} at position {_currToken.position}");
                    break;
                default: throw new Exception($"Unexpected token {_currToken.value} at position {_currToken.position}");
            }
            return factor;
        }

        string Differentiate()
        {
            string expression = String.Empty;
            for (int i=_currentPosition-1; i<Tokens.Count-2;i++)
            {
                expression += Tokens[i].value;
            }            

            string[] keys = new string[] { "tan", "cos", "sin" };

            string sKeyResult = keys.FirstOrDefault<string>(s => expression.StartsWith(s));
            if (sKeyResult != null && sKeyResult.Length > 0)
            {

                string trigValue = expression.Substring(expression.IndexOf("(")+1, expression.IndexOf(")") - expression.IndexOf("(") - 1);
                switch (sKeyResult)
                {
                    case "tan":
                        expression = "sec(" + trigValue + ")*sec(" + trigValue + ")";
                        break;
                    case "sin":
                        expression = "cos(" + trigValue + ")";
                        break;
                    case "cos":
                        expression = "sin(-" + trigValue + ")";
                        break;
                    case "cot":
                        expression = "csc(-" + trigValue + ")*(csc(" + trigValue + ")";
                        break;
                    case "sec":
                        expression = "sec(" + trigValue + ")*(tan(" + trigValue + ")";
                        break;
                    case "csc":
                        expression = "csc(-" + trigValue + ")*(cot(" + trigValue + ")";
                        break;
                    default: break;
                }
                Lexer lexer = new Lexer(expression);
                List<Token> TokenList = lexer.Tokenize();
                this.Tokens = TokenList;
            }
            else
            {
                Lexer lexer = new Lexer(expression);
                List<Token> TokenList = lexer.Tokenize();
                if (TokenList != null)
                {
                    for (int i = 0; i < TokenList.Count; i++)
                    {
                        if (TokenList[i].type == TokenType.Identifier)
                        {
                            if (TokenList[i + 1].value != "^")
                            {
                                TokenList[i].value = "1";
                                TokenList[i].type = TokenType.Number;
                            }
                        }

                        if (TokenList[i].type == TokenType.Number)
                        {
                            if (i != TokenList.Count && TokenList[i + 1].value is "+" or "-" or " " or "" && ((i != 0 && TokenList[i - 1].value is "+" or "-") || i == 0))
                            {
                                TokenList[i].value = "0";
                            }
                        }

                        if (TokenList[i].value == "^")
                        {
                            if (i + 1 <= TokenList.Count)
                            {
                                double powerValue = Convert.ToDouble(TokenList[i + 1].value);
                                if (!Double.IsNaN(powerValue))
                                {
                                    TokenList[i + 1].value = (Convert.ToDouble(TokenList[i + 1].value) - 1).ToString();
                                    TokenList.Insert(i - 1, new Token(TokenType.Number, powerValue.ToString(), i - 1));
                                    TokenList.Insert(i, new Token(TokenType.Number, "*", i));
                                    i = i + 2;
                                }
                            }
                        }
                    }
                    expression = "";
                    for (int i = 0; i < TokenList.Count; i++)
                    {
                        expression += TokenList[i].value;
                    }
                    Tokens = TokenList;
                }
            }
            return expression;
        }

        void Match(string expected)
        {
            if (_currentPosition > 1 && Tokens[_currentPosition-1].value == expected)
            {
                GetToken();
            }
            else
            {
                throw new Exception($"Expected token {expected} at position {_currentPosition}");
            }
        }
    }
}
