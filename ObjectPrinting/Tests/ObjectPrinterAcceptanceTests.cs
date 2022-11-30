using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using FluentAssertions;


namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeType<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeType<string>(x => x.ToUpper())
                //3. Для числовых типов указать культуру
                .SetCulture<int>(CultureInfo.CurrentCulture);
                //4. Настроить сериализацию конкретного свойства
                //.SerializeProperty(p.).SetSerialization(x => { })
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //.SelectString(p => p.name).Crop(10)
                //6. Исключить из сериализации конкретного свойства
                //.ExcludeProperty(p => p.name);
            
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void ExcludeType_Int()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine;
            s1.Should().Be(result);
        }
        
        [Test]
        public void ExcludeType_String()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<string>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludeType_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<double>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
    }
}