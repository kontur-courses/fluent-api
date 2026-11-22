namespace ObjectPrinting.Tests;

public class Student
{
    public string FirstName;
    public string LastName;
    public int Age;
    public double AverageGrade;

    public string University { get; set; }
    public string Specialty { get; set; }
    public int Course { get; set; }
    public decimal Scholarship { get; set; }
    public bool IsBudget { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public Guid StudentId { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Индексатор
    public string this[int index] => Courses[index];

    private readonly List<string> Courses = new()
    {
        "C#", "Algorithms", "Databases"
    };
}