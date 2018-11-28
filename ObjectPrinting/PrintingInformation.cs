using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingInformation
    {
        private readonly StringBuilder totalPrinting = new StringBuilder();
        private readonly HashSet<object> addedObjects = new HashSet<object>();
        
        public bool AddObjectAndSetCurrent(object obj, int nestingLevel, string fullName)
        {
            if (!addedObjects.Add(obj))
                return false;
            CurrentIndentation = new string('\t', nestingLevel + 1);
            CurrentNestingLevel = nestingLevel;
            CurrentFullName = fullName;
            CurrentObject = obj;
            return true;
        }

        public void AddPrinting(string printing)
        {
            totalPrinting.Append(printing);
        }

        public void AddPrintingNewLine(string printing)
        {
            totalPrinting.Append($"{printing}{Environment.NewLine}");
        }
        
        public string GetPrinting()
            => totalPrinting.ToString();

        public object CurrentObject { get; private set; }
        public string CurrentIndentation { get; private set; }
        public int CurrentNestingLevel { get; private set; }
        public string CurrentFullName { get; private set; }
    }
}   