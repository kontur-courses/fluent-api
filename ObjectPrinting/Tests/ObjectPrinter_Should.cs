using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void NotContainExcludedField()
        {
            var obj = new ClassWithTwoIntAndOneStringFields
                {FirstIntFieldValue = 15, SecondIntFieldValue = 20, StringFieldValue = "hello"};

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringFields>()
                .Excluding(p => p.FirstIntFieldValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .NotContain(nameof(ClassWithTwoIntAndOneStringFields.FirstIntFieldValue));
        }


        [Test]
        public void ContainNonExcludedFields()
        {
            var obj = new ClassWithTwoIntAndOneStringFields
                {FirstIntFieldValue = 15, SecondIntFieldValue = 20, StringFieldValue = "hello"};

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringFields>()
                .Excluding(p => p.FirstIntFieldValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .Contain(nameof(ClassWithTwoIntAndOneStringFields.SecondIntFieldValue))
                .And
                .Contain(nameof(ClassWithTwoIntAndOneStringFields.StringFieldValue));
        }


        [Test]
        public void NotContainExcludedProperty()
        {
            var obj = new ClassWithTwoIntAndOneStringProperties
                {FirstIntPropValue = 15, SecondIntPropValue = 20, StringPropValue = "hello"};

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringProperties>()
                .Excluding(p => p.FirstIntPropValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .NotContain(nameof(ClassWithTwoIntAndOneStringProperties.FirstIntPropValue));
        }

        [Test]
        public void ContainNonExcludedProperties()
        {
            var obj = new ClassWithTwoIntAndOneStringProperties
                {FirstIntPropValue = 15, SecondIntPropValue = 20, StringPropValue = "hello"};

            var printer = ObjectPrinter
                .For<ClassWithTwoIntAndOneStringProperties>()
                .Excluding(p => p.FirstIntPropValue);

            var result = printer.PrintToString(obj);

            result.Should()
                .Contain(nameof(ClassWithTwoIntAndOneStringProperties.SecondIntPropValue))
                .And
                .Contain(nameof(ClassWithTwoIntAndOneStringProperties.StringPropValue));
        }

        [Test]
        public void AllowSpecializationForAllObjectOfSomeType()
        {
            var obj = new ClassWithTwoStringFields
                {FirstStringFieldValue = "hello", SecondStringFieldValue = "world"};

            var printer = ObjectPrinter.For<ClassWithTwoStringFields>()
                .Printing<string>().Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result
                .Should()
                .ContainAll(obj.FirstStringFieldValue.ToUpper(), obj.SecondStringFieldValue.ToUpper());
        }

        [Test]
        public void AllowSpecializationForExactField()
        {
            var obj = new ClassWithTwoStringFields
                {FirstStringFieldValue = "Value1", SecondStringFieldValue = "Value2"};

            var printer = ObjectPrinter.For<ClassWithTwoStringFields>()
                .Printing(x => x.FirstStringFieldValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result
                .Should()
                .Contain(obj.FirstStringFieldValue.ToUpper());
        }

        [Test]
        public void NotUseExactFieldSpecializationOnAllObjectOfSameType()
        {
            var obj = new ClassWithTwoStringFields
                {FirstStringFieldValue = "Value1", SecondStringFieldValue = "Value2"};

            var printer = ObjectPrinter.For<ClassWithTwoStringFields>()
                .Printing(x => x.FirstStringFieldValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result
                .Should()
                .NotContain(obj.SecondStringFieldValue.ToUpper());
        }

        [Test]
        public void AllowSpecializationForExactProperty()
        {
            var obj = new ClassWithTwoStringProperties {FirstStringPropValue = "Value1", SecondPropValue = "Value2"};

            var printer = ObjectPrinter
                .For<ClassWithTwoStringProperties>()
                .Printing(x => x.FirstStringPropValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result
                .Should()
                .Contain(obj.FirstStringPropValue.ToUpper());
        }

        [Test]
        public void NotUseExactPropertySerializationOnAllObjectOfSameType()
        {
            var obj = new ClassWithTwoStringProperties
                {FirstStringPropValue = "Value1", SecondPropValue = "Value2"};

            var printer = ObjectPrinter
                .For<ClassWithTwoStringProperties>()
                .Printing(x => x.FirstStringPropValue)
                .Using(str => str.ToUpper());

            var result = printer.PrintToString(obj);

            result
                .Should()
                .NotContain(obj.SecondPropValue.ToUpper());
        }

        [Test]
        public void NotContainObjectsOfExcludedType()
        {
            var obj = new ClassWithTwoIntAndOneStringFields
                {FirstIntFieldValue = 10, SecondIntFieldValue = 20, StringFieldValue = "hello"};

            var result = obj.PrintToString(config => config.Excluding<int>());

            result.Should()
                .NotContainAll(
                    nameof(ClassWithTwoIntAndOneStringFields.FirstIntFieldValue),
                    nameof(ClassWithTwoIntAndOneStringFields.SecondIntFieldValue));
        }

        [Test]
        public void AllowSettingCultureInfo()
        {
            var obj = new ClassWithFloatField {FloatFieldValue = 10.5f};

            var printerWithPoint = ObjectPrinter.For<ClassWithFloatField>()
                .Printing<float>().Using(CultureInfo.InvariantCulture);

            var printerWithComma = ObjectPrinter.For<ClassWithFloatField>()
                .Printing<float>().Using(CultureInfo.GetCultureInfo("ru"));

            var resultWithPoint = printerWithPoint.PrintToString(obj);
            var resultWithComma = printerWithComma.PrintToString(obj);

            using (new AssertionScope())
            {
                resultWithPoint.Should().Contain("10.5");
                resultWithComma.Should().Contain("10,5");
            }
        }

        [Test]
        public void AllowTrimmingStringValues()
        {
            var obj = new ClassWithOneStringField {StringFieldValue = "abcdef"};
            var expectedTrimmedName = "abc";
            var unexpectedSubstring = "abcd";
            var printer = ObjectPrinter.For<ClassWithOneStringField>()
                .Printing<string>().TrimmedToLength(expectedTrimmedName.Length);

            var result = printer.PrintToString(obj);

            result.Should()
                .Contain(expectedTrimmedName)
                .And
                .NotContain(unexpectedSubstring);
        }

        [Test]
        [Timeout(1000)]
        public void NotThrowWhenCycledLinksFound()
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
        public void SerializeAllElementsOfArray()
        {
            var array = new[] {10, 20, 30};

            var result = array.PrintToString();

            result.Should().ContainAll(array.Select(x => x.ToString()));
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
                .Printing<float>(obj => 5f);

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void NotThrow_WhenNullGiven()
        {
            var obj = (ClassWithFloatField) null;

            Action action = () => obj.PrintToString();

            action.Should().NotThrow();
        }
    }
}