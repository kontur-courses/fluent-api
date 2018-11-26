using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void PrintAllPropertiesByDefault()
        {
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);

            var serializedInstance = instance.PrintToString();

            serializedInstance.Should()
                .Contain(nameof(instance.StringProperty)).And
                .Contain(nameof(instance.IntProperty)).And
                .Contain(nameof(instance.DoubleProperty));

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void NotPrintExcludedTypes()
        {
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Excluding<double>();

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should()
                .Contain(nameof(instance.StringProperty)).And
                .Contain(nameof(instance.IntProperty)).And
                .NotContain(nameof(instance.DoubleProperty));

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void UseAlternativeSerializationFunctionForType_WhenItIsSpecified()
        {
            string SerializationFunc(int i) => i + ".00";
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Printing<int>().Using(SerializationFunc);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().Contain(SerializationFunc(instance.IntProperty));

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void UseAlternativeCulture_WhenItIsSpecified()
        {
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);
            var printerWithRuCulture = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU"));
            var printerWithEnCulture = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Printing<double>().Using(CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"));

            var instanceSerializedWithEnCulture = printerWithEnCulture.PrintToString(instance);
            var instanceSerializedWithRuCulture = printerWithRuCulture.PrintToString(instance);

            instanceSerializedWithEnCulture.Should().NotBe(instanceSerializedWithRuCulture);

            Console.WriteLine(instanceSerializedWithRuCulture);
            Console.WriteLine(instanceSerializedWithEnCulture);
        }

        [Test]
        public void UseAlternativeSerializationFunctionForProperty_WhenItIsSpecified()
        {
            string SerializationFunc(int i) => i + ".00";
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Printing(inst => inst.IntProperty).Using(SerializationFunc);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().Contain(SerializationFunc(instance.IntProperty));

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void TrimStringPropertyToParticularLength_WhenTrimmingFunctionIsSpecified()
        {
            const int trimLength = 5;
            var instance = new ClassWithStringIntDouble("HelloWorld", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Printing(inst => inst.StringProperty).TrimmedToLength(trimLength);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().
                Contain(instance.StringProperty.Substring(0, trimLength)).And
                .NotContain(instance.StringProperty);

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void NotPrintExcludedProperties()
        {
            var instance = new ClassWithStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>()
                .Excluding(inst => inst.DoubleProperty);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should()
                .Contain(nameof(instance.StringProperty)).And
                .Contain(nameof(instance.IntProperty)).And
                .NotContain(nameof(instance.DoubleProperty));

            Console.WriteLine(serializedInstance);
        }

        [Test]
        public void PrintElementsOfCollection()
        {
            const int minIntValue = 1;
            const int maxIntValue = 9;
            const double doubleValue = 0.9;
            const string stringValue = "str";
            var instances = new List<ClassWithStringIntDouble>();
            for (int intValue = minIntValue; intValue < maxIntValue; intValue++)
                instances.Add(new ClassWithStringIntDouble(stringValue, intValue, doubleValue));
            var objectPrinter = ObjectPrinter.For<ClassWithStringIntDouble>();

            var serializedCollection = objectPrinter.PrintToString(instances);

            for (int intValue = 0; intValue < maxIntValue; intValue++)
                serializedCollection.Should()
                    .Contain(intValue.ToString());

            Console.WriteLine(serializedCollection);
        }

        [Test, Timeout(10000)]
        public void CorrectlyPrintObjectsWithInfiniteNesting()
        {
            var instance = new ClassContainsItself();
            Action printingAction = () => instance.PrintToString();
            printingAction.Should().NotThrow();
        }
    }

    internal class ClassWithStringIntDouble
    {
        public int IntProperty { get; }
        public double DoubleProperty { get; }
        public string StringProperty { get; }

        internal ClassWithStringIntDouble(string s, int i, double d)
        {
            IntProperty = i;
            DoubleProperty = d;
            StringProperty = s;
        }
    }

    internal class ClassContainsItself
    {
        public ClassContainsItself Self { get; }

        internal ClassContainsItself()
        {
            Self = this;
        }
    }
}