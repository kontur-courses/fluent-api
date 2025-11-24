namespace Tests.TestEntities;

public class SuperComplexClass
{
    public string ShortName { get; set; } = null!;
    public string VeryLongDescription { get; set; } = null!;
    public int Age { get; set; }
    public double Weight { get; set; }
    public decimal Salary { get; set; }
    public decimal Bonus { get; set; }
    public DateTime BirthDate { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, int> Scores { get; set; } = new();
    public SuperComplexClass Manager { get; set; }
    public SuperComplexClass Assistant { get; set; }
    public List<SuperComplexClass> TeamMembers { get; set; } = new();
}