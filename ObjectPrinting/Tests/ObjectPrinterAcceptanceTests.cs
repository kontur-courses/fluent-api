using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class TestClass
    {
        public int number { get; set; }
        public TestClass Parent { get; set; }
    }

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
            var printer1 = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-US"));
            var printer2 = ObjectPrinter.For<Person>()
                    .Printing<double>().Using(CultureInfo.GetCultureInfo("de-DE"));
            string s1 = printer1.PrintToString(person);
            string s2 = printer2.PrintToString(person);
            Console.WriteLine(s1);
            Console.WriteLine(s2);
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
        public void ObjPrinter_IEnumerable_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<List<Person>>();
            var lp = new List<Person> {person, person};
            string s1 = printer.PrintToString(lp);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_CircleReference_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<TestClass>();
            var first = new TestClass(){ number = 1, Parent = null };
            var second = new TestClass(){ number = 2, Parent = first };
            first.Parent = second;
            
            string s1 = printer.PrintToString(first);
            
            Console.WriteLine(s1);
        }
        
        [Test]
        public void AcceptanceTests()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(x => String.Format("{0} : {1}", x, x))
                .Printing<double>().Using(CultureInfo.GetCultureInfo("de-DE"));
//                .Printing<int>().Using(CultureInfo.GetCultureInfo("de-DE"))
//                 .Printing(x => x.Name).Using(x => String.Format("{0} : {1}", x, x.Length+"ababa"));
//                 .Printing(x => x.Name).TrimmedToLength(3)
//                .Excluding(x => x.Name);

            //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
                //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
                //8. ...с конфигурированием
                
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
        }
    }
}