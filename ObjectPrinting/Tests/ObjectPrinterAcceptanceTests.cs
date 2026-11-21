using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

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
                //1. Исключить из сериализации свойства определенного типа ExcludingType<Type>()
                //2. Указать альтернативный способ сериализации для определенного типа PrintTypeAs<Type>
                //3. Для числовых типов указать культуру WithCulture<Type>
                //4. Настроить сериализацию конкретного свойства 
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства Excluding
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //8. ...с конфигурированием
        }

        [Test]
        public void PrintToString_ExcludingType_PrintWothoutType()
        {
            var person = new Person() { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.ExcludingType<int>());

            result.Should().
                Contain("Name").And.
                NotContain("Age").And.Contain("Height");
        }

        [Test]
        public void Print_ExcludingMember_SkipsOnlyThatProperty()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.Excluding(p => p.Age));

            result.Should().Contain("Name = Arseny")
                .And.Contain("Height")
                .And.NotContain("Age");
        }

        [Test]
        public void Excluding_ThrowsArgumentException_WhenSelectorIsNotMemberAccess()
        {
            var config = ObjectPrinter.For<Person>();

            var result = config.Invoking(c =>
                         c.Excluding(p => p.Age + 1));
            
            result.Should().Throw<ArgumentException>();
        }

        [Test]
        public void Print_PrintTypeAs_UsesCustomPrinterInsteadOfDefault()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.PrintTypeAs<int>(i => $"[{i}]"));

            result.Should().Contain("Age = [19]")
                .And.Contain("Height");
        }

        [Test]
        public void Print_PrintTypeAs_WorksForNonFinalTypes()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config => config.PrintTypeAs<Guid>(guid => $"ID:{guid}"));

            result.Should().Contain("Id = ID:");
        }

        [Test]
        public void Print_SelfReference_DoesNotCrash()
        {
            var a = new Node("a", null);
            a.Next = a;

            var result = ObjectPrinter.For<Node>().PrintToString(a);

            result.Should().Contain("Name = a");
        }

        [Test]
        public void Print_MutualReference_DoesNotCrash()
        {
            var a = new Node("a", null);
            var b = new Node("b",a);
            a.Next = b;

            var result = ObjectPrinter.For<Node>().PrintToString(a);

            result.Should().Contain("Name = a").And.Contain("Name = b");
        }

        [Test]
        public void Print_CyclicReference_ContainsCyclicMarker()
        {
            var a = new Node("a", null);
            a.Next = a;

            var result = ObjectPrinter.For<Node>().PrintToString(a);

            result.Should().Contain("cycle reference");
        }

        [Test]
        public void Print_WithCulture_UsesSpecifiedCulture()
        {
            var person = new Person { Age = 19, Height = 180.5, Name = "Arseny" };

            var result = person.Print(config =>
                config.WithCulture<double>(CultureInfo.InvariantCulture));

            result.Should().Contain("Height = 180.5");
        }

        [Test]
        public void Print_PrintingMember_UsesCustomPrinterForThatProperty()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config =>
                config.Printing(p => p.Name).Using(n => n.ToUpper()));

            result.Should().Contain("Name = ARSENY")
                            .And.Contain("Age = 19");
        }

        [Test]
        public void Print_TrimToLength_TruncatesStringValue()
        {
            var person = new Person { Age = 19, Height = 180, Name = "Arseny" };

            var result = person.Print(config =>
                config.Printing(p => p.Name).TrimToLength(3));

            result.Should().Contain("Name = Ars")
                .And.Contain("Age = 19");
        }

        [Test]
        public void Print_Array_PrintsElements()
        {
            var arr = new[] { 1, 2, 3 };

            var result = ObjectPrinter.For<int[]>().PrintToString(arr);

            result.Should().Contain("[0] = 1")
                        .And.Contain("[1] = 2")
                        .And.Contain("[2] = 3");
        }

        [Test]
        public void Print_List_PrintsElements()
        {
            var list = new List<string> { "a", "b", "c" };

            var result = ObjectPrinter.For<List<string>>().PrintToString(list);

            result.Should().Contain("[0] = a")
                .And.Contain("[1] = b")
                .And.Contain("[2] = c");
        }
        [Test]
        public void Print_Dictionary_PrintsKeyValuePairs()
        {
            var dict = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } };

            var result = ObjectPrinter.For<Dictionary<string, int>>().PrintToString(dict);

            result.Should().Contain("[one] = 1")
                .And.Contain("[two] = 2");
        }

        [Test]
        public void Print_NestedCollections_PrintsCorrectly()
        {
            var nested = new List<int[]> { new[] { 1, 2 }, new[] { 3, 4 } };

            var result = ObjectPrinter.For<List<int[]>>().PrintToString(nested);

            result.Should().Contain("[0] = ")
                .And.Contain("[0] = 1")
                .And.Contain("[1] = 4");
        }
    }
}