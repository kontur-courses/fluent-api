using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    internal class ObjectPrinterTests
    {
        [SetUp]
        public void SetUp()
        {
            _marks = new[] {2, 2, 2, 5};
            _person = new Person("Alex", 15, 123.1, _marks) {NickName = "commonNickName", Width = 100.1};
            _printer = ObjectPrinter.For<Person>();
        }

        private static Person _person;
        private static PrintingConfig<Person> _printer;
        private static int[] _marks;

        [Test]
        public void ObjectPrinter_CircleField_CanBeSerializedWithoutException()
        {
            _person.BestFriend = _person;

            var line = _person.PrintToString();
            var firstBestFriendEnd = line.IndexOf("bestFriend", StringComparison.Ordinal) + "bestFriend".Length;
            var lineAfterFirstFriend = line.Substring(firstBestFriendEnd, line.Length - firstBestFriendEnd);

            lineAfterFirstFriend.Should().NotContain("bestFriend");
        }

        [Test]
        public void ObjectPrinter_FieldSerialization_CanBeExcluded()
        {
            var line = _printer.Excluding(p => p.NickName).PrintToString(_person);

            line.Should().NotContain("commonNickName");
        }

        [Test]
        public void ObjectPrinter_FinalTypes_SerializeLikeCommon()
        {
            const double number = 123.123;
            var line = number.PrintToString();
            var lineWithoutCommonValue = line.Replace(number.ToString(CultureInfo.CurrentCulture), "");

            line.Should().Contain(number.ToString(CultureInfo.CurrentCulture));
            lineWithoutCommonValue.Should().NotContain(number.ToString(CultureInfo.CurrentCulture));
        }

        [Test]
        public void ObjectPrinter_IenumerableFieldSerialization_ContainAllElements()
        {
            var line = _person.PrintToString();

            foreach (var mark in _marks) line.Should().Contain(mark.ToString());
        }

        [Test]
        public void ObjectPrinter_IFormattableFieldCulture_CanBeChanged()
        {
            var line = _printer.For<double>().WithCulture(CultureInfo.GetCultureInfo(1034)).PrintToString(_person);

            line.Should().Contain("Width = 100,1");
        }

        [Test]
        public void ObjectPrinter_IFormattablePropertyCulture_CanBeChanged()
        {
            var line = _printer.For<double>().WithCulture(CultureInfo.GetCultureInfo(1034)).PrintToString(_person);

            line.Should().Contain("Height = 123,1");
        }

        [Test]
        public void ObjectPrinter_PropertySerialization_CanBeExcluded()
        {
            var line = _printer.Excluding(p => p.Name).PrintToString(_person);

            line.Should().NotContain("Alex");
        }

        [Test]
        public void ObjectPrinter_Settings_CanChangeSerialization()
        {
            var line = _person.PrintToString(settings =>
                settings.For<string>().WithSerialization(p => p.ToString() + "changed"));

            line.Should().Contain("changed");
        }

        [Test]
        public void ObjectPrinter_StringField_CanBeTrim()
        {
            var line = _printer.For(p => p.Name).Trim(3).PrintToString(_person);

            line.Should().NotContain("Alex");
            line.Should().Contain("Ale");
        }

        [Test]
        public void ObjectPrinter_TypeFieldSerialization_CanBeChanged()
        {
            var line = _printer.For<string>().WithSerialization(p => p.ToString() + " string changed")
                .PrintToString(_person);

            line.Should().Contain("Alex string changed");
        }

        [Test]
        public void ObjectPrinter_TypePropertySerialization_CanBeChanged()
        {
            var line = _printer.For<string>().WithSerialization(p => p.ToString() + " string changed")
                .PrintToString(_person);

            line.Should().Contain("commonNickName string changed");
        }

        [Test]
        public void ObjectPrinter_TypeSerialization_CanBeExcluded()
        {
            var line = _printer.Excluding<string>().PrintToString(_person);

            line.Should().NotContain("Alex");
            line.Should().NotContain("commonNickName");
        }

        [Test]
        public void ObjectPrinter_SyntaxSugar_CanBeUsedLikeCommonMethods()
        {
            var firstLine=_printer.PrintToString(_person);
            var secondLine=_person.PrintToString();

            firstLine.Should().Be(secondLine);
        }
    }
}