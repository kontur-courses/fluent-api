using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ObjectPrinterTests
    {
        [Test] //1
        public void ObjectPrinter_ShouldExcludeFieldOrProperty_WithPassedType()
        {
            var person = new Person { Name = "Alex", Age = 19, Birthdate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<string>();

            Approvals.Verify(printer.PrintToString(person));
        }
        [Test] //2
        public void ObjectPrinter_ShouldAlternativeSerializer_ForPassedType()
        {
            var person = new Person { Name = "Alex", Surname="White", Age = 19, Birthdate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .WithSerializer<string>(s => $"String field: {s}")
                .WithSerializer<int>(n => $"Int field: {n}");

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test] //3
        public void ObjectPrinter_ShouldAddCulture_ForDouble()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Height = 1.99 };

            var printer = ObjectPrinter.For<Person>()
                   .Printing(p => p.Height)
                   .WithCulture(new CultureInfo("en-US"))
                   .ToParentObjectConfig();

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test] //3
        public void ObjectPrinter_ShouldAddCulture_ForDateTime()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Birthdate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                    .Printing(p => p.Birthdate)
                    .WithCulture(CultureInfo.GetCultureInfo("en-US")).ToParentObjectConfig();

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test] //4
        public void ObjectPrinter_ShouldAlternativeSerializer_ForSpecificPropertyOrField()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Birthdate = new DateTime(2021, 1, 21)};

            var printer = ObjectPrinter.For<Person>()
                .WithSerializer(p => p.Name, name => $"My name is {name}")
                .WithSerializer(p => p.Birthdate, date => date.ToString("M.dd"));

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test] //5
        public void ObjectPrinter_ShouldTruncateSerializer_ForString()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Birthdate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                    .Printing(obj => obj.Name).Truncate(3);

            Approvals.Verify(printer.PrintToString(person));
        }
        [Test] //6
        public void ObjectPrinter_ShouldExclude_SpecificPropertyOrField()
        {
            var person = new Person { Name = "Alex", Surname = "White", Age = 19, Birthdate = new DateTime(2021, 1, 21) };

            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.Name)
                .Exclude(p => p.Birthdate);

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldPrintField_ForNestedClass()
        {
            var person = new Person
            {
                Name = "Alex",
                Surname = "White",
                Age = 19,
                Birthdate = new DateTime(2021, 1, 21),
                test = new Person { Name = "Mordor" }
            };

            var printer = ObjectPrinter.For<Person>();

            Approvals.Verify(printer.PrintToString(person));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintArray()
        {
            var list = new[] { new Person { Name = "Mary" }, new Person { Name = "Alice" } };

            var printer = ObjectPrinter.For<Person[]>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintList()
        {
            var list = new List<Person> { new Person { Name = "Mary" }, new Person { Name = "Alice" } };

            var printer = ObjectPrinter.For<List<Person>>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectPrintDictionary()
        {
            var list = new Dictionary<string, Person> 
            {
                { "person1", new Person { Name = "Mary" } },
                { "person2",  new Person { Name = "Alice" } } 
            };

            var printer = ObjectPrinter.For<Dictionary<string, Person>>();

            Approvals.Verify(printer.PrintToString(list));
        }

        [Test] //7
        public void ObjectPrinter_ShouldCorrectHandleCycleReference()
        {
            var first = new A();
            var second = new B();
            first.Cycle = second;
            second.Cycle = first;

            var printer = ObjectPrinter.For<A>();
            Func<string> act = () => printer.PrintToString(first);

            act.Should().NotThrow<StackOverflowException>();
            Approvals.Verify(printer.PrintToString(first));
        }
    }

    public class A
    {
        public string AField => "Hello";
        public B Cycle;
    }
    public class B
    {
        public int BField => 10;
        public A Cycle;
    }
}
