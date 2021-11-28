using System;

namespace ObjectPrinting.Tests
{
    public static class PersonFactory
    {
        public static Person Get() =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Alex",
                Age = 19,
                Height = 123.5f,
                Money = 9871
            };
    }
}