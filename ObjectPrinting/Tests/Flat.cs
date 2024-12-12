using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Flat
{
    public int Floor { get; set; }
    public int ApartmentNumber { get; set; }
    public IEnumerable<Room> Rooms { get; set; }
}