using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ContactBook
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Contact Book - Day 9";
            var contacts = Seed();

            while (true)
            {
                ShowMenu();
                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1": AddContact(contacts); break;
                    case "2": ListContacts(contacts); break;
                    case "3": SearchByName(contacts); break;
                    case "4": FilterByEmailDomain(contacts); break;
                    case "5": FilterByTag(contacts); break;
                    case "6": GroupByFirstLetter(contacts); break;
                    case "7": SortMenu(contacts); break;
                    case "0": Info("Bye 👋"); return;
                    default: Warn("Invalid choice."); break;
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== Contact Book ===");
            Console.WriteLine("1) Add contact");
            Console.WriteLine("2) List contacts");
            Console.WriteLine("3) Search by name (contains)");
            Console.WriteLine("4) Filter by email domain (e.g., gmail.com)");
            Console.WriteLine("5) Filter by tag (e.g., work, family)");
            Console.WriteLine("6) Group by first letter of name");
            Console.WriteLine("7) Sort (name/email/created)");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");
        }

        // ---- Actions ----
        static void AddContact(List<Contact> contacts)
        {
            Console.Write("Full name: ");
            var name = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) { Warn("Name required."); return; }

            Console.Write("Email (optional): ");
            var email = (Console.ReadLine() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(email) && !IsEmail(email))
            {
                Warn("Invalid email format. Example: ali@example.com");
                return;
            }

            Console.Write("Phone (optional, digits/+/-/space): ");
            var phone = (Console.ReadLine() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(phone) && !IsPhone(phone))
            {
                Warn("Invalid phone format.");
                return;
            }

            Console.Write("Tags (comma separated, e.g., work,team): ");
            var tagLine = Console.ReadLine() ?? "";
            var tags = tagLine.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                              .Select(t => t.ToLowerInvariant()).Distinct().ToList();

            contacts.Add(new Contact
            {
                Name = name,
                Email = email,
                Phone = phone,
                Tags = tags,
                CreatedAt = DateTime.Now
            });

            Notify("Added ✅");
        }

        static void ListContacts(List<Contact> contacts)
        {
            if (contacts.Count == 0) { Info("No contacts."); return; }

            // Default view: ordered by name
            var data = contacts.OrderBy(c => c.Name).ToList();
            PrintTable(data);
        }

        static void SearchByName(List<Contact> contacts)
        {
            Console.Write("Name contains: ");
            var q = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            var results = contacts
                .Where(c => c.Name.ToLowerInvariant().Contains(q))
                .OrderBy(c => c.Name)
                .ToList();

            PrintResults(results);
        }

        static void FilterByEmailDomain(List<Contact> contacts)
        {
            Console.Write("Domain (e.g., gmail.com): ");
            var domain = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            var results = contacts
                .Where(c => !string.IsNullOrWhiteSpace(c.Email) &&
                            c.Email.ToLowerInvariant().EndsWith("@" + domain) ||
                            c.Email.ToLowerInvariant().EndsWith(domain)) // user may type full address or just domain
                .OrderBy(c => c.Name)
                .ToList();

            PrintResults(results);
        }

        static void FilterByTag(List<Contact> contacts)
        {
            Console.Write("Tag (e.g., work, family): ");
            var tag = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            var results = contacts
                .Where(c => c.Tags.Any(t => t == tag))
                .OrderBy(c => c.Name)
                .ToList();

            PrintResults(results);
        }

        static void GroupByFirstLetter(List<Contact> contacts)
        {
            if (contacts.Count == 0) { Info("No contacts."); return; }

            var groups = contacts
                .GroupBy(c => char.ToUpperInvariant(c.Name.FirstOrDefault('?')))
                .OrderBy(g => g.Key);

            foreach (var g in groups)
            {
                Console.WriteLine($"\n[{g.Key}]");
                PrintTable(g.OrderBy(c => c.Name).ToList(), header: false);
            }
        }

        static void SortMenu(List<Contact> contacts)
        {
            Console.WriteLine("Sort by: 1) Name  2) Email  3) CreatedAt (newest first)");
            Console.Write("Choose: ");
            var s = (Console.ReadLine() ?? "").Trim();

            IEnumerable<Contact> q = contacts;
            switch (s)
            {
                case "1": q = contacts.OrderBy(c => c.Name); break;
                case "2": q = contacts.OrderBy(c => c.Email); break;
                case "3": q = contacts.OrderByDescending(c => c.CreatedAt); break;
                default: Warn("Unknown option."); return;
            }

            PrintTable(q.ToList());
        }

        // ---- Helpers & UI ----
        static void PrintResults(List<Contact> results)
        {
            if (results.Count == 0) { Warn("No matches."); return; }
            PrintTable(results);
        }

        static void PrintTable(List<Contact> rows, bool header = true)
        {
            if (header)
            {
                Console.WriteLine("\n#  Name                      Email                      Phone            Tags                 Created");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
            for (int i = 0; i < rows.Count; i++)
            {
                var c = rows[i];
                var tagStr = (c.Tags.Count == 0 ? "—" : string.Join(';', c.Tags));
                Console.WriteLine($"{i + 1,2} {Trunc(c.Name, 24),-24} {Trunc(c.Email ?? "—", 26),-26} {Trunc(c.Phone ?? "—", 15),-15} {Trunc(tagStr, 20),-20} {c.CreatedAt:g}");
            }
        }

        static string Trunc(string s, int n) => (s ?? "—").Length <= n ? s ?? "—" : (s ?? "—")[..(n - 1)] + "…";

        static bool IsEmail(string email) =>
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        static bool IsPhone(string phone) =>
            Regex.IsMatch(phone, @"^[\d+\-\s]{5,}$");

        static void Warn(string msg) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(msg); Console.ResetColor(); }
        static void Notify(string msg) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(msg); Console.ResetColor(); }
        static void Info(string msg) { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(msg); Console.ResetColor(); }

        static List<Contact> Seed() => new()
        {
            new Contact { Name="Ali Razi", Email="ali.razi@gmail.com", Phone="+1 416 555 1001", Tags=new(){"work","team"}, CreatedAt=DateTime.Now.AddMinutes(-30) },
            new Contact { Name="Sina Siari", Email="sina@company.com", Phone="+98 912 111 2222", Tags=new(){"family"}, CreatedAt=DateTime.Now.AddMinutes(-20) },
            new Contact { Name="Maryam Z.", Email="maryam@yahoo.com", Phone="0935 222 3333", Tags=new(){"friends"}, CreatedAt=DateTime.Now.AddMinutes(-10) },
        };
    }

    class Contact
    {
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
