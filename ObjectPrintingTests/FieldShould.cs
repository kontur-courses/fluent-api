namespace ObjectPrintingTests
{
    public class FieldShould
    {
        [Test]
        public void PrintPersonWithFields_WhenFieldsIsInitial()
        {
            var person = new PersonWithFields
            {
                Name = "Anna", Age = 15,
                BestFriend = "Bill",
                CountFriends = 1
            };
            var printer = new PrintingConfig<PersonWithFields>();
            var stringWithField=printer.PrintToString(person);
            stringWithField.Should().Contain("BestFriend = Bill")
                .And.Contain("CountFriends = 1");
        }
    }
}
