using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .WithType<double>().SpecificSerialization(x => "")
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture)
                .WithField(p => p.Age).SpecificSerialization(p => "")
                .WithType<string>().TrimString(6)
                .WithField(p => p.Age).Exclude();

            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            var s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>();

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tName = Alex" + Environment.NewLine + "\tHeight = 180,5" +
                    Environment.NewLine + "\tAge = 19" + Environment.NewLine + "");
        }

        [Test]
        public void SpecificTypeSerialization()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<double>().SpecificSerialization(x => $"double{x}double");

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tId = Guid" + Environment.NewLine + "\tName = Alex" +
                    Environment.NewLine + "\tHeight = double180,5double" + Environment.NewLine + "\tAge = 19" +
                    Environment.NewLine + "");
        }

        [Test]
        public void NumberCulture()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture);

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tId = Guid" + Environment.NewLine + "\tName = Alex" +
                    Environment.NewLine + "\tHeight = 180,5" + Environment.NewLine + "\tAge = 19" +
                    Environment.NewLine + "");
        }

        [Test]
        public void SpecificFieldSerialization()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.");

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tId = Guid" + Environment.NewLine + "\tName = Alex" +
                    Environment.NewLine + "\tHeight = 180,5" + Environment.NewLine + "\tAge = 19y.o." +
                    Environment.NewLine + "");
        }

        [Test]
        public void TrimString()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<string>().TrimString(6);

            printer.PrintToString(actual).Should()
                .Be("Person");
        }

        [Test]
        public void ExcludeField()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithField(p => p.Age).Exclude();

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tId = Guid" + Environment.NewLine + "\tName = Alex" +
                    Environment.NewLine + "\tHeight = 180,5" + Environment.NewLine + "");
        }

        [Test]
        public void MixFilters()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .WithType<double>().SpecificSerialization(x => $"double{x}double")
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture)
                .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.")
                .WithField(p => p.Name).Exclude();

            printer.PrintToString(actual).Should()
                .Be("Person" + Environment.NewLine + "\tHeight = double180,5double" + Environment.NewLine +
                    "\tAge = 19y.o." + Environment.NewLine + "");
        }

        [Test]
        public void CycleRefsCheck()
        {
            var parent = new CycleRef { Id = 0, Child = null };
            var child = new CycleRef { Id = 1, Child = null };
            parent.Child = child;
            child.Child = parent;

            var printer = ObjectPrinter.For<CycleRef>()
                .Exclude<int>();

            printer.PrintToString(parent).Should()
                .Be("CycleRef" + Environment.NewLine + "\tChild = CycleRef" + Environment.NewLine +
                    "\t\tChild = cycled... No more this field" + Environment.NewLine + "");
        }

        [Test]
        public void ListSerialize()
        {
            var col = new List<int>
            {
                5, 2, 19
            };

            var printer = ObjectPrinter.For<List<int>>();
            var str = printer.PrintToString(col);

            printer.PrintToString(col).Should()
                .Be("List`1<Int32>" + Environment.NewLine + "\t[0] = 5" + Environment.NewLine + "\t[1] = 2" +
                    Environment.NewLine + "\t[2] = 19" + Environment.NewLine + "");
        }

        [Test]
        public void ArraySerialize()
        {
            var arr = new[] { 5, 2, 19 };

            var printer = ObjectPrinter.For<int[]>();
            var str = printer.PrintToString(arr);

            printer.PrintToString(arr).Should()
                .Be("Int32[]" + Environment.NewLine + "\t[0] = 5" + Environment.NewLine + "\t[1] = 2" +
                    Environment.NewLine + "\t[2] = 19" + Environment.NewLine + "");
        }

        [Test]
        public void DictionarySerialize()
        {
            var dict = new Dictionary<string, int>
            {
                { "a", 1 },
                { "b", 7 },
                { "abc", 2 }
            };

            var printer = ObjectPrinter.For<Dictionary<string, int>>();
            var str = printer.PrintToString(dict);

            printer.PrintToString(dict).Should()
                .Be("Dictionary`2<Int32>" + Environment.NewLine + "\t[0] = a : 1" + Environment.NewLine +
                    "\t[1] = b : 7" + Environment.NewLine + "\t[2] = abc : 2" + Environment.NewLine + "");
        }
    }
}