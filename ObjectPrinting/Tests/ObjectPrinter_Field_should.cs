using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Field_should
    {
        private static ClassWithField _class;
        
        [SetUp]
        public void SetUp()
        {
            _class  = new ClassWithField();
        }

        [Test]
        public void PrintToString_WhenField()
        {
            _class.PrintToString().Should().Contain(nameof(_class.IntField));
        }
        
        [Test]
        public void ObjectPrinter_WhenExcludingType()
        {
            _class.PrintToString(c=>c.Excluding<int>()).Should().NotContain(nameof(_class.IntField));
        }

        [Test]
        public void ObjectPrinter_WhenExcludingField()
        {
            _class.PrintToString(c=>c.Excluding(p=>p.IntField)).Should().NotContain(nameof(_class.IntField));
        }
        
        [Test]
        public void ObjectPrinter_For_WhenAlternativeFieldSerial()
        {
            _class.PrintToString(c=>c.AlternativeFor(p=>p.IntField).Using(field=>$"({field})"))
                .Should().Contain($"{nameof(_class.IntField)} = ({_class.IntField})");
        }
        
        [Test]
        public void PrintToString_WhenAlternativeTypeSerial()
        {
            _class.PrintToString(ser => ser.AlternativeFor<int>().Using(field => $"({field})")).
                Should().Contain($"{nameof(_class.IntField)} = ({_class.IntField})");
        }

        [Test]
        public void PrintToString_WhenTakeOnlySerial()
        {
            var person = new ClassWithField(){StrField = "String"};
            person.PrintToString(ser => ser.AlternativeFor<string>().TakeOnly(1)).
                Should().NotContain(person.StrField).And.Contain($"{nameof(person.StrField)} = {person.StrField[0].ToString()}");
        }

        [TestCase("en-GB", 50.5, "50.5")]
        [TestCase("ru-RU", 50.5, "50,5")]
        public void PrintToString_WhenAlternativeCultureSerial(string culture, double height, string expectHeight)
        {
            var person = new ClassWithField(){DoublField = height};
            var result = person.PrintToString(ser => ser.AlternativeFor<double>().Using(new CultureInfo(culture)));
            result.Should().Contain($"{nameof(person.DoublField)} = {expectHeight}");
        }
    }

    internal class ClassWithField
    {
        public int IntField;
        public string StrField;
        public double DoublField;
    }
}