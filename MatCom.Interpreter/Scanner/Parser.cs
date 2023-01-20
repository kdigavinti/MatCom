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
            Lexer lexer = new Lexer(_expression);
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

        /// <summary>
        /// VALIDATE THE EXPRESSION BEFORE PARSING IT. MAINLY USED DURING GRAPH POINTS.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool Validate(string expression)
        {
            expression = expression.Replace("x", "1", StringComparison.OrdinalIgnoreCase);
            if (Parse(expression) != "")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// GETS THE CURRENT TOKEN IN THE LIST.
        /// </summary>
        void NextToken()
        {
            if(this._currentPosition < this.Tokens.Count)
            {
                _currToken = this.Tokens[_currentPosition];
                _currentPosition++;
            }
        }

        /// <summary>
        /// GETS THE NEXT TOKEN IN THE LIST.
        /// </summary>
        /// <returns></returns>
        Token AdvanceToken()
        {
            if(this._currentPosition + 1 < this.Tokens.Count)
            {
                Token token = this.Tokens[_currentPosition+1];
                return token;
            }
            return null;
        }

        /// <summary>
        /// ACCEPT A TOKEN IF IT MATCHES THE TOKEN TYPE
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool AcceptToken(TokenType type)
        {
            if(_currToken.Type == type)
            {
                NextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        /// USED TO HANDLE ERRORS. IF A PARTICULAR TOKEN IS NOT FOUND AT THE GIVEN POSITION THIS THROWS AN EXCEPTION.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool ExpectToken(TokenType type)
        {
            if (AcceptToken(type))
                return true;
            throw new Exception($"Unexpected Token {_currToken} at position {_currentPosition}");
        }

        /// <summary>
        /// USED FOR ASSIGNMENT OPERATOR.
        /// </summary>
        /// <returns></returns>
        AST? Statement()
        {
            AST? expression;
            if (this._currToken.Type == TokenType.Identifier && Tokens[_currentPosition].Value == "=") ///THIS LOGIC IS USED TO STORE THE VALUE BY REFERENCES.
            {
                //Assign the environment values. 
                string[] split = _expression.Split("=");
                if (split.Length > 1)
                { 
                    string left = split[0].Trim();
                    string right = split[1].Trim();
                    EnvVariables.AssignValueByRef(left, right);
                }
               return Assignment();
            }
            else  //CHECK IF THE ASSIGNMENT IS AN EXPRESSION.
            { 
                expression = BooleanExpression();
                return expression;
            }
            return null;
        }

        /// <summary>
        /// ASSIGN THE VALUE TO THE IDENTIFIER
        /// </summary>
        /// <returns></returns>
        AST? Assignment()
        {
            AST? variable = new ASTStringLeaf(_currToken.Value);
            NextToken();
            while(_currToken.Type != TokenType.EOF && _currToken.Value == "=")
            {                
                NextToken();
                AST? rightNode = BooleanExpression();
                EnvVariables.Assign(variable.ToString(), rightNode.Eval());
                variable = new ASTStringLeaf(variable.ToString() + "=" + rightNode.Eval().ToString());
              //  NextToken();
            }
            return variable;
        }

        /// <summary>
        /// CALCULATE BOOLEAN VALUES.
        /// </summary>
        /// <returns></returns>
        AST? BooleanExpression()
        {
            AST? boolExpression = Expression();
            string _operator = string.Empty;
            while(_currToken.Type != TokenType.EOF && boolExpression != null && _currToken.Value is "==" or "!=" or "<" or "<=" or ">" or ">=")
            {
                _operator = _currToken.Value.ToString();
                NextToken();
                AST? rightNode = Expression();
                boolExpression = new ASTBinaryOp(boolExpression, rightNode, _operator);
            }
            return boolExpression;
        }


        /// <summary>
        /// CALCULATE FOR + or - or BOOLEAN || operators.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        AST? Expression()
        {
            AST? expression = Term();
            string _operator = string.Empty;
            while(_currToken.Type != TokenType.EOF && expression != null && _currToken.Value is "+" or "-" or "||")
            {
                _operator = _currToken.Value;
                NextToken();
                if (_currToken.Type == TokenType.Operator)
                    throw new Exception($"Invalid Operator {_currToken.Value} at position {_currToken.Position}");
                AST? rightNode = Term();
                expression = new ASTBinaryOp(expression, rightNode, _operator);
            }
            return expression;
        }


        /// <summary>
        /// CALCULATE FOR * or / or % or && operators.
        /// </summary>
        /// <returns></returns>
        AST? Term()
        {
            AST? term = Power();
            string _operator = string.Empty;
            while (_currToken.Type != TokenType.EOF && term != null && _currToken.Value is "*" or "/" or "%" or "&&")
            {
                _operator = _currToken.Value;
                NextToken();
                AST? rightNode = Power();
                term = new ASTBinaryOp(term, rightNode, _operator);
            }
            return term;
        }

        /// <summary>
        /// CALCULATE POWER OF A VALUE.
        /// </summary>
        /// <returns></returns>
        AST? Power()
        {
            AST? power = Factor();
            string _operator = string.Empty;
            while(_currToken.Type != TokenType.EOF && power != null && _currToken.Value == "^")
            {
                _operator = _currToken.Value;
                NextToken();
                AST? rightNode = Factor();
                power = new ASTBinaryOp(power, rightNode, _operator);
            }
            return power;
        }

        /// <summary>
        /// THIS HANDLES FOR TERMINALS. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        AST? Factor()
        {
            AST? factor = null;
            switch (_currToken.Type)
            {
                case TokenType.LeftParantheses:
                    NextToken();
                    factor = BooleanExpression();
                    Match(")");
                    break;
                case TokenType.Unary:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.Value));
                    NextToken();
                    break;
                case TokenType.Operator:
                    NextToken();
                    factor = BooleanExpression();
                    break;
                case TokenType.Identifier:
                    object identifierValue = EnvVariables.GetValue(_currToken.Value);
                    if (identifierValue != null)
                    {
                        factor = new ASTNumericLeaf(Convert.ToDouble(identifierValue.ToString()));
                        NextToken();
                    }
                    else
                        throw new Exception($"unknown variable {_currToken.Value} ");
                    break;
                case TokenType.Constants:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.Value));
                    NextToken();
                    break;
                case TokenType.Number:
                    factor = new ASTNumericLeaf(Convert.ToDouble(_currToken.Value));
                    NextToken();
                    break;
                case TokenType.Functions:
                    string functionName = _currToken.Value;                    
                    NextToken();
                    if (_currToken.Value == "(")
                    {
                        NextToken();
                        if(functionName.ToLower() == "diffvalue")
                        {
                            _expression = Differentiate();
                            _currentPosition = 0;
                            NextToken();
                            factor = BooleanExpression();
                            factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), factor.Eval().ToString()));
                        }
                        else if(functionName.ToLower() == "diff")
                        {
                            _expression = Differentiate();
                            factor = new ASTStringLeaf(_expression);
                        }
                        else if ((_currToken.Type != TokenType.Number && _currToken.Type != TokenType.Identifier) || (NextToken != null && AdvanceToken().Value != ")"))
                        {
                            factor = BooleanExpression();
                            factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), factor.Eval().ToString()));
                            Match(")");
                        }
                        else
                        {
                            if (_currToken.Type == TokenType.Identifier)
                                factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), EnvVariables.GetValue(_currToken.Value).ToString()));
                            else
                                factor = new ASTNumericLeaf(Constants.FunctionValue(functionName.ToLower(), _currToken.Value));
                            NextToken();
                            Match(")");
                        }
                       
                    }
                    else
                        throw new Exception($"Invalid Function {_currToken.Value} at position {_currToken.Position}");
                    break;
                default: throw new Exception($"Unexpected token {_currToken.Value} at position {_currToken.Position}");
            }
            return factor;
        }

        string Differentiate() //DIFFERNTIATION OF A FUNCTION.
        {
            string expression = String.Empty;
            for (int i=_currentPosition-1; i<Tokens.Count-2;i++)
            {
                expression += Tokens[i].Value;
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
                        if (TokenList[i].Type == TokenType.Identifier)
                        {
                            if (TokenList[i + 1].Value != "^")
                            {
                                TokenList[i].Value = "1";
                                TokenList[i].Type = TokenType.Number;
                            }
                        }

                        if (TokenList[i].Type == TokenType.Number)
                        {
                            if (i != TokenList.Count && TokenList[i + 1].Value is "+" or "-" or " " or "" && ((i != 0 && TokenList[i - 1].Value is "+" or "-") || i == 0))
                            {
                                TokenList[i].Value = "0";
                            }
                        }

                        if (TokenList[i].Value == "^")
                        {
                            if (i + 1 <= TokenList.Count)
                            {
                                double powerValue = Convert.ToDouble(TokenList[i + 1].Value);
                                if (!Double.IsNaN(powerValue))
                                {
                                    TokenList[i + 1].Value = (Convert.ToDouble(TokenList[i + 1].Value) - 1).ToString();
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
                        expression += TokenList[i].Value;
                        _currentPosition = i;
                        _currToken = TokenList[i];
                    }
                    Tokens = TokenList;
                }
            }
            return expression;
        }

        void Match(string expected)
        {
            if (_currentPosition > 1 && Tokens[_currentPosition-1].Value == expected)
            {
                NextToken();
            }
            else
            {
                throw new Exception($"Expected token {expected} at position {_currentPosition}");
            }
        }
    }
}
