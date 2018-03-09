using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
	public class ObjectPrinterAcceptanceTests
    {
		[Test]
		public void Demo()
		{
            var person = new Person { Name = "Alexeeeeeeeey", Age = 19, Height = 178.5 };
            var printer = ObjectPrinter.For<Person>()
				//1. Исключить из сериализации свойства определенного типа
				.Excluding<Guid>()
				//2. Указать альтернативный способ сериализации для определенного типа
				.Printing<int>().Using(g => g.ToString())
				//3. Для числовых типов (int, double, long) указать культуру
				.Printing<double>().WithCulture(CultureInfo.CurrentCulture)
				//4. Настроить сериализацию конкретного свойства
				.Printing(obj => obj.Height).Using(h => $"{h} cm")
				//5. Настроить обрезание строковых свойств 
			    //   (метод должен быть виден только для строковых свойств)
				.Printing<string>().ShrinkedToLength(4)
				//6. Исключить из сериализации конкретного свойства
				.Excluding(obj => obj.Age)
                .Build();
            
            string s1 = printer.PrintToString(person);
		    Console.WriteLine(s1);

			//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию		
			string s2 = person.PrintToString();
		    Console.WriteLine(s2);
			//8. ...с конфигурированием
			string s3 = person.PrintToString(config => 
                config.Printing<double>().WithCulture(CultureInfo.InvariantCulture));
		    Console.WriteLine(s3);
		}
    }
}