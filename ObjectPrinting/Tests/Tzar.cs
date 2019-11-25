namespace ObjectPrinting.Tests
{
    public class Tzar
	{
		public static int IdCounter = 0;
        public int Age { get; set; }
		public string Name { get; set; }
		public Tzar Parent1 { get; set; }

        public Tzar Parent2 { get; set; }
        public int Id { get; set; }

		public Tzar(string name, int age, Tzar parent1, Tzar parent2)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
            Parent1 = parent1;
            Parent2 = parent2;
        }
	}
}