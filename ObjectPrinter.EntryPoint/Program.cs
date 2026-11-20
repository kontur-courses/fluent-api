using ObjectPrinting;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var root = new TestNode
{
    Id = 1,
    Name = "Root",
    IsActive = true,
    Price = 149.90m,
    CreatedAt = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc),
    Description = null,
    Rating = 5,
    Scores = [10, 20, 30]
};

var child = new TestNode
{
    Id = 2,
    Name = "Child",
    Parent = root
};

root.Children.Add(child);
root.NamedNodes["first"] = child;
root.Self = root;

/*
var printer = ObjectPrinter.For<TestNode>()
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
*/

var printer = ObjectPrinter.For<TestNode>().Excluding<int>();

Console.WriteLine(printer.PrintToString(root));
//root.PrintToString();

public class TestNode
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public bool IsActive { get; init; }
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Description { get; init; }
    public int? Rating { get; init; }

    public int[] Scores { get; init; } = [];
    public List<TestNode> Children { get; } = [];
    public Dictionary<string, TestNode> NamedNodes { get; } = new();

    public TestNode? Parent { get; set; }
    public TestNode? Self { get; set; }

    private int _privateInt;
}