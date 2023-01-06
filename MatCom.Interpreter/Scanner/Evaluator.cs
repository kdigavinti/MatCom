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
        ConcurrentDictionary<double, double> Points = new ConcurrentDictionary<double, double>();
        public List<GraphPoint> GraphPoints = new List<GraphPoint>();

        public Evaluator()
        {
            Points.Clear();
        }

        public Evaluator(Queue<Token> tokens)
        {
            this.tokens = tokens;
        }

        //added by Kiran
        public double ParseExpressionForSingleValue(string expression, double x)
        {
            try
            {
                Parser parser = new Parser();
                expression = expression.Replace("x", x.ToString());
                double y = Convert.ToDouble(parser.Parse(expression));
                return y;
            }
            catch(DivideByZeroException ex)
            {
                return double.NaN;
            }
        }

        public void Parsing(string expression, double x)
        {
            Parser parser = new Parser();
            expression = expression.Replace("x", x.ToString(), StringComparison.OrdinalIgnoreCase);   
            double y = Convert.ToDouble(parser.Parse(expression));
            //if(!Double.IsNaN(y))
                Points.TryAdd(x, y);
        }

        private bool Validate(string expression)
        {
            Parser parser = new Parser();
            expression = expression.Replace("x", "1", StringComparison.OrdinalIgnoreCase);
            if(parser.Parse(expression) != "")
            {
                return true;
            }
            return false;
        }

        public static double ParseFunction(string expression, double x)
        {
            Parser parser = new Parser();
            expression = expression.Replace("x", x.ToString());
            double y = Convert.ToDouble(parser.Parse(expression));
            return y;
        }

        public async Task RunTasks(decimal xmin, decimal xmax, decimal steps, string expression)
        {
            var tasksList = new List<Task>();   
            for(decimal i = xmin; i <= xmax; i = i + steps)
            {
                string exp = expression;
                var runTask = Task.Run(() => Parsing(exp, (double)i));
                tasksList.Add(runTask);
            }
            await Task.WhenAll(tasksList);
        }

        public async Task<List<GraphPoint>> Evaluate(List<double> inputPoints, string expression)
        {
            if (Validate(expression))
            {
                await Task.Run(() => Parallel.ForEach(inputPoints, p =>
                {
                    try
                    {
                        Parsing(expression, p);
                    }
                    catch(DivideByZeroException ex)
                    {
                        Points.TryAdd(p,double.NaN);
                    }
                }));
                foreach (var item in Points.OrderBy(x => x.Key))
                {
                    GraphPoints.Add(new GraphPoint(item.Key, item.Value));
                }
            }
            return GraphPoints;
        }
        public List<GraphPoint> EvaluateNoAsync(List<double> inputPoints, string expression)
        {
            if (Validate(expression))
            {
                foreach (double d in inputPoints)
                {
                    try
                    {
                        Parsing(expression, d);
                    }
                    catch (DivideByZeroException ex)
                    {
                        Points.TryAdd(d, double.NaN);
                    }
                }
                foreach (var item in Points.OrderBy(x => x.Key))
                {
                    GraphPoints.Add(new GraphPoint(item.Key, item.Value));
                }
            }
            return GraphPoints;
        }

        /*    public void Differentiate(string expression, string value)
            {
                if(Validate(expression))
                {
                    string[] keys = new string[] { "tan", "cos", "sin" };

                    string sKeyResult = keys.FirstOrDefault<string>(s => expression.Contains(s));
                    if (sKeyResult != null && sKeyResult.Length > 0)
                    {

                        string trigValue = expression.Substring(expression.IndexOf("("), expression.IndexOf(")") - expression.IndexOf("("));
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
                            default: break;
                        }
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
                            Parser parser = new Parser();
                            parser.Parse("x=" + value);
                            string output = parser.Parse(expression);
                        }
                    }
                }
            }*/
        //public async Task RunTasks(List<double> points, string expression)
        //{
        //    var tasksList = new List<Task>();
        //    for (int i = 0;i<points.Count;i++)
        //    {
        //        string exp = expression;
        //        var runTask = Task.Run(() => Parsing(exp, points[i]));
        //        tasksList.Add(runTask);
        //    }
        //    await Task.WhenAll(tasksList);
        //}

        //public async void EvaluateParallel(decimal xmin, decimal xmax, decimal steps, string expression)
        //{
        //    await RunTasks(xmin, xmax, steps, expression);


        //    foreach (var item in Points.OrderBy(x => x.Key))
        //    {
        //        GraphPoints.Add(new GraphPoint(item.Key, item.Value));
        //    }
        //}



        //public static string RootPolynomial(double left, double right, string expression)
        //{
        //    List<GraphPoint> graphPoints = new List<GraphPoint>();
        //    double tolerance = 0.01;
        //    double mid = (left + right)/2;
        //    double yMid = ParseFunction(expression, mid);
        //    int iterations = 0;

        //    if (ParseFunction(expression, left) * ParseFunction(expression, right) > 0)
        //    {
        //        return "";
        //    }

        //    if (yMid != 0)
        //    {
        //        while (Math.Abs(yMid) >= tolerance && iterations < 500)
        //        {
        //            if (ParseFunction(expression, mid) == 0.0)
        //            {
        //                break;
        //            }
        //            else if ((ParseFunction(expression, mid) * ParseFunction(expression, left)) > 0)
        //            {
        //                left = mid;
        //            }
        //            else
        //            {
        //                right = mid;
        //            }
        //            mid = Math.Round((left + right) / 2, 4);
        //            iterations++;
        //        }
        //    }
        //    return mid.ToString();
        //}

        //public static List<GraphPoint> Evaluate(decimal xmin, decimal xmax, decimal steps, string expression)
        //{            
        //    List<GraphPoint> graphPoints = new List<GraphPoint>();
        //    ConcurrentDictionary<double, double> points = new ConcurrentDictionary<double, double>();
        //    double y;
        //    Parser p = new Parser();
        //    List<Task> list = new List<Task>();
        //    for (decimal i=xmin; i<=xmax; i = i + steps)
        //    {
        //        string exp = expression;
        //        double x = Math.Round( (double)i, 2);
        //    }
        //    foreach(var item in points.OrderBy(x=>x.Key))
        //    {
        //        graphPoints.Add(new GraphPoint(item.Key, item.Value));
        //    }
        //    return graphPoints;
        //}

    }
}
