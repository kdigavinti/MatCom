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
using Environment = System.Environment;

namespace MatCom.UI
{
    /// <summary>
    /// Interaction logic for Calculator.xaml
    /// </summary>
    public partial class Calculator : Page
    {
        Parser _parser;
        public Calculator()
        {
            InitializeComponent();
            txtInput.Focus();
            SetRichTextBoxInput("MatCom > ");          
            _parser = new Parser();
        }

        private void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;
            string tag = btn.Tag.ToString();
            if (tag == "inv")
            {
                txtInput.Text = "1/" + txtInput.Text;
            }
            else if (tag == "negate")
            {
                txtInput.Text = txtInput.Text.StartsWith("-") ? txtInput.Text.Substring(1) : "-" + txtInput.Text;
            }
            else if (tag.StartsWith("deg-"))
            {
                txtInput.Text = tag.Substring(4);
            }
            else
            {
                if (txtInput.Text.Contains("()"))
                {
                    txtInput.Text = txtInput.Text.Insert(txtInput.Text.LastIndexOf("(")+1, tag);
                }
                else
                    txtInput.Text += tag;
            }
            txtInput.Focus();
            if (tag.Contains("()"))
            {
                txtInput.SelectionStart = txtInput.Text.LastIndexOf("(") + 1;
            }
            else
                txtInput.SelectionStart = txtInput.Text.Length;


        }

        private void TxtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {  
                EvaluateExpression(txtInput.Text);
            }
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            EvaluateExpression(txtInput.Text);
        }

        private void BtnCE_Click(object sender, RoutedEventArgs e)
        {
            rtbOutputWindow.Document.Blocks.Clear();
            rtbOutputWindow.AppendText("MatCom > ");
            rtbOutputWindow.ScrollToEnd();
            txtInput.Text = "";
            txtInput.Focus();
            _parser = new Parser();
            lstVwVariables.Items.Clear();
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtInput.Text = "";
            txtInput.Focus();
        }

        private void SetRichTextBoxInput(string inputStr)
        {
            rtbOutputWindow.AppendText(inputStr);
            rtbOutputWindow.ScrollToEnd();
        }



        private void EvaluateExpression(string expression)
        {
            //string richText = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd).Text;
            try
            {
                if (string.IsNullOrEmpty(expression))
                {
                    //SetRichTextBoxInput("Enter an expression...\nMatCom > ");
                    return;
                }
                SetRichTextBoxInput(expression);
                //_parser = new Parser();
                string result = _parser.Parse(expression);
                Dictionary<string, string> valuesByRef = _parser.EnvVariables.ValuesByRef;
                Dictionary<string, object> values = _parser.EnvVariables.Values;
                foreach (string key in valuesByRef.Keys)
                {
                    var item = new { Variable = key, ActualValue = values[key], Dependency = valuesByRef[key] };
                    if(!lstVwVariables.Items.Contains(item))
                        lstVwVariables.Items.Add(item);
                    
                }
                expression = expression.Replace(" ", "");
                if (valuesByRef.ContainsKey(expression))
                {
                    var item = new { Variable = expression, ActualValue = result, Dependency = valuesByRef[expression] };
                    if (!lstVwVariables.Items.Contains(item))
                        lstVwVariables.Items.Add(item);
                }
                
                SetRichTextBoxInput("\r>> " + result);
                SetRichTextBoxInput("\nMatCom > ");
            }
            catch (Exception ex)
            {

                TextRange rangeOfText1 = new TextRange(rtbOutputWindow.Document.ContentEnd, rtbOutputWindow.Document.ContentEnd);
                rangeOfText1.Text = "\r>> " + ex.Message;
                rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                //rangeOfText1.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange rangeOfText2 = new TextRange(rtbOutputWindow.Document.ContentEnd, rtbOutputWindow.Document.ContentEnd);
                rangeOfText2.Text = "\nMatCom > ";
                rangeOfText2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                //rangeOfText2.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
        }
        private void TabTokens_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lstVwTokens.Items.Clear();
            List<Token> tokens = new List<Token>();
            tokens = _parser.Tokens;
            foreach (Token token in tokens)
            {
                lstVwTokens.Items.Add(token);
            }
        }
    }
}
