namespace ObjectPrinting.Tests
{
    public class Tzar
    {
        public static int IdCounter;

        public Tzar(string name, int age, Tzar previousTzar = null, Tzar nextTzar = null)
        {
            Id = IdCounter++;
            Name = name;
            Age = age;
            PreviousTzar = previousTzar;
            NextTzar = nextTzar;
        }

        public int Age { get; set; }
        public string Name { get; set; }
        public Tzar PreviousTzar { get; set; }

        public Tzar NextTzar { get; set; }
        public int Id { get; set; }
    }
}