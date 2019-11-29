using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class NestedObjectTests_Should
    {
        public PrintingConfig<NestedObjectContainer> Printer;
        public NestedObjectContainer Container = new NestedObjectContainer();

        [SetUp]
        public void SetUp()
        {
            Printer = ObjectPrinter.For<NestedObjectContainer>();
        }

        [Test]
        public void StopUnwrappingObject_WhenNestingLevelGreaterThanDefault()
        {
            Printer.PrintObject(Container)
                .Should()
                .Contain("...");
        }

        [Test]
        public void StopUnwrappingObject_WhenNestingLevelGreaterThanGiven()
        {
            Printer.UsingMaxRecursionLevel(10)
                .PrintObject(Container)
                .Should()
                .Contain("...");
        }

        [Test]
        public void SerializeArraysWithoutErrors()
        {
            Action arraySerialization = () => ObjectPrinter.For<int[]>().PrintObject(new[] {1, 2, 4});
            arraySerialization.ShouldNotThrow();
        }
        
        [Test]
        public void SerializeListsWithoutErrors()
        {
            Action listSerialization = () => ObjectPrinter.For<List<int>>().PrintObject(new List<int>{1, 2, 4});
            listSerialization.ShouldNotThrow();
        }
        
        [Test]
        public void SerializeDictionaryWithoutErrors()
        {
            Action dictSerialization = () => ObjectPrinter
                .For<Dictionary<int,int>>()
                .PrintObject(new Dictionary<int, int>{{1,1},{2,2},{3,4}});
            dictSerialization.ShouldNotThrow();
        }
        
        [Test]
        public void SerializeLists()
        {
            Console.Error.WriteLine(ObjectPrinter
                .For<Dictionary<int,int>>()
                .PrintObject(new Dictionary<int, int>{{1,1},{2,2},{3,4}}));
        }
    }
}