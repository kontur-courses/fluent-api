using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace ObjectPrinting.Solved
{
    public record PrintingConfig<TOwner>(SerializationSettings Settings)
    {
        private static readonly ImmutableArray<Type> FinalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime),
            typeof(TimeSpan)
        ];
        
        public PrintingConfig() : this(new SerializationSettings())
        {
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, typeof(TPropType));
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            if (memberSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException();
            var memberExpression = (MemberExpression)memberSelector.Body;
            if (memberExpression.Member.MemberType != MemberTypes.Field &&
                memberExpression.Member.MemberType != MemberTypes.Property)
                throw new ArgumentException();
            return new PropertyPrintingConfig<TOwner, TPropType>
                (this, memberExpression.Member);
        }

        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            return new PrintingConfig<TOwner>(Settings with
            {
                ExcludedPropertyTypes = Settings.ExcludedPropertyTypes.Add(typeof(TPropType))
            });
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            if (obj is null)
                return "null" + Environment.NewLine;
            
            var type = obj.GetType();
            if (Settings.AlternativeSerialization.TryGetValue(type, out var serializer) &&
                !Settings.ExcludedPropertyTypes.Contains(type))
            {
                return serializer(obj) + Environment.NewLine;
            }
            if (FinalTypes.Contains(type))
                return obj + Environment.NewLine;
            
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            sb.Append(SerializeInstance(obj, nestingLevel, new string('\t', nestingLevel + 1)));
            return sb.ToString();
        }

        private StringBuilder SerializeInstance(object obj, int nestingLevel, string identation)
        {
            var result = new StringBuilder();
            
            foreach (var member in GetPropertiesAndFields(obj.GetType()))
            {
                var memberValue = GetValue(obj, member);
                
                if (Settings.MethodsForSerializingPropertiesAndFields.TryGetValue(member, out var serializer))
                    result.Append(identation + member.Name + " = " + serializer(memberValue));
                else
                    result.Append(identation + member.Name + " = " + PrintToString(memberValue,nestingLevel + 1));
            }

            return result;
        }

        private IEnumerable<MemberInfo> GetPropertiesAndFields(Type type)
        {
            foreach (var property in type.GetProperties())
                yield return property;
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                yield return field;
        }

        private object GetValue(object obj, MemberInfo member)
        {
            if (member is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(obj);
            if (member is FieldInfo fieldInfo)
                return fieldInfo.GetValue(obj);
            throw new ArgumentException();
        }
    }
}