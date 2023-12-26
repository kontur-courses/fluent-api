namespace ObjectPrintingTest.TestTypes;

public class PhoneBook
{
    public PhoneBook(string town, Dictionary<string, Person> numberToPerson)
    {
        Town = town;
        NumberToPerson = numberToPerson;
    }

    public string Town { get; set; }
    public Dictionary<string, Person> NumberToPerson { get; set; }
}