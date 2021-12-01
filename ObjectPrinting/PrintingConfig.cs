using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly HashSet<Type> FinalTypes = new()
        {
            typeof(int),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        };
        
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<string> excludedMembers = new();
        private readonly Dictionary<Type, Delegate> typesSerializers = new();
        private readonly Dictionary<Type, CultureInfo> cultures = new();

        public PrintingConfig<TOwner> Exclude<TExcluding>()
        {
            excludedTypes.Add(typeof(TExcluding));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TExcluding>(Expression<Func<TOwner, TExcluding>> memberSelector)
        {
            var memberExpression = (MemberExpression) memberSelector.Body;
            excludedMembers.Add(memberExpression.Member.Name);
            return this;
        }

        public MemberPrintingConfig<TOwner, TMember> Printing<TMember>()
        {
            return new MemberPrintingConfig<TOwner, TMember>(this);
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (typesSerializers.ContainsKey(obj.GetType()))
                return (string) typesSerializers[obj.GetType()].DynamicInvoke(obj);
            
            if (FinalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var indentation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);

            var members = GetTypeFieldsAndProperties(type);
            
            foreach (var memberInfo in members)
            {
                if (excludedMembers.Contains(memberInfo.Name)) continue;
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                    {
                        var fieldInfo = (FieldInfo) memberInfo;
                    
                        if (excludedTypes.Contains(fieldInfo.FieldType)) continue;

                        sb.Append(PrintField(obj, fieldInfo, indentation, nestingLevel));
                        break;
                    }
                    case MemberTypes.Property:
                    {
                        var propertyInfo = (PropertyInfo) memberInfo;
                    
                        if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                    
                        sb.Append(PrintProperty(obj, propertyInfo, indentation, nestingLevel));
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        internal void AddCustomTypeCulture<T>(CultureInfo culture)
        {
            cultures[typeof(T)] = culture;
        }

        internal void AddCustomTypeSerializer<T>(Func<T, string> serializer)
        {
            typesSerializers[typeof(T)] = serializer;
        }

        private string PrintField(object obj, FieldInfo fieldInfo, string indentation, int nestingLevel)
        {
            return indentation + fieldInfo.Name + " = " +
                   PrintToString(fieldInfo.GetValue(obj),
                       nestingLevel + 1);
        }

        private string PrintProperty(object obj, PropertyInfo propertyInfo, string indentation, int nestingLevel)
        {
            return indentation + propertyInfo.Name + " = " +
                   PrintToString(propertyInfo.GetValue(obj),
                       nestingLevel + 1);
        }

        private IEnumerable<MemberInfo> GetTypeFieldsAndProperties(Type type)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            
            return type.GetFields(bindingFlags)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(bindingFlags));
        }
    }
}