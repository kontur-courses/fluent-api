using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 199999, Height =  1.228 };
        }
        
        [Test]
        public void ObjPrinter_ExcludingType_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void StandartObjectPrinter_ShouldNotWorkExceptions()
        {
            var printer = ObjectPrinter.For<Person>();
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_UsingFormatType_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => String.Format("{0} : {1}", x, x.Length));
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_UsingCultureInfoForType_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("de-DE"))
                .Printing<int>().Using(CultureInfo.GetCultureInfo("de-DE"));
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_UsingFuncForProperty_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                    .Printing(x => x.Name).Using(x => String.Format("{0} : {1}", x, x.Length+"ababa"));
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_Excluding_WorksCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Printing(x => x.Name).TrimmedToLength(3);

            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_Trim_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(3);

            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_ExcludingProperty_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);

            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void AcceptanceTests()
        {
            var printer = ObjectPrinter.For<Person>();
//                .Printing<double>().Using(x => String.Format("{0} : {1}", x, x))
//                .Printing<double>().Using(CultureInfo.GetCultureInfo("de-DE"))
//                .Printing<int>().Using(CultureInfo.GetCultureInfo("de-DE"))
//                 .Printing(x => x.Name).Using(x => String.Format("{0} : {1}", x, x.Length+"ababa"));
//                 .Printing(x => x.Name).TrimmedToLength(3)
//                .Excluding(x => x.Name);
                
                //.Serializing<DateTime>().Using(d => d.toString());
                // Serializing(x => x.Name).Using();
            
            //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
            
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}