using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using FluentAssertions;
using Microsoft.VisualBasic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ObjectPrinting.Solved;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private string text;
        private Person person = new Person
        {
            Name = "Bill",
            Height = 175.3,
            Age = 35
        };

        [Test]
        public void PrintingConfig_EmptyInput_ShouldReturnEmptyLine()
        {
            var common = "";
            var config = ObjectPrinter.For<string>();

            text = config.PrintToString(common, 10);
            var expected = "";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_StringInput_ShouldReturnSameString()
        {
            var common = "some text";
            var config = ObjectPrinter.For<string>();

            text = config.PrintToString(common, 10);
            var expected = "some text";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_MassiveInput()
        {
            var common = new [] { 2.0, 5.3, 10.154, 13.15, -1.65 };
            var config = ObjectPrinter.For<double[]>();

            text = config.PrintToString(common, 10);
            var expected = @"(coll) 
[2, 5,3, 10,154, 13,15, -1,65]";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_ListInput()
        {
            var common = new List<int>() { 2, 5, 10, 13, -1 };
            var config = ObjectPrinter.For<List<int>>();

            text = config.PrintToString(common, 10);
            var expected = @"(coll) 
[2, 5, 10, 13, -1]";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DictionaryIntIntInput()
        {
            var common = new Dictionary<int, int>()
            {
                [2] = 5,
                [10] = 13,
                [4] = -1
            };
            var config = ObjectPrinter.For<Dictionary<int, int>>();

            text = config.PrintToString(common, 10);
            var expected = @"(dict) 
    2 = 5;
    10 = 13;
    4 = -1;";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DictionaryStringIntInput()
        {
            var common = new Dictionary<string, int>()
            {
                ["five"] = 5,
                ["thirteen"] = 13,
                ["minus one"] = -1
            };
            var config = ObjectPrinter.For<Dictionary<string, int>>();

            text = config.PrintToString(common, 10);
            var expected = "(dict) \n" +
                           "\tfive = 5;\n" +
                           "\tthirteen = 13;\n" +
                           "\tminus one = -1;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_ListOfPersonInput()
        {
            var common = new List<Person>() { person, person, person };
            var config = ObjectPrinter.For<List<Person>>();

            text = config.PrintToString(common, 10);
            var expected = "(coll) \n" +
                           "[Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n" +
                           ", Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n" +
                           ", Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n" +
                           "]";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DeleteIntTypeFromPerson()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<int>().IgnoreType();
            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_IgnoreIntPropertyThanOverrideIntType_ShouldIgnoreAllInt()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<int>()
                .IgnoreType()
                .SelectProperty(x => x.Age)
                .PrintAs(x => x + " полных лет");
            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_OverrideStringTypeThanOverrideStringProp_ShouldProcessWithProp()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<string>()
                .PrintAs(x => x.ToUpper())
                .SelectProperty(x => x.Name)
                .PrintAs(x => x.ToLower());
            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = bill;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_DeletePropertyFromPerson()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectProperty(x => x.Name).IgnoreProperty();
            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_CustomTypeSerialize()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectType<string>().PrintAs(x => x.ToUpper());
            person.FriendsNames = new List<string>() { "Alex", "Allay", "King" };

            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = BILL;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35;\n" +
                           "\t(coll) FriendsNames: \n" +
                           "\t\t[ALEX, ALLAY, KING];\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_CustomPropSerialize()
        {
            var config = ObjectPrinter.For<Person>();

            config.SelectProperty(x => x.Age).PrintAs(x => x + " полных лет");

            text = config.PrintToString(person, 10);

            var expected = "Person:\n" +
                           "\tId = 00000000-0000-0000-0000-000000000000;\n" +
                           "\tName = Bill;\n" +
                           "\tHeight = 175,3;\n" +
                           "\tAge = 35 полных лет;\n";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_SetCultureForDouble()
        {
            var config = ObjectPrinter.For<double>();

            config.SelectType<double>().SetCulture(CultureInfo.InvariantCulture);

            text = config.PrintToString(153.2, 10);

            var expected = "153.2";

            text.Should().Be(expected);
        }

        [Test]
        public void PrintingConfig_BigObject()
        {
            var person = new Person
            {
                Name = "Bill",
                Age = 35,
                ArmsLenght = new int[] { 540, 600 },
                FriendsNames = new List<string>() { "Rick", "Morgan", "Alex" },
                Dad = new Person
                {
                    Name = "Alex",
                    Age = 65,
                    Awards = new Dictionary<string, int>()
                    {
                        ["Mayor"] = 40,
                        ["President"] = 55
                    }
                },
                Awards = new Dictionary<string, int>()
                {
                    ["Best friend"] = 12,
                    ["Best dad"] = 25,
                    ["Best worker"] = 29
                }
            };

            var printer = ObjectPrinter.For<Person>();
       
            printer.SelectType<string>().IgnoreType()
                .SelectType<char>().IgnoreType()
            //2. Указать альтернативный способ сериализации для определенного типа
                .SelectType<string>().PrintAs(x => $"№{x}")
            //3. Для числовых типов указать культуру
                .SelectType<int>().SetCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства                
                .SelectProperty(x => x.Age).PrintAs(x => $"№{x}")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SelectProperty(x => x.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
                .SelectProperty(x => x.Name).IgnoreProperty();

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            text = person.PrintToString();

            //8. ...с конфигурированием
            //var configLine = person.PrintToString(s => s.Excluding(p => p.Age));
      

        }

        [TearDown]
        public void TearDown()
        {
            var testCont = TestContext.CurrentContext;

            var fileName = testCont.Test.ID;
            File.WriteAllText(@"..\\text.txt", text);
            text = null;
        }
    }
}