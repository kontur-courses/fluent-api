using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private class Source
        {
            public double DoubleProp1 { get; set; }
            public double DoubleProp2 { get; set; }
            public string StrProp1 { get; set; }
            public string StrProp2 { get; set; }

        }

        [Test]
        public void PrintToString_WhenNoSettings_ReturnResultUsingDefaultHandlers()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
            var expected = $"{nameof(Person)}\r\n" +
                           $"\t{nameof(Person.Id)} = {person.Id.GetType().Name}\r\n" +
                           $"\t{nameof(Person.Name)} = {person.Name}\r\n" +
                           $"\t{nameof(Person.Height)} = {person.Height}\r\n" +
                           $"\t{nameof(Person.Age)} = {person.Age}\r\n";

            printer.PrintToString(person).Should().Be(expected);
            person.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenExcludingAllProperties_ReturnOnlyTypeName()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<int>();
            printer.PrintToString(person).Should().Be($"{nameof(Person)}\r\n");
        }

        [Test]
        public void PrintToString_WhenExcludingConcreteTypeProperties()
        {
            var obj = new Source {StrProp1 = "Alex", StrProp2 = "Blade"};

            var printer = ObjectPrinter.For<Source>()
                .Excluding<string>();

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenExcludingConcreteProperty()
        {
            var obj = new Source {StrProp1 = "Alex", StrProp2 = "Blade"};

            var printer = ObjectPrinter.For<Source>()
                .Excluding(source => source.StrProp2);

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n" +
                           $"\t{nameof(Source.StrProp1)} = {obj.StrProp1}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenChangedTypePropertyHandler()
        {
            var obj = new Source {StrProp1 = "Alex", StrProp2 = "Blade"};
            var handler = "I am string";

            var printer = ObjectPrinter.For<Source>()
                .Printing<string>().Using(s => handler);

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n" +
                           $"\t{nameof(Source.StrProp1)} = {handler}\r\n" +
                           $"\t{nameof(Source.StrProp2)} = {handler}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenChangedConcretePropertyHandler()
        {
            var obj = new Source {StrProp1 = "Alex", StrProp2 = "Blade"};
            var handler1 = "I am string";
            var handler2 = "I am another string";

            var printer = ObjectPrinter.For<Source>()
                .Printing(source => source.StrProp1).Using(s => handler1)
                .Printing<string>().Using(s => handler2);

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n" +
                           $"\t{nameof(Source.StrProp1)} = {handler1}\r\n" +
                           $"\t{nameof(Source.StrProp2)} = {handler2}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenAllowTrimStringSetting()
        {
            var obj = new Source {StrProp1 = "Alex", StrProp2 = "Blade"};
            var trimLength = 3;

            var printer = ObjectPrinter.For<Source>()
                .Printing<string>(source => source.StrProp1).TrimmedToLength(trimLength);

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n" +
                           $"\t{nameof(Source.StrProp1)} = {obj.StrProp1.Substring(0, trimLength)}\r\n" +
                           $"\t{nameof(Source.StrProp2)} = {obj.StrProp2}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenAllowCultureInfoNumberSetting()
        {
            var obj = new Source {DoubleProp1 = 3.3, DoubleProp2 = 2};
            var cultureInfo = CultureInfo.InvariantCulture;

            var printer = ObjectPrinter.For<Source>()
                .Printing(source => source.DoubleProp1).Using(cultureInfo);

            var expected = $"{nameof(Source)}\r\n" +
                           $"\t{nameof(Source.DoubleProp1)} = {obj.DoubleProp1.ToString(cultureInfo)}\r\n" +
                           $"\t{nameof(Source.DoubleProp2)} = {obj.DoubleProp2}\r\n" +
                           $"\t{nameof(Source.StrProp1)} = {obj.StrProp1}\r\n" +
                           $"\t{nameof(Source.StrProp2)} = {obj.StrProp2}\r\n";

            printer.PrintToString(obj).Should().Be(expected);
        }
    }
}