using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void NotContainExcludedField()
        {
            var obj = new ClassWithTwoIntAndOneStringFields
                {FirstIntFieldValue = 15, SecondIntFieldValue = 20, StringFieldValue = "hello"};
            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoIntAndOneStringFields))
                .AppendLine(
                    $"\t{nameof(ClassWithTwoIntAndOneStringFields.SecondIntFieldValue)} = {obj.SecondIntFieldValue}")
                .AppendLine($"\t{nameof(ClassWithTwoIntAndOneStringFields.StringFieldValue)} = {obj.StringFieldValue}")
                .ToString();

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringFields>()
                .Excluding(p => p.FirstIntFieldValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .Be(expected);
        }

        [Test]
        public void NotContainExcludedProperty()
        {
            var obj = new ClassWithTwoIntAndOneStringProperties()
                {FirstIntPropValue = 15, SecondIntPropValue = 20, StringPropValue = "hello"};
            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoIntAndOneStringProperties))
                .AppendLine(
                    $"\t{nameof(ClassWithTwoIntAndOneStringProperties.SecondIntPropValue)} = {obj.SecondIntPropValue}")
                .AppendLine(
                    $"\t{nameof(ClassWithTwoIntAndOneStringProperties.StringPropValue)} = {obj.StringPropValue}")
                .ToString();

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringProperties>()
                .Excluding(p => p.FirstIntPropValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .Be(expected);
        }

        [Test]
        public void NotContainObjectsOfExcludedType()
        {
            var obj = new ClassWithTwoIntAndOneStringFields
                {FirstIntFieldValue = 10, SecondIntFieldValue = 20, StringFieldValue = "hello"};
            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoIntAndOneStringFields))
                .AppendLine($"\t{nameof(ClassWithTwoIntAndOneStringFields.StringFieldValue)} = {obj.StringFieldValue}")
                .ToString();

            var result = obj.PrintToString(config => config.Excluding<int>());

            result.Should()
                .Be(expected);
        }

        [Test]
        public void AllowSpecializationForExactField()
        {
            var obj = new ClassWithTwoStringFields
                {FirstStringFieldValue = "Value1", SecondStringFieldValue = "Value2"};
            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoStringFields))
                .AppendLine($"\t{nameof(ClassWithTwoStringFields.FirstStringFieldValue)} = VALUE1")
                .AppendLine($"\t{nameof(ClassWithTwoStringFields.SecondStringFieldValue)} = Value2")
                .ToString();

            var printer = ObjectPrinter.For<ClassWithTwoStringFields>()
                .Printing(x => x.FirstStringFieldValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result.Should().Be(expected);
        }

        [Test]
        public void AllowSpecializationForExactProperty()
        {
            var obj = new ClassWithTwoStringProperties
                {FirstStringPropValue = "Value1", SecondPropValue = "Value2"};
            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoStringProperties))
                .AppendLine($"\t{nameof(ClassWithTwoStringProperties.FirstStringPropValue)} = VALUE1")
                .AppendLine($"\t{nameof(ClassWithTwoStringProperties.SecondPropValue)} = Value2")
                .ToString();

            var printer = ObjectPrinter.For<ClassWithTwoStringProperties>()
                .Printing(x => x.FirstStringPropValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result.Should().Be(expected);
        }

        [Test]
        public void AllowSpecializationForAllObjectOfSameType()
        {
            var obj = new ClassWithTwoStringProperties
                {FirstStringPropValue = "Value1", SecondPropValue = "Value2"};

            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithTwoStringProperties))
                .AppendLine($"\t{nameof(ClassWithTwoStringProperties.FirstStringPropValue)} = VALUE1")
                .AppendLine($"\t{nameof(ClassWithTwoStringProperties.SecondPropValue)} = VALUE2")
                .ToString();

            var printer = ObjectPrinter
                .For<ClassWithTwoStringProperties>()
                .Printing<string>()
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result.Should().Be(expected);
        }

        [Test]
        public void AllowSettingCultureInfo()
        {
            var obj = new ClassWithFloatField {FloatFieldValue = 10.5f};

            var expectedResultWithComma = new StringBuilder()
                .AppendLine(nameof(ClassWithFloatField))
                .AppendLine($"\t{nameof(ClassWithFloatField.FloatFieldValue)} = 10,5")
                .ToString();

            var expectedResultWithPoint = new StringBuilder()
                .AppendLine(nameof(ClassWithFloatField))
                .AppendLine($"\t{nameof(ClassWithFloatField.FloatFieldValue)} = 10.5")
                .ToString();

            var printerWithPoint = ObjectPrinter.For<ClassWithFloatField>()
                .Printing<float>().Using(CultureInfo.InvariantCulture);

            var printerWithComma = ObjectPrinter.For<ClassWithFloatField>()
                .Printing<float>().Using(CultureInfo.GetCultureInfo("ru"));

            var resultWithPoint = printerWithPoint.PrintToString(obj);
            var resultWithComma = printerWithComma.PrintToString(obj);

            using (new AssertionScope())
            {
                resultWithPoint.Should().Be(expectedResultWithPoint);
                resultWithComma.Should().Be(expectedResultWithComma);
            }
        }

        [Test]
        public void AllowTrimmingStringValues()
        {
            var obj = new ClassWithOneStringField {StringFieldValue = "abcdef"};
            var trimmedSize = 3;

            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithOneStringField))
                .AppendLine($"\t{nameof(ClassWithOneStringField.StringFieldValue)} = abc")
                .ToString();

            var printer = ObjectPrinter.For<ClassWithOneStringField>()
                .Printing<string>().TrimmedToLength(trimmedSize);

            var result = printer.PrintToString(obj);

            result.Should().Be(expected);
        }

        [Test]
        [Timeout(1000)]
        public void NotThrowWhenCycledLinksFoundOnDefault()
        {
            var linkNode1 = new LinkNode();
            var linkNode2 = new LinkNode();
            var linkNode3 = new LinkNode();

            linkNode1.Next = linkNode2;
            linkNode1.Previous = linkNode3;

            linkNode2.Next = linkNode3;
            linkNode2.Previous = linkNode1;

            linkNode3.Next = linkNode1;
            linkNode3.Previous = linkNode2;

            Action action = () => ObjectPrinter
                .For<LinkNode>()
                .PrintToString(linkNode1);

            action.Should().NotThrow();
        }

        [Test]
        [Timeout(1000)]
        public void AllowThrowingOnCycleFound()
        {
            var linkNode1 = new LinkNode();
            var linkNode2 = new LinkNode();
            var linkNode3 = new LinkNode();

            linkNode1.Next = linkNode2;
            linkNode1.Previous = linkNode3;

            linkNode2.Next = linkNode3;
            linkNode2.Previous = linkNode1;

            linkNode3.Next = linkNode1;
            linkNode3.Previous = linkNode2;

            Action action = () => ObjectPrinter
                .For<LinkNode>()
                .OnCycleFound().Throw()
                .PrintToString(linkNode1);

            action.Should().Throw<Exception>();
        }

        [Test]
        [Timeout(1000)]
        public void AddTextWhenCycleWasFound()
        {
            var linkNode1 = new LinkNode();
            var linkNode2 = new LinkNode();

            linkNode1.Next = linkNode2;
            linkNode1.Previous = linkNode2;

            linkNode2.Next = linkNode1;
            linkNode2.Previous = linkNode1;

            var cycleText = "CYCLE!!!";

            var cycledNodeText = new StringBuilder()
                .AppendLine($"\t\t{nameof(LinkNode.Next)} = {cycleText}")
                .AppendLine($"\t\t{nameof(LinkNode.Previous)} = {cycleText}");

            var expected = new StringBuilder()
                .AppendLine(nameof(LinkNode))
                .AppendLine($"\t{nameof(LinkNode.Next)} = {nameof(LinkNode)}")
                .Append(cycledNodeText)
                .AppendLine($"\t{nameof(LinkNode.Previous)} = {nameof(LinkNode)}")
                .Append(cycledNodeText)
                .ToString();


            var result = ObjectPrinter
                .For<LinkNode>()
                .OnCycleFound().AddText(cycleText)
                .PrintToString(linkNode1);

            result.Should().Be(expected);
        }

        [Test]
        public void SerializeAllElementsOfArray()
        {
            var array = new[] {"hello", "great", "world"};

            var expected = new StringBuilder()
                .AppendLine("String[]")
                .AppendLine($"\t{array[0]}")
                .AppendLine($"\t{array[1]}")
                .AppendLine($"\t{array[2]}")
                .ToString();

            var result = array.PrintToString();

            result.Should().Be(expected);
        }

        [Test]
        public void SerializeAllElementsOfList()
        {
            var list = new List<int> {10, 20, 30};

            var result = list.PrintToString();

            result.Should().ContainAll(list.Select(x => x.ToString()));
        }

        [Test]
        public void SerializeAllElementsOfDictionary()
        {
            var dictionary = new Dictionary<int, string>()
                {{1, "one"}, {2, "two"}, {100, "one hundred"}};

            var result = dictionary.PrintToString();

            result.Should()
                .ContainAll(dictionary.Keys.Select(x => x.ToString()))
                .And
                .ContainAll(dictionary.Values);
        }

        [Test]
        public void Throw_WhenMemberSelectorGiven()
        {
            Action action = () => ObjectPrinter.For<ClassWithFloatField>()
                .Printing(obj => 5f);

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void NotThrow_WhenNullGiven()
        {
            var obj = (ClassWithFloatField) null;

            Action action = () => obj.PrintToString();

            action.Should().NotThrow();
        }

        [Test]
        public void AllowSettingCustomEndLine()
        {
            var obj = new ClassWithFloatField {FloatFieldValue = 0f};

            var endLine = "\n \n";

            var expected = new StringBuilder()
                .Append(nameof(ClassWithFloatField))
                .Append(endLine)
                .Append($"\t{nameof(ClassWithFloatField.FloatFieldValue)} = {obj.FloatFieldValue}")
                .Append(endLine)
                .ToString();

            var result = obj.PrintToString(config => config.SetEndLine("\n \n"));

            result.Should().Be(expected);
        }

        [Test]
        public void AllowSettingCustomIndentation()
        {
            var obj = new ClassWithFloatField {FloatFieldValue = 0f};

            Func<int, string> customIndentaion = level => new string(' ', level + 1);

            var expected = new StringBuilder()
                .AppendLine(nameof(ClassWithFloatField))
                .AppendLine(' ' + $"{nameof(ClassWithFloatField.FloatFieldValue)} = {obj.FloatFieldValue}")
                .ToString();

            var result = obj.PrintToString(config => config.SetIndentation(customIndentaion));

            result.Should().Be(expected);
        }
    }
}