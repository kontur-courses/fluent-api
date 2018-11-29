using System;
using System.Data.SqlTypes;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinterShouldExcludeTypes()
        {
            var person = new Person();
            ObjectPrinter
                .For<Person>()
                .Exclude<Guid>()
                .Exclude<string>()
                .Exclude<int>()
                .Exclude<double>()
                .PrintToString(person)
                .Should()
                .Be("Person" + Environment.NewLine);

        }

        [Test]
        public void ObjectPrinterShouldExcludeProperts()
        {
            var person = new Person();
            ObjectPrinter
                .For<Person>()
                .Exclude(p => p.Id)
                .Exclude(p => p.Name)
                .Exclude(p => p.Height)
                .Exclude(p => p.Age)
                .PrintToString(person)
                .Should()
                .Be("Person" + Environment.NewLine);

        }

        [Test]
        public void ObjectPrinterShouldBeAbleTrimString()
        {
            var person = new Person();
            person.Name = "aba";
            ObjectPrinter
                .For<Person>()
                .Exclude(p => p.Id)
                .Exclude(p => p.Age)
                .Exclude(p => p.Height)
                .Serialise(p => p.Name)
                .Trimming(1)
                .PrintToString(person)
                .Should()
                .Be("Person" + Environment.NewLine + "\t\tName = a");

        }

        [Test]
        public void ObjectPrinterShouldBeAbleSetSerialisersForType()
        {
            int integer = 1;
            ObjectPrinter
                .For<int>()
                .Serialise<int>()
                .Using(i => ((char)i).ToString())
                .PrintToString(integer)
                .Should()
                .Be("1" + Environment.NewLine);

        }
    }
}