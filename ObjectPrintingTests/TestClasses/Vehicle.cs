namespace ObjectPrintingTests.TestClasses
{
    public class Vehicle
    {
        public static int CarNumber { get; set; }
        public string BrandAuto { get; set; }
        public int? Power { get; set; }
        public double? Weight { get; set; }
        private int YearOfCarProduction { get; }
        public int? AgeOfCar { get; }

        public Vehicle(string brandAuto, int power, double? weight, int yearOfCarProduction)
        {
            CarNumber = CarNumber++;
            BrandAuto = brandAuto;
            Power = power;
            Weight = weight;
            YearOfCarProduction = yearOfCarProduction;
            AgeOfCar = DateTime.Today.Year - YearOfCarProduction;
        }
    }
}
