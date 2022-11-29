using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinterTests.HelperClasses;
using ObjectPrinting;

namespace ObjectPrinterTests
{
    public class ObjectPrinterShould
    {
        [Test]
        public void HandleObject_WitCollections()
        {
            var collection = new Collections();
            var expected = @"Collections
	Array = [1, 2, 3, 4, 5, 7, 8, 9]
	List = [1, 2, 3, 4, 5, 7, 8, 9, 10]
	Dictionary = {
		one: 1,

		two: 2,

		three: 3,

		four: 4,

		five: 5,

		six: 6,

	}
";
            var result = collection.PrintToString();
            result.Should().Be(expected);
        }


        [Test]
        public void HandleArray()
        {
            var array = new Collections().Array;

            var expected = @"[100,00 %, 200,00 %, 300,00 %, 400,00 %, 500,00 %, 700,00 %, 800,00 %, 900,00 %]";
            var result = array
                .PrintToString(p => p.Printing<int>()
                    .Using(i => i.ToString("P")));

            result.Should().Be(expected);
        }


        [Test]
        public void HandleDictionary()
        {
            var dict = new Collections().Dictionary;

            var config = ObjectPrinter.For<Dictionary<string, int>>()
                .Printing<int>()
                .Using(i => i.ToString("P"));

            var expected = @"{
	one: 100,00 %,

	two: 200,00 %,

	three: 300,00 %,

	four: 400,00 %,

	five: 500,00 %,

	six: 600,00 %,

}";

            var result = config.PrintToString(dict);
            result.Should().Be(expected);
        }


        [Test]
        public void ExcludeType()
        {
            var myClass = new TwoInt();
            var config = ObjectPrinter.For<TwoInt>().Excluding<int>();
            var expected = @"TwoInt
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }


        [Test]
        public void ExcludeProperty()
        {
            var myClass = new TwoInt();
            var config = ObjectPrinter.For<TwoInt>().Excluding(m => m.A);
            var expected = @"TwoInt
	B = 0
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }


        [Test]
        public void SerializeType()
        {
            var myClass = new TwoInt { A = 15, B = 15 };
            var config = ObjectPrinter.For<TwoInt>()
                .Printing<int>()
                .Using(i => i.ToString("X"));
            var expected = @"TwoInt
	A = F
	B = F
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }

        [Test]
        public void SerializeProperty()
        {
            var myClass = new TwoInt { A = 15, B = 15 };
            var config = ObjectPrinter.For<TwoInt>()
                .Printing(m => m.B)
                .Using(i => i.ToString("X"));
            var expected = @"TwoInt
	A = 15
	B = F
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }

        [Test]
        public void SerializeTypeAndProperty()
        {
            var myClass = new TwoInt { A = 15, B = 15 };
            var config = ObjectPrinter
                .For<TwoInt>()
                .Printing<int>()
                .Using(i => i.ToString("X"))
                .Printing(m => m.A)
                .Using(i => i.ToString("D5"));
            var expected = @"TwoInt
	A = 00015
	B = F
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }

        [TestCase("af-ZA")]
        [TestCase("be-BY")]
        [TestCase("da-DK")]
        [TestCase("es")]
        [TestCase("ja-JP")]
        [TestCase("")]
        public void SetCultureType(string nameCulture)
        {
            var culture = CultureInfo.GetCultureInfo(nameCulture);
            var myClass = new TwoDateTime { A = DateTime.Today, B = DateTime.Now };

            var config = ObjectPrinter.For<TwoDateTime>()
                .Printing<DateTime>()
                .Using(culture);

            var expected = @$"TwoDateTime
	A = {myClass.A.ToString(culture)}
	B = {myClass.B.ToString(culture)}
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }

        [TestCase("af-ZA")]
        [TestCase("be-BY")]
        [TestCase("da-DK")]
        [TestCase("es")]
        [TestCase("ja-JP")]
        [TestCase("")]
        public void SetCultureProperty(string nameCulture)
        {
            var culture = CultureInfo.GetCultureInfo(nameCulture);
            var myClass = new TwoDateTime { A = DateTime.Today, B = DateTime.Now };
            var config = ObjectPrinter.For<TwoDateTime>()
                .Printing(m => m.A)
                .Using(culture);
            var expected = @$"TwoDateTime
	A = {myClass.A.ToString(culture)}
	B = {myClass.B}
";

            var result = config.PrintToString(myClass);

            result.Should().Be(expected);
        }


        [Test]
        public void PrioritySerializeGreaterCulture()
        {
            var myClass = new TwoDateTime { A = DateTime.Now, B = DateTime.Today };
            var config = ObjectPrinter
                .For<TwoDateTime>()
                .Printing<DateTime>()
                .Using(d => d.Month.ToString())
                .Printing<DateTime>()
                .Using(CultureInfo.InvariantCulture);
            var expected = @$"TwoDateTime
	A = {myClass.A.Month}
	B = {myClass.B.Month}
";

            var result = config.PrintToString(myClass);
            result.Should().Be(expected);
        }

        [TestCase(0, 10)]
        [TestCase(4, 10)]
        [TestCase(10, 10)]
        public void TrimAllStrings(int maxLen, int lengthStrings)
        {
            var myClass = new TwoString
            {
                A = new string('a', lengthStrings),
                B = new string('b', lengthStrings)
            };

            var config = ObjectPrinter
                .For<TwoString>()
                .Printing<string>()
                .TrimmedToLength(maxLen);

            var expected = @$"TwoString
	A = {myClass.A[..maxLen]}
	B = {myClass.B[..maxLen]}
";

            var result = config.PrintToString(myClass);
            result.Should().Be(expected);
        }

        [TestCase(0, 10)]
        [TestCase(4, 10)]
        [TestCase(10, 10)]
        public void TrimStringProperty(int maxLen, int lengthString)
        {
            var myClass = new TwoString
            {
                A = new string('a', lengthString),
                B = "abcde"
            };

            var config = ObjectPrinter
                .For<TwoString>()
                .Printing(m => m.A)
                .TrimmedToLength(maxLen);

            var expected = @$"TwoString
	A = {myClass.A[..maxLen]}
	B = {myClass.B}
";

            var result = config.PrintToString(myClass);
            result.Should().Be(expected);
        }


        [Test]
        public void SerializationWithTrimmedString()
        {
            var myClass = new TwoString { A = "123", B = "123" };
            var config = ObjectPrinter
                .For<TwoString>()
                .Printing<string>()
                .Using(s => s + "!!!")
                .Printing<string>()
                .TrimmedToLength(5);
            var expected = @$"TwoString
	A = {myClass.A}!!
	B = {myClass.B}!!
";

            var result = config.PrintToString(myClass);
            result.Should().Be(expected);
        }


        [Test]
        public void NotThrowException_OnRecursion()
        {
            var parent = new Parent();
            var child = new Child();
            parent.Child = child;
            child.Parent = parent;

            Action action = () => parent.PrintToString();

            action.Should().NotThrow<StackOverflowException>();
        }
    }
}