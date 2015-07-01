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

using Calculator;

namespace wpf_Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cExpression_TextChanged( object sender, TextChangedEventArgs e )
        {
            //cResult.Content = "= " + cExpression.Text;
            cResult.Content = EvaluateExpression( cExpression.Text );
        }

        private void cCalculate_Click( object sender, RoutedEventArgs e )
        {
            cResult.Content = EvaluateExpression( cExpression.Text );
        }

        private string EvaluateExpression( string expression )
        {
            double? result = Calculator.Calculator.EvaluateExpression( cExpression.Text );
            return result == null ? "Invalid expression" : "= " + result;
        }

    }
}
