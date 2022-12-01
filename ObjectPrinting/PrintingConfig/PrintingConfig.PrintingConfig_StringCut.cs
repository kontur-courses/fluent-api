using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectPrinting.PrintingConfig
{
    public partial class PrintingConfig<TOwner>
    {
        private Dictionary<string, Func<string, string>> StringCutFunctions =
            new Dictionary<string, Func<string, string>>();

        private string GetStringCut(string s, string propertyName)
        {
            if (!StringCutFunctions.ContainsKey(propertyName)) return s;
            else return StringCutFunctions[propertyName](s);
        }

        internal void SetStringCut(Expression<Func<TOwner, string>> propertyExpression, int maxLength)
        {
            CheckExpression(propertyExpression);
            var name = GetFullName(propertyExpression);
            StringCutFunctions[name] = (s) =>
            {
                if (s.Length > maxLength) return s.Substring(0, maxLength);
                else return s;
            };
        }
    }
}