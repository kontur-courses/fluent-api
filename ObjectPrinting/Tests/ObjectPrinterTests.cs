using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
using ObjectPrinting.Tests.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using static System.Environment;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private const string tab = "\t";
        private const string equal = " = ";
        private const string comma = ",";

        private const string openCollectionString = "[";
        private const string closeCollectionString = "]";

        private const string openFigureBracket = "{";
        private const string closeFigureBracket = "}";

        private const string ignoredMark = "ignored";

        [Test]
        public void Should_ExcludeProperty()
        {
            var obj = new TestClass() { DoubleProperty = 50 };
            var expected =
                nameof(TestClass) + NewLine +
                tab + nameof(TestClass.DoubleProperty) + equal + obj.DoubleProperty;

            var actual = obj.PrintToString(config => config.Exclude(o => o.IntegerProperty));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_UseTypeSerializator()
        {
            var obj = new TestClass() { IntegerProperty = 10, DoubleProperty = 50 };
            var doubleSerializator = new Func<double, string>(number => number.ToString() + "(double)");
            var expected =
                nameof(TestClass) + NewLine +
                tab + nameof(TestClass.IntegerProperty) + equal + obj.IntegerProperty + NewLine +
                tab + nameof(TestClass.DoubleProperty) + equal + doubleSerializator(obj.DoubleProperty);

            var actual = obj.PrintToString(config => config.SerializeTypeAs(doubleSerializator));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_UseCulture()
        {
            var obj = new TestClass() { IntegerProperty = 10, DoubleProperty = 50.01 };
            var doubleCulture = CultureInfo.GetCultureInfo("ru");
            var expected =
                nameof(TestClass) + NewLine +
                tab + nameof(TestClass.IntegerProperty) + equal + obj.IntegerProperty + NewLine +
                tab + nameof(TestClass.DoubleProperty) + equal + obj.DoubleProperty.ToString(doubleCulture);

            var actual = obj.PrintToString(config => config.SetTypeCulture<double>(doubleCulture));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_ApplyPropertySerializer()
        {
            var obj = new TestClass() { IntegerProperty = 1, DoubleProperty = 5 };
            var serializer = new Func<int, string>(number => "Integer" + number.ToString());
            var expected =
                nameof(TestClass) + NewLine +
                tab + nameof(TestClass.IntegerProperty) + equal + serializer(obj.IntegerProperty) + NewLine +
                tab + nameof(TestClass.DoubleProperty) + equal + obj.DoubleProperty;

            var actual = obj.PrintToString(config => config.ConfigurePropertySerialization(o => o.IntegerProperty)
                                                                .SetSerializer(serializer));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_TrimStringProperty_WhenItsLengthIsGreaterThanSettedMax()
        {
            var obj = new TestClassWithString() { Field = "1234567" };
            var maxLength = obj.Field.Length - 4;
            var expected =
                nameof(TestClassWithString) + NewLine +
                tab + nameof(TestClassWithString.Field) + equal + obj.Field[..maxLength];

            var actual = obj.PrintToString(config => config.ConfigurePropertySerialization(o => o.Field)
                                                                .SetMaxLength(maxLength));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_NotTrimStringProperty_WhenItsLengthIsLowerThanSettedMax()
        {
            var obj = new TestClassWithString() { Field = "1234567" };
            var maxLength = obj.Field.Length + 1;
            var expected =
                nameof(TestClassWithString) + NewLine +
                tab + nameof(TestClassWithString.Field) + equal + obj.Field;

            var actual = obj.PrintToString(config => config.ConfigurePropertySerialization(o => o.Field)
                                                                .SetMaxLength(maxLength));

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_DetectLoopReference()
        {
            var obj = new TestClassWithLoopReference() { Property = 10 };
            obj.LoopReference = obj;
            var expected =
                nameof(TestClassWithLoopReference) + NewLine +
                tab + nameof(TestClassWithLoopReference.Property) + equal + obj.Property.ToString() + NewLine +
                tab + nameof(TestClassWithLoopReference.LoopReference) + equal + "loop reference";

            var actual = obj.PrintToString();

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_SerializeArray()
        {
            var obj = new TestClassWithArray() { Array = new int[] { 1, 2, 3 } };
            var expected =
                nameof(TestClassWithArray) + NewLine +
                tab + nameof(TestClassWithArray.Array) + equal + openCollectionString + NewLine +
                tab + tab + obj.Array[0] + comma + NewLine +
                tab + tab + obj.Array[1] + comma + NewLine +
                tab + tab + obj.Array[2] + NewLine +
                tab + closeCollectionString;

            var actual = obj.PrintToString();

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_SerializeList()
        {
            var obj = new TestClassWithList() { List = new List<int>() { 1, 2, 3 } };
            var expected =
                nameof(TestClassWithList) + NewLine +
                tab + nameof(TestClassWithList.List) + equal + openCollectionString + NewLine +
                tab + tab + obj.List[0] + comma + NewLine +
                tab + tab + obj.List[1] + comma + NewLine +
                tab + tab + obj.List[2] + NewLine +
                tab + closeCollectionString;

            var actual = obj.PrintToString();

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_SerializeDictionary()
        {
            var obj = new TestClassWithDictionary() { Dictionary = new Dictionary<int, long>() { { 1, 2 }, { 3, 4 } } };
            var expected =
                nameof(TestClassWithDictionary) + NewLine +
                tab + nameof(TestClassWithDictionary.Dictionary) + equal + openFigureBracket + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + 1 + NewLine +
                tab + tab + tab + "Value" + equal + obj.Dictionary[1] + NewLine +
                tab + tab + closeFigureBracket + comma + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + 3 + NewLine +
                tab + tab + tab + "Value" + equal + obj.Dictionary[3] + NewLine +
                tab + tab + closeFigureBracket + NewLine +
                tab + closeFigureBracket;

            var actual = obj.PrintToString();

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_IgnoreDictionaryKey_WhenItsTypeIsExcluded()
        {
            var obj = new TestClassWithDictionary() { Dictionary = new Dictionary<int, long>() { { 1, 2 }, { 3, 4 } } };
            var expected =
                nameof(TestClassWithDictionary) + NewLine +
                tab + nameof(TestClassWithDictionary.Dictionary) + equal + openFigureBracket + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + ignoredMark + NewLine +
                tab + tab + tab + "Value" + equal + obj.Dictionary[1] + NewLine +
                tab + tab + closeFigureBracket + comma + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + ignoredMark + NewLine +
                tab + tab + tab + "Value" + equal + obj.Dictionary[3] + NewLine +
                tab + tab + closeFigureBracket + NewLine +
                tab + closeFigureBracket;

            var actual = obj.PrintToString(config => config.Exclude<int>());

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_IgnoreDictionaryValue_WhenItsTypeIsExcluded()
        {
            var obj = new TestClassWithDictionary() { Dictionary = new Dictionary<int, long>() { { 1, 2 }, { 3, 4 } } };
            var expected =
                nameof(TestClassWithDictionary) + NewLine +
                tab + nameof(TestClassWithDictionary.Dictionary) + equal + openFigureBracket + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + 1 + NewLine +
                tab + tab + tab + "Value" + equal + ignoredMark + NewLine +
                tab + tab + closeFigureBracket + comma + NewLine +
                tab + tab + openFigureBracket + NewLine +
                tab + tab + tab + "Key" + equal + 3 + NewLine +
                tab + tab + tab + "Value" + equal + ignoredMark + NewLine +
                tab + tab + closeFigureBracket + NewLine +
                tab + closeFigureBracket;

            var actual = obj.PrintToString(config => config.Exclude<long>());

            actual.Should().Be(expected);
        }

        [Test]
        public void Should_SerializeDictionaryAsEmpty_WhenPairTypesAreExcluded()
        {
            var obj = new TestClassWithDictionary() { Dictionary = new Dictionary<int, long>() { { 1, 2 }, { 3, 4 } } };
            var expected =
                nameof(TestClassWithDictionary) + NewLine +
                tab + nameof(TestClassWithDictionary.Dictionary) + equal + openFigureBracket + closeFigureBracket;

            var actual = obj.PrintToString(config => config.Exclude<int>()
                                                           .Exclude<long>());

            actual.Should().Be(expected);
        }
    }
}