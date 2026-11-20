using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved.Tests.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void PrintToString_Default_ShouldPrintAllPropertiesValues()
        {
            var p = new Person { Name = "Alex", Age = 20 };

            var s = p.PrintToString();

            s.Should().Contain("Alex").And.Contain("20");
        }

        [Test]
        public void PrintToString_ExcludingType_ShouldExcludeAllSameType()
        {
            var p = new Person { Name = "Alex", Height = 2, Age = 20 };

            var s = ObjectPrinter.For<Person>().Excluding<int>().PrintToString(p);

            Console.WriteLine(s);
            s.Should().NotContain("Age").And.NotContain("= 20");
            s.Should().Contain("Alex").And.Contain("2");
        }

        [Test]
        public void PrintToString_PrintingType_ShouldUseCustomSerializer()
        {
            var p = new Person { Age = 20 };

            var s = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "Число: " + x)
                .PrintToString(p);

            s.Should().Contain("Число: 20");
        }

        [Test]
        public void PrintToString_PrintingTypeWithInvariantCulture_ShouldUseDotSeparator()
        {
            var p = new Person { Height = 1.75 };

            var s = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(p);

            s.Should().Contain("1.75").And.NotContain("1,75");
        }

        [Test]
        public void PrintToString_PrintingTypeWithRuCulture_ShouldUseCommaSeparator()
        {
            var p = new Person { Height = 1.75 };

            var s = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru-RU"))
                .PrintToString(p);

            s.Should().Contain("1,75");
        }

        [Test]
        public void PrintToString_PrintingProperty_ShouldUseCustomSerializer()
        {
            var p = new Person { Age = 20 };

            var s = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(x => $"Возраст: {x}")
                .PrintToString(p);

            s.Should().Contain("Возраст: 20");
        }

        [Test]
        public void PrintToString_PropertyAndTypeSerializers_ShouldPreferProperty()
        {
            var p = new Person { Age = 20 };

            var s = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "type: " + x)
                .Printing(p => p.Age).Using(x => "property: " + x)
                .PrintToString(p);

            s.Should().Contain("prorerty: 20").And.NotContain("type: 20");
        }

        [Test]
        public void PrintToString_TrimmedToLength_ShouldTrimString()
        {
            var p = new Person { Name = "Alisher" };

            var s = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(4)
                .PrintToString(p);

            s.Should().Contain("Alis").And.NotContain("Alisher");
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldExcludeThatProperty()
        {
            var p = new Person { Name = "Alex", Age = 20 };

            var s = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .PrintToString(p);

            s.Should().NotContain("Age").And.NotContain("= 20").And.Contain("Alex");
        }

        [Test]
        public void PrintToString_PropertyExcludedAndConfigured_ShouldPreferExclusion()
        {
            var p = new Person { Age = 20 };

            var s = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(x => "Возраст: " + x)
                .Excluding(p => p.Age)
                .PrintToString(p);

            s.Should().NotContain("Age").And.NotContain("Возраст: ");
        }

        [Test]
        public void PrintToString_SelfReference_ShouldMarkAsCycle()
        {
            var n = new Node { Name = "X" };
            n.Parent = n;

            var s = ObjectPrinter.For<Node>().PrintToString(n);

            s.Should().Contain("is cycle");
        }

        [Test]
        public void PrintToString_TwoNodesCycle_ShouldNotThrow()
        {
            var a = new Node { Name = "A" };
            var b = new Node { Name = "B", Parent = a };
            a.Children.Add(b);

            Action act = () => ObjectPrinter.For<Node>().PrintToString(a);

            act.Should().NotThrow();
        }

        [Test]
        public void PrintToString_SharedObjectInTwoBranches_ShouldNotMarkAsCycle()
        {
            var shared = new Node { Name = "S" };
            var root = new Node { Name = "Root" };
            root.Children.Add(shared);
            root.Children.Add(shared);

            var s = ObjectPrinter.For<Node>().PrintToString(root);

            s.Should().NotContain("is cycle").And.Contain("S");
        }

        [Test]
        public void PrintToString_NullProperty_ShouldPrintNull()
        {
            var p = new Person { Name = null, Age = 20 };

            var s = ObjectPrinter.For<Person>().PrintToString(p);

            s.Should().Contain("is null").And.Contain("= 20");
        }

        [Test]
        public void PrintToString_NestedObject_ShouldBePrintedRecursively()
        {
            var parent = new Node { Name = "Parent" };
            var child = new Node { Name = "Child", Parent = parent };

            var s = ObjectPrinter.For<Node>().PrintToString(child);

            s.Should().Contain("Child").And.Contain("Parent");
        }

        [Test]
        public void PrintToString_ArrayListAndDictionary_ShouldPrintAllElements()
        {
            var h = new CollectionsHolder
            {
                Numbers = new[] { 1, 2 },
                Strings = new List<string> { "x", "y" },
                Map = new Dictionary<string, int> { ["k"] = 7 }
            };

            var s = ObjectPrinter.For<CollectionsHolder>().PrintToString(h);

            s.Should().Contain("[0]").And.Contain("1").And.Contain("2");
            s.Should().Contain("x").And.Contain("y");
            s.Should().Contain("k").And.Contain("7");
        }

        [Test]
        public void PrintToString_EmptyCollections_ShouldNotThrow()
        {
            var h = new CollectionsHolder
            {
                Numbers = new int[0],
                Strings = new List<string>(),
                Map = new Dictionary<string, int>()
            };

            Action act = () => ObjectPrinter.For<CollectionsHolder>().PrintToString(h);

            act.Should().NotThrow();
        }

        [Test]
        public void PrintToString_ListOfComplexObjects_ShouldPrintEachItem()
        {
            var list = new List<Node> { new() { Name = "a" }, new() { Name = "b" } };

            var s = ObjectPrinter.For<List<Node>>().PrintToString(list);

            s.Should().Contain("Node").And.Contain("a").And.Contain("b");
        }

        [Test]
        public void PrintToString_ExtensionWithConfig_ShouldApplyConfiguration()
        {
            var p = new Person { Name = "Alex", Age = 20 };

            var s = p.PrintToString(cfg => cfg.Excluding(p => p.Age));

            s.Should().NotContain("Age").And.NotContain("= 20").And.Contain("Alex");
        }

        [Test]
        public void PrintToString_AllFeaturesTogether_ShouldApplyEachCorrectly()
        {
            var p = new Person { Name = "Alex", Age = 19 };

            var s = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Age).Using(x => $"Возраст:{x}")
                .Printing(p => p.Name).TrimmedToLength(2)
                .Excluding(p => p.Age)
                .PrintToString(p);

            s.Should().NotContain("Возраст").And.NotContain("= 20");
            s.Should().NotContain("Id");
            s.Should().Contain("Al");
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(x => $"Возраст:{x}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(2)
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
    }
}