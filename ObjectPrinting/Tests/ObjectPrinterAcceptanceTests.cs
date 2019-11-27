using System;
using System.Globalization;
using System.Collections.Generic;
using NUnit.Framework;
using FluentAssertions;
using System.Reflection;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2.Указать альтернативный способ сериализации для определенного типа
                .ChangePrintFor<string>().Using(s => s.Trim())
                //3. Для числовых типов указать культуру
                .ChangePrintFor<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .ChangePrintFor(p => p.Name).Using(value => value.ToUpper())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ChangePrintFor(p => p.Name).TrimToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);

            var result = printer.PrintToString(person);
            Console.Write(result);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию    
            //8. ...с конфигурированием
        }

        [Test]
        public void Excluding_Type()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            var result = printer.PrintToString(person);

            result.Should().Be(
                "Person" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = 19" +
                "\r\n\tSecondName = null" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_Type_Using_Function()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<int>().Using(s => (s * 100).ToString());

            var result = printer.PrintToString(person);

            result.Should().Be(
                "Person" +
                "\r\n\tId = 00000000-0000-0000-0000-000000000000" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = 1900" +
                "\r\n\tSecondName = null" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_Int_Using_CultureInfo()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<int>().Using(CultureInfo.CurrentCulture);

            var result = printer.PrintToString(person);

            result.Should().Be(
                "Person" +
                "\r\n\tId = 00000000-0000-0000-0000-000000000000" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = ru-RU" +
                "\r\n\tSecondName = null" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_Property_Using_Function()
        {
            var person = new Person { Name = "Alex", Age = 19 , SecondName = "Shmalex"};
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor(p => p.Name).Using(value => value.ToUpper());

            var result = printer.PrintToString(person);

            result.Should().Be(
                "Person" +
                "\r\n\tId = 00000000-0000-0000-0000-000000000000" +
                "\r\n\tName = ALEX" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = 19" +
                "\r\n\tSecondName = Shmalex" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_String_TrimToLength()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };
            var printer = ObjectPrinter.For<Person>()
                .ChangePrintFor<string>().TrimToLength(3);

            var result = printer.PrintToString(person);

            result.Should().Be(
                 "Person" +
                "\r\n\tId = 00000000-0000-0000-0000-000000000000" +
                "\r\n\tName = Ale" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = 19" +
                "\r\n\tSecondName = Shm" +
                "\r\n");
        }

        [Test]
        public void Excluding_Property()
        {
            var person = new Person { Name = "Alex", Age = 19, SecondName = "Shmalex" };
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.SecondName);

            var result = printer.PrintToString(person);

            result.Should().Be(
                "Person" +
                "\r\n\tId = 00000000-0000-0000-0000-000000000000" +
                "\r\n\tName = Alex" +
                "\r\n\tHeight = 0" +
                "\r\n\tAge = 19" +
                "\r\n");
        }

        [Test]
        public void PrintToString_List()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>();

            var result = printer.PrintToString(list);

            result.Should().Be(
                "List`1" +
                "\r\n\t1" +
                "\r\n\t2" +
                "\r\n\t3" +
                "\r\n");
        }

        [Test]
        public void PrintToString_Dictionary()
        {
            var list = new Dictionary<int, string>() {
                { 1 , "a" },
                { 2 , "b" },
                { 3 , "c" }};
            var printer = ObjectPrinter.For<Dictionary<int, string>>();

            var result = printer.PrintToString(list);

            result.Should().Be(
                "Dictionary`2" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 1" +
                "\r\n\t\tValue = a" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 2" +
                "\r\n\t\tValue = b" +
                "\r\n\tKeyValuePair`2" +
                "\r\n\t\tKey = 3" +
                "\r\n\t\tValue = c" +
                "\r\n");
        }

        [Test]
        public void SetMaxNumberItems()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>().SetMaxNumberListItems(2);

            var result = printer.PrintToString(list);

            result.Should().Be(
                "List`1" +
                "\r\n\t1" +
                "\r\n\t2" +
                "\r\n");
        }

        [Test]
        public void PrintToString_CircularLinks()
        {
            var person = new CyclicalPerson {
                Number = 1,
                NextPerson = new CyclicalPerson {
                    Number = 2,
                    NextPerson = new CyclicalPerson {
                        Number = 3
                    }
                }
            };
            person.NextPerson.NextPerson.NextPerson = person;
            var printer = ObjectPrinter.For<CyclicalPerson>().SetMaxNumberListItems(2);

            var result = printer.PrintToString(person);

            result.Should().Be(
                "CyclicalPerson" +
                "\r\n\tNextPerson = CyclicalPerson" +
                "\r\n\t\tNextPerson = CyclicalPerson" +
                "\r\n\t\t\tNextPerson = Is higher in the hierarchy by 2 steps" +
                "\r\n\t\t\tNumber = 3" +
                "\r\n\t\tNumber = 2" +
                "\r\n\tNumber = 1" +
                "\r\n");
        }

        [Test]
        public void PrintToString_CircularLinkArray()
        {
            var array = new object[1];
            array[0] = array;

            var printer = ObjectPrinter.For<object[]>();

            var result = printer.PrintToString(array);

            Console.WriteLine(result);

            result.Should().Be(
                "Object[]" +
                "\r\n\tIs higher in the hierarchy by 0 steps" +
                "\r\n");
        }

        [Test]
        public void PrintToString_ClassWithArrayOfClasses()
        {
            var person = new PersonWithArray {
                Number = 1,
                PeopleWithArray = new PersonWithArray[] {
                    new PersonWithArray {
                        Number = 2,
                        PeopleWithArray = null
                    }
                }
            };

            var printer = ObjectPrinter.For<PersonWithArray>().SetMaxNumberListItems(2);

            var result = printer.PrintToString(person);

            result.Should().Be(
                "PersonWithArray" +
                "\r\n\tNumber = 1" +
                "\r\n\tPeopleWithArray = PersonWithArray[]" +
                "\r\n\t\tPersonWithArray" +
                "\r\n\t\t\tNumber = 2" +
                "\r\n\t\t\tPeopleWithArray = null" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_List()
        {
            var list = new List<int>() { 1, 2, 3 };
            var printer = ObjectPrinter.For<List<int>>()
                .ChangePrintFor<List<int>>().Using(l => "ThisIsList");

            var result = printer.PrintToString(list);

            result.Should().Be(
                "List`1" +
                "\r\n\tThisIsList" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_ArrayInClass()
        {
            var personWithArray = new PersonWithArray { Number = 1, PeopleWithArray = new PersonWithArray[1] };
            var printer = ObjectPrinter.For<PersonWithArray>()
                .ChangePrintFor<PersonWithArray[]>().Using(l => "ThisIsList");

            var result = printer.PrintToString(personWithArray);

            result.Should().Be(
                "PersonWithArray" +
                "\r\n\tNumber = 1" +
                "\r\n\tPeopleWithArray = ThisIsList" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_ListOfList()
        {
            var list = new List<List<int>>() { new List<int>() { 1, 2 } };
            var printer = ObjectPrinter.For<List<List<int>>>()
                .ChangePrintFor<List<int>>().Using(l => "ThisIsList");

            var result = printer.PrintToString(list);

            result.Should().Be(
                "List`1" +
                "\r\n\tList`1" +
                "\r\n\t\tThisIsList" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_PropertyArray()
        {
            var person = new PersonWithArray
            {
                Number = 1,
                PeopleWithArray = new PersonWithArray[] {
                    new PersonWithArray {
                        Number = 2,
                        PeopleWithArray = null
                    }
                }
            };

            var printer = ObjectPrinter.For<PersonWithArray>().ChangePrintFor(p => p.PeopleWithArray).Using(l => "This is array");

            var result = printer.PrintToString(person);

            result.Should().Be(
                "PersonWithArray" +
                "\r\n\tNumber = 1" +
                "\r\n\tPeopleWithArray = This is array" +
                "\r\n");
        }

        [Test]
        public void ChangePrintFor_PropertyList()
        {
            var person = new PersonWithList
            {
                Number = 1,
                PeopleWithList = new List<PersonWithList> {
                    new PersonWithList {
                        Number = 2,
                        PeopleWithList = null
                    }
                }
            };

            var printer = ObjectPrinter.For<PersonWithList>().ChangePrintFor(p => p.PeopleWithList).Using(l => "This is list");

            var result = printer.PrintToString(person);

            result.Should().Be(
                "PersonWithList" +
                "\r\n\tNumber = 1" +
                "\r\n\tPeopleWithList = This is list" +
                "\r\n");
        }

        [Test]
        public void Immutable_Using()
        {
            var person = new PersonWithList
            {
                Number = 1,
                PeopleWithList = null
            };

            var printerConfig = ObjectPrinter.For<PersonWithList>().ChangePrintFor(p => p.Number);
            var printerWithCastom = printerConfig.Using(l => (l * 100).ToString());
            var printerWithCultureInfo = printerConfig.Using(CultureInfo.CurrentCulture);

            var resultWithCastom = printerWithCastom.PrintToString(person);
            var resultWithCultureInfo = printerWithCultureInfo.PrintToString(person);

            resultWithCastom.Should().Be(
                "PersonWithList" +
                "\r\n\tNumber = 100" +
                "\r\n\tPeopleWithList = null" +
                "\r\n");

            resultWithCultureInfo.Should().Be(
                "PersonWithList" +
                "\r\n\tNumber = ru-RU" +
                "\r\n\tPeopleWithList = null" +
                "\r\n");
        }

        [Test]
        public void Immutable_ChangePrintFor()
        {
            var person = new PersonWithList
            {
                Number = 1,
                PeopleWithList = null
            };

            var printerConfig = ObjectPrinter.For<PersonWithList>();
            var printerForNumber = printerConfig.ChangePrintFor(p => p.Number).Using(n => (n * 100).ToString());
            var printerForList = printerConfig.ChangePrintFor(p => p.PeopleWithList).Using(l => "This is list");

            var resultWithCastom = printerForNumber.PrintToString(person);
            var resultWithCultureInfo = printerForList.PrintToString(person);

            resultWithCastom.Should().Be(
                "PersonWithList" +
                "\r\n\tNumber = 100" +
                "\r\n\tPeopleWithList = null" +
                "\r\n");

            resultWithCultureInfo.Should().Be(
                "PersonWithList" +
                "\r\n\tNumber = 1" +
                "\r\n\tPeopleWithList = This is list" +
                "\r\n");
        }
    }
}