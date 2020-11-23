using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        private readonly HashSet<Type> _excludedTypes = new HashSet<Type>();

        private HashSet<string> _excludedMembers = new HashSet<string>();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propertyName = ((MemberExpression)memberSelector.Body).Member.Name;
            _excludedMembers.Add(propertyName);
            return this;
        }

        internal PrintingConfig<TOwner> Excluding<TPropType>()
        {
            _excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            var type = obj.GetType();
            if (finalTypes.Contains(type))
                return obj + Environment.NewLine;
            var identation = GetIdentation('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            foreach (var memberInfo in type.GetFieldsAndProperties())
            {
                if (IsExclude(memberInfo))
                    continue;
                sb.Append(identation + memberInfo.Name + " = "
                          + PrintToString(memberInfo.GetValue(obj), nestingLevel + 1));
            }

            return sb.ToString();
        }


        private string GetIdentation(char identationSymbol, int nestingLevel)
        {
            return new string(identationSymbol, nestingLevel);

        }

        private bool IsExclude(MemberInfo memberInfo)
        {
            var name = memberInfo.Name;
            var type = memberInfo.GetValueType();
            return _excludedTypes.Contains(type) || _excludedMembers.Contains(name);
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetFields(bindingFlags).Cast<MemberInfo>().Concat(type.GetProperties(bindingFlags));
        }
    }
}