using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Extensions;
using FluentAssertions;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void Print_To_String()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 2.3, Surname = "VERYBIGSURNAME"};
            person.Dict = new Dictionary<int, Person> {{1, new Person()}};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Parent)
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(_ => "big boy")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing<string>().Using(p => p.Name, x => x + " (nice name, bro)")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimToLength(p => p.Surname, 10)
                .ThrowIfCyclicReferences();
                
            
            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            Console.WriteLine(new Person {Name = "lol", Surname = "no lol"}.PrintToString());
            //8. ...с конфигурированием
            Console.WriteLine(new Person().PrintToString(ObjectPrinter
                .For<Person>()
                .Exclude<Guid>()
                .Exclude(p => p.Parent)));
            
            Console.WriteLine(s1);
        }

        [Test]
        public void ExcludeType()
        {
            var person = new Person();

            var printer = ObjectPrinter.For<Person>().Exclude<string>();

            var result = printer.PrintToString(person);

            result.Should().NotContain("Name");
            result.Should().NotContain("Surname");
        }

        [Test]
        public void ExcludeMember()
        {
            var person = new Person();

            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Name);

            var result = printer.PrintToString(person);

            result.Should().NotContain("Name");
            result.Should().Contain("Surname");
        }

        [Test]
        public void UseAlternativeSerializer_ForType()
        {
            var person = new Person();

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(n => "a");

            var result = printer.PrintToString(person);

            result.Should().Contain("Age = a");
        }

        [Test]
        public void UseAlternativeSerializer_ForMember()
        {
            var person = new Person {Name = "Alex"};

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .Using(p => p.Name, n => n + " (nice name, bro)");

            var result = printer.PrintToString(person);

            result.Should().Contain("Name = Alex (nice name, bro)");
            result.Should().NotContain("Surname = null (nice name, bro)");
        }

        [Test]
        public void UseAlternativeCulture_ForFormattable()
        {
            var person = new Person {Height = 2.1};

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture);

            var result = printer.PrintToString(person);

            result.Should().Contain("2.1");
        }

        [Test]
        public void TrimAllStringFieldsAndProperties()
        {
            var person = new Person {Name = "Abcdef", Surname = "Abcdef"};

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .TrimToLength(3);

            var result = printer.PrintToString(person);

            result.Should().NotContain("Abcdef");
            result.Should().Contain("Name = Abc");
            result.Should().Contain("Surname = Abc");
        }
        
        [Test]
        public void TrimSpecificStringMember()
        {
            var person = new Person {Name = "Abcdef", Surname = "Abcdef"};

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .TrimToLength(p => p.Name, 3);

            var result = printer.PrintToString(person);
            
            result.Should().Contain("Name = Abc");
            result.Should().Contain("Surname = Abcdef");
        }

        [Test]
        public void ThrowsOnCyclicReferenceWhenNeeded()
        {
            var person = new Person();

            person.Parent = person;

            var printer = ObjectPrinter.For<Person>().ThrowIfCyclicReferences();

            Action act = () => printer.PrintToString(person);

            act.Should().Throw<Exception>();
        }
        
        [Test]
        public void DoesntThrowsOnCyclicReference()
        {
            var person = new Person();

            person.Parent = person;

            var printer = ObjectPrinter.For<Person>();

            var result = printer.PrintToString(person);

            result.Should().Contain("Cyclic reference on Person");
            Console.WriteLine(result);
        }
    }
}