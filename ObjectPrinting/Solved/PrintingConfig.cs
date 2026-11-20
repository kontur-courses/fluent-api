using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<(Type? Type, string Name)> excludedMembers = new();
        private readonly Dictionary<Type, Func<object, string>> typePrinters = new();
        private readonly Dictionary<(Type? Type, string Name), Func<object, string>> memberPrinters = new();

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, RegisterTypePrinter);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var key = GetMemberKey(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                print => { memberPrinters[key] = value => print((TPropType)value); });
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            excludedMembers.Add(GetMemberKey(memberSelector));
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private static (Type? Type, string Name) GetMemberKey<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body is not MemberExpression member)
                throw new ArgumentException(nameof(memberSelector));

            return (member.Member.DeclaringType, member.Member.Name);
        }

        private void RegisterTypePrinter<TPropType>(Func<TPropType, string> print)
        {
            typePrinters[typeof(TPropType)] = value => print((TPropType)value);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            var currentPath = new HashSet<object>();
            return PrintValue(obj, nestingLevel);

            string PrintValue(object? value, int level)
            {
                if (value == null)
                    return "null" + Environment.NewLine;

                var type = value.GetType();
                if (typePrinters.TryGetValue(type, out var print))
                    return print(value) + Environment.NewLine;

                var finalTypes = new[]
                {
                    typeof(int), typeof(double), typeof(float), typeof(string),
                    typeof(DateTime), typeof(TimeSpan)
                };
                if (finalTypes.Contains(type))
                    return value + Environment.NewLine;

                if (!currentPath.Add(value))
                    return "Parent" + Environment.NewLine;

                var indentation = new string('\t', level + 1);
                var sb = new StringBuilder();
                sb.AppendLine(type.Name);
                var memberFlag = BindingFlags.Public | BindingFlags.Instance;
                foreach (var property in type.GetProperties(memberFlag))
                {
                    if (excludedTypes.Contains(property.PropertyType) ||
                        excludedMembers.Contains((property.DeclaringType, property.Name)))
                        continue;

                    sb.Append(indentation + property.Name + " = " +
                              PrintMember(property, property.GetValue(value)!, level + 1));
                }

                foreach (var field in type.GetFields(memberFlag))
                {
                    if (excludedTypes.Contains(field.FieldType) || excludedMembers.Contains((field.DeclaringType,
                            field.Name)))
                        continue;

                    sb.Append(indentation + field.Name + " = " +
                              PrintMember(field, field.GetValue(value)!, level + 1));
                }

                currentPath.Remove(value);
                return sb.ToString();
            }

            string PrintMember(MemberInfo member, object value, int level)
            {
                if (memberPrinters.TryGetValue((member.DeclaringType, member.Name), out var print))
                    return print(value) + Environment.NewLine;

                return PrintValue(value, level);
            }
        }
    }
}