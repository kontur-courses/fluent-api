using System;
using NUnit.Framework;
using FluentAssertions;
using System.Globalization;

namespace ObjectPrinting.Tests
{
    class ObjectPrinting_Tests
    {
        private Person personWithParents;
        private Person personWithoutParents;

        [SetUp]
        public void SetUp()
        {
            personWithParents = new Person
            {
                Age = 10,
                Height = 154.2,
                Name = "Oleg",
                Weight = 55.6,
                Father = new Person
                {
                    Age = 35,
                    Height = 184.5,
                    Name = "Egor",
                    Weight = 82.8
                },
                Mother = new Person
                {
                    Age = 33,
                    Height = 174.3,
                    Name = "Anna",
                    Weight = 66.7
                }
            };
            personWithoutParents = new Person
            {
                Age = 20,
                Height = 182.2,
                Name = "Oleg",
                Weight = 76.2
            };
        }

        [Test]
        public void ShouldReturnString_WithSortedPropertiesByName()
        {
            var obj = new
            {
                C = "xcv",
                A = 123,
                B = 12
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 123\r\n" +
                "\tB = 12\r\n" +
                "\tC = xcv\r\n";

            var actual = obj.PrintToString();

            actual.Should().Be(expected);
        }


        #region Excluding tests
        [Test]
        public void ShouldSupport_TypeExcluding()
        {
            var expected = 
                "Person\r\n" +
                "\tAge = 20\r\n" +
                "\tFather = null\r\n" +
                "\tMother = null\r\n" +
                "\tName = Oleg\r\n";

            var actual = personWithoutParents.PrintToString(
                cfg => cfg
                .Excluding<double>()
                .Excluding<Guid>());

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldSupport_MemberExcluding()
        {
            var expected =
                "Person\r\n" +
                "\tAge = 10\r\n" +
                "\tFather = Person\r\n" +
                "\t\tAge = 35\r\n" +
                "\t\tFather = null\r\n" +
                "\t\tName = Egor\r\n" +
                "\tName = Oleg\r\n";

            var actual = personWithParents.PrintToString(
                cfg => cfg
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding(p => p.Mother));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldReturnEmptyString_WhenPrintingObjIsExcluded()
        {
            var expected = string.Empty;

            var actual = personWithParents.PrintToString(
                cfg => cfg.Excluding<Person>());

            actual.Should().Be(expected);
        }
        #endregion

        #region Printing Tests
        [Test]
        public void ShouldSupport_TypePrinting()
        {
            var expected =
                "Person\r\n" +
                "\tAge = 20\r\n" +
                "\tHeight = qwerty\r\n" +
                "\tName = Oleg\r\n" +
                "\tWeight = qwerty\r\n";

            var actual = personWithoutParents.PrintToString(
                cfg => cfg
                .Excluding<Guid>()
                .Excluding(p => p.Father)
                .Excluding(p => p.Mother)
                .Printing<double>().Using(d => "qwerty"));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldSupport_MemberPrinting()
        {
            var expected =
                "Person\r\n" +
                "\tAge = 10\r\n" +
                "\tFather = FFFather\r\n" +
                "\tMother = MMMother\r\n" +
                "\tName = Oleg\r\n";

            var actual = personWithParents.PrintToString(
                cfg => cfg
                .Excluding<Guid>()
                .Excluding<double>()
                .Printing(p => p.Father).Using(p => "FFFather")
                .Printing(p => p.Mother).Using(p => "MMMother"));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldPrintWithMemberPriority()
        {
            var expected =
                "Person\r\n" +
                "\tHeight = member printing\r\n" +
                "\tWeight = type printing\r\n";

            var actual = personWithoutParents.PrintToString(
                cfg => cfg
                .Excluding<Guid>()
                .Excluding<int>()
                .Excluding<string>()
                .Excluding(p => p.Mother)
                .Excluding(p => p.Father)
                .Printing<double>().Using(d => "type printing")
                .Printing(p => p.Height).Using(d => "member printing"));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldPrintLastUserPrinting_IfUserPrintedSeveralPrintingForSameType()
        {
            var obj = new
            {
                A = 123
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = zxc\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<int>().Using(i => "qwe")
                .Printing<int>().Using(i => "asd")
                .Printing<int>().Using(i => "zxc"));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldUseUserPrinting_WhenPrintingUsedForTypeOfObjectTypePrinting()
        {
            var number = 5;
            var expected = "asdqwe\r\n";

            var actual = number.PrintToString(
                cfg => cfg
                .Printing<int>().Using(n => "asdqwe"));

            actual.Should().Be(expected);
        }
        #endregion

        #region Culture Tests
        [Test]
        public void ShouldSupport_CultureWithNumericTypes()
        {
            var obj = new
            {
                A = 12.3,
                B = 123.23
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 12.3\r\n" +
                "\tB = 123,23\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing(p => p.A).Using(CultureInfo.InvariantCulture)
                .Printing(p => p.B).Using(CultureInfo.CurrentCulture));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldPrintLastUserCulture_IfUserChoseSeveralCulturesForSameType()
        {
            var obj = new
            {
                A = 123.123
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 123.123\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<double>().Using(CultureInfo.CurrentCulture)
                .Printing<double>().Using(CultureInfo.InvariantCulture));

            actual.Should().Be(expected);
        }
        #endregion

        #region TrimmedToLength Tests
        [Test]
        public void ShouldSupport_TrimmedToLengthWithStringType()
        {
            var obj = new
            {
                A = "123456789",
                B = "987654321"
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 1234\r\n" +
                "\tB = 98\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing(p => p.A).TrimmedToLength(4)
                .Printing(p => p.B).TrimmedToLength(2));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldSupportTrimToLengthSeveralTimes()
        {
            var obj = new
            {
                A = "123456789"
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 1\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<string>().TrimmedToLength(1)
                .Printing<string>().TrimmedToLength(4)
                .Printing<string>().TrimmedToLength(6));

            actual.Should().Be(expected);
        }
        #endregion

        [Test]
        public void ShouldTrimToLengthWithPrintedByUserStringType()
        {
            var obj = new
            {
                A = "123456789",
                B = "987654321"
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = qwe\r\n" +
                "\tB = qwe\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<string>().Using(s => "qwerty")
                .Printing<string>().TrimmedToLength(3));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldPrintPrintingByUser_IfContainsCultureAndPrintingByUser()
        {
            var obj = new
            {
                A = 123.2
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = 123/2\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<double>().Using(i => "123/2")
                .Printing<double>().Using(CultureInfo.CurrentCulture));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldPrintPrintingByUser_IfContainsMemberCultureAndTypePrintingByUser()
        {
            var obj = new
            {
                A = 123.2,
                B = 321.1
            };

            var expected =
                obj.GetType().Name + "\r\n" +
                "\tA = qwe\r\n" +
                "\tB = qwe\r\n";

            var actual = obj.PrintToString(
                cfg => cfg
                .Printing<double>().Using(d => "qwe")
                .Printing(p => p.A).Using(CultureInfo.InvariantCulture));

            actual.Should().Be(expected);
        }

        [Test]
        public void ShouldHandleCircularReferences()
        {
            var father = new Person { Name = "Father" };
            var person = new Person { Name = "Person", Father = father };
            father.Father = person;

            var expected =
                "Person\r\n" +
                "\tFather = Person\r\n" +
                "\t\tFather = * circular reference *\r\n" +
                "\t\tId = Guid\r\n" +
                "\t\tName = Father\r\n" +
                "\tId = Guid\r\n" +
                "\tName = Person\r\n";

            var actual = person.PrintToString(
                cfg => cfg
                .Excluding<int>()
                .Excluding<double>()
                .Excluding(p => p.Mother));

            actual.Should().Be(expected);
        }
    }
}