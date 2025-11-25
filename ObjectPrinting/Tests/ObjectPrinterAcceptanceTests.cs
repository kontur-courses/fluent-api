using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void DemonstrationTest()
        {
            var person = new Person { Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            person.Phone = new Phone {Name = "Смартфон Vivo", Owner = person};
            PrintingConfig<Person>.SerializerDelegate phoneSerializer
                = (obj, nestingLevel, deepnessLevel, path) => "MyPhone" + Environment.NewLine;

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludePropertyOfType<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .WithTypeSerializationStyle<Phone>(phoneSerializer)
                //3. Для числовых типов указать культуру
                .SetTypeCulture<double>(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .GetPropertyByName(x => x.Name) 
                    .SetSerializationStyle((obj, nestingLevel, deepnessLevel, path) => "MyName")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .GetPropertyByName(x => x.Surname).TrimStringToLength(3)
                //6. Исключить из сериализации конкретное свойства
                .GetPropertyByName(x => x.Id).Exclude();
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            Console.WriteLine("\n\n\n" + s2);
            //8. ...с конфигурированием
            var s3 = person.PrintToString(x => x.GetPropertyByName(p => p.Name).TrimStringToLength(3));
            Console.WriteLine("\n\n\n" + s3);
        }

        [Test]
        public void ExcludeType_ShouldRemove_PropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .ExcludePropertyOfType<Guid>()
                .ExcludePropertyOfType<double>()
                .ExcludePropertyOfType<int>()
                .ExcludePropertyOfType<Phone>();

            var expected =
                "Person\r\n" +
                "\tName = Alex\r\n" +
                "\tSurname = Smith\r\n";
            
            var ans = printer.PrintToString(person);
            ans.Should().Be(expected);
        }

        [Test]
        public void WithTypeSerializationStyle_ShouldSetUniqueSerializationStyle_ToPropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            person.Phone = new Phone {Name = "Смартфон Vivo"};
            var printer = ObjectPrinter
                .For<Person>()
                .WithTypeSerializationStyle<Guid>((obj, nestingLevel, deepnessLevel, path) => "999999999" + Environment.NewLine)
                .WithTypeSerializationStyle<Phone>((obj, nestingLevel, deepnessLevel, path) => "My phone" + Environment.NewLine);
            var ans = printer.PrintToString(person);
            
            var expected =
                "Person\r\n\t" +
                "Id = 999999999\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Phone = My phone\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void SetTypeCulture_ShouldSetTypeCulture_ToPropertiesWithThisType()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .SetTypeCulture<double>(CultureInfo.CurrentCulture);
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        // [Test]
        // public void GetPropertyByName_ShouldReturn_PropertyInfoByName()
        // {
        //     var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
        //     var printer = ObjectPrinter
        //         .For<Person>()
        //         .GetPropertyByName(x => x.Id);
        //     var ans = printer.PropertyName;
        //     
        //     var expected = "Id";
        //     
        //     ans.Should().Be(expected);
        // }

        [Test]
        public void SetSerializationStyle_ShouldSetUniqueSerializationStyle_ToGotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Name)
                .SetSerializationStyle((obj, nestingLevel, deepnessLevel, path) => "My name" + Environment.NewLine);
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = My name\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void TrimStringToLength_ShouldTrimPropertyLength_ToGotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Very long surname", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Surname).TrimStringToLength(5);
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Very \r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void Exclude_ShouldRemove_GotProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Phone).Exclude();
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void PrivateFieldsAndProperties_ShouldBeSerialized()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var secretCode = "1";
            person.SetSecretCode(secretCode);
            person.PassportName = "passport";
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Phone).Exclude();
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "PassportName = passport\r\n\t" +
                "Age = 19\r\n\t" +
                $"SecretCode = {secretCode}\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void SetSerializeStyle_ForFieldOrProperty_ShouldAffect_OnlyOnThisFieldOrProperty()
        {
            var person = new Person {Name = "Alex", Surname = "Smith", Height = 180.5, Age = 19};
            var phone = new Phone {Name = "Phone"};
            person.Phone = phone;
            var printer = ObjectPrinter
                .For<Person>()
                .GetPropertyByName(x => x.Name)
                .SetSerializationStyle((obj, nestingLevel, deepnessLevel, path) => "My name");
            var ans = printer.PrintToString(person);
            
            var baseGuid = Guid.Empty.ToString();
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = My name\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Phone = Phone\r\n\t\t" +
                $"Id = {baseGuid}\r\n\t\t" +
                "Name = Phone\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void LoopReferencesTest()
        {
            var baseGuid = Guid.Empty.ToString();
            var person = new Person {Name = "Alex", Surname = "Smith", Age = 19, Height = 180.5};
            var phone = new Phone {Name = "My phone", Owner = person};
            person.Phone = phone;
            var printer = ObjectPrinter
                .For<Person>();
            var ans = printer.PrintToString(person);
            
            var expected =
                "Person\r\n\t" +
                $"Id = {baseGuid}\r\n\t" +
                "Name = Alex\r\n\t" +
                "Surname = Smith\r\n\t" +
                "Height = 180,5\r\n\t" +
                "Phone = Phone\r\n\t\t" +
                $"Id = {baseGuid}\r\n\t\t" +
                "Name = My phone\r\n\t\t" +
                "Owner = Person\r\n\t\t\t" +
                $"Id = {baseGuid}\r\n\t\t\t" +
                "Name = Alex\r\n\t\t\t" +
                "Surname = Smith\r\n\t\t\t" +
                "Height = 180,5\r\n\t\t\t" +
                "Phone = Deepness exceeded!\r\n\t\t\t" +
                "Age = 19\r\n\t" +
                "Age = 19\r\n";
            
            ans.Should().Be(expected);
        }

        [Test]
        public void Array_ShouldBeSerializedAs_JSON()
        {
            var array = new int[]{1, 2, 3};
            var printer = ObjectPrinter
                .For<int[]>()
                .WithTypeSerializationStyle<int>((obj, nestingLevel, deepnessLevel, path) =>
                    obj.GetType() + Environment.NewLine);
            var ans = printer.PrintToString(array);

            var expected = new StringBuilder();
            expected.Append("Int32[]\r\n");
            foreach(var i in array)
                expected.Append("\t" + i.GetType() + "\r\n");
            
            ans.Should().Be(expected.ToString());
        }

        [Test]
        public void List_ShouldBeSerializedAs_JSON()
        {
            var list = new List<int> {1, 2, 3};
            var printer = ObjectPrinter
                .For<List<int>>();
            var ans = printer.PrintToString(list);

            var expected = new StringBuilder();
            expected.Append("List`1\r\n");
            foreach(var i in list)
                expected.Append("\t" + i.ToString() + "\r\n");
            
            ans.Should().Be(expected.ToString());
        }

        [Test]
        public void Dictionary_ShouldBeSerializedAs_JSON()
        {
            var dict = new Dictionary<int, string>
            {
                {1, "one"},
                {2, "two"},
                {3, "three"}
            };
            var printer = ObjectPrinter
                .For<Dictionary<int, string>>()
                .WithTypeSerializationStyle<int>((obj, nestingLevel, deepnessLevel, path) => "number");
            var ans = printer.PrintToString(dict);

            var expected = new StringBuilder();
            expected.Append("Dictionary`2\r\n");
            foreach(var i in dict)
                expected.Append("\t" + "number" + " = " + i.Value + "\r\n");
            
            ans.Should().Be(expected.ToString());
        }

        [Test]
        public void PropertyPrintingConfig_ShouldValidate_NullSelector()
        {
            var config = new PrintingConfig<Person>();
            Action test = () => new PropertyPrintingConfig<Person, string>(config, null);
            test.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void PropertyPrintingConfig_ShouldValidate_NotFieldOrPropertySelector()
        {
            var config = new PrintingConfig<Person>();
            Action test = () => new PropertyPrintingConfig<Person, string>(config, a => "aaaa");
            test.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PropertyPrintingConfig_ShouldValidate_ExternalVariables()
        {
            var sd = new Rectangle();
            var config = new PrintingConfig<Person>();

            Action test = () => new PropertyPrintingConfig<Person, int>(config, x => sd.Bottom);
            test.Should().Throw<ArgumentException>();
        }
    }
}