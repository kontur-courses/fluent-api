using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, BirthDate = new DateTime(2001, 2, 3)};
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Demo()
        {
            printer
                .Excluding<Guid>()
                .Printing<int>().Using(i => (i + 1).ToString())
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Name).TrimmedToLength(10)
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingInt()
        {
            printer.Excluding<int>();
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", null, null, "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingDouble()
        {
            printer.Excluding<double>();
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", null, "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingGuid()
        {
            printer.Excluding<Guid>();
            Assert.AreEqual(
                GetCorrectPrintingConfig(null, "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingDateTime()
        {
            printer.Excluding<DateTime>();
            Assert.AreEqual(GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", null),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingString()
        {
            printer.Excluding<string>();
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", null, "0\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingInt()
        {
            printer.Printing<int>().Using(i => (i + 1).ToString());
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "20", "1", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingString()
        {
            printer.Printing<string>().Using(i => i + i);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "AlexAlex", "0\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingGuid()
        {
            printer.Printing<Guid>().Using(i => "Guid");
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid", "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDoubleWithCulture()
        {
            person.Height = 171.5;
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "171.5\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingDateTimeWithCulture()
        {
            printer.Printing<DateTime>().Using(CultureInfo.InvariantCulture);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", "02/03/2001 00:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingAge()
        {
            printer.Printing(p => p.Age).Using(p => $"!{p}!");
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "!19!", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingBirthDate()
        {
            printer.Printing(p => p.BirthDate).Using(p => $"!{p}!");
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", "!03.02.2001 0:00:00!"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenPrintingWithCutting()
        {
            printer.Printing(p => p.Name).TrimmedToLength(3);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Ale", "0\r\n", "19\r\n", "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingAge()
        {
            printer.Excluding(p => p.Age);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", null, "0\r\n", "03.02.2001 0:00:00\r\n"),
                printer.PrintToString(person));
        }

        [Test]
        public void PrintToString_CorrectResult_WhenExcludingBirthDate()
        {
            printer.Excluding(p => p.BirthDate);
            Assert.AreEqual(
                GetCorrectPrintingConfig("Guid\r\n", "Alex\r\n", "0\r\n", "19\r\n", "0\r\n", null),
                printer.PrintToString(person));
        }

        private string GetCorrectPrintingConfig(string id, string name, string height, string age, string weight,
            string birthDate)
        {
            return $"{nameof(Person)}\r\n" +
                   (!string.IsNullOrEmpty(id) ? $"\t{nameof(person.Id)} = {id}" : "") +
                   (!string.IsNullOrEmpty(name) ? $"\t{nameof(person.Name)} = {name}" : "") +
                   (!string.IsNullOrEmpty(height) ? $"\t{nameof(person.Height)} = {height}" : "") +
                   (!string.IsNullOrEmpty(age) ? $"\t{nameof(person.Age)} = {age}" : "") +
                   (!string.IsNullOrEmpty(weight) ? $"\t{nameof(person.Weight)} = {weight}" : "") +
                   (!string.IsNullOrEmpty(birthDate) ? $"\t{nameof(person.BirthDate)} = {birthDate}" : "");
        }
    }
}