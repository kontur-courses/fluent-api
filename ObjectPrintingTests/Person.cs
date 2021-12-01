using System;
using System.Collections.Generic;

namespace ObjectPrintingTests;

internal class Person
{
    public int Age;
    public DateTime Birthdate;
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public double Height { get; set; }
    public Person Father { get; set; }
    public Person Mother { get; set; }
    public List<int> Grades { get; set; }
}