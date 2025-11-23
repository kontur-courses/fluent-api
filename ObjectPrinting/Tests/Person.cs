using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Person
{
    public string FirstName;
    public Company Company;
    public Address Address;
    public List<string> Tags;
    
    public string Name { get; set; }
    public int Age { get; set; }
    public double Score { get; set; }
    public Guid Id { get; set; }

    public Person()
    {
        Tags = new List<string>();
    }
}

public class Company
{
    public string CompanyName { get; set; }
    public string Industry { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
