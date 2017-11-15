using System;
using System.Globalization;
using NUnit.Framework;

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
				.Exclude<Guid>()
				//2. Указать альтернативный способ сериализации для определенного типа
				.Printing<Guid>().Using(g => g.ToString())
				//3. Для числовых типов (int, double, long) указать культуру
				.Printing<int>().SetCulture(CultureInfo.InvariantCulture)
				//4. Настроить сериализацию конкретного свойства
				.Printing(obj => obj.Height).Using(h => $"{h} cm")
				//5. Настроить обрезание строковых свойств 
			    //   (метод должен быть виден только для строковых свойств)
				.Printing<string>().ShrinkToLength(15)
				//6. Исключить из сериализации конкретного свойства
				.Exclude(obj => obj.Name);
            
            string s1 = printer.PrintToString(person);

			//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию		
			string s2 = person.PrintToString();
			//8. ...с конфигурированием
			string s3 = person.PrintToString(config => 
                config.Printing<double>().SetCulture(CultureInfo.CurrentCulture));
		}
	}
}