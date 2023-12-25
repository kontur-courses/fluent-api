using System;

namespace StringSerializer.Tests.TestModels;

public class Airplane
{
    public string? ModelName { get; set; }
    public double Length { get; set; }
    public double Height { get; set; }
    public double MaxMass { get; set; }
    public int MaxFlyingHeight { get; set; }
    public int MaxSpeed { get; set; }
    public DateTime ProductionDate { get; set; }
    public Engine? Engine { get; set; }
    public Airplane? Ancestor { get; set; }
}