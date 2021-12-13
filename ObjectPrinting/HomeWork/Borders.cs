using System;

namespace ObjectPrinting.HomeWork
{
    public class Borders
    {
        public int Start { get; }
        public int Length { get; }

        public Borders(int start, int length)
        {
            if (start < 0 || length < 0)
                throw new ArgumentException("Индексы начала и конца строки должны быть положительными");

            Start = start;
            Length = length;
        }
    }
}
