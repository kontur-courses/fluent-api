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
				.ExcludeType<Guid>()
				//2. Указать альтернативный способ сериализации для определенного типа
				.ConfigureType<Guid>()
					.SetSerializer(config => config.ToString())
				//3. Для числовых типов (int, double, long) указать культуру
				.ConfigureType<int>()
					.SetSerializer(i => "")
				.ConfigureType<int>()
					.SetCulture(CultureInfo.CurrentUICulture)
				//4. Настроить сериализацию конкретного свойства
				.ConfigureProperty(obj => obj.Name)
					.SetSerializer(e => e.ToString())
				//5. Настроить обрезание строковых свойств 
				.ConfigureType<string>()
					.ShrinkToLength(10)
			    //   (метод должен быть виден только для строковых свойств)
				//6. Исключить из сериализации конкретного свойства
				.ExcludeProperty(obj => obj.Name);
            
            string s1 = printer.PrintToString(person);

			//7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию		
			string s2 = person.PrintToString();
			//8. ...с конфигурированием
			string s3 = person.PrintToString(o =>
				o.ConfigureType<int>().SetCulture(CultureInfo.CurrentCulture));
		}
	}
}