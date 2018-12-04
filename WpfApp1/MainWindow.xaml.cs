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

namespace WpfApp1
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            TextBox textBox = tb;
            textBox.Text += button.Content.ToString();
        }

        private void Result_click(object sender, RoutedEventArgs e)
        {
            this.result();
        }

        private void result()
        {
            try
            {
                int num = 0;
                if (this.tb.Text.Contains("+"))
                {
                    num = this.tb.Text.IndexOf("+");
                }
                else if (this.tb.Text.Contains("-"))
                {
                    num = this.tb.Text.IndexOf("-");
                }
                else if (this.tb.Text.Contains("*"))
                {
                    num = this.tb.Text.IndexOf("*");
                }
                else if (this.tb.Text.Contains("/"))
                {
                    num = this.tb.Text.IndexOf("/");
                }
                string a = this.tb.Text.Substring(num, 1);
                double num2 = Convert.ToDouble(this.tb.Text.Substring(0, num));
                double num3 = Convert.ToDouble(this.tb.Text.Substring(num + 1, this.tb.Text.Length - num - 1));
                if (a == "+")
                {
                    TextBox textBox = this.tb;
                    textBox.Text = textBox.Text + "=" + (num2 + num3);
                }
                else if (a == "-")
                {
                    TextBox textBox = this.tb;
                    textBox.Text = textBox.Text + "=" + (num2 - num3);
                }
                else if (a == "*")
                {
                    TextBox textBox = this.tb;
                    textBox.Text = textBox.Text + "=" + num2 * num3;
                }
                else
                {
                    TextBox textBox = this.tb;
                    textBox.Text = textBox.Text + "=" + num2 / num3;
                }
            }
            catch (Exception e)
            {
                tb.Text = "Error";
            }

        }

        private void Off_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            this.tb.Text = "";
        }

        private void R_Click(object sender, RoutedEventArgs e)
        {
            if (this.tb.Text.Length > 0)
            {
                this.tb.Text = this.tb.Text.Substring(0, this.tb.Text.Length - 1);
            }
        }
    }
}
