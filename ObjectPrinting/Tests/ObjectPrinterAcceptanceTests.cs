using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Common;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private static readonly string expectedDemo = "Person" + Environment.NewLine +
            "\t" + "Id = 00000000-0000-0000-0000-000000000000" + Environment.NewLine +
            "\t" + "Name = AL" + Environment.NewLine +
            "\t" + "Height = 0" + Environment.NewLine +
            "\t" + "Parent = loop reference" + Environment.NewLine +
            "\t" + "SubPerson = SubPerson" + Environment.NewLine +
            "\t\t" + "Name = lowercase" + Environment.NewLine +
            "\t\t" + "Values = [" + Environment.NewLine +
            "\t\t\t" + "1," + Environment.NewLine +
            "\t\t\t" + "2," + Environment.NewLine +
            "\t\t\t" + "3" + Environment.NewLine +
            "\t\t" + "]" + Environment.NewLine +
            "\t" + "Doubles = [" + Environment.NewLine +
            "\t\t" + "1.01(TypeSerializer)," + Environment.NewLine +
            "\t\t" + "2.02(TypeSerializer)," + Environment.NewLine +
            "\t\t" + "3.03(TypeSerializer)" + Environment.NewLine +
            "\t" + "]" + Environment.NewLine +
            "\t" + "Dictionary = {" + Environment.NewLine +
            "\t\t" + "{" + Environment.NewLine +
            "\t\t\t" + "Key = ignored" + Environment.NewLine +
            "\t\t\t" + "Value = 2,02" + Environment.NewLine +
            "\t\t" + "}," + Environment.NewLine +
            "\t\t" + "{" + Environment.NewLine +
            "\t\t\t" + "Key = ignored" + Environment.NewLine +
            "\t\t\t" + "Value = 4,04" + Environment.NewLine +
            "\t\t" + "}" + Environment.NewLine +
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