using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchasesControl
{
    static partial class Program
    {
        static List<string> providersTable = new List<string>();
        static MenuState ProvidersMenu()
        {
            int prevBuffSize = Console.BufferWidth;
            var state = MenuState.Cancel;

            do
            {
                if (state == MenuState.Choise)
                    WriteProvidersTable();

                providersTable.ForEach(t => Console.WriteLine(t));

                Console.WriteLine();
                if (state == MenuState.Error)
                    Console.WriteLine("Ошибка ввода");

                state = WriteMenu(false, (() => AddEditProvider(), "Добавить поставщика"),
                                         (EditProvider, "Редактировать поставщика"),
                                         (DeleteProvider, "Удалить поставщика"));
                Console.Clear();
            } while (state != MenuState.Back);
            Console.BufferWidth = prevBuffSize;
            return MenuState.Choise;
        }

        static void WriteProvidersTable()
        {
            string[] columns = new string[] { "ID", "Наименование", "Адрес", "Эл. почта", "Номер телефона" };
            int[] widths = new int[columns.Length];
            providersTable.Clear();
            for (int i = 0; i < columns.Length; i++)
                widths[i] = columns[i].Length;

            foreach (var c in Provider.providers)
            {
                if (c.id.ToString().Length > widths[0])
                    widths[0] = c.id.ToString().Length;
                if (c.name.Length > widths[1])
                    widths[1] = c.name.Length;
                if (c.address.Length > widths[2])
                    widths[2] = c.address.Length;
                if (c.email.Length > widths[3])
                    widths[3] = c.email.Length;
                if (c.phone.Length > widths[4])
                    widths[4] = c.phone.Length;
            }
            for (int i = 0; i < widths.Length; i++)
                widths[i] += 3;

            int w = widths.Sum() + Environment.NewLine.Length;
            if (w > Console.BufferWidth)
                Console.BufferWidth = widths.Sum() + Environment.NewLine.Length;

            var temp = new (string, int)[columns.Length];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = (columns[i], widths[i]);
            WriteTable(providersTable, temp);

            foreach (var c in Provider.providers)
            {
                temp[0] = (c.id.ToString(), widths[0]);
                temp[1] = (c.name, widths[1]);
                temp[2] = (c.address, widths[2]);
                temp[3] = (c.email, widths[3]);
                temp[4] = (c.phone, widths[4]);
                WriteTable(providersTable, temp);
            }
        }

        static MenuState AddEditProvider(Provider p = null)
        {
            string name, address, email, phone;

            name = ReadLine(String.Format("Введите наименование поставщика{0}: ",
                p != null ? " (" + p.name + ")" : ""), p != null);
            address = ReadLine(String.Format("Введите адрес поставщика{0}: ",
                p != null ? " (" + p.address + ")" : ""), p != null);
            email = ReadLine(String.Format("Введите эл. почту поставщика{0}: ",
                p != null ? " (" + p.email + ")" : ""), p != null);
            phone = ReadLine(String.Format("Введите номер телефона поставщика{0}: ",
                p != null ? " (" + p.phone + ")" : ""), p != null);

            Console.Clear();
            Console.WriteLine("Наименование: {0}", string.IsNullOrWhiteSpace(name) ? p.name : name);
            Console.WriteLine("Адрес: {0}", string.IsNullOrWhiteSpace(address) ? p.address : address);
            Console.WriteLine("Эл. почта: {0}", string.IsNullOrWhiteSpace(email) ? p.email : email);
            Console.WriteLine("Номер телефона: {0}", string.IsNullOrWhiteSpace(phone) ? p.phone : phone);
            Console.WriteLine("{0} данного поставщика?", p == null ? "Добавить" : "Сохранить");
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
                Provider.providers.Add(new Provider
                {
                    id = Provider.providers.Count > 0 ? Provider.providers.Last().id + 1 : 0,
                    name = name,
                    address = address,
                    email = email,
                    phone = phone
                });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(name)) p.name = name;
                if (!string.IsNullOrWhiteSpace(address)) p.address = address;
                if (!string.IsNullOrWhiteSpace(email)) p.email = email;
                if (!string.IsNullOrWhiteSpace(phone)) p.phone = phone;
            }
            return MenuState.Choise;
        }

        static MenuState EditProvider()
        {
            providersTable.ForEach(t => Console.WriteLine(t));
            Console.WriteLine();

            Console.Write("Введите ID поставщика, которого необходимо отредактировать: ");
            int id;
            bool success = int.TryParse(Console.ReadLine(), out id);
            Provider c;
            if (!success || (c = Provider.providers.FirstOrDefault(t => t.id == id)) == null)
                return MenuState.Error;

            Console.Clear();
            var s = AddEditProvider(c);
            WritePurchasesTable();
            return s;
        }

        static MenuState DeleteProvider()
        {
            providersTable.ForEach(t => Console.WriteLine(t));
            Console.WriteLine();

            Console.Write("Введите ID поставщика, которого необходимо удалить: ");
            int id;
            bool success = int.TryParse(Console.ReadLine(), out id);
            Provider c;
            if (!success || (c = Provider.providers.FirstOrDefault(t => t.id == id)) == null)
                return MenuState.Error;

            Provider.providers.Remove(c);
            Purchase.purchases.RemoveAll(t => t.provider == c);
            WritePurchasesTable();
            return MenuState.Choise;
        }
    }
}
