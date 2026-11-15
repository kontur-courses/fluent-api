using ObjectPrinting;
using ObjectPrinting.Solved.Tests;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine(ObjectPrinter.For<DateTime>().PrintToString(DateTime.Now));


var printer = ObjectPrinter.For<Person>()
  // Исключить тип
  .Excluding<Guid>()
  // Настроить сериализацию конкретного типа
  .Printing<int>().Using(value => value.ToString())
  // Обрезание строк
  .Printing<string>().TrimmedToLength(10)
  // Культура для типов, которые ее поддерживают
  .Printing<double>().Using(CultureInfo.InvariantCulture)

  // Исключить свойство/поле
  .Excluding(obj => obj.Height)
  // Настроить сериализацию конкретного свойства/поля
  .Printing(obj => obj.Age).Using(value => value.ToString())
  // Обрезание строк
  .Printing(obj => obj.Name).TrimmedToLength(10);

var person = new Person();

printer.PrintToString(person);
person.PrintToString();
