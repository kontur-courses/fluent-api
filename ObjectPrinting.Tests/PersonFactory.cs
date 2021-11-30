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
    }
}