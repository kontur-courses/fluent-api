using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Alex", Age = 19, Relatives = new Person[] {new Person() {Name = "AS"}, new Person()}, Persons =
                    new Dictionary<Person, Person>()
                    {
                        {new Person(), new Person()},
                        {new Person(), new Person()}
                    },
                Height = 0.1
            };
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Name).Using(x => "wqw")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).SetMaxLength(1)
                //6. Исключить из сериализации конкретного свойства
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
        public void Excluding_Type_FromSerialization()
        {
            var serializable = person.PrintToString(config => config.Excluding<Guid>());
            serializable.Should().NotContain(nameof(person.Id));
        }

        [Test]
        public void Excluding_Property_FromSerialization()
        {
            var serializable = person.PrintToString(config => config.Excluding(x => x.Age));
            serializable.Should().NotContain(nameof(person.Age)).And.Contain($"{nameof(person.Field)} = {person.Field}");
        }

        [Test]
        public void Excluding_Field_FromSerialization()
        {
            var serializable = person.PrintToString(config => config.Excluding(x => x.Field));
            serializable.Should().NotContain(nameof(person.Field)).And.Contain($"{nameof(person.Age)} = {person.Age}");
        }

        [Test]
        public void ChangeSerialization_ForType()
        {
            var serializable = person.PrintToString(config => config.Printing<int>().Using(x => x + "q"));
            serializable.Should().Contain($"{nameof(person.Age)} = {person.Age}q").And.Contain($"{nameof(person.Field)} = {person.Field}q");
        }

        [Test]
        public void ChangeSerialization_ForProperty()
        {
            var serializable = person.PrintToString(config => config.Printing(x => x.Age).Using(x => x + "q"));
            serializable.Should().Contain($"{nameof(person.Age)} = {person.Age}q").And.Contain($"{nameof(person.Field)} = {person.Field}");
        }

        [Test]
        public void ChangeSerialization_ForField()
        {
            var serializable = person.PrintToString(config => config.Printing(x => x.Field).Using(x => x + "q"));
            serializable.Should().Contain($"{nameof(person.Age)} = {person.Age}").And.Contain($"{nameof(person.Field)} = {person.Field}q");
        }

        [Test]
        public void SetMaxLength_ForStringField()
        {
            var serializable = person.PrintToString(config => config.Printing(x => x.FamilyName).SetMaxLength(1));
            serializable.Should().Contain($"{nameof(person.FamilyName)} = {person.FamilyName[0]}")
                .And.NotContain($"{nameof(person.FamilyName)} = {person.FamilyName}")
                .And.Contain($"{nameof(person.Name)} = {person.Name}");
        }

        [Test]
        public void SetMaxLength_ForAllStrings()
        {
            var serializable = person.PrintToString(config => config.Printing<string>().SetMaxLength(1));
            serializable.Should().Contain($"{nameof(person.Name)} = {person.Name[0]}")
                .And.NotContain($"{nameof(person.Name)} = {person.Name}")
                .And.Contain($"{nameof(person.FamilyName)} = {person.FamilyName[0]}")
                .And.NotContain($"{nameof(person.FamilyName)} = {person.FamilyName}");
        }

        [Test]
        public void SetMaxLength_ForStringProperty()
        {
            var serializable = person.PrintToString(config => config.Printing(x => x.Name).SetMaxLength(1));
            serializable.Should().Contain($"{nameof(person.Name)} = {person.Name[0]}")
                .And.NotContain($"{nameof(person.Name)} = {person.Name}")
                .And.Contain($"{nameof(person.FamilyName)} = {person.FamilyName}");
        }
    }
}