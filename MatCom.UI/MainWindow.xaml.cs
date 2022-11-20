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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CALCULATOR = "CALCULATOR";
        private const string GRAPH = "GRAPH";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemCalculator_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(CALCULATOR);
        }

        private void MenuGraph_Click(object sender, RoutedEventArgs e)
        {
            DisplayPage(GRAPH);
        }

        private void DisplayPage(string page)
        {
            if (string.IsNullOrEmpty(page)) return;
            page = page.Trim().ToUpper();

            menuItemCalculator.Background = (page == CALCULATOR) ? Brushes.LightBlue : Brushes.Transparent;
            frameCalculator.Visibility = (page == CALCULATOR) ? Visibility.Visible : Visibility.Collapsed;

            menuItemGraph.Background = (page == GRAPH) ? Brushes.LightBlue : Brushes.Transparent;
            frameGraph.Visibility = (page == GRAPH) ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}
