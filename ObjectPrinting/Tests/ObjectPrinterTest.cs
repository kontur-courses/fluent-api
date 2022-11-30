using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTest
    {
        [Test]
        public void ExcludingType_ShouldExcludeAllPropertiesOfThisType()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            var s = printer.PrintToString(person);
            s.Should().NotContain("Alex").And.NotContain("Rubanov").And.Contain("Age");
            Console.WriteLine(s);
        }
        
        [Test]
        public void ExcludingProperty_ShouldExcludeOnlyThisProperty()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            var s = printer.PrintToString(person);
            s.Should().NotContain("Age").And.Contain("Alex");
            Console.WriteLine(s);
        }

        [Test]
        public void PrintingType_ShouldReprintAllPropertiesOfThisType()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(s => "\"" + s + "\"");
            var s = printer.PrintToString(person);
            s.Should().Contain("\"Alex\"")
                .And.Contain("\"Rubanov\"")
                .And.Contain("\"\"")
                .And.NotContain("\"19\"");
            Console.WriteLine(s);
        }
        
        [Test]
        public void PrintingProperty_ShouldReprintOnlyThisProperty()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Patronymic).Using(s => "\"" + s + "\"");
            var s = printer.PrintToString(person);
            s.Should().NotContain("\"Alex\"")
                .And.Contain("\"\"");
            Console.WriteLine(s);
        }

        [Test]
        public void PrintingCultureInfo_ShouldPrint_BaseOnSelectedCultureInfo()
        {
            var person = new Person { Height = 174.8 };
            var printerEn = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en-US"));
            var printerRu = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru-RU"));

            var ru = printerRu.PrintToString(person);
            var en = printerEn.PrintToString(person);

            Console.WriteLine("ru-RU - " + new CultureInfo("ru-RU").NumberFormat.NumberDecimalSeparator);
            Console.WriteLine(ru);
            Console.WriteLine("en-US - " + new CultureInfo("en-US").NumberFormat.NumberDecimalSeparator);
            Console.WriteLine(en);
            
            ru.Should().Contain("174,8");
            en.Should().Contain("174.8");
        }

        [Test]
        public void PrintingTrimmedToLength_ShouldSaveText_WhenItShort()
        {
            var person = new Person { Name = "Alex" };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(10);

            var s = printer.PrintToString(person);
            
            Console.WriteLine(s);
            s.Should().Contain("Alex").And.NotContain("Alex...");
        }
        
        [Test]
        public void PrintingTrimmedToLength_ShouldTrimText_WhenItLong()
        {
            var person = new Person { Name = "Very Long Name" };
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(10);

            var s = printer.PrintToString(person);
            
            Console.WriteLine(s);
            s.Should().Contain("Very Long ...");
        }

        [Test]
        public void PrintToString_ShouldStop_WhenCircleLinks()
        {
            var sun = new Person();
            var dad = new Person();
            sun.Parent = dad;
            dad.Parent = sun;
            var printer = ObjectPrinter.For<Person>();
            var s = printer.PrintToString(sun);
            Console.WriteLine(s);
            s.Should().Contain("...");
        }

        [Test]
        public void PrintToString_ObjectExtension_ShouldPrintWithDefaultSettings()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>();
            var s = person.PrintToString();
            s.Should().Be(printer.PrintToString(person));
            Console.WriteLine(s);
        }

        [Test]
        public void PrintToString_Object_ExtensionWithConfig_ShouldPrintWithSelectedSettings()
        {
            var person = new Person { Name = "Alex", Surname = "Rubanov", Patronymic = "", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Excluding<string>();
            var s = person.PrintToString(config => config.Excluding<string>());
            s.Should().Be(printer.PrintToString(person));
            Console.WriteLine(s);
        }
    }
}