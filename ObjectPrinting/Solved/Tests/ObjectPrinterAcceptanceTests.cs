using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        Person person;

        [SetUp]
        public void SetUp()
        {
            var alterEgo = new Person { Name = "Ivan IV", Age = 52, Height = 164 };
            person = new Person { Name = "Misha", Age = 21, Height = 172, AlterEgo = alterEgo };
        }
        [Test]
        public void Demo()
        {
            person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ExcludeType()
        {
            var printind = ObjectPrinter.For<Person>().Excluding<Guid>();
            var result = printind.PrintToString(person);
            Assert.IsFalse(result.Contains("Id"));
        }

        [Test]

        public void ExcludingFieldFirstLevel()
        {
            var printind = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            var result = printind.PrintToString(person);
            Assert.IsTrue(!result.Contains($"Age = {person.Age}") && result.Contains($"Age = {person.AlterEgo.Age}"));
        }

        [Test]
        public void ExcludingFieldDeepLevel()
        {
            var printind = ObjectPrinter.For<Person>().Excluding(p => p.AlterEgo.Age);
            var result = printind.PrintToString(person);
            Assert.IsTrue(result.Contains($"Age = {person.Age}") && !result.Contains($"Age = {person.AlterEgo.Age}"));
        }

	    [Test]
        public void TrimedAllStrings()
        {
            var printind = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(2);
            var result = printind.PrintToString(person);
            var nameTrim = $"Name = {person.Name[0..2]}" + Environment.NewLine;
            var nameEgo = $"Name = {person.AlterEgo.Name[0..2]}" + Environment.NewLine;
            Assert.IsTrue(result.Contains(nameTrim) && result.Contains(nameEgo));
        }

	    [Test]
        public void TrimedSomeStrings()
        {
            var printind = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(2);
            var nameTrim = $"Name = {person.Name[0..2]}" + Environment.NewLine;
            var nameEgo = $"Name = {person.AlterEgo.Name}";
            var result = printind.PrintToString(person);
            Assert.IsTrue(result.Contains(nameTrim) && result.Contains(nameEgo));
        }

	    [TestCase(2, 3)]
	    [TestCase(2, 2)]
	    [TestCase(3, 2)]
        public void TrimedAllAndSomeStrings(int stringTrim, int fieldTrim)
        {
            var printind = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(stringTrim)
                .Printing(p => p.AlterEgo.Name).TrimmedToLength(fieldTrim);
            var result = printind.PrintToString(person);
            var nameTrim = $"Name = {person.Name[0..stringTrim]}" + Environment.NewLine;
            var nameEgo = $"Name = {person.AlterEgo.Name[0..fieldTrim]}" + Environment.NewLine;
            Assert.IsTrue(result.Contains(nameTrim) && result.Contains(nameEgo));
        }

        [Test]
        public void NotReferenceCicle()
        {
            person.AlterEgo = person;
            //hmm... it's logic
            var printing = ObjectPrinter.For<Person>();
            printing.PrintToString(person);
        }

        [Test]
        public void PrintingWithCulture()
        {
            var culture = CultureInfo.InvariantCulture;
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(culture);
            var result = printer.PrintToString(person);
            var heightPerson = $"Height = {person.Height.ToString(culture)}";
            var heightEgo = $"Height = {person.AlterEgo.Height.ToString(culture)}";
            Assert.IsTrue(result.Contains(heightPerson) && result.Contains(heightEgo));
        }
    }
}