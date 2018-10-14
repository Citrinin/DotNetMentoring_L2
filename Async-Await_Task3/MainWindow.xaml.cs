using System;
using System.Collections.ObjectModel;
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

namespace Async_Await_Task3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Cart = new Cart();
            DataContext = this;
        }
        public ObservableCollection<Product> ProductList { get; set; } = new ObservableCollection<Product>
        {
            new Product{Name = "Orange", Price = 364.2},
            new Product{Name = "Banana", Price = 150.5},
            new Product{Name = "Strawberry", Price = 178.4},
            new Product{Name = "Blueberry", Price = 245.7},
            new Product{Name = "Blackberry", Price = 714.5},
            new Product{Name = "Cranberry", Price = 354.9},
            new Product{Name = "Raspberry", Price = 425.1},
            new Product{Name = "Pineapple", Price = 970.1},
            new Product{Name = "Peach", Price = 378.2}
        };

        public Cart Cart { get; set; }

        private void GoToCartButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AddToCart_OnClick(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button)?.DataContext as Product;
            await Cart.AddItemToCart(product);
        }
    }
}
