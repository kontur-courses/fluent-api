using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19, Parent = new Person {Name = "Andrew", Age = 90}};
            var listPersons = new List<Person>();
            var dictionaryPersons = new Dictionary<string, Person>();
            var listlistPersons = new List<List<Person>>();
            dictionaryPersons.Add("abc", person);
            dictionaryPersons.Add("vcsadasd", person);
            listPersons.Add(person);
            listPersons.Add(person);
            listlistPersons.Add(listPersons);
            listlistPersons.Add(listPersons);

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing(x => x.Age).Using(x => x > 18 ? "Старый" : "Молодой")
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(2);

            var listPrinter = ObjectPrinter.For<List<Person>>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"));
            
            var listlistPrinter = ObjectPrinter.For<List<List<Person>>>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"));
            
            var arrayPrinter = ObjectPrinter.For<Person[]>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"));
            
            var dictionaryPrinter = ObjectPrinter.For<Dictionary<string, Person>>()
                .Excluding<Guid>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru"));

            string s1 = printer.PrintToString(person);
            
            string s2 = person.PrintToString();

            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
            Console.WriteLine(listPrinter.PrintToString(listPersons));
            Console.WriteLine(arrayPrinter.PrintToString(listPersons.ToArray()));
            Console.WriteLine(dictionaryPrinter.PrintToString(dictionaryPersons));
            Console.WriteLine(listlistPrinter.PrintToString(listlistPersons));
        }
    }
}