using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

using static System.Environment;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private static readonly string expectedDemo = "Person" + NewLine +
            "\t" + "Id = 00000000-0000-0000-0000-000000000000" + NewLine +
            "\t" + "Name = AL" + NewLine +
            "\t" + "Height = 0" + NewLine +
            "\t" + "Parent = loop reference" + NewLine +
            "\t" + "SubPerson = SubPerson" + NewLine +
            "\t\t" + "Name = lowercase" + NewLine +
            "\t\t" + "Values = [" + NewLine +
            "\t\t\t" + "1," + NewLine +
            "\t\t\t" + "2," + NewLine +
            "\t\t\t" + "3" + NewLine +
            "\t\t" + "]" + NewLine +
            "\t" + "Doubles = [" + NewLine +
            "\t\t" + "1.01(TypeSerializer)," + NewLine +
            "\t\t" + "2.02(TypeSerializer)," + NewLine +
            "\t\t" + "3.03(TypeSerializer)" + NewLine +
            "\t" + "]" + NewLine +
            "\t" + "Dictionary = {" + NewLine +
            "\t\t" + "{" + NewLine +
            "\t\t\t" + "Key = ignored" + NewLine +
            "\t\t\t" + "Value = 2,02" + NewLine +
            "\t\t" + "}," + NewLine +
            "\t\t" + "{" + NewLine +
            "\t\t\t" + "Key = ignored" + NewLine +
            "\t\t\t" + "Value = 4,04" + NewLine +
            "\t\t" + "}" + NewLine +
            "\t" + "}";

        [Test]
        public void DemoWithPrinter()
        {
            var person = GetPerson();

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeTypeAs<double>(number => number.ToString() + "(TypeSerializer)")
                //3. Для числовых типов указать культуру
                .SetTypeCulture<float>(CultureInfo.GetCultureInfo("ru"))
                //4. Настроить сериализацию конкретного свойства
                .ConfigurePropertySerialization(person => person.Name)
                    .SetSerializer(name => name.ToUpper())
                    //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .SetMaxLength(2)
                .ConfigurePropertySerialization(person => person.Height)
                    .SetSerializer(height => height.ToString())
                //6. Исключить из сериализации конкретного свойства
                .Exclude(person => person.Age);

            var actual = printer.PrintToString(person);
            actual.Should().Be(expectedDemo);
        }

        [Test]
        public void DemoWithObjectExtension()
        {
            var person = GetPerson();

            var actual = person.PrintToString(config => config.Exclude<int>()
                                                              .SerializeTypeAs<double>(n => n.ToString() + "(TypeSerializer)")
                                                              .SetTypeCulture<float>(CultureInfo.GetCultureInfo("ru"))
                                                              .ConfigurePropertySerialization(person => person.Name)
                                                                  .SetSerializer(name => name.ToUpper())
                                                                  .SetMaxLength(2)
                                                              .ConfigurePropertySerialization(person => person.Height)
                                                                  .SetSerializer(height => height.ToString())
                                                              .Exclude(person => person.Age));

            actual.Should().Be(expectedDemo);
        }

        private static Person GetPerson()
        {
            var person = new Person()
            {
                Name = "Alex",
                Age = 19,
                Doubles = new double[] { 1.01, 2.02, 3.03 }
            };
            person.Parent = person;
            person.SubPerson = new SubPerson()
            {
                Name = "lowercase",
                Values = new long[] { 1, 2, 3 }
            };
            return person;
        }
    }
}