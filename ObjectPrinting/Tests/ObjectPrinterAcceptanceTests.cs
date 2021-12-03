﻿using NUnit.Framework;
using System.Globalization;
using FluentAssertions;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<string>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).CropToLength(1)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude_OnPerson_ShouldExclude()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude<string>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForType_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 190\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => (((int)x)*10).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 20\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(x => (((int)x) + 1).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForStringProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).CropToLength(2);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludingProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void AllPossibleParameters_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Ale\r\n\tHeight = 100,10223\r\n";
            var person = new Person { Name = "Alex", Age = 10, Height = 100.10223 };
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude(p => p.Id)
                .Printing(p => p.Name)
                .CropToLength(3)
                .Printing<Guid>()
                .Using(x => x.ToString() + " нули хехе")
                .Printing<double>()
                .Using(CultureInfo.InstalledUICulture)
                .Printing<int>()
                .Using(x => ((int)x + 5).ToString())
                .Exclude<int>();

            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void Double_OnRussianCulture_ShouldBeRussian()
        {
            var num = 1.2d;
            var printer = ObjectPrinter.For<double>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("ru-ru"));

            printer.PrintToString(num).Should().Be("1,2\r\n");
        }

        [Test]
        public void Double_OnAmericanCulture_ShouldBeAmerican()
        {
            var num = 1.2d;
            var printer = ObjectPrinter.For<double>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("en-us"));

            printer.PrintToString(num).Should().Be("1.2\r\n");
        }

        [Test]
        public void PrintToString_OnNodeWithNoOtherNodesAndNoConfigs_ShouldWorkCorrect()
        {
            var node = new Node();

            var printer = ObjectPrinter.For<Node>();
            var actual = printer.PrintToString(node);
            actual.Should().Be("Node\r\n\tOtherNode = null\r\n\tValue = 0\r\n");
        }

        [Test]
        public void PrintToString_OnObjWithCyclicReference_ShouldHandleIt()
        {
            var node = new Node(2);
            node.AddNode(new Node(3));

            var printer = ObjectPrinter.For<Node>();
            var actual = printer.PrintToString(node);
            actual.Should().Be("Node\r\n\tOtherNode = Node\r\n\t\tOtherNode = Cyclic reference found with Node\r\n\t\tValue = 3\r\n\tValue = 2\r\n");
        }

        [Test]
        public void PrintToString_OnObjWithList_ShouldWorkCorrect()
        {
            var obj = new ClassWithList();
            obj.Values.Add(100);
            obj.Values.Add(200);
            obj.Values.Add(300);

            var expected = "ClassWithList\r\n\tValues = List`1\r\n\t\tCapacity = 4\r\n\t\tElements:\r\n\t\t\t100\r\n\t\t\t200\r\n\t\t\t300\r\n\t\tCount = 3\r\n\tValue = 0\r\n";
            var printer = ObjectPrinter.For<ClassWithList>();
            var actual = printer.PrintToString(obj);
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_TypeAndPropertyConflictInClass_ShouldGoForProperty()
        {
            var person = new Person() { Age = 1, Name = "Alex"};

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(x => (1 + ((int)x)).ToString())
                .Printing(p => p.Age)
                .Using(x => (2 + ((int)x)).ToString())
                .Exclude(p => p.Height)
                .Exclude(p => p.Id)
                .Exclude(p => p.Name);

            var exp = "Person\r\n\tAge = 3\r\n";

            printer.PrintToString(person).Should().Be(exp);
        }

        [Test]
        public void PrintToString_ClassWithDictionary_ShouldWork()
        {
            var dictClass = new ClassWithDict();
            dictClass.Dict.Add("a", "b");
            dictClass.Dict.Add("b", "c");

            var expected = "ClassWithDict\r\n\tDict = Dictionary`2\r\n\t\tCount = 2\r\n\t\tElements:\r\n\t\t\tKeyValuePair`2\r\n\t\t\t\tKey = a\r\n\t\t\t\tValue = b\r\n\t\t\tKeyValuePair`2\r\n\t\t\t\tKey = b\r\n\t\t\t\tValue = c\r\n";
            var actual = ObjectPrinter.For<ClassWithDict>()
                .Exclude(x => x.Dict.Comparer)
                .Exclude(x => x.Dict.Keys)
                .Exclude(x => x.Dict.Values)
                .PrintToString(dictClass);
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ClassWithArray_ShouldWork()
        {
            var arrayClass = new ClassWithArray();

            var expected = "ClassWithArray\r\n\tarray = Int32[]\r\n\t\tLength = 3\r\n\t\tElements:\r\n\t\t\t1\r\n\t\t\t2\r\n\t\t\t3\r\n\t\tLongLength = Int64\r\n\t\tRank = 1\r\n\t\tSyncRoot = Cyclic reference found with Int32[]\r\n\t\tIsReadOnly = Boolean\r\n\t\tIsFixedSize = Boolean\r\n\t\tIsSynchronized = Cyclic reference found with Boolean\r\n";
            var actual = ObjectPrinter.For<ClassWithArray>().PrintToString(arrayClass);
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ClassWithArrayInArray_ShouldWork()
        {
            var arrayClass = new ClassWithArrayInArray();
            arrayClass.array = new object[2];
            arrayClass.array[0] = new int[2];
            arrayClass.array[1] = new double[2];
            var inner1 = (int[])arrayClass.array[0];
            inner1[0] = 0;
            inner1[1] = 1;
            var inner2 = (double[])arrayClass.array[1];
            inner2[0] = 2.0d;
            inner2[1] = 3.0d;

            var expected = "ClassWithArrayInArray\r\n\tarray = Object[]\r\n\t\tLength = 2\r\n\t\tElements:\r\n\t\t\tInt32[]\r\n\t\t\t\tLength = 2\r\n\t\t\t\tElements:\r\n\t\t\t\t\t0\r\n\t\t\t\t\t1\r\n\t\t\t\tLongLength = Int64\r\n\t\t\t\tRank = 1\r\n\t\t\t\tSyncRoot = Cyclic reference found with Int32[]\r\n\t\t\t\tIsReadOnly = Boolean\r\n\t\t\t\tIsFixedSize = Boolean\r\n\t\t\t\tIsSynchronized = Cyclic reference found with Boolean\r\n\t\t\tDouble[]\r\n\t\t\t\tLength = 2\r\n\t\t\t\tElements:\r\n\t\t\t\t\t2\r\n\t\t\t\t\t3\r\n\t\t\t\tLongLength = Cyclic reference found with Int64\r\n\t\t\t\tRank = 1\r\n\t\t\t\tSyncRoot = Cyclic reference found with Double[]\r\n\t\t\t\tIsReadOnly = Cyclic reference found with Boolean\r\n\t\t\t\tIsFixedSize = Cyclic reference found with Boolean\r\n\t\t\t\tIsSynchronized = Cyclic reference found with Boolean\r\n\t\tLongLength = Cyclic reference found with Int64\r\n\t\tRank = 1\r\n\t\tSyncRoot = Cyclic reference found with Object[]\r\n\t\tIsReadOnly = Cyclic reference found with Boolean\r\n\t\tIsFixedSize = Cyclic reference found with Boolean\r\n\t\tIsSynchronized = Cyclic reference found with Boolean\r\n";
            var actual = ObjectPrinter.For<ClassWithArrayInArray>().PrintToString(arrayClass);
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ClassICollectionWithFieldsInList_ShouldWork()
        {
            var list = new List<MyCollection<int>>();
            list.Add(new MyCollection<int>());
            list[0].Add(1);
            list[0].Add(2);
            var exp = "List`1\r\n\tCapacity = 4\r\n\tElements:\r\n\t\tMyCollection`1\r\n\t\t\tCount = 42\r\n\t\t\tElements:\r\n\t\t\t\t1\r\n\t\t\t\t2\r\n\t\t\tIsReadOnly = Boolean\r\n\t\t\tIsSynchronized = Boolean\r\n\t\t\tSyncRoot = null\r\n\tCount = 1\r\n";
            var actual = ObjectPrinter.For<List<MyCollection<int>>>().PrintToString(list);
            actual.Should().Be(exp);
        }

        [Test]
        public void PrintToString_ClassICollectionWithFields_ShouldWork()
        {
            var list = new MyCollection<int>();
            list.Add(1);
            list.Add(2);

            var exp = "MyCollection`1\r\n\tCount = 42\r\n\tElements:\r\n\t\t1\r\n\t\t2\r\n\tIsReadOnly = Boolean\r\n\tIsSynchronized = Boolean\r\n\tSyncRoot = null\r\n";
            var actual = ObjectPrinter.For<MyCollection<int>>().PrintToString(list);
            actual.Should().Be(exp);
        }
    }
}