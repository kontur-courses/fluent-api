using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ObjectPrinting.Tests
{
    class ObjectPrinter_Should
    {
        private class Container<T>
        {
            public T Content { get; set; }

            public Container(T value) => Content = value;
        }

        class CyclicObject
        {
            public CyclicObject Cyclic { get; set; }
        }

        [Test]
        public void Printer_Defult_Should()
        {
            var container = new Container<int>(10);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = 10" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int>>();
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_ExcludingType_Should()
        {
            var container = new Container<int>(10);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int>>()
                .Excluding<int>();
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_ExcludingProperty_Should()
        {
            var container = new Container<int>(10);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int>>()
                .Excluding(o => o.Content);
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_UsingCulture_Should()
        {
            var container = new Container<double>(1.1);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = 1,10 ₽" + Environment.NewLine;
            var cultureInfo = CultureInfo.CurrentCulture;

            var printingConfig = ObjectPrinter.For<Container<double>>()
                .Printing<double>().Using(CultureInfo.CurrentCulture);
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_UsingNewTypeSerialize_Should()
        {
            var container = new Container<int>(10);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = 11" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int>>()
                .Printing<int>().Using(x => (x + 1).ToString());
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_UsingNewPropertySerialize_Should()
        {
            var container = new Container<int>(10);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = 11" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int>>()
                .Printing(o => o.Content).Using(x => (x + 1).ToString());
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_UsingTrimm_Should()
        {
            var container = new Container<string>("string");
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = stri" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<string>>()
                .Printing(o => o.Content).TrimmedToLength(4);
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_CollectionArray_Should()
        {
            var container = new Container<int[]>(new []{ 1, 2, 3 });
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = Int32[] { 1 2 3 }" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<int[]>>();
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_CollectionDictionary_Should()
        {
            var dic = new Dictionary<int, string>();
            dic.Add(1, "1");
            dic.Add(2, "2");
            var container = new Container<Dictionary<int, string>>(dic);
            var type = container.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tContent = Dictionary`2 { [1, 1] [2, 2] }" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<Container<Dictionary<int, string>>>();
            var actual = printingConfig.PrintToString(container);

            actual.Should().Be(expectedResult);
        }

        [Test]
        public void Printer_CyclicReferences_Should()
        {
            var cyclic = new CyclicObject();
            cyclic.Cyclic = cyclic;
            var type = cyclic.GetType();
            var expectedResult = type.Name + Environment.NewLine + "\tCyclic = CyclicObject(...)" + Environment.NewLine;

            var printingConfig = ObjectPrinter.For<CyclicObject>();
            var actual = printingConfig.PrintToString(cyclic);

            actual.Should().Be(expectedResult);
        }
    }
}
