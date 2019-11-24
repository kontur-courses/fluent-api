using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person1 = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<DateTime>().Using(d => d.ToString())

                //3. Для числовых типов указать культуру
                .Serializing<byte>().Using(CultureInfo.CurrentCulture)
                .Serializing<short>().Using(CultureInfo.CurrentCulture)
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                .Serializing<long>().Using(CultureInfo.CurrentCulture)
                .Serializing<float>().Using(CultureInfo.CurrentCulture)
                .Serializing<double>().Using(CultureInfo.CurrentCulture)

                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Height).Using(a => a.ToString())

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).WithMaxLength(10)

                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            Person person2 = new Person { Name = "Bob", Age = 20 };
            string s2 = person2.PrintToString();

            //8. ...с конфигурированием
            Person person3 = new Person { Name = "Clara", Age = 21 };
            string s3 = person3
                .GetObjectPrinter()
                .Excluding<Guid>()
                .Excluding(p => p.Age)
                .PrintToString();
        }
    }

    [TestFixture]
    public class ObjectPrinter_Should
    {
        [TestCase("Guid", ExpectedResult = "Person\r\n\tName = Alex\r\n\tHeight = 170\r\n\tAge = 19\r\n")]
        [TestCase("string", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tHeight = 170\r\n\tAge = 19\r\n")]
        [TestCase("int", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 170\r\n")]
        [TestCase("double", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tAge = 19\r\n")]
        public string Exclude_PropsOfAnyType(string typeName)
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            switch (typeName)
            {
                case "Guid":
                    return ObjectPrinter.For<Person>().Excluding<Guid>().PrintToString(person);
                case "string":
                    return ObjectPrinter.For<Person>().Excluding<string>().PrintToString(person);
                case "int":
                    return ObjectPrinter.For<Person>().Excluding<int>().PrintToString(person);
                case "double":
                    return ObjectPrinter.For<Person>().Excluding<double>().PrintToString(person);
                default:
                    throw new NotImplementedException();
            }
        }

        [TestCase("Name", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tHeight = 170\r\n\tAge = 19\r\n")]
        [TestCase("Height", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tAge = 19\r\n")]
        [TestCase("Age", ExpectedResult = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alex\r\n\tHeight = 170\r\n")]
        public string Exclude_AnyProperty(string propName)
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };
            switch (propName)
            {
                case "Name":
                    return ObjectPrinter.For<Person>().Excluding(p => p.Name).PrintToString(person);
                case "Height":
                    return ObjectPrinter.For<Person>().Excluding(p => p.Height).PrintToString(person);
                case "Age":
                    return ObjectPrinter.For<Person>().Excluding(p => p.Age).PrintToString(person);
                default:
                    throw new NotImplementedException();
            }
        }

        [Test]
        public void Use_UserSerializerForAnyType()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };

            ObjectPrinter.For<Person>()
                .Serializing<Guid>()
                .Using(guid => $"{guid.ToString().Substring(0, 3)}...")
                .PrintToString(person)
                .Should().Be("Person\r\n\tId = 000...\r\n\tName = Alex\r\n\tHeight = 170\r\n\tAge = 19\r\n");
        }

        [Test]
        public void Use_UserSerializerForAnyProperty()
        {
            var person = new Person { Id = new Guid(), Name = "Alex", Age = 19, Height = 170 };

            ObjectPrinter.For<Person>()
                .Serializing(p => p.Id)
                .Using(id => $"{id.ToString().Substring(0, 3)}...")
                .PrintToString(person)
                .Should().Be("Person\r\n\tId = 000...\r\n\tName = Alex\r\n\tHeight = 170\r\n\tAge = 19\r\n");
        }

        [TestCase("byte", 33, ExpectedResult = "33,000\r\n")]
        [TestCase("short", 33, ExpectedResult = "33,000\r\n")]
        [TestCase("int", 33, ExpectedResult = "33,000\r\n")]
        [TestCase("long", 33, ExpectedResult = "33,000\r\n")]
        [TestCase("float", 33, ExpectedResult = "33,000\r\n")]
        [TestCase("double", 33, ExpectedResult = "33,000\r\n")]
        public string Use_UserCultureForAnyNumberType(string typeName, byte value)
        {
            var culture = new CultureInfo("ru-ru", false);
            culture.NumberFormat.NumberDecimalDigits = 3;

            switch (typeName)
            {
                case "byte": return ObjectPrinter.For<byte>().Serializing<byte>().Using(culture).PrintToString(value);
                case "short": return ObjectPrinter.For<short>().Serializing<short>().Using(culture).PrintToString(value);
                case "int": return ObjectPrinter.For<int>().Serializing<int>().Using(culture).PrintToString(value);
                case "long": return ObjectPrinter.For<long>().Serializing<long>().Using(culture).PrintToString(value);
                case "float": return ObjectPrinter.For<float>().Serializing<float>().Using(culture).PrintToString(value);
                case "double": return ObjectPrinter.For<double>().Serializing<double>().Using(culture).PrintToString(value);
                default:
                    throw new NotImplementedException();
            }
        }

        [TestCase("1234567890", (ushort)5, ExpectedResult = "12345\r\n")]
        [TestCase("123", (ushort)5, ExpectedResult = "123\r\n")]
        public string Can_TrancateStringProperties(string value, ushort maxLength) =>
            ObjectPrinter.For<string>().Serializing<string>().WithMaxLength(maxLength).PrintToString(value);

        [Test]
        public void Can_TrancateAnyStringProperty()
        {
            var classWithStrings = new ClassWithStrings() { s1 = "1234567890", s2 = "1234567890" };
            ObjectPrinter.For<ClassWithStrings>()
                .Serializing(obj => obj.s2).WithMaxLength(3)
                .PrintToString(classWithStrings)
                .Should().Be("ClassWithStrings\r\n\ts1 = 1234567890\r\n\ts2 = 123\r\n");
        }

        [Test]
        public void Support_Collections()
        {
            var obj1 = new ClassWithStrings { s1 = "12345", s2 = "67890" };
            var obj2 = new ClassWithStrings { s1 = "09876", s2 = "54321" };
            var sb = new StringBuilder();

            var arr = new ClassWithStrings[] { obj1, obj2 };
            sb.Append(arr.PrintToString());

            var dictionary = new Dictionary<int, ClassWithStrings> { { 0, obj1 }, { 1, obj2 } };
            sb.Append(dictionary.PrintToString());

            var list = new List<ClassWithStrings> { obj1, obj2 };
            sb.Append(list.PrintToString());

            var asmLocation = Assembly.GetAssembly(typeof(ObjectPrinter_Should)).Location;
            var path = Path.GetDirectoryName(asmLocation);
            var filename = path + @"\..\..\CollectionsPrintingOutput.txt";
            File.WriteAllText(filename, sb.ToString());
        }

        [Test]
        [Description("This test never fails, because StackOverflowException can't be catched!!! See output file!")]
        public void Support_CircularReferences()
        {
            var obj1 = new ClassWithCircularReference { ObjectName = "obj1" };
            var obj2 = new ClassWithCircularReference { ObjectName = "obj2" };
            var obj3 = new ClassWithCircularReference { ObjectName = "obj3" };
            obj1.OtherObject = obj2;
            obj2.OtherObject = obj3;
            obj3.OtherObject = obj1;

            var asmLocation = Assembly.GetAssembly(typeof(ObjectPrinter_Should)).Location;
            var path = Path.GetDirectoryName(asmLocation);
            var filename = path + @"\..\..\ObjectsWithCircularReferencesOutput.txt";
            if (File.Exists(filename)) File.Delete(filename);

            string output = string.Empty;
            Assert.DoesNotThrow(() => output = obj1.PrintToString());

            File.WriteAllText(filename, output);
        }
    }
}