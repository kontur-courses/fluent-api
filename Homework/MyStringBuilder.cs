using System;
using System.Text;

namespace Homework
{
    public class MyStringBuilder
    {
        private readonly StringBuilder inner = new();
        private (string?, bool) last;
        
        public void Clear()
        {
            inner.Clear();
        }

        private void Add(string line, bool newLineRequired)
        {
            if (newLineRequired) inner.AppendLine(line);
            else inner.Append(line);
        }
        
        public void Append(string line)
        {
            if (last.Item1 is not null) Add(last.Item1, last.Item2);
            last = (line, false);
        }

        public void AppendLine(string line)
        {
            if (last.Item1 is not null) Add(last.Item1, last.Item2);
            last = (line, true);
        }
        
        public string? Last()
        {
            return last.Item1;
        }

        public override string ToString()
        {
            if (last.Item1 is not null) Add(last.Item1, last.Item2);
            return inner.ToString();
        }

        public void ReplaceLast(string line)
        {
            if (last.Item1 is null) throw new ArgumentException("there are no line to replace");
            last = (line, last.Item2);
        }

        public bool IsEmpty()
        {
            return last.Item1 is null;
        }
    }
}