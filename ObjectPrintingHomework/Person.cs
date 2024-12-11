using System.Diagnostics.Contracts;

namespace ObjectPrintingHomework;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public int CountEyes { get; set; }
    public string Surname { get; set; }
    public DateTime DateBirth {get; set; }
    public Person[] Parents { get; set; }
    public Person[] Friends { get; set; }
    public Dictionary<string, int> LimbToNumbersFingers {get; set;}
    public List<Person> Childs {get; set;}
}