using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

using ObjectPrinting;

namespace Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void PrintAllPropertiesByDefault()
        {
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);

            var serializedInstance = instance.PrintToString();

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1").And
                .Contain("DoubleProperty = 1.1").And
                .Contain("StringProperty = str");
        }

        [Test]
        public void PrintAllFieldByDefault()
        {
            var instance = new ClassWithFieldsStringIntDouble("str", 1, 1.1);

            var serializedInstance = instance.PrintToString();

            serializedInstance.Should().StartWith("ClassWithFieldsStringIntDouble").And
                .Contain("DoubleField = 1.1").And
                .Contain("IntField = 1").And
                .Contain("StringField = str");
        }

        [Test]
        public void NotPrintExcludedTypes()
        {
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Excluding<double>();

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1").And
                .Contain("StringProperty = str").And
                .NotContain("DoubleProperty");
        }

        [Test]
        public void UseAlternativeSerializationFunctionForType_WhenItIsSpecified()
        {
            string SerializationFunc(int i) => i + ".00";
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Printing<int>().Using(SerializationFunc);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1.00").And
                .Contain("DoubleProperty = 1.1").And
                .Contain("StringProperty = str");
        }

        [Test]
        public void UseAlternativeCulture_WhenItIsSpecified()
        {
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);
            var enCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
            var ruCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU");

            var printerWithRuCulture = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Printing<double>().Using(ruCulture);
            var printerWithEnCulture = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Printing<double>().Using(enCulture);

            var instanceSerializedWithEnCulture = printerWithEnCulture.PrintToString(instance);
            var instanceSerializedWithRuCulture = printerWithRuCulture.PrintToString(instance);

            instanceSerializedWithEnCulture.Should().NotBe(instanceSerializedWithRuCulture);
        }

        [Test]
        public void UseAlternativeSerializationFunctionForProperty_WhenItIsSpecified()
        {
            string SerializationFunc(int i) => i + ".00";
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Printing(inst => inst.IntProperty).Using(SerializationFunc);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1.00").And
                .Contain("DoubleProperty = 1.1").And
                .Contain("StringProperty = str");
        }

        [Test]
        public void UseAlternativeSerializationFunctionForField_WhenItIsSpecified()
        {
            string SerializationFunc(int i) => i + ".00";
            var instance = new ClassWithFieldsStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithFieldsStringIntDouble>()
                .Printing(inst => inst.IntField).Using(SerializationFunc);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithFieldsStringIntDouble").And
                .Contain("IntField = 1.00").And
                .Contain("DoubleField = 1.1").And
                .Contain("StringField = str");
        }

        [Test]
        public void TrimStringPropertyToParticularLength_WhenTrimmingFunctionIsSpecified()
        {
            const int trimLength = 5;
            var instance = new ClassWithPropertiesStringIntDouble("HelloWorld", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Printing(inst => inst.StringProperty).TrimmedToLength(trimLength);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1").And
                .Contain("DoubleProperty = 1.1").And
                .Contain("StringProperty = Hello").And
                .NotContain("StringProperty = HelloWorld");
        }

        [Test]
        public void TrimStringFieldToParticularLength_WhenTrimmingFunctionIsSpecified()
        {
            const int trimLength = 5;
            var instance = new ClassWithFieldsStringIntDouble("HelloWorld", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithFieldsStringIntDouble>()
                .Printing(inst => inst.StringField).TrimmedToLength(trimLength);

            var serializedInstance = objectPrinter.PrintToString(instance);
            
            serializedInstance.Should().StartWith("ClassWithFieldsStringIntDouble").And
                .Contain("IntField = 1").And
                .Contain("DoubleField = 1.1").And
                .Contain("StringField = Hello").And
                .NotContain("StringField = HelloWorld");

        }

        [Test]
        public void NotPrintExcludedProperties()
        {
            var instance = new ClassWithPropertiesStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithPropertiesStringIntDouble>()
                .Excluding(inst => inst.DoubleProperty);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithPropertiesStringIntDouble").And
                .Contain("IntProperty = 1").And
                .Contain("StringProperty = str").And
                .NotContain("DoubleProperty");
        }

        [Test]
        public void NotPrintExcludedFields()
        {
            var instance = new ClassWithFieldsStringIntDouble("str", 1, 1.1);
            var objectPrinter = ObjectPrinter.For<ClassWithFieldsStringIntDouble>()
                .Excluding(inst => inst.DoubleField);

            var serializedInstance = objectPrinter.PrintToString(instance);

            serializedInstance.Should().StartWith("ClassWithFieldsStringIntDouble").And
                .Contain("IntField = 1").And
                .Contain("StringField = str").And
                .NotContain("DoubleField");
        }

        [Test]
        public void PrintElementsOfCollection()
        {
            const int minIntValue = 1;
            const int maxIntValue = 3;
            var instances = new List<ClassWithOneIntField>();
            for (int intValue = minIntValue; intValue <= maxIntValue; intValue++)
                instances.Add(new ClassWithOneIntField(intValue));
            var objectPrinter = ObjectPrinter.For<ClassWithOneIntField>();

            var serializedCollection = objectPrinter.PrintToString(instances);

            serializedCollection.Should().Be("List`1\r\n" +
                                             "\tClassWithOneIntField\r\n" +
                                             "\t\tIntField = 1\r\n" +
                                             "\tClassWithOneIntField\r\n" +
                                             "\t\tIntField = 2\r\n" +
                                             "\tClassWithOneIntField\r\n" +
                                             "\t\tIntField = 3\r\n");
        }

        [Test, Timeout(10000)]
        public void CorrectlyPrintObjectsWithInfiniteNesting()
        {
            var instance = new ClassContainsItself();
            Action printingAction = () => instance.PrintToString();
            printingAction.Should().NotThrow();
        }

        [Test, Timeout(10000)]
        public void StopPrintingObjectWithDots_WhenNestingLevelIsMoreThan10()
        {
            var instance = new ClassContainsItself();
            var serializedInstance = instance.PrintToString();
            serializedInstance.Should().EndWith("..." + Environment.NewLine);
        }

        [Test]
        public void PrintNull()
        {
            object instance = null;
            var serializedInstance = instance.PrintToString();
            serializedInstance.Should().Be("null" + Environment.NewLine);
        }

        [Test]
        public void PrintBuiltInTypesLikeSimpleToStringMethod()
        {
            var someValuesWithBuiltInTypes = new object[] {10, 'c', 1.1, "str", new DateTime()};

            foreach (var value in someValuesWithBuiltInTypes)
                value.PrintToString().Should().Be(value + Environment.NewLine);
        }
    }

    internal class ClassWithPropertiesStringIntDouble
    {
        public int IntProperty { get; }
        public double DoubleProperty { get; }
        public string StringProperty { get; }

        internal ClassWithPropertiesStringIntDouble(string s, int i, double d)
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

    internal class ClassWithFieldsStringIntDouble
    {
        public int IntField;
        public double DoubleField;
        public string StringField;

        internal ClassWithFieldsStringIntDouble(string s, int i, double d)
        {
            IntField = i;
            DoubleField = d;
            StringField = s;
        }
    }

    internal class ClassWithOneIntField
    {

        public int IntField;

        internal ClassWithOneIntField(int i)
        {
            IntField = i;
        }
    }
}