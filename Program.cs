using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PurchasesControl
{
    static partial class Program
    {
        const int SEPARATOR_COUNT = 20;
        static void Main(string[] args)
        {
            ReadData();
            WriteProductsTable();
            WriteProvidersTable();
            WritePurchasesTable();

            Console.Title = "Система управления закупками";

            var state = MenuState.Choise;

            do
            {
                if (state == MenuState.Choise)
                {
                    Console.Write(new string('*', Console.BufferWidth));
                    Console.WriteLine("Добро пожаловать в систему управления закупками");
                    Console.Write(new string('*', Console.BufferWidth));
                }

                if (state == MenuState.Error)
                    Console.WriteLine("Ошибка ввода");
                state = WriteMenu(true, (ProductMenu, "Работа со списком товаров"),
                                (ProvidersMenu, "Работа со списком поставщиков"),
                                (PurchasesMenu, "Работа со списком закупок"));
            } while (state != MenuState.Back);
            SaveData();
        }

        static void ReadData()
        {
            bool error = false;
            if (File.Exists("data.txt") && new FileInfo("data.txt").Length > 0)
            {
                string[] data = Regex.Split(File.ReadAllText("data.txt"), "^~{" + SEPARATOR_COUNT + "}\r\n", RegexOptions.Multiline);
                string[] products = data[0].Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] providers = data[1].Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] purchases = data[2].Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                int id;
                string[] splitData;
                foreach (var p in products)
                {
                    try
                    {
                        splitData = p.Split(new string[] { "\t" },
                            StringSplitOptions.RemoveEmptyEntries);
                        id = int.Parse(splitData[0]);
                        if (Product.products.Any(t => t.id == id))
                            throw new ArgumentException();

                        Product.products.Add(new Product
                        {
                            id = id,
                            title = splitData[1],
                            price = decimal.Parse(splitData[2]),
                            category = splitData[3]
                        });
                    }
                    catch { error = true; }
                }

                foreach (var p in providers)
                {
                    try
                    {
                        splitData = p.Split(new string[] { "\t" },
                            StringSplitOptions.RemoveEmptyEntries);
                        id = int.Parse(splitData[0]);
                        if (Provider.providers.Any(t => t.id == id))
                            throw new ArgumentException();

                        Provider.providers.Add(new Provider
                        {
                            id = id,
                            name = splitData[1],
                            address = splitData[2],
                            phone = splitData[3],
                            email = splitData[4]
                        });
                    }
                    catch { error = true; }
                }

                foreach (var p in purchases)
                {
                    try
                    {
                        splitData = p.Split(new string[] { "\t" },
                            StringSplitOptions.RemoveEmptyEntries);
                        id = int.Parse(splitData[0]);
                        if (Purchase.purchases.Any(t => t.id == id))
                            throw new ArgumentException();

                        var ps = new (Product product, int count)[(splitData.Length - 4) / 2];
                        for (int i = 0; i < ps.Length; i++)
                            ps[i] = (Product.products.First(t => t.id == int.Parse(splitData[1 + 2 * i])),
                                int.Parse(splitData[2 + 2 * i]));

                        Purchase.purchases.Add(new Purchase
                        {
                            id = id,
                            products = new List<(Product product, int count)>(ps),
                            provider = Provider.providers
                                .First(t => t.id == int.Parse(splitData[splitData.Length - 3])),
                            purchseDate = DateTime.Parse(splitData[splitData.Length - 2]),
                            sale = decimal.Parse(splitData[splitData.Length - 1])

                        });
                    }
                    catch { error = true; }
                }
            }

            if (error)
            {
                Console.WriteLine("При чтении файла были обнаружены некорректные записи.");
                Console.WriteLine("Они не были добавлены в таблицы и будут удалены");
                Console.WriteLine("Нажмите любую клавишу, чтобы продолжить");
                Console.ReadKey();
            }
        }

        static void SaveData()
        {
            var writer = new StreamWriter("data.txt");
            foreach (var p in Product.products)
                writer.WriteLine(string.Join("\t", p.id, p.title, p.price, p.category));

            writer.WriteLine(new string('~', Program.SEPARATOR_COUNT));

            foreach (var p in Provider.providers)
                writer.WriteLine(string.Join("\t", p.id, p.name, p.address, p.phone, p.email));

            writer.WriteLine(new string('~', Program.SEPARATOR_COUNT));

            foreach (var p in Purchase.purchases)
                writer.WriteLine(string.Join("\t", p.id,
                    string.Join("\t", p.products.Select(t => string.Join("\t", t.product.id, t.count))),
                    p.provider.id, p.purchseDate.ToString("dd.MM.yyyy"), p.sale));

            writer.Close();
        }

        static MenuState WriteMenu(bool main, params (Func<MenuState> action, string text)[] lines)
        {
            do
            {
                Console.WriteLine("Выберите необходимое действие");
                for (int i = 0; i < lines.Length && i < 9; i++)
                    Console.WriteLine("{0} - {1}", i + 1, lines[i].text);

                if (main)
                    Console.WriteLine("{0} - Выход из программы", lines.Length + 1 < 10 ? lines.Length + 1 : 0);
                else
                    Console.WriteLine("{0} - В предыдущее меню", lines.Length + 1 < 10 ? lines.Length + 1 : 0);

                var key = Console.ReadKey(true);
                Console.Clear();
                if (lines.Length >= 10 && key.KeyChar < '0' || lines.Length < 10 && key.KeyChar <= '0' || key.KeyChar > '9' ||
                    key.KeyChar.ToInt() > lines.Length + 1)
                    return MenuState.Error;
                else if (lines.Length >= 10 && key.KeyChar == '0' || key.KeyChar.ToInt() == lines.Length + 1)
                    return MenuState.Back;
                else
                    return lines[key.KeyChar.ToInt() - 1].action.Invoke();
            } while (true);
        }

        static void WriteTable(List<string> list, params (string text, int width)[] fields)
        {
            list.Add(string.Join("", fields.Select(f => f.text.PadRight(f.width))));
        }

        static string ReadLine(string text, bool allowEmpty = false)
        {
            bool success;
            string str;
            do
            {
                Console.Write(text);
                str = Console.ReadLine().Trim();
                success = !string.IsNullOrWhiteSpace(str) || allowEmpty;
                if (!success)
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string('\0', Console.BufferWidth));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
            } while (!success);
            return str;
        }

        static int ToInt(this char c)
        {
            return int.Parse(c.ToString());
        }

        enum MenuState
        {
            Choise,
            Cancel,
            Back,
            Error
        }
    }
}
