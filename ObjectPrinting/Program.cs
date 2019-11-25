using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class Program
    {
        private static void Main()
        {
            var personTester = new Person { Name = "Man", Age = -45 };
            var person3 = new Person { Name = "Man", Age = 45 };
            var person2 = new Person { Name = "Dan", Age = 23, Parent = person3 };
            var person1 = new Person { Name = "Alex", Age = 19, Parent = person2 };
            var loooper = new Person {Age = 15, Name = "SASAA"};
            var loooper2 = new Person {Age = 15, Name = "SASAA2", Parent = loooper};
            loooper.Parent = loooper2;

            var printer1 = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(x => "abcd");
                //.Printing(x => x.Name).Using(x => "Name");

            var printer2 = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                //.Printing<int>().Using(x => "abcd")
                //.Printing(x => x.Name).Using(x => "Name")
                .Excluding<double>();

            var myCulture = (CultureInfo)new CultureInfo("ru-RU").Clone();
            myCulture.NumberFormat.NegativeSign = "!";

            var printer3 = ObjectPrinter.For<Person>()
                .Printing<int>().SetCulture(myCulture)
                .Excluding(x => x.Name);

            Console.WriteLine(printer1.PrintToString(person1));
            Console.WriteLine("========================");
            Console.WriteLine(printer2.PrintToString(person1));
            Console.WriteLine("========================");
            Console.WriteLine(printer2.PrintToString(loooper));
            Console.WriteLine("========================");
            Console.WriteLine(printer3.PrintToString(personTester));
            Console.WriteLine("========================");
            
            var temp = new List<Person>
            {
                new Person { Name = "Man", Age = -45 },
                new Person { Name = "Man", Age = 45 },
                new Person { Name = "Dan", Age = 23 },
                new Person { Name = "Alex", Age = 19 },
            };
            var convr = temp.Serialize().Excluding<int>();
            
            Console.WriteLine(convr.PrintToString(temp));

            var temp2 = new List<object>
            {
                new Person { Name = "Man", Age = -45 },
                new List<Person>()
                {
                    new Person { Name = "Man", Age = -45 },
                },
            };

            var convr2 = temp2.Serialize().Excluding<int>();

            Console.WriteLine("========================");
            Console.WriteLine(convr2.PrintToString(temp2));

            var returner = new Dictionary<string, int>()
            {
                ["a"] = 5,
            };
            
            Console.WriteLine(returner.Serialize().PrintToString(returner));
        }
    }
}