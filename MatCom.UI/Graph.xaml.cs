using MatCom.Interpreter.Scanner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MatCom.UI
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : Page
    {

        double _canvasWidth = 0.0, _canvasHeight = 0.0, _canvasTop=0.0;
        double _xAxisLinesGap = 20, _yAxisLinesGap = 20;
        double _xOriginalMin = -50, _xOriginalMax = 50;
        double _xMin = 0, _xMax = 0;
        double _zoomFactor = 1;
        double _steps = 1;
        double _stepsToCalculatePoints = 0.1;
        
        System.Windows.Point _origin;

        System.Windows.Point _last, _start;
        bool _isDragged = false, _fitToScreen = true;
        string _expression;
        List<ExpressionValues> _expressionValues;
        PathGeometry _pathGeometry;
        Pen _pen = new Pen(new SolidColorBrush(),1);
        List<Point> _zeroCrossingPoints = new List<Point>();

        public Graph()
        {
            InitializeComponent();
            //chartCanvas.Children.Clear();
            //DawGridLines();

            //this.StateChanged += (sender, e) => PlotGraph();            
            this.SizeChanged += (sender, e) => PlotGraph();
            _expressionValues = new List<ExpressionValues>();
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            OnZoomInClick();
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            OnZoomOutClick();
        }

        private void OnZoomInClick()
        {
            double newStep = ((_steps * 5).ToString().Contains("5")) ? _steps * 0.4 : _steps * 0.5;
            if (newStep >= 0.001)
            {
                //if(_xAxisLinesGap <= 80)
                //{
                //    _xAxisLinesGap = _xAxisLinesGap + 20;
                //    _yAxisLinesGap = _yAxisLinesGap + 20;
                //}

                _zoomFactor = _zoomFactor * 0.5;
                _steps = newStep;
                if (_steps == 1) _stepsToCalculatePoints = 0.2;
                _stepsToCalculatePoints = (_stepsToCalculatePoints >= 0.01) ? _stepsToCalculatePoints * 0.5 : _stepsToCalculatePoints;
                PlotGraph();
            }
        }

        private void OnZoomOutClick()
        {
            double newStep = ((_steps * 5).ToString().Contains("2")) ? _steps * 2.5 : _steps * 2;
            if (newStep <= 10000)
            {
                _zoomFactor = _zoomFactor * 2;
                double labelStep = _steps * 5;
                labelStep = Math.Truncate(labelStep * 1000) / 1000; ;
                _steps = (labelStep >= 0 && labelStep.ToString().Contains("2")) ? _steps * 2.5 : _steps * 2;
                if (_steps == 1) _stepsToCalculatePoints = 0.2;
                _stepsToCalculatePoints = (_steps >= 1) ? _stepsToCalculatePoints * 10 : _stepsToCalculatePoints;
                PlotGraph();
            }
        }

        private void ChartCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                OnZoomInClick();
            }
            else
            {
                OnZoomOutClick();
            }
        }

        //private void CalStepsToCalculatePoints()
        //{
        //    if(_steps == 0.001)
        //    {
        //        _stepsToCalculatePoints = 0.0002;
        //    }
        //    else if (_steps <= 0.01)
        //    {
        //        _stepsToCalculatePoints = 0.002;
        //    }
        //    else if (_steps <= 0.1)
        //    {
        //        _stepsToCalculatePoints = 0.02;
        //    }
        //    else if (_steps <= 0.4)
        //    {
        //        _stepsToCalculatePoints = 0.1;
        //    }
        //    else if (_steps <= 1)
        //    {
        //        _stepsToCalculatePoints = 0.2;
        //    }
        //    else
        //        _stepsToCalculatePoints = _stepsToCalculatePoints * 10;
        //}

        private void BtnFit_Click(object sender, RoutedEventArgs e)
        {            
            ResetValues();
            PlotGraph();
        }
        private void ResetValues()
        {
            _canvasWidth = 0.0; _canvasHeight = 0.0;
            _xAxisLinesGap = 20; _yAxisLinesGap = 20;
            _xOriginalMin = -50; _xOriginalMax = 50;
            _xMin = 0; _xMax = 0;
            _zoomFactor = 1;
            _steps = 1;
            _stepsToCalculatePoints = 0.1;
            _fitToScreen = true;
            _expressionValues = new List<ExpressionValues>();
            txtBlockErrorMessage.Text = "";
            txtBlockErrorMessage.Visibility = Visibility.Collapsed;
        }      
        private void TxtF1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtF1.Text.Trim()))
                {
                    _expression = txtF1.Text.Trim();
                    ResetValues();
                    _zeroCrossingPoints.Clear();
                    PlotGraph();
                }

            }
        }

        

        /*private void ChartCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            _start = e.GetPosition(chartCanvas);
            _isDragged = true;
        }

        private void ChartCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            _isDragged = false;
        }
        private void ChartCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragged == false)
                return;

            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {

                var pos = e.GetPosition(chartCanvas);
                _last = pos;
                _fitToScreen = false;

                System.Windows.Point newOrigin = new System.Windows.Point(_origin.X, _origin.Y);

                newOrigin.X = _origin.X + (_last.X - _start.X) * 0.05;
                newOrigin.Y = _origin.Y + (_last.Y - _start.Y) * 0.05;

                _origin = newOrigin;
                PlotGraph();
            }
        }*/
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            CaptureMouse();
            _start = e.GetPosition(chartCanvas);
            _isDragged = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            _isDragged = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragged == false)
                return;

            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var pos = e.GetPosition(chartCanvas);
                _last = pos;
                _fitToScreen = false;

                System.Windows.Point newOrigin = new System.Windows.Point(_origin.X, _origin.Y);

                newOrigin.X = _origin.X + (_last.X - _start.X) * 0.05;
                newOrigin.Y = _origin.Y + (_last.Y - _start.Y) * 0.05;

                _origin = newOrigin;
                PlotGraph();
            }

        }


        public async Task<List<GraphPoint>> CalculatePoints(string function)
        {
            try
            {
                List<double> xPoints = new List<double>();
                decimal x = (decimal)_xOriginalMin;
                decimal transformX = 0;
                while (x <= (decimal)_xOriginalMax)
                {
                    //transformX = _origin.X + (x * _xAxisLinesGap / (_zoomFactor * _steps));
                    transformX = (decimal)_origin.X + (x * (decimal)_xAxisLinesGap / ((decimal)_steps));
                    if (0 <= transformX && transformX <= (decimal)_canvasWidth)
                    {
                        _xMin = (x < (decimal)_xMin) ? (double)x : _xMin;
                        _xMax = (x > (decimal)_xMax) ? (double)x : _xMax;
                        xPoints.Add((double)x);
                    }
                    x = x + (decimal)_stepsToCalculatePoints;
                }

                ExpressionValues expValues = _expressionValues.Where(i => i.Expression == function).FirstOrDefault();
                List<GraphPoint> graphPoints;
                Evaluator eval = new Evaluator();
                if (expValues == null)
                {
                    //graphPoints = Evaluator.Evaluate((decimal)_xMin, (decimal)_xMax, (decimal)_stepsToCalculatePoints, _expression);
                    await eval.Evaluate(xPoints, _expression);
                    graphPoints = eval.GraphPoints;
                    expValues = new ExpressionValues()
                    {
                        Expression = _expression,
                        GraphPoints = graphPoints
                    };
                    //_expressionValues.Clear();
                    _expressionValues.Add(expValues);
                    expValues.GraphPoints.OrderBy(i => i.X);
                }
                else
                {
                    List<double> xPointsToEvaluate = xPoints.Where(x => !expValues.GraphPoints.Any(pt => pt.X == x)).ToList();
                    await eval.Evaluate(xPointsToEvaluate, _expression);
                    graphPoints = eval.GraphPoints;
                    expValues.GraphPoints.AddRange(graphPoints);
                    expValues.GraphPoints = expValues.GraphPoints.DistinctBy(i => i.X).OrderBy(j => j.X).ToList();
                    graphPoints = expValues.GraphPoints;
                }
                return graphPoints;
            }
            catch(Exception ex)
            {
                txtBlockErrorMessage.Text = ex.Message;
                txtBlockErrorMessage.Visibility = Visibility.Visible;
                return null;
            }
            
        }

        private void DawGridLines()
        {            
            UIElement btnZoomInEle = (UIElement)chartCanvas.FindName("btnZoomIn");
            UIElement btnZoomOutEle = (UIElement)chartCanvas.FindName("btnZoomOut");
            UIElement btnFitEle = (UIElement)chartCanvas.FindName("btnFit");
            chartCanvas.Children.Clear();
            //add the buttons again
            chartCanvas.Children.Add(btnZoomInEle);
            chartCanvas.Children.Add(btnZoomOutEle);
            chartCanvas.Children.Add(btnFitEle);
            Point relativePoint = chartCanvas.TransformToAncestor(Application.Current.MainWindow)
                          .Transform(new Point(0, 0));
            _canvasTop = relativePoint.Y;
            List<AxisLabel> axisLabels = new List<AxisLabel>();            
            _canvasWidth = chartCanvas.ActualWidth + 100; //allow margin
            _canvasHeight = chartCanvas.ActualHeight + 100; //allow margin

            //draw origin lines
            if (_fitToScreen)
            {
                _origin.X = (chartCanvas.ActualWidth / 2);
                _origin.Y = (chartCanvas.ActualHeight / 2);
            }
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                DrawPositiveXGridLines();
            });
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                DrawNegativeXGridLines();
            });
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                DrawPositiveYGridLines();
            });
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                DrawNegativeYGridLines();
            });

            //DrawPositiveXGridLines();
            //DrawNegativeXGridLines();
            //DrawPositiveYGridLines();
            //DrawNegativeYGridLines();

            axisLabels.Add(new AxisLabel(_origin.X, _origin.Y, "0", AxisType.Origin));

            AddAxisLabels(axisLabels);
            //stopwatch.Stop();
            //MessageBox.Show(stopwatch.ElapsedTicks.ToString());
        }

        ////draw x-axis lines - above the origin
        private void DrawPositiveXGridLines()
        {
            double y = _origin.Y;
            double x = _origin.X;
            double idx = -1;
            List<AxisLabel> axisLabels = new List<AxisLabel>();
            Line xAxisLine;
            while (y >= 10)
            {
                idx++;
                if (0 <= y && y <= _canvasHeight)
                {
                    xAxisLine = new Line()
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = _canvasWidth,
                        Y2 = y,
                        Stroke = (y == _origin.Y) ? Brushes.Black : Brushes.LightGray,
                        //Stroke = Brushes.LightGray,
                        StrokeThickness = (idx != 0 && idx % 5 == 0) ? 1.5 : 0.75,
                    };
                    chartCanvas.Children.Add(xAxisLine);                    
                    if (idx != 0 && idx % 5 == 0)
                    {
                        axisLabels.Add(new AxisLabel(_origin.X, y, FormatLabel(_steps * idx), AxisType.YAxis));
                        axisLabels.Add(new AxisLabel(xAxisLine.X1, y, FormatLabel(_steps * idx), AxisType.YAxis));
                    }

                }

                y = y - _xAxisLinesGap;

            }
            AddAxisLabels(axisLabels);
        }

        ////draw x-axis lines - below the origin
        private void DrawNegativeXGridLines()
        {
            double y = _origin.Y;
            double x = _origin.X;
            double idx = -1;
            List<AxisLabel> axisLabels = new List<AxisLabel>();
            Line xAxisLine;
            
            while (y <= _canvasHeight)
            {
                idx++;
                if (0 <= y && y <= _canvasHeight)
                {
                    xAxisLine = new Line()
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = _canvasWidth,
                        Y2 = y,
                        Stroke = (y == _origin.Y) ? Brushes.Black : Brushes.LightGray,
                        //Stroke = Brushes.LightGray,
                        StrokeThickness = (idx != 0 && idx % 5 == 0) ? 1.5 : 0.75,
                    };

                    chartCanvas.Children.Add(xAxisLine);
                    
                    if (idx != 0 && idx % 5 == 0)
                    {
                        axisLabels.Add(new AxisLabel(_origin.X, y, "-" + FormatLabel(_steps * idx), AxisType.YAxis));
                        axisLabels.Add(new AxisLabel(xAxisLine.X1, y, "-" + FormatLabel(_steps * idx), AxisType.YAxis));
                        //axisLabels.Add(new AxisLabel(xAxisLine.X2, y, "-" + FormatLabel(_steps * idx), AxisType.YAxis));
                    }
                }
                y += _xAxisLinesGap;
            }
            AddAxisLabels(axisLabels);
        }

        ////draw y-axis lines - right to the origin
        private void DrawPositiveYGridLines()
        {
            double y = _origin.Y;
            double x = _origin.X;
            double idx = -1;
            List<AxisLabel> axisLabels = new List<AxisLabel>();
            Line yAxisLine;
          
            while (x <= _canvasWidth)
            {
                idx++;
                if (0 <= x && x <= _canvasWidth)
                {
                    _xOriginalMax = (idx > _xOriginalMax) ? idx : _xOriginalMax;
                    yAxisLine = new Line()
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = _canvasHeight,
                        Stroke = (x == _origin.X) ? Brushes.Black : Brushes.LightGray,
                        //Stroke = Brushes.LightGray,
                        StrokeThickness = (idx != 0 && idx % 5 == 0) ? 1.5 : 0.75,
                    };

                    chartCanvas.Children.Add(yAxisLine);
                    
                    if (idx != 0 && idx % 5 == 0)
                    {
                        axisLabels.Add(new AxisLabel(x, _origin.Y, FormatLabel(_steps * idx), AxisType.XAxis));
                        axisLabels.Add(new AxisLabel(x, yAxisLine.Y1, FormatLabel(_steps * idx), AxisType.XAxis));
                    }                    
                }

                x = x + _yAxisLinesGap;

            }
            _xOriginalMax = _xOriginalMax + _steps * 10;
            AddAxisLabels(axisLabels);
        }

        ////draw y-axis lines - left to the origin
        private void DrawNegativeYGridLines()
        {
            double y = _origin.Y;
            double x = _origin.X;
            double idx = -1; double negIdx = -1;
            List<AxisLabel> axisLabels = new List<AxisLabel>();
            Line yAxisLine;

            while (x >= 10)
            {
                idx++;
                negIdx = idx * -1;
                if (0 <= x && x <= _canvasWidth)
                {
                    _xOriginalMin = (negIdx < _xOriginalMin) ? negIdx : _xOriginalMax;
                    yAxisLine = new Line()
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = _canvasHeight,
                        Stroke = (x == _origin.X) ? Brushes.Black : Brushes.LightGray,
                        //Stroke = Brushes.LightGray,
                        StrokeThickness = (idx != 0 && idx % 5 == 0) ? 1.5 : 0.75,
                    };
                    chartCanvas.Children.Add(yAxisLine);                   
                    if (idx != 0 && idx % 5 == 0)
                    {
                        axisLabels.Add(new AxisLabel(x, _origin.Y, FormatLabel(_steps * negIdx), AxisType.XAxis));
                        axisLabels.Add(new AxisLabel(x, yAxisLine.Y1, FormatLabel(_steps * negIdx), AxisType.XAxis));
                    }
                }
                x = x - _yAxisLinesGap;
            }
            AddAxisLabels(axisLabels);
            _xOriginalMin = _xOriginalMin - _steps * 20;
        }
        private void ClearZeroCrossingPoints()
        {            
            for (int i = 0; i < chartCanvas.Children.Count; i++)
            {
                UIElement element = chartCanvas.Children[i];
                if (element is TextBlock)
                {
                    TextBlock txtBlock = (TextBlock)element;
                    if (txtBlock.Name.ToLower().StartsWith("zerocrossingpointlabel"))
                    {
                        chartCanvas.Children.Remove(txtBlock);
                        i--;
                    }
                }
                if (element is Ellipse)
                {
                    Ellipse ellipseEle = (Ellipse)element;
                    if (ellipseEle.Name.ToLower().StartsWith("zerocrossingpoint"))
                    {
                        chartCanvas.Children.Remove(ellipseEle);
                        i--;
                    }
                }
            }
        }
        private List<ZeroCrossingRange> FindZeroCrossingPointsRange()
        {
            List<ZeroCrossingRange> lstZeroCrossingRanges = new List<ZeroCrossingRange>();
            foreach(ExpressionValues expvalues in _expressionValues)
            {
                if(expvalues.Expression == txtF1.Text.Trim())
                {
                    string previousPoint = "", currentPoint = "";
                    double previousPointY = 0.0, currentPointY = 0.0;
                    double previousPointX = 0.0, currentPointX = 0.0;
                    foreach (GraphPoint p in expvalues.GraphPoints)
                    {
                        currentPointY = p.Y;
                        currentPointX = p.X;
                        currentPoint = (currentPointY >= 0) ? "P" : "N";
                        if ((previousPoint == "N" && currentPoint=="P") ||(previousPoint == "P" && currentPoint == "N"))
                        {
                            lstZeroCrossingRanges.Add(new ZeroCrossingRange() { X1 = previousPointX, X2 = currentPointX });
                        }                        
                        previousPoint = currentPoint;
                        previousPointY = currentPointY;
                        previousPointX = currentPointX;
                    }
                }
            }
            return lstZeroCrossingRanges;
        }
        private void AddZeroCrossingPoints(List<Point> points)
        {
            double pointDia = 10;
            for(int i=0; i < points.Count; i++)
            {
                double xPoint = points[i].X, yPoint = points[i].Y;
                double xCanvasPoint = _origin.X + (xPoint * _xAxisLinesGap / (_steps));
                double yCanvasPoint = _origin.Y - (yPoint * _yAxisLinesGap / (_steps));
                Ellipse ellipse = new Ellipse();
                ellipse.Height = pointDia;
                ellipse.Width = pointDia;
                ellipse.StrokeThickness = 3;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                ellipse.Name = "zeroCrossingPoint" + i;
                Canvas.SetZIndex(ellipse, 99);
                Canvas.SetTop(ellipse, yCanvasPoint - pointDia / 2);
                Canvas.SetLeft(ellipse, xCanvasPoint - pointDia / 2);
                chartCanvas.Children.Add(ellipse);


                TextBlock textBlock = new TextBlock();
                textBlock.Name = "zeroCrossingPointLabel" + i;
                string precision = (_steps <= 0.5) ? "N6" : "N3";
                //textBlock.Text = "(x: " + xPoint.ToString(precision) + " , y: " + yPoint.ToString(precision) + ")";
                textBlock.Text = "(x: " + xPoint.ToString(precision) + ")";
                textBlock.FontSize = 14.0;
                textBlock.Background = new SolidColorBrush(Colors.White);
                textBlock.TextAlignment = TextAlignment.Right;
                textBlock.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                textBlock.Arrange(new Rect(textBlock.DesiredSize));
                textBlock.Padding = new Thickness(5);
                Canvas.SetZIndex(textBlock, 90);
                Canvas.SetLeft(textBlock, xCanvasPoint);
                Canvas.SetTop(textBlock, yCanvasPoint);

                chartCanvas.Children.Add(textBlock);
            }
            
        }
        private void BtnZeroCrossing_Click(object sender, RoutedEventArgs e)
        {
            ClearZeroCrossingPoints();
            _zeroCrossingPoints.Clear();
            List<ZeroCrossingRange> lstZeroCrossingRanges = FindZeroCrossingPointsRange();            
            foreach (ZeroCrossingRange zeroCrossingRange in lstZeroCrossingRanges)
            {
                string zeroCrossingPoint = Evaluator.RootPolynomial(zeroCrossingRange.X1, zeroCrossingRange.X2, txtF1.Text);
                if(!string.IsNullOrEmpty(zeroCrossingPoint))
                    _zeroCrossingPoints.Add(new Point(Convert.ToDouble(zeroCrossingPoint), 0));
            }
            AddZeroCrossingPoints(_zeroCrossingPoints);
        }

        private void BtnClearZeroCrossing_Click(object sender, RoutedEventArgs e)
        {
            ClearZeroCrossingPoints();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _zeroCrossingPoints.Clear();
            _expressionValues.Clear();
            chartCanvas.Children.Clear();
            txtF1.Text = "";
            _expression = "";
            ResetValues();
            DawGridLines();
        }

        private void ChartCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            /*
             * FindName function not working
            UIElement uiEle = (UIElement) chartCanvas.FindName("mouseposition");
            var ele = chartCanvas.FindName("mouseposition");
            */
            Point p = e.GetPosition(chartCanvas);
            TextBlock txtBlock = null;
            for (int i = 0; i < chartCanvas.Children.Count; i++)
            {
                UIElement element = chartCanvas.Children[i];
                if (element is TextBlock)
                {
                    txtBlock = (TextBlock)element;
                    if (txtBlock.Name == "mouseposition")
                    {
                        break;
                    }
                    else
                    {
                        txtBlock = null;
                    }
                }
            }
            if (txtBlock != null)
                chartCanvas.Children.Remove(txtBlock);
            
            //if (_pathGeometry != null && _pathGeometry.FillContains(p))
            if(_pathGeometry != null && _pathGeometry.StrokeContains(_pen, p))
            {
                double xPoint = (p.X - _origin.X) * _steps / _xAxisLinesGap;
                //double yPoint = (_origin.Y - p.Y) * _steps / _yAxisLinesGap;
                Evaluator evaluator = new Evaluator();
                double yPoint = evaluator.ParseExpressionForSingleValue(_expression, xPoint);
                //xPoint = p.X;
                //yPoint = p.Y;
                TextBlock textBlock = new TextBlock();
                textBlock.Name = "mouseposition";
                string precision = (_steps <= 0.5) ? "N6" : "N3";
                textBlock.Text = "(x: " + xPoint.ToString(precision) + " , y: " + yPoint.ToString(precision) + ")";
                textBlock.FontSize = 14.0;
                textBlock.Background = new SolidColorBrush(Colors.White);
                textBlock.TextAlignment = TextAlignment.Right;
                textBlock.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
                textBlock.Arrange(new Rect(textBlock.DesiredSize));
                textBlock.Padding = new Thickness(10);
                Canvas.SetLeft(textBlock, p.X); //include margin
                Canvas.SetTop(textBlock, p.Y); //include margin

                chartCanvas.Children.Add(textBlock);
            }
            
        }

        private string FormatLabel(double val)
        {
            return val.ToString("N3").TrimEnd('0').TrimEnd('.');
        }
               

        private void AddAxisLabels(List<AxisLabel> axisLabels)
        {
            for (int i = 0; i < axisLabels.Count; i++)
            {
                //double x = Math.Truncate(axisLabels[i].X * 1000) / 1000;
                //double y = Math.Truncate(axisLabels[i].Y * 1000) / 1000;
                AddtextBlock(axisLabels[i].X, axisLabels[i].Y, axisLabels[i].Label, axisLabels[i].Axis);
            }
        }

        private void AddtextBlock(double x, double y, string label, AxisType axis)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = label;
            textBlock.FontSize = 14.0;
            textBlock.Background = new SolidColorBrush(Colors.White);
            textBlock.TextAlignment = TextAlignment.Right;
            textBlock.Measure(new System.Windows.Size(Double.PositiveInfinity, Double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));
            textBlock.Margin = new Thickness(5);
            if (axis == AxisType.YAxis)
            {
                double marginX = - 10;
                if(!( 0 <= _origin.X && _origin.X <= _canvasWidth))
                {
                    marginX = textBlock.ActualWidth + 10;
                }
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth + marginX); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 - 5); //include margin

            }
            else if (axis == AxisType.XAxis)
            {
                double marginY = 10;
                if (0 <= _origin.Y && _origin.Y <= _canvasHeight)
                {
                    if(y==0)
                        marginY = textBlock.ActualHeight / 2 - 50;                    
                    else
                        marginY = 10;

                }
                else
                {
                    if (y == 0)
                        marginY = 10;
                    else
                        marginY = textBlock.ActualHeight / 2 - 50;

                }
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth / 2 - 5); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 + marginY); //include margin
            }
            else if (axis == AxisType.Origin)
            {
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth - 10); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 + 10); //include margin
            }
            chartCanvas.Children.Add(textBlock);            
        }

        private async void PlotGraph()
        {
            try
            {
                if (this.ActualWidth > 0 && this.ActualHeight > 0)
                {                    
                    DawGridLines();
                    
                    if (!string.IsNullOrEmpty(_expression))
                    {
                        List<GraphPoint> graphPoints =await CalculatePoints(_expression);
                        if (graphPoints != null)
                        {
                            PlotCurve(graphPoints, Brushes.Blue);
                            ClearZeroCrossingPoints();
                            AddZeroCrossingPoints(_zeroCrossingPoints);
                        }                        
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void PlotCurve(List<GraphPoint> values, System.Windows.Media.SolidColorBrush color)
        {
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            foreach (var value in values)
            {
                //double xPoint = _origin.X + (value.X * _xAxisLinesGap / (_zoomFactor * _steps));
                //double yPoint = _origin.Y - (value.Y * _yAxisLinesGap / (_zoomFactor * _steps));
                double xPoint = _origin.X + (value.X * _xAxisLinesGap / (_steps));
                double yPoint = _origin.Y - (value.Y * _yAxisLinesGap / (_steps));
                if(xPoint>=0 && xPoint<=_canvasWidth && yPoint>=-2*_canvasHeight && yPoint<=2*_canvasHeight)
                    points.Add(new System.Windows.Point(xPoint, yPoint));

            }
            DrawBezierCurve(points, color);
        }

        private void DrawBezierCurve(List<System.Windows.Point> points, System.Windows.Media.SolidColorBrush color)
        {
            if (points == null || (points != null && points.Count == 0)) return;

            // Create a PathFigure to be used for the PathGeometry of myPath.
            PathFigure pathFigure = new PathFigure();

            // Set the starting point for the PathFigure             
            System.Windows.Point startPoint = new System.Windows.Point(points[0].X, points[0].Y);
            //System.Windows.Point endPoint = new System.Windows.Point(points[points.Count - 1].X, points[points.Count - 1].Y);
            pathFigure.StartPoint = startPoint;

            // Create a PointCollection that holds the Points used to specify 
            // the points of the BezierSegment below.
            PointCollection pointCollection = new PointCollection(points.Count);
            foreach (var pt in points)
            {
                pointCollection.Add(pt);
            }

            PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();

            for (int i = 0; i < points.Count - 2; i = i + 2)
            {
                //System.Windows.Point p1 = points[i];
                //System.Windows.Point p2 = points[i + 1];
                //System.Windows.Point p3 = points[i + 2];
                BezierSegment seg = new BezierSegment()
                {
                    Point1 = points[i], //p1,
                    Point2 = points[i + 1], //p2,
                    Point3 = points[i + 2], //p3,
                    IsSmoothJoin = true,
                    IsStroked = true
                };

                pathSegmentCollection.Add(seg);


            }
            pathFigure.Segments = pathSegmentCollection;

            PathFigureCollection pathFigureColl = new PathFigureCollection();
            pathFigureColl.Add(pathFigure);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures = pathFigureColl;

            // Create a path to draw a geometry with.
            Path path = new Path();
            path.Stroke = color;
            path.StrokeThickness = 3;

            // specify the shape (quadratic Bezier curve) of the path using the StreamGeometry.
            path.Data = pathGeometry;

            chartCanvas.Children.Add(path);
            _pathGeometry = pathGeometry;
            
            Canvas.SetZIndex(path, 1);
        }
    }
}
public class AxisLabel : GraphPoint
{
    public string Label { get; set; }
    public AxisType Axis { get; set; }
    public AxisLabel(double x, double y, string label, AxisType axis) : base(x, y)
    {
        Label = label;
        Axis = axis;
    }

}

public enum AxisType
{
    Origin = 0,
    XAxis = 1,
    YAxis = 2
}

public class ExpressionValues
{
    public string Expression { get; set; }
    public List<GraphPoint> GraphPoints { get; set; }
}
public class ZeroCrossingRange
{
    public double X1 { get; set; }
    public double X2 { get; set; }
}