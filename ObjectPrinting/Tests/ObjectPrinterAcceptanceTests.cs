// using System;
// using System.Diagnostics.CodeAnalysis;
// using System.Globalization;
// using NUnit.Framework;
// using ObjectPrinting.Solved;
//
// namespace ObjectPrinting.Tests
// {
//     [TestFixture]
//     public class ObjectPrinterAcceptanceTests
//     {
//         [Test]
//         public void Demo()
//         {
//             var person = new Person { Name = "Alex", Age = 19 };
//
//             var printer = ObjectPrinter.For<Person>();
//             printer
//                 //1. Исключить из сериализации свойства определенного типа
//                 .Exluding<int>()
//                 //2. Указать альтернативный способ сериализации для определенного типа
//                 //3. Для числовых типов указать культуру
//                 .Printing<double>().By(CultureInfo.CurrentCulture)
//                 //4. Настроить сериализацию конкретного свойства
//                 .Printing<int>().By(s => s.ToString())
//                 //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
//                 
//                 //6. Исключить из сериализации конкретного свойства
//                 .Exluding(p = p.Age);
//             
//             string s1 = printer.PrintToString(person);
//
//             //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
//             //8. ...с конфигурированием
//         }
//     }
// }