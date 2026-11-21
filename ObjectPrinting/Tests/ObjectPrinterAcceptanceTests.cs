using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "Alexandr",
                Age = 19,
                Height = 182.5,
                Father = new Person { Name = "Ivan", Age = 45, Height = 178.3 }
            };

            var printer = ObjectPrinter.For<Person>()
                // 1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                // 2. Указать альтернативный способ сериализации для определенного типа
                .For<int>().Using(i => i.ToString("X"))
                // 3. Для числовых типов указать культуру
                .For<double>().Using(CultureInfo.InvariantCulture)
                // 4. Настроить сериализацию конкретного свойства
                .For(p => p.Age).Using(age => $"{age} years")
                // 5. Настроить обрезание строковых свойств (метод виден только для string)
                .For(p => p.Name).Truncate(4)
                // 6. Исключить из сериализации конкретное свойство
                .Exclude(p => p.Father);

            string s1 = printer.PrintToString(person);

            // 7. Синтаксический сахар в виде метода расширения, сериализующего по умолчанию
            string s2 = person.PrintToString();

            // 8. ...с конфигурированием
            string s3 = person.PrintToString(cfg => cfg
                .Exclude<Guid>()
                .For(p => p.Name).Truncate(4));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);

            Assert.That(s1, Does.Not.Contain("Id ="));
            Assert.That(s1, Does.Not.Contain("Father ="));
            Assert.That(s1, Does.Contain("Name = Alex" + Environment.NewLine));
            Assert.That(s1, Does.Contain("Height = 182.5" + Environment.NewLine));
            Assert.That(s1, Does.Contain("Age = 19 years" + Environment.NewLine));
        }

        [Test]
        public void Truncate_PropertyRuleOverridesTypeRule()
        {
            var sample = new TwoStrings { First = "HelloWorld", Second = "HelloWorld" };

            var result = ObjectPrinter.For<TwoStrings>()
                .For<string>().Truncate(3)
                .For(x => x.First).Truncate(6)
                .PrintToString(sample);

            Assert.That(result, Does.Contain("First = HelloW" + Environment.NewLine));
            Assert.That(result, Does.Contain("Second = Hel" + Environment.NewLine));
        }

        [Test]
        public void Config_IsImmutable()
        {
            var person = new Person { Name = "Alexander", Age = 25, Height = 180.5 };

            var basePrinter = ObjectPrinter.For<Person>().Exclude<Guid>();
            var truncatedPrinter = basePrinter.For(p => p.Name).Truncate(4);
            var excludedAgePrinter = basePrinter.Exclude(p => p.Age);

            var baseOutput = basePrinter.PrintToString(person);
            var truncatedOutput = truncatedPrinter.PrintToString(person);
            var excludedAgeOutput = excludedAgePrinter.PrintToString(person);

            Assert.That(baseOutput, Does.Contain("Name = Alexander" + Environment.NewLine));
            Assert.That(baseOutput, Does.Contain("Age = 25" + Environment.NewLine));

            Assert.That(truncatedOutput, Does.Contain("Name = Alex" + Environment.NewLine));
            Assert.That(truncatedOutput, Does.Contain("Age = 25" + Environment.NewLine));

            Assert.That(excludedAgeOutput, Does.Contain("Name = Alexander" + Environment.NewLine));
            Assert.That(excludedAgeOutput, Does.Not.Contain("Age ="));
        }

        [Test]
        public void Culture_AppliesToNumericTypes()
        {
            var person = new Person { Height = 180.5 };

            var ruOutput = person.PrintToString(cfg => cfg.For<double>().Using(CultureInfo.GetCultureInfo("ru-RU")));
            var invOutput = person.PrintToString(cfg => cfg.For<double>().Using(CultureInfo.InvariantCulture));

            Assert.That(ruOutput, Does.Contain("Height = 180,5" + Environment.NewLine));
            Assert.That(invOutput, Does.Contain("Height = 180.5" + Environment.NewLine));
        }

        [Test]
        public void CyclicReference_DoesNotStackOverflow()
        {
            var person = new Person { Name = "Loop" };
            person.Father = person;

            var output = person.PrintToString();

            Assert.That(output, Does.Contain("Father = <cyclic reference>" + Environment.NewLine));
        }

        private class TwoStrings
        {
            public string First { get; set; } = string.Empty;
            public string Second { get; set; } = string.Empty;
        }
    }
}
