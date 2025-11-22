using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public abstract class ObjectPrinterTestsSetup
    {
        protected Person TestPerson;
        protected Company TestCompany;
        protected PrintingConfig<Person> PersonConfig;
        protected PrintingConfig<Company> CompanyConfig;
        protected PrintingConfig<Node> NodeConfig;
        private CultureInfo originalCulture;

        [SetUp]
        public void BaseSetUp()
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            TestPerson = new Person
            {
                Name = "John Doe",
                Age = 30,
                Height = 180.5,
                BirthDate = new DateTime(1990, 1, 1)
            };

            TestCompany = new Company
            {
                Name = "Test Company",
                Employees = new System.Collections.Generic.List<Person> { TestPerson },
                Budget = 1000000.75m
            };

            PersonConfig = new PrintingConfig<Person>();
            CompanyConfig = new PrintingConfig<Company>();
            NodeConfig = new PrintingConfig<Node>();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalCulture;
        }
    }
}