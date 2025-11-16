using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class PersonGroup
{
    public string GroupName { get; set; }
    public Person Leader { get; set; }
    public Person[] Members { get; set; }
    public int[] Scores { get; set; }
    public List<double> ScoresList { get; set; }
    public Dictionary<string, Dictionary<string, int>> ScoresDictionary { get; set; }
}