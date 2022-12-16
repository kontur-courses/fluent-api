using ObjectPrinting.Core;

namespace ObjectPrinting.Sandbox
{
    internal class Program
    {
        private static void Main()
        {
            ObjectPrinter<Person>.MaxCollectionSize = 3;
            var res = ObjectPrinter<Person>.Print(GetTestPerson());
            Console.WriteLine(res);

        }
        public static Person GetTestPerson()
        {
            return new Person
            {
                Name = "Иванов Иван Иванович",
                Age = 23,
                Height = 179.3,
                Id = Guid.NewGuid(),
                OtherPersons = new[]
                {
                    Guid.NewGuid(),
                    Guid.Empty,
                    Guid.NewGuid(),
                    Guid.NewGuid()
                },
                Biography = @"Съешь ещё этих мягких французских булок, да выпей чаю. 
Съешь ещё этих мягких французских булок, да выпей чаю. Съешь ещё этих мягких французских булок, да выпей чаю.",
                Dates = new()
                {
                    [DateTime.Now] = "Today",
                    [DateTime.Now - TimeSpan.FromDays(15)] = "15 days ago"
                }
            };
        }
    }
}