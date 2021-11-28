using ObjectPrintingTests.Persons;

namespace ObjectPrintingTests.Helpers
{
    public static class PersonDescription
    {
        public static (string, string, string, string) GetPersonFields(Person person)
        {
            var id = $"{nameof(Person.Id)} = {person.Id}";
            var name = $"{nameof(Person.Name)} = {person.Name}";
            var height = $"{nameof(Person.Height)} = {person.Height}";
            var age = $"{nameof(Person.Age)} = {person.Age}";
            return (id, name, height, age);
        }

        public static ObjectDescription GetDefaultDescription(Person person)
        {
            var (id, name, height, age) = GetPersonFields(person);
            return new ObjectDescription(nameof(Person))
                .WithFields(id, name, height, age);
        }
    }
}