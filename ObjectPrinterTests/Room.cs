namespace ObjectPrinterTests;

public class Room
{
    public double Size { get; set; }
    public Room[] NeighborRooms { get; set; }


    public static Room GetStudioHouse()
    {
        var house = new Room
        {
            NeighborRooms = new[] { new Room { Size = 10 } },
            Size = 15
        };

        house.NeighborRooms[0].NeighborRooms = new[] { house };

        return house;
    }
}