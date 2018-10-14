using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Async_Await_Task3
{
    public class Cart: INotifyPropertyChanged
    {
        private double _totalPrice;
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public double TotalPrice => _totalPrice;

        public Task AddItemToCart(Product product)
        {
           return Task.Run(() =>
            {
                var cartItem = this.CartItems.FirstOrDefault(item => item.Product == product);
                if (cartItem != null)
                {
                    cartItem.Amount++;
                }
                else
                {
                    this.CartItems.Add(new CartItem {Product = product, Amount = 1});
                }
                Thread.Sleep(3000);
                _totalPrice = CartItems.Sum(item => item.Amount * item.Product.Price);
                OnPropertyChanged(nameof(TotalPrice));
            });
        }
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}