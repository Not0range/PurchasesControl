using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasesControl
{
    public class Product
    {
        public static List<Product> products = new List<Product>();
        public int id;
        public string title;
        public decimal price;
        public string category;

        public override string ToString()
        {
            return $"{title} ({category})";
        }
    }

    public class Provider
    {
        public static List<Provider> providers = new List<Provider>();
        public int id;
        public string name;
        public string address;
        public string phone;
        public string email;

        public override string ToString()
        {
            return name;
        }
    }

    public class Purchase
    {
        public static List<Purchase> purchases = new List<Purchase>();
        public int id;
        public List<(Product product, int count)> products;
        public Provider provider;
        public DateTime purchseDate;
        public decimal sale;

        public decimal Price
        {
            get
            {
                return products.Sum(p => p.product.price * p.count) * (100 - sale) / 100;
            }
        }
    }
}
