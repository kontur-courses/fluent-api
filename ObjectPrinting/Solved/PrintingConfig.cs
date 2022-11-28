#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public class PrintingConfig<TOwner>
    {
        private IEnumerable<MemberInfo> _members;
        private Dictionary<Type, Func<object, string>> _alternativeTypesPrints = new();
        private Dictionary<string, Func<object, string>> _alternativePropertiesPrints = new();

        private static readonly Type[] _finalTypes = 
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        
        public PrintingConfig()
        {
            _members = typeof(TOwner).GetProperties();
            _members = _members.Concat(typeof(TOwner).GetFields());
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name = GetPropertyNameFromExpression(memberSelector);
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            _alternativePropertiesPrints[name] = propertyConfig.AlternativePrintInvoke;
            return propertyConfig;
        }
        
        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            var type = typeof(TPropType);
            var propertyConfig = new PropertyPrintingConfig<TOwner, TPropType>(this);
            _alternativeTypesPrints[type] = propertyConfig.AlternativePrintInvoke;
            return propertyConfig;
        } 

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var name = GetPropertyNameFromExpression(memberSelector);
            _members = _members.Where(property => property.Name != name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            var type = typeof(TPropType);
            _members = _members.Where(member => GetTypeFromMemberInfo(member) != type);
            return this;
        }
        
        private string GetPropertyNameFromExpression<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var arguments = memberSelector.Body.ToString().Split('.');
            if (arguments.Length != 2)
                throw new ArgumentException();
            var name = arguments[1];
            return name;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, -1, new HashSet<object>());
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> printedObjects)
        {
            printedObjects.Add(obj);
            if (obj is null)
                return "null";

            if (_finalTypes.Contains(obj.GetType()))
                return obj.ToString();

            var stringBuilder = new StringBuilder();
            foreach (var memberInfo in _members)
            {
                var memberType = GetTypeFromMemberInfo(memberInfo);
                var memberValue = GetValueFromMemberInfo(obj, memberInfo);
                
                if (memberType.IsValueType == false && memberType != typeof(string) && 
                    memberValue is not null && printedObjects.Contains(memberValue))
                    continue;
                printedObjects.Add(memberValue);
                
                if (_alternativePropertiesPrints.ContainsKey(memberInfo.Name))
                {
                    AddLineToPrint(stringBuilder, nestingLevel + 1, memberInfo.Name, _alternativePropertiesPrints[memberInfo.Name](memberValue));
                    continue;
                }
                
                if (_alternativeTypesPrints.ContainsKey(memberType))
                {
                    AddLineToPrint(stringBuilder, nestingLevel + 1, memberInfo.Name, _alternativeTypesPrints[memberType](memberValue));
                    continue;
                }
                
                AddLineToPrint(stringBuilder, nestingLevel + 1, memberInfo.Name, PrintToString(memberValue,nestingLevel + 1, new HashSet<object>(printedObjects)));
            }
            return stringBuilder.ToString();
        }

        private void AddLineToPrint(StringBuilder stringBuilder, int shiftLevel, string name, string value)
        {
            var shift = new string('\t', shiftLevel);
            stringBuilder.Append($"{Environment.NewLine}{shift}{name} = {value}");
        }

        private Type? GetTypeFromMemberInfo(MemberInfo memberInfo)
        {
            return memberInfo is FieldInfo
                ? (memberInfo as FieldInfo)?.FieldType
                : (memberInfo as PropertyInfo)?.PropertyType;
        }
        
        private object? GetValueFromMemberInfo(object obj, MemberInfo memberInfo)
        {
            return memberInfo is FieldInfo
                ? (memberInfo as FieldInfo)?.GetValue(obj)
                : (memberInfo as PropertyInfo)?.GetValue(obj);
        }
    }
}