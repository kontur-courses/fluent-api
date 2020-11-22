using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinterTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 193.2};
        }

        private Person person;


        [Test]
        [SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i + "XXXX")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //.Printing(p => p.Name).TrimmedToLength(10)
                .Printing<string>().TrimmedToLength(5)
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
        public void PrintToString_PersonWithoutId_ExcludingGuid()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 193,2\r\n\tAge = 19\r\n");
        }

        [Test]
        public void PrintToString_ModifiedInt_PrintingWithXAtTheEnd()
        {
            var printer = ObjectPrinter.For<int>().Printing<int>().Using(i => i + "X");

            printer.PrintToString(5).Should().Be("5X");
        }

        [Test]
        public void PrintToString_PrintingDoubleInInvariantCulture_PrintingUsingInvariantCulture()
        {
            var printer = ObjectPrinter.For<double>().Printing<double>().Using(CultureInfo.InvariantCulture);

            printer.PrintToString(2.5).Should().Be("2.5");
        }

        [Test]
        public void PrintToString_PrintingDateTimeInInvariantCulture_PrintingUsingInvariantCulture()
        {
            var printer = ObjectPrinter.For<DateTime>().Printing<DateTime>().Using(CultureInfo.InvariantCulture);


            var actual = printer.PrintToString(DateTime.MinValue);
            printer.PrintToString(DateTime.MinValue).Should()
                .Be(DateTime.MinValue.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void PrintToString_PersonPropertiesExcludedId_ExcludingPersonId()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);
            

            printer.PrintToString(person).Should().Be("Person\r\n\tName = Alex\r\n\tHeight = 193,2\r\n\tAge = 19\r\n");
        }
        
        [Test]
        [Timeout(1000)]
        public void PrintToString_ShouldNotThrow_SelfReference()
        {
            var personWithFriend = new PersonWithFriend {Name = "Alex", Age = 19};
            personWithFriend.Friend = personWithFriend;

            personWithFriend.PrintToString();
        }
    }
}