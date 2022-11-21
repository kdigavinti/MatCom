using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;

namespace MatCom.Interpreter.Scanner
{
    public class Evaluator
    {
        Queue<Token> tokens = new Queue<Token>();
        
        public Evaluator(Queue<Token> tokens)
        {
            this.tokens = tokens;
        }

      public static void Parsing(string expression, double x, ConcurrentDictionary<double, double> points)
        {
            Parser p = new Parser();
            //p.Parse("x = " + i.ToString());
            expression = expression.Replace("x", x.ToString("F2"));
            double y = Math.Round(Convert.ToDouble(p.Parse(expression)), 2);
            points.TryAdd(x, y);
        }

       public static List<GraphPoint> Evaluate(decimal xmin, decimal xmax, decimal steps, string expression)
        {            
            List<GraphPoint> graphPoints = new List<GraphPoint>();
            ConcurrentDictionary<double, double> points = new ConcurrentDictionary<double, double>();
            double y;
            Parser p = new Parser();
            for (decimal i=xmin; i<=xmax; i = i + steps)
            {
                string exp = expression;
                double x = Math.Round( (double)i, 2);
                //ParameterizedThreadStart pts = new ParameterizedThreadStart(Parsing);
                Thread th = new Thread(pts => Parsing(exp, x, points));
                th.Start();
                th.Join();
                //th.Start(exp, x, points);
                //Task<double> task = Task<double>.Factory.StartNew(() =>
                //{
                //    double result = Parsing(expression.Replace("x", tmp.ToString()));
                //        return result;
                //});
                //points.Add(new GraphPoint((double)i, task.Result));

                /*  y = Convert.ToDouble(p.Parse(expression.Replace("x", i.ToString())));
                  points.Add(new GraphPoint((double)i, y));*/
            }
            foreach(var item in points.OrderBy(x=>x.Key))
            {
                graphPoints.Add(new GraphPoint(item.Key, item.Value));
            }
            return graphPoints;
       /*     if(this.tokens != null && this.tokens.Count > 0)
            {
                Stack<double> stack = new Stack<double>();
                double leftOperand, rightOperand, result;
                foreach (var token in this.tokens)
                {
                    leftOperand = 0;
                    rightOperand = 0;
                    result = 0;
                    if(token.Type == TokenType.Number|| token.Type == TokenType.Unary || token.Type == TokenType.Constants)
                    {
                        if(token.Value != null && double.TryParse(token.Value, out _)) 
                            stack.Push(double.Parse(token.Value));
                        Print(token.Value);
                    }
                    //else if(token.Type == TokenType.Identifier)
                    //{
                    //    stack.Push(token.Value);
                    //}
                    else if(token.Type == TokenType.Operator)
                    {
                        if(stack.Count > 1)
                        {
                            rightOperand = stack.Pop();
                            leftOperand = stack.Pop();
                        }
                        else
                        {
                            throw new Exception("Invalid Expression");
                        }
                        switch (token.Value)
                        {
                            case "+": result = leftOperand + rightOperand;break;
                            case "-": result = leftOperand - rightOperand;break;
                            case "*": result = leftOperand * rightOperand; break;
                            case "/": 
                                if(rightOperand == 0)
                                {
                                    throw new DivideByZeroException("Divisor cannot be zero");
                                }
                                result = leftOperand / rightOperand; break;
                            case "^": result = Math.Pow(leftOperand, rightOperand);break;
                            case "%": result = leftOperand % rightOperand;break;
                            
                            default: throw new ArgumentException("Unknown Operator");
                        }
                        stack.Push(result);
                        Print(token.Value);                       
                    }
                    else if(token.Type == TokenType.Functions)
                    {
                        if (stack.Count >= 1)
                        {
                            rightOperand = stack.Pop();
                        }
                        switch (token.Value.ToLower())
                        {
                            case "abs": result = Math.Abs(rightOperand);break;
                            case "log": result = Math.Log(rightOperand);break;
                            case "exp": result = Math.Exp(rightOperand);break;
                            case "sqrt": result = Math.Sqrt(rightOperand);break;
                            case "sin": result = Math.Sin(rightOperand); break;
                            case "cos": result = Math.Cos(rightOperand);break;
                            case "tan": result = Math.Tan(rightOperand);break;
                            case "csc": result = 1/ Math.Sin(rightOperand); break;
                            case "sec": result = 1/ Math.Cos(rightOperand); break;
                            case "cot": result = 1 / Math.Tan(rightOperand);break;                                
                            default: break;
                        }
                        stack.Push(result);
                        Print(token.Value);
                    }
                  
                }
                void Print(string action) => System.Diagnostics.Debug.WriteLine("{0,-4} {1,-18}", action + ":", $"stack[ {string.Join(" ", stack.Reverse())} ]");
                if (stack.Count != 1)
                    throw new Exception("Invalid Expression");

                //System.Diagnostics.Debug.WriteLine(stack.Pop());
                return stack.Pop().ToString();
            }

            return "";*/
        }

    }
}
