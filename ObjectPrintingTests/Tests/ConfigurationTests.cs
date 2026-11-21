using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrintingTests.Tests
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void Excluding_Type_DoesNotPrintMembersOfThatType()
        {
            var person = new Person { Name = "Alex" };

            var result = ObjectPrinter
                .For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            result.Should().NotContain("Id");
            result.Should().Contain("Name = Alex");
        }

        [Test]
        public void Excluding_Member_DoesNotPrintThatMember()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var result = ObjectPrinter
                .For<Person>()
                .Excluding(x => x.Age)
                .PrintToString(person);

            result.Should().NotContain("Age").And.Contain("Alex");
        }

        [Test]
        public void TypeSerializer_ForFinalType_PrintsFormattedValue()
        {
            var person = new Person { Age = 19 };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                    .Using(value => value.ToString("X"))
                .PrintToString(person);

            result.Should().Contain("Age = 13");
        }

        [Test]
        public void TypeSerializer_ForComplexType_OverridesDefaultPrinting()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<Person>()
                    .Using(value => $"Person({value.Name})")
                .PrintToString(person);

            result.Should().Be($"Person(Alex){Environment.NewLine}");
        }

        [Test]
        public void TypeSerializer_ForComplexType_DoesNotPrintInnerMembers()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<Person>()
                    .Using(value => "REDACTED")
                .PrintToString(person);

            result.Should().Contain("REDACTED")
                  .And.NotContain("Name")
                  .And.NotContain("Age");
        }

        [Test]
        public void TypeCulture_ForDouble_UsesProvidedCulture()
        {
            var person = new Person { Height = 1.5 };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .PrintToString(person);

            result.Should().Contain("Height = 1.5");
        }

        [Test]
        public void TypeCulture_ForDateTime_UsesProvidedCulture()
        {
            var lesson = new Lesson
            {
                Title = "Release",
                Date = new DateTime(2024, 2, 1),
            };

            var result = ObjectPrinter
                .For<Lesson>()
                .Printing<DateTime>()
                    .Using(CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(lesson);

            result.Should().Contain("Date = 2/1/2024");
        }

        [Test]
        public void TypeCulture_ForDateTime_DiffersBetweenCultures()
        {
            var lesson = new Lesson
            {
                Title = "Release",
                Date = new DateTime(2024, 2, 1),
            };

            var american = ObjectPrinter
                .For<Lesson>()
                .Printing<DateTime>()
                    .Using(CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(lesson);

            var russian = ObjectPrinter
                .For<Lesson>()
                .Printing<DateTime>()
                    .Using(CultureInfo.GetCultureInfo("ru-RU"))
                .PrintToString(lesson);

            american.Should().Contain("Date = 2/1/2024");
            russian.Should().Contain("Date = 01.02.2024");
        }

        [Test]
        public void MemberCulture_ForDateTimeProperty_UsesProvidedCulture()
        {
            var lesson = new Lesson
            {
                Title = "Release",
                Date = new DateTime(2024, 2, 1),
            };

            var result = ObjectPrinter
                .For<Lesson>()
                .Printing(x => x.Date)
                    .Using(CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(lesson);

            result.Should().Contain("Date = 2/1/2024");
        }

        [Test]
        public void PropertySerializer_ForProperty_ChangesOnlySelectedMember()
        {
            var contact = new Contact
            {
                FirstName = "alex",
                LastName = "ivanov",
                PublicNickname = "nick",
            };

            var result = ObjectPrinter
                .For<Contact>()
                .Printing(x => x.FirstName)
                    .Using(name => name.ToUpper())
                .PrintToString(contact);

            result.Should().Contain("FirstName = ALEX");
            result.Should().Contain("LastName = ivanov");
            result.Should().Contain("PublicNickname = nick");
        }

        [Test]
        public void PropertySerializer_ForPublicField_ChangesOnlySelectedMember()
        {
            var contact = new Contact
            {
                FirstName = "alex",
                LastName = "ivanov",
                PublicNickname = "nick",
            };

            var result = ObjectPrinter
                .For<Contact>()
                .Printing(x => x.PublicNickname)
                    .Using(name => name.ToUpper())
                .PrintToString(contact);

            result.Should().Contain("PublicNickname = NICK");
            result.Should().Contain("FirstName = alex");
            result.Should().Contain("LastName = ivanov");
        }

        [Test]
        public void TrimmedToLength_ForProperty_TrimsLongString()
        {
            var person = new Person { Name = "VeryLongName123" };

            var result = ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .TrimmedToLength(4)
                .PrintToString(person);

            result.Should().Contain("Name = Very").And.NotContain("Name = VeryL");
        }

        [TestCase("abc", 4, "abc", TestName = "length < maxLen: not trimmed")]
        [TestCase("abcd", 4, "abcd", TestName = "length == maxLen: not trimmed")]
        [TestCase("abcde", 4, "abcd", TestName = "length > maxLen: trimmed")]
        public void TrimmedToLength_ForProperty_HandlesBoundaries(
            string input, int maxLen, string expected)
        {
            var person = new Person { Name = input };

            var result = ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .TrimmedToLength(maxLen)
                .PrintToString(person);

            result.Should().Contain($"Name = {expected}");

            if (input != expected)
            {
                result.Should().NotContain($"Name = {input}");
            }
        }

        [Test]
        public void TrimmedToLength_ForProperty_WithNullValue_PrintsNull()
        {
            var person = new Person { Name = null! };

            var result = ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .TrimmedToLength(4)
                .PrintToString(person);

            result.Should().Contain("Name = null");
        }

        [Test]
        public void TrimmedToLength_ForProperty_ThrowsOnNegative()
        {
            var action = () => ObjectPrinter
                .For<Person>()
                .Printing(x => x.Name)
                    .TrimmedToLength(-1);

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("maxLen");
        }

        [Test]
        public void TrimmedToLength_ForType_TrimsLongString()
        {
            var person = new Person { Name = "VeryLongName123" };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<string>()
                    .TrimmedToLength(4)
                .PrintToString(person);

            result.Should().Contain("Name = Very").And.NotContain("Name = VeryL");
        }

        [Test]
        public void TrimmedToLength_ForType_ThrowsOnNegative()
        {
            var action = () => ObjectPrinter
                .For<Person>()
                .Printing<string>()
                    .TrimmedToLength(-1);

            action.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("maxLen");
        }

        [Test]
        public void MemberSetting_OverridesTypeSetting()
        {
            var person = new Person { Name = "VeryLongName123" };

            var result = ObjectPrinter
                .For<Person>()
                .Printing<string>()
                    .TrimmedToLength(4)
                .Printing(x => x.Name)
                    .TrimmedToLength(5)
                .PrintToString(person);

            result.Should().Contain("Name = VeryL").And.NotContain("Name = VeryLo");
        }
    }
}