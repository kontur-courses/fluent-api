using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingResult
    {
        private readonly StringBuilder totalPrinting = new StringBuilder();
        private readonly HashSet<object> addedObjects = new HashSet<object>();
        
        public bool AddObjectAndSetCurrent(object obj, int nestingLevel, string fullName)
        {
            if (!addedObjects.Add(obj))
                return false;
            CurrentIdentation = new string('\t', nestingLevel + 1);
            CurrentNestingLevel = nestingLevel;
            CurrentFullName = fullName;
            CurrentObject = obj;
            return true;
        }

        public void AddPrinting(string printing)
        {
            totalPrinting.Append(printing);
        }

        
        public string GetPrinting()
            => totalPrinting.ToString();

        public object CurrentObject { get; private set; }
        public string CurrentIdentation { get; private set; }
        public int CurrentNestingLevel { get; private set; }
        public string CurrentFullName { get; private set; }
    }
}