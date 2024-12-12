using System;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

public class ObjectPrinterTests : BaseVerifyTests
{
	private readonly Person person = new()
	{
		Name = "Alex",
		Surname = "Kash",
		Age = 19,
		Height = 184.1,
		Weight = 80.1
	};

	[Test]
	public Task ExcludeType_Verify() =>
		Verify(person.PrintToString(p => p.Exclude<double>()));

	[Test]
	public Task ExcludeProperty_Verify() =>
		Verify(person.PrintToString(p => p.Exclude(t => t.Id)));

	[Test]
	public Task CustomTypeSerialize_Verify() =>
		Verify(person.PrintToString(p => p.PrintSettings<double>().Using(d => $"-_-{d}-_-")));

	[Test]
	public Task CustomPropertySerialize_Verify() =>
		Verify(person.PrintToString(p => p.PrintSettings(x => x.Name).Using(s => $"---{s}---")));

	[Test]
	public Task TrimStringProperty_Verify() =>
		Verify(person.PrintToString(p => p.PrintSettings(x => x.Name).Trim(3)));

	[Test]
	public Task SetCulture_Verify() =>
		Verify(person.PrintToString(p => p.UseCulture<double>(new CultureInfo("de"))));

	[Test]
	public Task ComplexSetup_Standard_Verify()
	{
		//1. Исключить из сериализации свойства определенного типа
		var printer = ObjectPrinter.For<Person>().Exclude<Guid>()
			//2. Указать альтернативный способ сериализации для определенного типа
			.PrintSettings<int>().Using(i => $"***{i}***")
			//3. Для числовых типов указать культуру
			.UseCulture<double>(CultureInfo.InvariantCulture)
			//4. Настроить сериализацию конкретного свойства
			.PrintSettings(x => x.Name).Using(p => $"---{p}---")
			//5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
			.PrintSettings(x => x.Surname).Trim(2)
			//6. Исключить из сериализации конкретного свойства
			.Exclude(x => x.Height);

		return Verify(printer.PrintToString(person));
	}

	[Test]
	public Task ExtensionMethod_DefaultSerialize_Verify() =>
		Verify(person.PrintToString());

	[Test]
	public Task ExtensionMethod_ConfigureSerialize_Verify() =>
		Verify(person.PrintToString(c => c.Exclude<Guid>()
			//2. Указать альтернативный способ сериализации для определенного типа
			.PrintSettings<int>().Using(i => $"***{i}***")
			//3. Для числовых типов указать культуру
			.UseCulture<double>(CultureInfo.InvariantCulture)
			//4. Настроить сериализацию конкретного свойства
			.PrintSettings(x => x.Name).Using(p => $"---{p}---")
			//5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
			.PrintSettings(x => x.Surname).Trim(2)
			//6. Исключить из сериализации конкретного свойства
			.Exclude(x => x.Height)));
}