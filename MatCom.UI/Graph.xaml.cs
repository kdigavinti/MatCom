using MatCom.Interpreter.Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        double _canvasWidth = 0.0, _canvasHeight = 0.0;
        double _xAxisLinesGap = 20, _yAxisLinesGap = 20;
        double _xOriginalMin = -50, _xOriginalMax = 50;
        double _xMin = 0, _xMax = 0;
        double _zoomFactor = 1;
        double _steps = 1;
        double _stepsToCalculatePoints = 0.2;

        System.Windows.Point _origin;

        System.Windows.Point _last, _start;
        bool _isDragged = false, _fitToScreen = true;
        string _expression;
        public Graph()
        {
            InitializeComponent();
            //chartCanvas.Children.Clear();
            //DawGridLines();

            //this.StateChanged += (sender, e) => PlotGraph();
            this.SizeChanged += (sender, e) => PlotGraph();
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _zoomFactor = _zoomFactor * 0.5;
            _stepsToCalculatePoints = (_stepsToCalculatePoints >= 0.01) ? _stepsToCalculatePoints * 0.5 : _stepsToCalculatePoints;
            PlotGraph();
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomFactor = _zoomFactor * 2;
            PlotGraph();
        }

        private void BtnLeftPan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRightPan_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TxtF1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtF1.Text.Trim()))
                {
                    _expression = txtF1.Text.Trim();
                    PlotGraph();
                }

            }
        }

        private void ChartCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {   
            double zoomFactor = 0;
            if (e.Delta > 0)
            {
                zoomFactor = zoomFactor * 2;

            }
            else
            {
                zoomFactor = zoomFactor * 0.5;
                _stepsToCalculatePoints = (_stepsToCalculatePoints >= 0.01) ? _stepsToCalculatePoints * 0.5 : _stepsToCalculatePoints;

            }

            PlotGraph();
        }

        private void ChartCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ChartCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
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


        public List<GraphPoint> CalculatePoints(string function)
        {
            double x = _xOriginalMin;
            double transformX = 0;
            while (x <= _xOriginalMax)
            {
                transformX = _origin.X + (x * _xAxisLinesGap / (_zoomFactor * _steps));
                if (0 <= transformX && transformX <= _canvasWidth)
                {
                    _xMin = (x < _xMin) ? x : _xMin;
                    _xMax = (x > _xMax) ? x : _xMax;
                }
                x = x + _stepsToCalculatePoints;
            }
            List<GraphPoint> graphPoints = Evaluator.Evaluate((decimal)_xMin, (decimal)_xMax, (decimal)_stepsToCalculatePoints, _expression);
            return graphPoints;

            /*double x = _xOriginalMin;

            if (function == "F1")
            {
                double transformX = 0;
                while (x <= _xOriginalMax)
                {
                    transformX = _origin.X + (x * _xAxisLinesGap / (_zoomFactor * _steps));
                    if (0 <= transformX && transformX <= _canvasWidth)
                    {
                        _xMin = (x < _xMin) ? x : _xMin;
                        _xMax = (x > _xMax) ? x : _xMax;
                    }
                    x = x + _stepsToCalculatePoints;
                }
                x = _xMin;
                while (x <= _xMax)
                {
                    //double y = (x - 2) * ((x + 1)*(x+1)) * (x - 4);
                    double y = x * x;
                    graphPoints.Add(new GraphPoint(x, y));
                    x = x + _stepsToCalculatePoints;
                }
            }

            else if (function == "F2")
            {
                //while (x <= xMax)
                //{
                //    double y = x;
                //    //Value val = new Value(x, y);
                //    values.Add(new Value(x, y));
                //    x = x + stepsToCalculatePoints;
                //    yMin = (y < yMin) ? y : yMin;
                //    yMax = (y > yMax) ? y : yMax;
                //}

                while (x <= _xOriginalMax)
                {
                    double xRad = x * Math.PI / 180;
                    double y = Math.Sin(xRad);
                    //Value val = new Value(x, y);
                    graphPoints.Add(new GraphPoint(xRad, y));
                    x = x + _steps;

                }
            }
            return graphPoints;*/
        }

        private void DawGridLines()
        {
            List<AxisLabel> axisLabels = new List<AxisLabel>();
            Line xAxisLine, yAxisLine;
            _canvasWidth = chartCanvas.ActualWidth;
            _canvasHeight = chartCanvas.ActualHeight;

            //draw origin lines
            if (_fitToScreen)
            {
                _origin.X = (_canvasWidth / 2);
                _origin.Y = (_canvasHeight / 2);
            }

            double y = _origin.Y;
            double x = _origin.X;


            //draw x-axis lines - below the origin
            int idx = 0;
            while (y <= _canvasHeight)
            {
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
                        axisLabels.Add(new AxisLabel(_origin.X, y, "-" + idx.ToString(), AxisType.YAxis));

                    }
                    idx++;
                }
                y += _xAxisLinesGap;

            }

            //draw x-axis lines - above the origin

            y = _origin.Y;
            x = _origin.X;
            idx = 0;
            while (y >= 10)
            {
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
                        axisLabels.Add(new AxisLabel(_origin.X, y, idx.ToString(), AxisType.YAxis));

                    }
                    idx++;
                }

                y = y - _xAxisLinesGap;

            }


            //draw y-axis lines - right to the origin

            y = _origin.Y;
            x = _origin.X;
            idx = 0;
            while (x <= _canvasWidth)
            {
                if (0 <= x && x <= _canvasWidth)
                {
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
                        axisLabels.Add(new AxisLabel(x, _origin.Y, idx.ToString(), AxisType.XAxis));

                    }
                    idx++;
                }

                x = x + _yAxisLinesGap;

            }

            //draw y-axis lines - left to the origin

            y = _origin.Y;
            x = _origin.X;
            idx = 0;
            while (x >= 10)
            {
                if (0 <= x && x <= _canvasWidth)
                {
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
                        axisLabels.Add(new AxisLabel(x, _origin.Y, "-" + idx.ToString(), AxisType.XAxis));

                    }
                    idx++;
                }

                x = x - _yAxisLinesGap;

            }

            axisLabels.Add(new AxisLabel(_origin.X, _origin.Y, "0", AxisType.Origin));

            AddAxisLabels(axisLabels);
        }

        private void AddAxisLabels(List<AxisLabel> axisLabels)
        {
            for (int i = 0; i < axisLabels.Count; i++)
            {
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
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth - 10); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 - 5); //include margin
            }
            else if (axis == AxisType.XAxis)
            {
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth / 2 - 5); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 + 10); //include margin
            }
            else if (axis == AxisType.Origin)
            {
                Canvas.SetLeft(textBlock, x - textBlock.ActualWidth - 10); //include margin
                Canvas.SetTop(textBlock, y - textBlock.ActualHeight / 2 + 10); //include margin
            }
            chartCanvas.Children.Add(textBlock);
        }

        private void PlotGraph()
        {
            try
            {
                if (this.ActualWidth > 0 && this.ActualHeight > 0)
                {
                    chartCanvas.Children.Clear();
                    DawGridLines();
                    if (!string.IsNullOrEmpty(_expression))
                    {
                        List<GraphPoint> points = CalculatePoints(_expression);
                        PlotCurve(points, Brushes.Blue);
                    }
                    
                    //PlotF1();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void PlotF1()
        {
            List<GraphPoint> values = CalculatePoints("F1");
            PlotCurve(values, Brushes.Blue);
        }

        public void PlotF2()
        {
            List<GraphPoint> values = CalculatePoints("F2");
            PlotCurve(values, Brushes.Red);
        }

        private void PlotCurve(List<GraphPoint> values, System.Windows.Media.SolidColorBrush color)
        {
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            foreach (var value in values)
            {
                double xPoint = _origin.X + (value.X * _xAxisLinesGap / (_zoomFactor * _steps));
                double yPoint = _origin.Y - (value.Y * _yAxisLinesGap / (_zoomFactor * _steps));
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