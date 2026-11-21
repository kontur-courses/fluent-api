using System;
using System.Collections;
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

        private readonly Type[] finalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        ];

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, RegisterTypePrinter);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(
            Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var key = GetMemberKey(memberSelector);
            return new PropertyPrintingConfig<TOwner, TPropType>(this,
                print => RegisterMemberPrinter(key, print));
        }

        private void RegisterMemberPrinter<TPropType>((Type? Type, string Name) key, Func<TPropType, string> print)
        {
            memberPrinters[key] = value => print((TPropType)value);
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
            return PrintToString(obj, nestingLevel, new HashSet<object>());
        }

        private string PrintToString(object? value, int level, HashSet<object> currentPath)
        {
            if (value == null)
                return "null" + Environment.NewLine;

            var type = value.GetType();
            if (typePrinters.TryGetValue(type, out var print))
                return print(value) + Environment.NewLine;

            if (finalTypes.Contains(type))
                return value + Environment.NewLine;

            if (!currentPath.Add(value))
                return "Parent" + Environment.NewLine;

            var indentation = new string('\t', level + 1);
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);

            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    var text = PrintToString(entry.Value, level + 1, currentPath);
                    sb.Append($"{indentation}[{entry.Key}] = {text}");
                }
            }
            
            else if (value is IEnumerable collection)
            {
                var index = 0;
                foreach (var item in collection)
                {
                    var text = PrintToString(item, level + 1, currentPath);
                    sb.Append($"{indentation}[{index}] = {text}");
                    index++;
                }
            }

            else
            {
                var flag = BindingFlags.Public | BindingFlags.Instance;
                foreach (var property in type.GetProperties(flag))
                {
                    var key = (property.DeclaringType, property.Name);
                    if (excludedTypes.Contains(property.PropertyType) || excludedMembers.Contains(key))
                        continue;

                    var propertyValue = property.GetValue(value);
                    var text = memberPrinters.TryGetValue(key, out var memberPrint)
                        ? memberPrint(propertyValue!) + Environment.NewLine
                        : PrintToString(propertyValue, level + 1, currentPath);
                    sb.Append($"{indentation}{property.Name} = {text}");
                }

                foreach (var field in type.GetFields(flag))
                {
                    var key = (field.DeclaringType, field.Name);
                    if (excludedTypes.Contains(field.FieldType) || excludedMembers.Contains(key))
                        continue;

                    var fieldValue = field.GetValue(value);
                    var text = memberPrinters.TryGetValue(key, out var memberPrint)
                        ? memberPrint(fieldValue!) + Environment.NewLine
                        : PrintToString(fieldValue, level + 1, currentPath);
                    sb.Append($"{indentation}{field.Name} = {text}");
                }
            }

            currentPath.Remove(value);
            return sb.ToString();
        }
    }
}