using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class TestClass
    {
        public int number { get; set; }
        public TestClass Parent { get; set; }
    }

    class StringContainer
    {
        public string StringInner { get; set; }
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
        public void ObjPrinter_ExcludingType_ShouldNotThrowExceptions_AndWorksCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();
            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().NotContain("Guid");
            Console.WriteLine(res);
        }
        
        [Test]
        public void StandartObjectPrinter_ShouldNotThrowExceptions_AndWorksCorrect()
        {
            var printer = ObjectPrinter.For<Person>();
            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            Console.WriteLine(res);
        }
        
        
        [Test]
        public void ObjPrinter_UsingFormatType_ShouldNotThrowExceptions_And_WorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => String.Format("{0} : {1}", x, x.Length));
            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().Contain($"{person.Name} : {person.Name.Length}");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_UsingCultureInfoForType_ShouldNotThrowExceptions()
        {
            var printer1 = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-US"));
            var printer2 = ObjectPrinter.For<Person>()
                    .Printing<double>().Using(CultureInfo.GetCultureInfo("de-DE"));
            var res1 = "";
            var res2 = "";
            Action act1 =  () => res1= printer1.PrintToString(person);
            Action  act2 = () => res2 = printer2.PrintToString(person);
            act1.Should().NotThrow();
            act2.Should().NotThrow();
            Console.WriteLine(res1);
            Console.WriteLine(res2);
        }
        
        [Test]
        public void ObjPrinter_UsingFuncForProperty_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                    .Printing(x => x.Name).Using(x => String.Format("{0} : {1}", x, x.Length+"ababa"));
            var res = "";
            Action act = ()  => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().Contain($" {person.Name} : {person.Name.Length}ababa");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_UsingCultureInfoForProperty_ShouldNotThrowExceptions_And_WorkCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Height).Using(CultureInfo.GetCultureInfo("en-US"));
            person.Money = 1.228;
            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().Contain($"Height = {person.Height.ToString(null, CultureInfo.GetCultureInfo("en-US"))}")
                .And.Contain($"Money = {person.Money}");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_ExcludingChain_WorksCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Printing(x => x.Name).TrimmedToLength(3);

            string s1 = printer.PrintToString(person);
            s1.Should().NotContain("Name");
            Console.WriteLine(s1);
        }
        
        [Test]
        public void ObjPrinter_Trim_ShouldNotThrowExceptions_And_WorksCorrect()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(person.Name.Length-1);
            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().Contain($"{person.Name.Substring(0,person.Name.Length-1)}")
                .And.NotContain($"{person.Name}");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_ExcludingProperty_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);

            var res = "";
            Action act = () => res = printer.PrintToString(person);
            act.Should().NotThrow();
            res.Should().NotContain("Name");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_IEnumerableExcluding_WorksCorrect()
        {
            var printer = ObjectPrinter.For<List<Person>>().Excluding<Guid>();
            var lp = new List<Person> {person, person};
            var res = "";
            Action act = () => res = printer.PrintToString(lp);
            act.Should().NotThrow();

            var expected = $"{lp.GetType().Name}\r\n";

            for (var i = 0; i < 2; i++)
            {
                expected += $"{person.GetType().Name}\r\n\t\t";
                var curPerson = lp[i];
                foreach (var prop in curPerson.GetType().Properties())
                {
                    if(prop.PropertyType.Name == "Guid")
                        continue;
                    expected += $"{prop.Name} = {prop.GetValue(curPerson)}\r\n\t\t";
                }

                expected = expected.TrimEnd() + "\r\n";
            }
            expected = expected.TrimEnd();
            res.TrimEnd().Should().Be(expected);
            
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_IEnumerable_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<List<Person>>();
            var lp = new List<Person> {person, person};
            var res = "";
            Action act = () => res = printer.PrintToString(lp);
            act.Should().NotThrow();
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_CircleReference_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<TestClass>();
            var first = new TestClass(){ number = 1, Parent = null };
            var second = new TestClass(){ number = 2, Parent = first };
            first.Parent = second;
            var res = "";
            Action act = () => res = printer.PrintToString(first);
            act.Should().NotThrow();
            res.Should().Contain("Parent = circle reference");
            Console.WriteLine(res);
        }
        
        [Test]
        public void ObjPrinter_CircleReferenceList_ShouldNotThrowExceptions()
        {
            var printer = ObjectPrinter.For<List<TestClass>>();
            var first = new TestClass(){ number = 1, Parent = null };
            var second = new TestClass(){ number = 2, Parent = first };
            first.Parent = second;

            var res = new List<TestClass>() {first, second};
            string print = "";
            Action act = () =>  print = printer.PrintToString(res);

            act.Should().NotThrow();
            Console.WriteLine(print);
        }
        
        [Test]
        public void ObjPrinter_NullRefrences_WorksCorrect()
        {
            var printer = ObjectPrinter.For<TestClass>();
            var first = new TestClass(){ number = 1, Parent = null };

            string print = "";
            Action act = () =>  print = printer.PrintToString(first);

            var expected = "TestClass\r\n\tnumber = 1\r\n\tParent = null";
            act.Should().NotThrow();
            print.TrimEnd().Should().Be(expected);
            Console.WriteLine(print);
        }
        
        [Test]
        public void ObjPrinter_Trim_WorksCorrect()
        {
            var printer = ObjectPrinter.For<StringContainer>()
                .Printing<string>().TrimmedToLength(3);
            
            var test = new StringContainer() { StringInner = "12345" };

            string print = "";
            Action act = () =>  print = printer.PrintToString(test);

            var expected = $"{test.GetType().Name}\r\n\tStringInner = {test.StringInner.Substring(0,3)}";
            act.Should().NotThrow();
            print.TrimEnd().Should().Be(expected);
            Console.WriteLine(print);
        }
        
        [Test]
        public void ObjPrinter_ChainMethods_WorksCorrect()
        {
            var printer = ObjectPrinter.For<TestClass>()
                .Printing<int>().Using(x => String.Format("{0} : {1}", x, x))
                .Excluding(x => x.Parent)
                .Printing<int>().Using(CultureInfo.GetCultureInfo("ru-RU"));
            
            var first = new TestClass(){ number = 10000000, Parent = null };

            string print = "";
            Action act = () =>  print = printer.PrintToString(first);

            var expected = $"TestClass\r\n\tnumber = {first.number} : {first.number}";
            act.Should().NotThrow();
            print.TrimEnd().Should().Be(expected);
            Console.WriteLine(print);
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