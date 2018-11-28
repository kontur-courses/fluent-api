using System;

namespace ObjectPrintingTests
{
    public class Employee : Person
    {
        private readonly Lazy<Company> employer;

        public Employee(Lazy<Company> employer)
        {
            this.employer = employer;
        }

        public Company Employer => employer.Value;
    }
}