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
            var _class = new ClassWithField {StrField = "String"};
            _class.PrintToString(ser => ser.AlternativeFor<string>().TakeOnly(1)).
                Should().NotContain(_class.StrField).And.Contain($"{nameof(_class.StrField)} = {_class.StrField[0].ToString()}");
        }

        [TestCase("en-GB", 50.5, "50.5")]
        [TestCase("ru-RU", 50.5, "50,5")]
        public void PrintToString_WhenAlternativeCultureSerial(string culture, double height, string expectHeight)
        {
            var _class = new ClassWithField {DoublField = height};
            var result = _class.PrintToString(ser => ser.AlternativeFor<double>().Using(new CultureInfo(culture)));
            result.Should().Contain($"{nameof(_class.DoublField)} = {expectHeight}");
        }
    }

    internal class ClassWithField
    {
        public int IntField;
        public string StrField;
        public double DoublField;
    }
}