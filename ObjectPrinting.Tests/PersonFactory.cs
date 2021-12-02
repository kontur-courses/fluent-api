using System.Text;

namespace ObjectPrinting.Tests
{
    public static class PersonFactory
    {
        public static Person CreateDefaultPerson() => new() {FirstName = "Ozzy", LastName = "Osbourne"};

        public static Person CreatePersonWithCycleReference()
        {
            var person = CreateDefaultPerson();
            person.Parent = person;
            return person;
        }

        public static string GetDefaultPersonPrinting(Person person) =>
            new StringBuilder()
                .AppendLine($"{nameof(Person)}")
                .AppendLine($"\t{nameof(person.LastName)} = {person.LastName}")
                .AppendLine($"\t{nameof(person.Id)} = {person.Id}")
                .AppendLine($"\t{nameof(person.FirstName)} = {person.FirstName}")
                .AppendLine($"\t{nameof(person.Height)} = {person.Height}")
                .AppendLine($"\t{nameof(person.Age)} = {person.Age}")
                .AppendLine($"\t{nameof(person.Parent)} = null")
                .ToString();
    }
}