using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.HomeWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;

namespace ObjectPrintingTests.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private const string ZeroGuid = "00000000-0000-0000-0000-000000000000";
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

            result.Should().Be($"Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n");

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }


        [Test]
        public void SerializationExcludeInt()
        {
            var person = new Person { Name = "Peter", Age = 38 };
            //var printer = ObjectPrinter.For<Person>().ExcludedType(x => typeof(int));
            var printer = ObjectPrinter.For<Person>().ExcludedType<int>();
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = Peter\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void SerializationExcludeString()
        {
            var person = new Person { Name = "John", Age = 59, Height = 175.5 };
            var printer = ObjectPrinter.For<Person>().ExcludedType<string>();
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tHeight = 175.5\r\n\tAge = 59\r\n");
        }

        [Test]
        public void SerializationExcludeOneField()
        {
            var person = new Person { Name = "Rick", Age = 70 };
            var printer = ObjectPrinter.For<Person>().ExcludedProperty(pr => pr.Age);
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = Rick\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void SerializationExcludeManyFieldsWithIncorrectExpression()
        {
            //var person = new Person { Name = "Gregory", Age = 49 };
            Action action = () => ObjectPrinter.For<Person>()
                 .ExcludedProperty(pr => pr.Name + new[] { 3, 4 }).ExcludedProperty(pr => pr.Age + 1).ExcludedProperty(pr => pr.Id);
            action.Should().Throw<InvalidExpressionException>();
        }

        [Test]
        public void SerializationExcludeManyFields()
        {
            var person = new Person { Name = "Gregory", Age = 49 };
            var printer = ObjectPrinter.For<Person>()
                .ExcludedProperty(pr => pr.Age)
                .ExcludedProperty(pr => pr.Father)
                .ExcludedProperty(pr => pr.Id);

            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tName = Gregory\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void TrimType()
        {
            var person = new Person { Name = "John", Age = 21, Height = 175.5, Id2 = 7};
            var printer = ObjectPrinter.For<Person>()
                .TrimType<string>(new Borders(1,2));
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tId2 = 7\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = oh\r\n\tHeight = 175.5\r\n\tAge = 21\r\n");
        }

        [Test]
        public void TrimMember()
        {
            var person = new Person { Name = "John", Age = 21, Height = 175.5};
            var printer = ObjectPrinter.For<Person>()
                .TrimProperty(per => per.Age, new Borders(1, 1));
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = John\r\n\tHeight = 175.5\r\n\tAge = 1\r\n");
        }

        [Test]
        public void SerializationWithTypeModification()
        {
            var person = new Person { Name = "Nick", Height = 190.8, Age = 26 };

            var printer = ObjectPrinter.For<Person>()
                .SpecialSerializationType<int>(value => (100 - value).ToString())
                .SpecialSerializationType<double>(value => (-1 * value).ToString(CultureInfo.InvariantCulture))
                .SpecialSerializationType<Guid>(value => "2");
            var result = printer.PrintToString(person);
            result.Should().Be($"Person\r\n\tId2 = 100\r\n\tFather = null\r\n\tId = 2\r\n\tName = Nick\r\n\tHeight = -190.8\r\n\tAge = 74\r\n");
        }

        [Test]
        public void SerializationWithFieldModification()
        {
            var person = new Person { Name = "Alex", Height = 17, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .SpecialSerializationField(pr => pr.Name, x => $"{x} - {x} = 0")
                .SpecialSerializationField(pr => pr.Height, x => (100 - x).ToString(CultureInfo.InvariantCulture))
                .SpecialSerializationField(pr => pr.Age, x => (-1.5).ToString(CultureInfo.InvariantCulture))
                .SpecialSerializationField(pr => pr.Id, x => "1");

            var result = printer.PrintToString(person);
            result.Should().Be("Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = 1\r\n\tName = Alex - Alex = 0\r\n\tHeight = 83\r\n\tAge = -1.5\r\n");
        }


        [Test]
        public void SerializationWithIncorrectExpression()
        {
            Action action = () => ObjectPrinter.For<Person>()
                .SpecialSerializationField(pr => pr.Height + 1, x => (100 - x).ToString(CultureInfo.InvariantCulture));

            action.Should().Throw<InvalidExpressionException>();
        }

        [Test]
        public void Cyclic()
        {
            var person = new Person { Name = "Alex", Height = 160, Age = 15 };
            var person1 = new Person { Name = "John", Father = person, Height = 190, Age = 39 };
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
            result.Should().Be($"Person\r\n\tId2 = 0\r\n\tFather = null\r\n\tId = {ZeroGuid}\r\n\tName = Scarlett\r\n\tHeight = 180,9\r\n\tAge = 17\r\n");
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
            result.Should().Be("List`1\r\n\tCapacity = 4\r\n\tCount = 4\r\n\tItems =\r\n\t\t" +
                               "1\r\n\t\t2\r\n\t\t3\r\n\t\t" + "4\r\n");
        }

        [Test]
        public void SerializationOfComplexList()
        {
            var printer = ObjectPrinter.For<List<Tuple<int, Point>>>();
            var dataList = new List<Tuple<int, Point>>
            {
                Tuple.Create(0, new Point(0,1)),
                Tuple.Create(1, new Point(1,2)),
                Tuple.Create(2, new Point(2,3)),
                Tuple.Create(3, new Point(3,4)),
            };
            var result = printer.PrintToString(dataList);
            result.Should().Be("List`1\r\n\tCapacity = 4\r\n\tCount = 4\r\n\tItems =\r\n\t\t" +
                               "Tuple`2\r\n\t\t\tItem1 = 0\r\n\t\t\tItem2 = Point\r\n\t\t\t\t" + 
                               "IsEmpty = False\r\n\t\t\t\tX = 0\r\n\t\t\t\tY = 1\r\n\t\t" +
                               "Tuple`2\r\n\t\t\tItem1 = 1\r\n\t\t\tItem2 = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\tX = 1\r\n\t\t\t\tY = 2\r\n\t\t" +
                               "Tuple`2\r\n\t\t\tItem1 = 2\r\n\t\t\tItem2 = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\tX = 2\r\n\t\t\t\tY = 3\r\n\t\t" +
                               "Tuple`2\r\n\t\t\tItem1 = 3\r\n\t\t\tItem2 = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\tX = 3\r\n\t\t\t\tY = 4\r\n");

        }

        [Test]
        public void SerializationOfArray()
        {
            var printer = ObjectPrinter.For<Array>();
            var dataList = new[] { 1, 2, 3, 4 };
            var result = printer.PrintToString(dataList);
            result.Should().Be("Int32[]\r\n\tLength = 4\r\n\tLongLength = 4\r\n\tRank = 1\r\n\t" +
                               "SyncRoot = this (parentObj)\r\n\tIsReadOnly = False\r\n\t" +
                               "IsFixedSize = True\r\n\tIsSynchronized = False\r\n");
        }

        [Test]
        public void SerializationOfDictionary()
        {
            var printer = ObjectPrinter.For<Dictionary<string, int>>();
            var data = new Dictionary<string, int>
            {
                {"Париж", -53},
                {"Берлин", 1237},
                {"Лондон", 47}
            };
            var result = printer.PrintToString(data);

            result.Should().Be("Dictionary`2\r\n\t" + "Comparer = " +
                               "GenericEqualityComparer`1\r\n\t" +
                               "Count = 3\r\n\t" + "Keys = KeyCollection\r\n\t\t" +
                               "Count = 3\r\n\t" + "Values = ValueCollection\r\n\t\t" +
                               "Count = 3\r\n\t" + "Items =\r\n\t\t" +
                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Париж\r\n\t\t\t" +
                               "Value = -53\r\n\t\t" +
                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Берлин\r\n\t\t\t" +
                               "Value = 1237\r\n\t\t" +
                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Лондон\r\n\t\t\t" +
                               "Value = 47\r\n");
        }

        [Test]
        public void SerializationOfComplexDictionary()
        {
            var printer = ObjectPrinter.For<Dictionary<string, Point>>();
            var data = new Dictionary<string, Point>
            {
                {"Точка", new Point(-53, 53)},
                {"Точка2", new Point(-1237, 1237)},
                {"Точка3", new Point(-47, 47)}
            };
            var result = printer.PrintToString(data);
            result.Should().Be("Dictionary`2\r\n\t" + "Comparer = " +
                               "GenericEqualityComparer`1\r\n\t" +
                               "Count = 3\r\n\t" + "Keys = KeyCollection\r\n\t\t" +
                               "Count = 3\r\n\t" + "Values = ValueCollection\r\n\t\t" +
                               "Count = 3\r\n\t" + "Items =\r\n\t\t" +

                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Точка\r\n\t\t\t" +
                               "Value = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\t" + 
                               "X = -53\r\n\t\t\t\t" + 
                               "Y = 53\r\n\t\t" +
                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Точка2\r\n\t\t\t" +
                               "Value = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\t" +
                               "X = -1237\r\n\t\t\t\t" +
                               "Y = 1237\r\n\t\t" +
                               "KeyValuePair`2\r\n\t\t\t" +
                               "Key = Точка3\r\n\t\t\t" +
                               "Value = Point\r\n\t\t\t\t" +
                               "IsEmpty = False\r\n\t\t\t\t" +
                               "X = -47\r\n\t\t\t\t" +
                               "Y = 47\r\n");
        }
    }
}