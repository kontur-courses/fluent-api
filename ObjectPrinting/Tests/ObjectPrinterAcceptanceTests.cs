using System;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting.Solved;


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
                .SetCulture<int>(CultureInfo.CurrentCulture)
                .ExcludeProperty(typeof(Person).GetProperty("Name")).
                //4. Настроить сериализацию конкретного свойства
                SerializeProperty("Age").SetSerialization(x => x + " лет")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SelectString("Name").Crop(10);
                //6. Исключить из сериализации конкретного свойства
                //.ExcludeProperty(p => p.name);
            
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
        
        [Test]
        public void Serialization_CropInt_ThrowException()
        {
            var person = new Person { Name = "Alex Ivanov", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Age").Crop(4);
            
            Action act = () => printer.PrintToString(person);

            act.Should().Throw<ArgumentException>();
        }
        
        
        [Test]
        public void Serialization_CropShortString()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Name").Crop(10);
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_CropString()
        {
            var person = new Person { Name = "Alex Ivanov", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SelectString("Name").Crop(4);
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_AgeAndHeight()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeProperty("Age").SetSerialization(x => x + " years old")
                .SerializeProperty("Height").SetSerialization(x => x + " kg");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tHeight = 70.5 kg" + Environment.NewLine +
                            "\tAge = 19 years old" + Environment.NewLine;
            s1.Should().Be(result);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void Serialization_Age()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeProperty("Age").SetSerialization(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
            s1.Should().Be(result);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludePropertyNameAndExcludeTypeInt()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(typeof(Person).GetProperty("Name"))
                .ExcludeType<int>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void ExcludeProperty_Name()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeProperty(typeof(Person).GetProperty("Name"));
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void SetCulture_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70.5, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SetCulture<double>(CultureInfo.CurrentCulture);
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70.5" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            
            //Console.WriteLine(s1);
        }

        [Test]
        public void ExcludeType_Int()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
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
        public void SerializeType_String()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<string>(x => x.ToUpper());
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = ALEX" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        [Test]
        public void SerializeType_Int()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<int>(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
            s1.Should().Be(result);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void SerializeType_Double()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<double>(x => x + " кг");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70 кг" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
        
        [Test]
        public void SerializeType_DoubleAndInt()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 70, Id = new Guid()};

            var printer = ObjectPrinter.For<Person>()
                .SerializeType<double>(x => x + " кг")
                .SerializeType<int>(x => x + " лет");
            
            string s1 = printer.PrintToString(person);
            string result = "Person" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tName = Alex" + Environment.NewLine +
                            "\tHeight = 70 кг" + Environment.NewLine +
                            "\tAge = 19 лет" + Environment.NewLine;
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
                            "\tName = Alex" + Environment.NewLine +
                            "\tId = Guid" + Environment.NewLine +
                            "\tAge = 19" + Environment.NewLine;
            s1.Should().Be(result);
            
            //Console.WriteLine(s1);
        }
    }
}