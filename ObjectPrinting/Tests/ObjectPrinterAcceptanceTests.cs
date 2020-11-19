using System;
using System.Globalization;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;

        private readonly Person person = new Person
            {FirstName = "Alex", LastName = "Terminator", Age = 19, Height = 1.79, Weight = 75.5};

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }
        
        [Test]
        public void Demo()
        {
            printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.FirstName).TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Weight);

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
        public void Exclude_Guid_FromSerialization()
        {
            var actual = printer.Excluding<Guid>().PrintToString(person);

            actual.Should().NotContain("Id");
        }
        
        [Test]
        public void Exclude_String_FromSerialization()
        {
            var actual = printer.Excluding<string>().PrintToString(person);

            actual.Should().NotContain("Name");
        }

        [Test]
        public void Change_Serialization_ForInteger()
        {
            var actual = printer.Printing<int>().Using(i => i.ToString("X")).PrintToString(person);

            actual.Should().Contain(person.Age.ToString("X"));
        }
        
        [Test]
        public void Change_Serialization_ForGuid()
        {
            var actual = printer.Printing<Guid>().Using(i => i + "!").PrintToString(person);

            actual.Should().Contain(person.Id + "!");
        }
        
        [Test]
        public void Change_Culture_ForDouble()
        {
            var actual = printer.Printing<double>().Using(CultureInfo.InvariantCulture).PrintToString(person);

            actual.Should().Contain(person.Height.ToString(CultureInfo.InvariantCulture));
        }
        
        [Test]
        public void Change_Culture_ForInt()
        {
            var actual = printer.Printing<int>().Using(CultureInfo.InvariantCulture).PrintToString(person);

            actual.Should().Contain(person.Age.ToString(CultureInfo.InvariantCulture));
        }
        
        [Test]
        public void Change_Serialization_ForProperty()
        {
            var actual = printer.Printing(p => p.Height).Using(h => h + "!").PrintToString(person);

            actual.Should().Contain(person.Height + "!");
            actual.Should().NotContain(person.Weight + "!");
        }

        [Test]
        public void Make_Alex_Flex()
        {
            var actual = printer.Printing(p => p.FirstName).Using(n => n.Replace("A", "F")).PrintToString(person);

            actual.Should().Contain(person.FirstName.Replace("A", "F"));
        }

        [Test]
        public void Trim_AllStringProperties_WithGreaterMaxLength()
        {
            var length = Math.Max(person.FirstName.Length, person.LastName.Length) + 1;
            var actual = printer.Printing<string>().TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName);
            actual.Should().Contain(person.LastName);
        }
        
        [Test]
        public void Trim_AllStringProperties_WithSmallerMaxLength()
        {
            var length = Math.Min(person.FirstName.Length, person.LastName.Length) - 1;
            var actual = printer.Printing<string>().TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName.Substring(0, length));
            actual.Should().Contain(person.LastName.Substring(0, length));
        }
        
        [Test]
        public void Trim_ChosenProperties_WithSmallerMaxLength()
        {
            var length = person.FirstName.Length - 1;
            var actual = printer.Printing(p => p.FirstName).TrimmedToLength(length).PrintToString(person);

            actual.Should().Contain(person.FirstName.Substring(0, length));
            actual.Should().Contain(person.LastName);
        }

        [Test]
        public void Exclude_ChosenProperty()
        {
            var actual = printer.Excluding(p => p.Weight).PrintToString(person);
            
            actual.Should().NotContain(nameof(person.Weight));
            actual.Should().Contain(nameof(person.Height));
        }

        [Test]
        public void Use_DefaultSerialization_WithExtension()
        {
            var actual = person.PrintToString();
            var expected = printer.PrintToString(person);

            actual.Should().Be(expected);
        }
        
        [Test]
        public void Use_ConfigSerialization_WithExtension()
        {
            var actual = person.PrintToString(config => config.Excluding(p => p.Age));

            actual.Should().NotContain(person.Age.ToString());
        }
    }
}