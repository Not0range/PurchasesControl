using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasesControl
{
    static partial class Program
    {
        static List<string> purchasesTable = new List<string>();
        static MenuState PurchasesMenu()
        {
            int prevBuffSize = Console.BufferWidth;
            var state = MenuState.Cancel;

            do
            {
                if (state == MenuState.Choise)
                    WritePurchasesTable();

                purchasesTable.ForEach(t => Console.WriteLine(t));

                Console.WriteLine();
                if (state == MenuState.Error)
                    Console.WriteLine("Ошибка ввода");

                state = WriteMenu(false, (() => AddEditPurchase(), "Добавить закупку"),
                                         (EditPurchase, "Редактировать закупку"),
                                         (DeletePurchase, "Удалить закупку"));
                Console.Clear();
            } while (state != MenuState.Back);
            Console.BufferWidth = prevBuffSize;
            return MenuState.Choise;
        }

        static void WritePurchasesTable()
        {
            string[] columns = new string[] { "ID", "Клиент", "Товары", "Дата закупки", "Скидка", "Стоимость" };
            int[] widths = new int[columns.Length];

            purchasesTable.Clear();
            for (int i = 0; i < columns.Length; i++)
                widths[i] = columns[i].Length;

            foreach (var d in Purchase.purchases)
            {
                int max;
                if (d.id.ToString().Length > widths[0])
                    widths[0] = d.id.ToString().Length;
                if (d.provider.ToString().Length > widths[1])
                    widths[1] = d.provider.ToString().Length;
                max = d.products.Max(t => t.ToString().Length);
                if (max > widths[2])
                    widths[2] = max;
                max = d.purchseDate.ToString("dd.MM.yy").Length;
                if (max > widths[3])
                    widths[3] = max;
                if (d.sale.ToString().Length > widths[4])
                    widths[4] = d.sale.ToString().Length;
                if (d.Price.ToString().Length > widths[5])
                    widths[5] = d.Price.ToString().Length;
            }
            for (int i = 0; i < widths.Length; i++)
                widths[i] += 3;

            int w = widths.Sum() + Environment.NewLine.Length;
            if (w > Console.BufferWidth)
                Console.BufferWidth = widths.Sum() + Environment.NewLine.Length;

            var temp = new (string, int)[columns.Length];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = (columns[i], widths[i]);
            WriteTable(purchasesTable, temp);

            foreach (var d in Purchase.purchases)
            {
                temp[0] = (d.id.ToString(), widths[0]);
                temp[1] = (d.provider.ToString(), widths[1]);
                temp[2] = (d.products[0].ToString(), widths[2]);
                temp[3] = (d.purchseDate.ToString("dd.MM.yy"), widths[3]);
                temp[4] = (d.sale.ToString(), widths[4]);
                temp[5] = (d.Price.ToString(), widths[5]);
                WriteTable(purchasesTable, temp);

                for (int i = 1; i < d.products.Count; i++)
                {
                    temp[0] = ("", widths[0]);
                    temp[1] = ("", widths[1]);
                    temp[2] = (d.products[i].ToString(), widths[2]);
                    temp[3] = ("", widths[3]);
                    temp[4] = ("", widths[4]);
                    temp[5] = ("", widths[5]);
                    WriteTable(purchasesTable, temp);
                }
            }
        }

        static MenuState AddEditPurchase(Purchase p = null)
        {
            string dateStr, providerStr, saleStr;
            int provider = 0;
            var products = new List<(Product product, int count)>();
            decimal sale = 0;
            DateTime date = DateTime.Now;
            bool success;

            if (p != null)
                p.products.ForEach(t => products.Add(t));

            providersTable.ForEach(t => Console.WriteLine(t));
            do
            {
                providerStr = ReadLine(String.Format("Введите ID поставщика{0}: ",
                    p != null ? " (" + p.provider.id + ")" : ""), p != null);
                if (string.IsNullOrWhiteSpace(providerStr))
                    break;

                success = int.TryParse(providerStr, out provider) && Provider.providers.Any(t => t.id == provider);
                if (!success)
                    Console.WriteLine("Ошибка ввода");
            } while (!success);

            ConsoleKeyInfo key;
            do
            {
                Console.Clear();
                Console.WriteLine("Текущий список товаров:");
                products.ForEach(t => Console.WriteLine(t));
                Console.WriteLine();
                Console.WriteLine("1 - Добавить товар в список");
                Console.WriteLine("2 - Удалить товар из списка");
                if (products.Count > 0)
                    Console.WriteLine("3 - Продолжить");
                key = Console.ReadKey(true);
                if (key.KeyChar == '1')
                    AddProductToList(products);
                else if (key.KeyChar == '2')
                    DeleteProductFromList(products); 
            } while (key.KeyChar != '3');
            Console.Clear();

            do
            {
                dateStr = ReadLine(String.Format("Введите дату закупки{0}: ",
                    p != null ? " (" + p.purchseDate.ToString("dd.MM.yy") + ")" : ""), p != null);
                if (string.IsNullOrWhiteSpace(dateStr))
                    break;

                success = DateTime.TryParse(dateStr, out date);
                if (!success)
                    Console.WriteLine("Ошибка ввода");
            } while (!success);

            do
            {
                saleStr = ReadLine(String.Format("Введите скидку{0}: ",
                    p != null ? " (" + p.sale + ")" : ""), p != null);
                if (string.IsNullOrWhiteSpace(saleStr))
                    break;

                success = decimal.TryParse(saleStr, out sale);
                if (!success)
                    Console.WriteLine("Ошибка ввода");
            } while (!success);

            Console.Clear();
            Console.WriteLine("Поставщик: {0}", string.IsNullOrWhiteSpace(providerStr) ? 
                p.provider : Provider.providers.First(t => t.id == provider));
            Console.WriteLine("Товары: {0}", products[0]);
            foreach (var t in products.Skip(1))
                Console.WriteLine("{0}{1}", new string(' ', "Товары: ".Length), t);
            Console.WriteLine("Дата закупки: {0}", string.IsNullOrWhiteSpace(dateStr) ? 
                p.purchseDate.ToString("dd.MM.yy") : date.ToString("dd.MM.yy"));
            Console.WriteLine("Дата закупки: {0}", string.IsNullOrWhiteSpace(saleStr) ?
                p.sale : sale);
            Console.WriteLine("{0} данную закупку?", p == null ? "Добавить" : "Сохранить");
            Console.WriteLine("1 - Да");
            Console.WriteLine("2 - Нет");
            do
            {
                var k = Console.ReadKey(true);
                if (k.KeyChar == '1')
                    break;
                else if (k.KeyChar == '2')
                    return MenuState.Cancel;

            } while (true);

            if (p == null)
            {
                Purchase.purchases.Add(new Purchase
                {
                    id = Purchase.purchases.Count > 0 ? Purchase.purchases.Last().id + 1 : 0,
                    provider = Provider.providers.First(t => t.id == provider),
                    products = products,
                    purchseDate = date,
                    sale = sale
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(providerStr)) p.provider = Provider.providers.First(t => t.id == provider);
                if (!string.IsNullOrWhiteSpace(dateStr)) p.purchseDate = date;
                if (!string.IsNullOrWhiteSpace(saleStr)) p.sale = sale;
                p.products = products;
            }
            return MenuState.Choise;
        }

        static void AddProductToList(List<(Product product, int count)> list)
        {
            Console.Clear();
            productTable.ForEach(t => Console.WriteLine(t));
            Console.WriteLine();

            string tempStr;
            int id, count = 0;
            bool success;
            Product p = null;
            do
            {
                tempStr = ReadLine("Введите ID товара: ");
                if (string.IsNullOrWhiteSpace(tempStr))
                    break;

                success = int.TryParse(tempStr, out id) && 
                    ((p = Product.products.FirstOrDefault(t => t.id == id)) != null);
                if (!success)
                    Console.WriteLine("Ошибка ввода");
            } while (!success);

            do
            {
                tempStr = ReadLine("Введите количество товара: ");
                if (string.IsNullOrWhiteSpace(tempStr))
                    break;

                success = int.TryParse(tempStr, out count) && count > 0;
                if (!success)
                    Console.WriteLine("Ошибка ввода");
            } while (!success);

            list.Add((p, count));
        }

        static void DeleteProductFromList(List<(Product product, int count)> list)
        {
            Console.Clear();
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine("{0}. {1}", i + 1, list[i]);
            Console.WriteLine();
            Console.Write("Введите номер позиции, которую необходимо удалить: ");
            int id;
            bool success = int.TryParse(Console.ReadLine(), out id);
            if (!success || id <= 0 || id > list.Count)
                return;

            list.RemoveAt(id - 1);
        }

        static MenuState EditPurchase()
        {
            purchasesTable.ForEach(t => Console.WriteLine(t));
            Console.WriteLine();

            Console.Write("Введите ID закупки, которую необходимо отредактировать: ");
            int id;
            bool success = int.TryParse(Console.ReadLine(), out id);
            Purchase d;
            if (!success || (d = Purchase.purchases.FirstOrDefault(t => t.id == id)) == null)
                return MenuState.Error;

            Console.Clear();
            return AddEditPurchase(d);
        }

        static MenuState DeletePurchase()
        {
            purchasesTable.ForEach(t => Console.WriteLine(t));
            Console.WriteLine();

            Console.Write("Введите ID закупки, которую необходимо удалить: ");
            int id;
            bool success = int.TryParse(Console.ReadLine(), out id);
            Purchase d;
            if (!success || (d = Purchase.purchases.FirstOrDefault(t => t.id == id)) == null)
                return MenuState.Error;

            Purchase.purchases.Remove(d);
            return MenuState.Choise;
        }
    }
}
