using ObjectPrinting;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine(ObjectPrinter.For<DateTime>().PrintToString(DateTime.Now));

/*
ObjectPrinter.For<TOwner>()
  // Исключить тип
  .Excluding<TType>()
  // Настроить сериализацию конкретного типа
  .Printing<TType>().Using(value => value.ToString())
  // Обрезание строк
  .Printing<string>().TrimmedToLength(int length)
  // Культура для типов, которые ее поддерживают
  .Printing<TType>().Using(CultureInfo info)

  // Исключить свойство/поле
  .Excluding(obj => obj.Property)
  // Настроить сериализацию конкретного свойства/поля
  .Printing(obj => obj.Property).Using(value => value.ToString())
  // Обрезание строк
  .Printing(obj => obj.Property).TrimmedToLength(int length)
*/