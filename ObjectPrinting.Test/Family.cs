namespace ObjectPrinting.Test;

public record Family
{
    public Person? Mom { get; set; }
    public Person? Dad { get; set; }
    public List<Person> Children { get; set; } = [];
}