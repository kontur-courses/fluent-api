using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printer;
        private Person person;
        
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = Person.CreatePerson();
        }

        [Test]
        public void Can_ExcludeSpecificType()
        {
            var serialized = printer.Excluding<Guid>().PrintToString(person);
            serialized.Should().NotContain("Guid");
            Console.WriteLine(serialized);
        }

        [Test]
        public void Can_ExcludeSpecificPropertyOrField()
        {
            var serialized = printer.Excluding(p => p.Age).Excluding(p => p.BoolField).PrintToString(person);
            serialized.Should().NotContain("Age = ");
            serialized.Should().NotContain("BoolField = ");
            Console.WriteLine(serialized);
        }
        

        [Test]
        public void Can_SpecifyAlternativeSterilizationForSpecificType()
        {
            var serialized = printer.Printing<int>().Using(i => $"IntNum {i}").PrintToString(person);
            serialized.Should().Contain("IntNum");
            Console.WriteLine(serialized);
        }

        [Test]
        public void Can_TrimStringPropertiesOrFields()
        {
            var serialized = printer.Printing(p => p.Name).TrimmedToLength(1)
                .PrintToString(person);
            Regex.IsMatch(serialized, $@"Name = \w{Environment.NewLine}").Should().Be(true);
            Console.WriteLine(serialized);
        }

        [Test]
        public void Can_SpecifyCultureForNumberTypes()
        {
            var serialized = printer.Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            Regex.IsMatch(serialized, @"\d+\.\d+").Should().Be(true);
            Console.WriteLine(serialized);
        }

        [Test]
        public void Can_SpecifyAlternativeSterilizationForSpecificPropertyOrField()
        {
            var serialized = printer
                .Printing(p => p.Height).Using(h => "ЫЫЫ")
                .Printing(p => p.BoolField).Using(b => "SSS")
                .PrintToString(person);
            serialized.Should().Contain("Height = ЫЫЫ");
            serialized.Should().Contain("BoolField = SSS");
            Console.WriteLine(serialized);
        }

        [Test]
        public void Has_ExtensionMethodForDefaultSterilization()
        {
            var serialized1 = person.PrintToString();
            var serialized2 = printer.PrintToString(person);
            serialized1.Should().Be(serialized2);
            Console.WriteLine(serialized1);
        }

        [Test]
        public void Has_ExtensionMethodForSterilizationWithConfiguration()
        {
            var serialized1 = person.PrintToString(s => s.Excluding(p => p.Age));
            
            printer.Excluding(p => p.Age);
            var serialized2 = printer.PrintToString(person);
            
            serialized1.Should().Be(serialized2);
            Console.WriteLine(serialized1);
        }

        [Test]
        public void ShouldSerializeDifferently_WithTwoPropsSameType()
        {
            printer
                .Printing(r => r.Height).Using(h => $"{h} cm")
                .Printing(r => r.Weight).Using(w => $"{w} kg");
            var serialized = printer.PrintToString(person);
            serialized.Should().Contain("cm");
            serialized.Should().Contain("kg");
            Console.WriteLine(serialized);
        }

        [Test]
        public void MemberSerialization_ShouldOverlaps_TypeSerialization()
        {
            printer
                .Printing(p => p.Age).Using(age => $"{age} y.o.")
                .Printing<int>().Using(d => $"IntNum {d}");
            var serialized = printer.PrintToString(person);
            serialized.Should().Contain("y.o.");
            Console.WriteLine(serialized);
        }

        [Test]
        public void LastUsingIsDecisive_WhenPrintingSameType()
        {
            printer
                .Printing<double>().Using(d => $"NUM {d}")
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var serialized = printer.PrintToString(person);
            serialized.Should().NotContain("NUM");
            Console.WriteLine(serialized);
        }

        [Test]
        public void ShouldHandle_CyclicReferences()
        {
            person = Person.CreatePerson();
            person.Friend = person;
            
            Action action = () =>  Console.Write(printer.PrintToString(person));
            action.ShouldNotThrow<StackOverflowException>();
        }

        [Test]
        public void ShouldHandle_Collections()
        {
            person = Person.CreatePerson();
            person.Numbers = new[] {1.1, 2.2, 3.3};
            person.Dict = new Dictionary<string, double>()
            {
                {"a", 1.1},
                {"b", 2.0}
            };
            
            var serialized = printer.PrintToString(person);
            serialized.Should().Contain("Dictionary");
            serialized.Should().Contain("[]");
            Console.WriteLine(serialized);
        }
        
        public void Demo()
        {
            printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => $"Number! {i}")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(h => $"{h} cm")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(5) // = i => i.Substring(0, maxLen)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);
            var s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(p => p.Excluding<Guid>());
            
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}