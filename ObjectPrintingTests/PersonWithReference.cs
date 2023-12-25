namespace ObjectPrintingTests;

public class PersonWithReference
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PersonWithReference Friend {  get; set; }
}
