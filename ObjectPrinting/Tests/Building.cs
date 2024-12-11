using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Building
{
    public string Name { get; set; }
    public Dictionary<int, List<Room>> Floors { get; set; }
}