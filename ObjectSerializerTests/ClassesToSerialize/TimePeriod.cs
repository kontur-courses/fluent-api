namespace ObjectSerializerTests.ClassesToSerialize;

public class TimePeriod
{
    public TimePeriod(Guid id, DateTime startTime, DateTime endTime,
        int totalPlaces, int bookedPlaces = 0)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Не правильный формат времени, конец мероприятия должен быть позже начала");

        if (totalPlaces <= 0)
            throw new ArgumentException("Число мест на период мероприятия должно быть больше нуля");

        Id = id;
        StartTime = startTime;
        EndTime = endTime;
        TotalPlaces = totalPlaces;
        BookedPlaces = bookedPlaces;
    }

    public Guid Id { get; }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public int TotalPlaces { get; }

    public int BookedPlaces { get; private set; }

    public static TimePeriod CreateTimePeriod(Guid id, DateTime startTime, DateTime endTime,
        int totalPlaces, int bookedPlaces)
    {
        return new TimePeriod(id, startTime, endTime, totalPlaces, bookedPlaces);
    }
}