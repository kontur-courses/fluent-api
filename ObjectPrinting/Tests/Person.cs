﻿using System;

namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
        
    public double Height { get; set; }
    
    public double Weight { get; set; }
    
    public int Age { get; set; }
        
    public DateTime Birthday { get; set; }
    
    public bool IsStudent { get; set; }
    
    public Person? Father { get; set; }
}