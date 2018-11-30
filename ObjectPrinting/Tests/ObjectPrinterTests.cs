using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person = new Person();

        [SetUp]
        public void SetUp()
        {
            person.Age = 20;
            person.Name = "Ilya";
            person.Height = 170.5;
        }

        [Test]
        public void Excluding_ExcludesStringType()
        {
            //1. Исключить из сериализации свойства определенного типа

            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            printer.PrintToString(person).Split().Should().NotContain(nameof(person.Name));
        }

        [Test]
        public void Using_SetsAlternativePrintingFunction()
        {
            //2. Указать альтернативный способ сериализации для определенного типа

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => "XXX");
            var result = printer.PrintToString(person);
            result.Split().Should().Contain("XXX");
        }

        [Test]
        public void Using_SetsCulture_ToNumberTypes()
        {
            //3. Для числовых типов указать культуру
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var result = printer.PrintToString(person);
            result.Split().Should().Contain((person.Height.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public void Using_SetsAlternativePrinting_ToProperties()
        {
            //4. Настроить сериализацию конкретного свойства
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Height).Using(x => (x * 2).ToString());
            var result = printer.PrintToString(person);
            result.Split().Should().Contain((person.Height * 2).ToString());
        }

        [Test]
        public void TrimmedToLength_Trims_StringPropertyValue()
        {
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            var length = 2;
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(length);
            var result = printer.PrintToString(person);
            result.Split().Should().Contain(person.Name.Substring(0, length));
        }

        [Test]
        public void Excluding_ExcludesProperties()
        {
            //6. Исключить из сериализации конкретное свойство
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);
            var result = printer.PrintToString(person);
            result.Split().Should().NotContain(person.Name);
        }


        [Test]
        public void Print_ResolvesCyclicReferences()
        {
            var root = new TreeNode(0);
            root.LeftNode = new TreeNode(1);
            root.RightNode = new TreeNode(2);
            root.RightNode.LeftNode = root;
            var printer = ObjectPrinter.For<TreeNode>();
            var result = printer.PrintToString(root);
        }


        [TestCaseSource(nameof(IenumerablesOfInt))]
        public void Print_PrintsCollectionInside_IenumerableOfInt(IEnumerable collection)
        {
            var printer = ObjectPrinter.For<IEnumerable>();
            var result = printer.PrintToString(collection);
            result.Split('\r', '\n', '\t').Should().Contain("1, 2, 3");
        }

        private static object[] IenumerablesOfInt =
        {
            new object[] {new List<int> {1, 2, 3}},
            new object[] {new HashSet<int> {1, 2, 3}},
            new object[] {new int[] {1, 2, 3}},
        };

        [TestCaseSource(nameof(IenumerablesOfString))]
        public void Print_PrintsCollectionInside_IenumerableOfString(IEnumerable collection)
        {
            var printer = ObjectPrinter.For<IEnumerable>();
            var result = printer.PrintToString(collection);
            result.Split('\r', '\n', '\t').Should().Contain("a, b, c");
        }

        private static object[] IenumerablesOfString =
        {
            new object[] {new List<string> {"a", "b", "c"}},
            new object[] {new HashSet<string> {"a", "b", "c"}},
            new object[] {new String[] {"a", "b", "c"}},
        };
    }
}