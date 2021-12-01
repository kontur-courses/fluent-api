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
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
                //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
            
            string result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        
        [Test]
        public void SerializationExcludeInt()
        {
            var person = new Person { Name = "Peter", Age = 38 };
            var printer = ObjectPrinter.For<Person>().ExcludedType<int>();
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Peter\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void SerializationExcludeString()
        {
            var person = new Person { Name = "John", Age = 59, Height = 175.5 };
            var printer = ObjectPrinter.For<Person>().ExcludedType<string>();
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = Guid\r\n\tHeight = 175.5\r\n\tAge = 59\r\n");
        }

        [Test]
        public void SerializationExcludeOneField()
        {
            var person = new Person { Name = "Rick", Age = 70 };
            var printer = ObjectPrinter.For<Person>().ExcludedField("Age");
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Rick\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void SerializationExcludeManyFields()
        {
            var person = new Person { Name = "Gregory", Age = 49 };
            var printer = ObjectPrinter.For<Person>()
                .ExcludedField("Id").ExcludedField("Name").ExcludedField("Age");
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void SerializationWithTypeModification()
        {
            var person = new Person { Name = "Nick", Height = 190.8, Age = 26 };

            var printer = ObjectPrinter.For<Person>()
                .SpecialSerializationType<int>(x => (100 - x).ToString())
                .SpecialSerializationType<double>(x => (-1*x).ToString(CultureInfo.InvariantCulture));
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 100\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Nick\r\n\tHeight = -190.8\r\n\tAge = 74\r\n");
        }

        [Test]
        public void SerializationWithFieldModification()
        {
            var person = new Person { Name = "Alex", Height = 17, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .SpecialSerializationField<double>("Height", x => (100 - x)
                    .ToString(CultureInfo.InvariantCulture))
                .SpecialSerializationField<int>("Age", x => (-1.5).ToString(CultureInfo.InvariantCulture))
                .SpecialSerializationField<Guid>("Id", x => "0");
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = 0\r\n\tName = Alex\r\n\tHeight = 83\r\n\tAge = -1.5\r\n");
        }

        
        [Test]
        public void Cyclic()
        {
            var person = new Person {Name = "Alex", Height = 160, Age = 15};
            var person1 = new Person {Name = "John", Father = person, Height = 190, Age = 39};
            person.Father = person1;
            var printer = ObjectPrinter.For<Person>();
            Action action = () => printer.PrintToString(person);
            action.Should().NotThrow<StackOverflowException>();
        }
        

        [Test]
        public void SerializationWithSpecialCulture()
        {
            var person = new Person { Name = "Scarlett", Height = 180.9, Age = 17 };
            var printer = ObjectPrinter.For<Person>().SetCulture(new CultureInfo("ru-RU"));
            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Scarlett\r\n\tHeight = 180,9\r\n\tAge = 17\r\n");
        }

        [Test]
        public void SerializationWithTrim()
        {
            var person = new Person { Name = "Alex", Height = 170, Age = 14 };
            var printer = ObjectPrinter.For<Person>().Trim(10, 80);
            var result = printer.PrintToString(person);
            result.Should().Be("d2 = 0\r\n\tFather = null\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 170\r\n\tAge ");
        }

        [Test]
        public void SerializationOfList()
        {
            var printer = ObjectPrinter.For<List<int>>();
            var dataList = new List<int>
            {
                1, 2, 3, 4
            };
            var result = printer.PrintToString(dataList);
            result.Should().Be("List`1\r\n\tCapacity = 4\r\n\tCount = 4\r\n\tItem[1] = Int32 index\r\n");
        }

        [Test]
        public void SerializationOfArray()
        {
            var printer = ObjectPrinter.For<Array>();
            var dataList = new [] {1,2,3,4};
            var result = printer.PrintToString(dataList);
            result.Should().Be("Int32[]\r\n\tLength = 4\r\n\tLongLength = 4\r\n\tRank = 1\r\n\t"+
                               "SyncRoot = this (parentObj)\r\n\tIsReadOnly = Boolean\r\n\t" +
                               "IsFixedSize = Boolean\r\n\tIsSynchronized = Boolean\r\n");
        }
    }
}