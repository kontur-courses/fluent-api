namespace ObjectPrinterTests.ForSerialization
{
    public static class Instances
    {
        public static readonly Person Person = new Person {Age = 15, Weight = 67.778, Height = 160.5, Name = "Danil"};

        public static readonly Employee EmployeeWithManySubordinates = new Employee
        {
            Name = "1",
            Subordinate = new Employee
            {
                Name = "2",
                Subordinate = new Employee
                {
                    Name = "3",
                    Subordinate = new Employee
                    {
                        Name = "4",
                        Subordinate = new Employee
                        {
                            Name = "5",
                            Subordinate = new Employee
                            {
                                Name = "6",
                                Subordinate = new Employee
                                {
                                    Name = "7",
                                    Subordinate = new Employee
                                    {
                                        Name = "8",
                                        Subordinate = new Employee
                                        {
                                            Name = "9",
                                            Subordinate = new Employee
                                            {
                                                Name = "10",
                                                Subordinate = new Employee {Name = "11", Subordinate = null}
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}