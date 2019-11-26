using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void Exclude_PropsOfAnyType()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            var typeString = "Person" + Environment.NewLine;
            var idString = "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine;
            var nameString = "\tName = Alex" + Environment.NewLine;
            var ageString = "\tAge = 19" + Environment.NewLine;
            var heightString = "\tHeight = 170" + Environment.NewLine;

            ObjectPrinter.For(person).Exclude<Guid>().PrintToString().Should().Be(typeString + nameString + heightString + ageString);
            ObjectPrinter.For(person).Exclude<string>().PrintToString().Should().Be(typeString + idString + heightString + ageString);
            ObjectPrinter.For(person).Exclude<int>().PrintToString().Should().Be(typeString + idString + nameString + heightString);
            ObjectPrinter.For(person).Exclude<double>().PrintToString().Should().Be(typeString + idString + nameString + ageString);
        }

        [Test]
        public void Exclude_AnyProperty()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            var typeString = "Person" + Environment.NewLine;
            var idString = "\tId = 00000000-0000-0000-0000-000000000000" + Environment.NewLine;
            var nameString = "\tName = Alex" + Environment.NewLine;
            var ageString = "\tAge = 19" + Environment.NewLine;
            var heightString = "\tHeight = 170" + Environment.NewLine;

            ObjectPrinter.For(person).Exclude(p => p.Id).PrintToString().Should().Be(typeString + nameString + heightString + ageString);
            ObjectPrinter.For(person).Exclude(p => p.Name).PrintToString().Should().Be(typeString + idString + heightString + ageString);
            ObjectPrinter.For(person).Exclude(p => p.Height).PrintToString().Should().Be(typeString + idString + nameString + ageString);
            ObjectPrinter.For(person).Exclude(p => p.Age).PrintToString().Should().Be(typeString + idString + nameString + heightString);
        }

        [Test]
        public void Use_UserSerializerForAnyType()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            var typeString = "Person" + Environment.NewLine;
            var idString = "\tId = 000..." + Environment.NewLine;
            var nameString = "\tName = Alex" + Environment.NewLine;
            var ageString = "\tAge = 19" + Environment.NewLine;
            var heightString = "\tHeight = 170" + Environment.NewLine;

            ObjectPrinter.For(person)
                .Serialize<Guid>()
                .Using(guid => $"{guid.ToString().Substring(0, 3)}...")
                .PrintToString()
                .Should().Be(typeString + idString + nameString + heightString + ageString);
        }

        [Test]
        public void Use_UserSerializerForAnyProperty()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            var typeString = "Person" + Environment.NewLine;
            var idString = "\tId = 000..." + Environment.NewLine;
            var nameString = "\tName = Alex" + Environment.NewLine;
            var ageString = "\tAge = 19" + Environment.NewLine;
            var heightString = "\tHeight = 170" + Environment.NewLine;

            ObjectPrinter.For(person)
                .Serialize(p => p.Id)
                .Using(id => $"{id.ToString().Substring(0, 3)}...")
                .PrintToString()
                .Should().Be(typeString + idString + nameString + heightString + ageString);
        }

        [Test, TestCaseSource(nameof(GetNumberStringsWithUserCulture))]
        public void Use_UserCultureForAnyNumberType(string printResult)
        {
            printResult.Should().Be("33,000" + Environment.NewLine);
        }

        private static IEnumerable<TestCaseData> GetNumberStringsWithUserCulture()
        {
            var culture = new CultureInfo("ru-ru", false);
            culture.NumberFormat.NumberDecimalDigits = 3;

            yield return new TestCaseData(ObjectPrinter.For<byte>(33).Serialize<byte>().Using(culture).PrintToString());
            yield return new TestCaseData(ObjectPrinter.For<short>(33).Serialize<short>().Using(culture).PrintToString());
            yield return new TestCaseData(ObjectPrinter.For<int>(33).Serialize<int>().Using(culture).PrintToString());
            yield return new TestCaseData(ObjectPrinter.For<long>(33).Serialize<long>().Using(culture).PrintToString());
            yield return new TestCaseData(ObjectPrinter.For<float>(33).Serialize<float>().Using(culture).PrintToString());
            yield return new TestCaseData(ObjectPrinter.For<double>(33).Serialize<double>().Using(culture).PrintToString());
        }

        [Test]
        public void Can_TrancateStringProperties()
        {
            ObjectPrinter.For("1234567890").Serialize<string>().WithMaxLength(5).PrintToString().Should().Be("12345" + Environment.NewLine);
            ObjectPrinter.For("123").Serialize<string>().WithMaxLength(5).PrintToString().Should().Be("123" + Environment.NewLine);
        }

        [Test]
        public void Can_TrancateAnyStringProperty()
        {
            var classWithStrings = new ClassWithStrings() { s1 = "1234567890", s2 = "1234567890" };
            ObjectPrinter.For(classWithStrings)
                .Serialize(obj => obj.s2).WithMaxLength(3)
                .PrintToString()
                .Should().Be($"ClassWithStrings{Environment.NewLine}\ts1 = 1234567890{Environment.NewLine}\ts2 = 123{Environment.NewLine}");
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Support_Collections()
        {
            var obj1 = new ClassWithStrings { s1 = "12345", s2 = "67890" };
            var obj2 = new ClassWithStrings { s1 = "09876", s2 = "54321" };
            var sb = new StringBuilder();

            var arr = new ClassWithStrings[] { obj1, obj2 };
            sb.Append(arr.GetObjectPrinter().PrintToString());

            var dictionary = new Dictionary<int, ClassWithStrings> { { 0, obj1 }, { 1, obj2 } };
            sb.Append(dictionary.GetObjectPrinter().PrintToString());

            var list = new List<ClassWithStrings> { obj1, obj2 };
            sb.Append(list.GetObjectPrinter().PrintToString());

            var en = Enumerable.Repeat(new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 }, 3);
            sb.Append(en.GetObjectPrinter().PrintToString());

            Approvals.Verify(sb.ToString());
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Support_CircularReferences()
        {
            var obj1 = new ClassWithCircularReference { ObjectName = "obj1" };
            var obj2 = new ClassWithCircularReference { ObjectName = "obj2" };
            var obj3 = new ClassWithCircularReference { ObjectName = "obj3" };
            obj1.NestedObject = obj2;
            obj2.NestedObject = obj3;
            obj3.NestedObject = obj1;

            Approvals.Verify(obj1.GetObjectPrinter().PrintToString());
        }
    }
}